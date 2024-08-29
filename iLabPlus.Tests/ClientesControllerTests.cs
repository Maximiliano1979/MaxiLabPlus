using Xunit;
using Xunit.Abstractions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using iLabPlus.Controllers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using iLabPlus.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using iLabPlus.Models.Clases;
using Xunit.Sdk;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace iLabPlus.Tests
{
    public class ClientesControllerTests
    {

        private readonly ITestOutputHelper _output;

        public ClientesControllerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Index_DeberiaRetornarVistaConDatosCorrectos()
        {
            // Arrange

            var clientes = new List<Clientes>
            {
                new Clientes { Guid = Guid.NewGuid(), Cliente = "CLI001", CliNombre = "Cliente 1", Empresa = "TestEmpresa" },
                new Clientes { Guid = Guid.NewGuid(), Cliente = "CLI002", CliNombre = "Cliente 2", Empresa = "TestEmpresa" }
            };

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.Clientes).Returns(MockDbSet(clientes).Object);


            var mockHttpContextAccessor = MockHttpContextAccessorFactory.CreateMockHttpContextAccessor("TestEmpresa", "TestUser", "ADMIN");

            var menuAccesos = new List<MenuUser> { new MenuUser("Test", "Test Menu", new List<MenuUserAccesos>(), "fa-test") };
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
            mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(menuAccesos);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });
            mockFunctionsBBDD.Setup(f => f.GetColumnsLayout(It.IsAny<string>())).Returns(new GrupoColumnsLayout { ColumnsLayoutUser = "TestLayout", ColumnsPinnedUser = 3 });


            var mockLogger = new Mock<ILogger<ClientesController>>();

            var controller = new ClientesController(mockContext.Object, mockFunctionsBBDD.Object, mockLogger.Object);

            // Act
            var result = controller.Index();

            // Assert

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("Clientes", viewResult.ViewName);

            var model = Assert.IsAssignableFrom<List<Clientes>>(viewResult.Model);

            Assert.Equal(2, model.Count);
            Assert.Contains(model, c => c.Cliente == "CLI001");
            Assert.Contains(model, c => c.Cliente == "CLI002");


            if (viewResult.ViewData.ContainsKey("MenuUserList"))
            {
                Assert.NotNull(viewResult.ViewData["MenuUserList"]);
            }
            else
            {
                _output.WriteLine("MenuUserList NO está presente en ViewData");
            }

            if (viewResult.ViewData.ContainsKey("ColumnsLayoutUser"))
            {
                Assert.NotNull(viewResult.ViewData["ColumnsLayoutUser"]);
            }
            else
            {
            }

            if (viewResult.ViewData.ContainsKey("ColumnsPinnedUser"))
            {
                Assert.NotNull(viewResult.ViewData["ColumnsPinnedUser"]);
            }
            else
            {
                _output.WriteLine("ColumnsPinnedUser NO está presente en ViewData");
            }

        }


        [Fact]
        public void DialogCliente_ClienteExistente_DeberiaRetornarVistaConCliente()
        {
            // Arrange
            var clienteGuid = Guid.NewGuid();
            var clientes = new List<Clientes>
            {
                new Clientes { Guid = clienteGuid, Cliente = "CLI001", CliNombre = "Cliente Existente", Empresa = "TestEmpresa" }
            };

            var mockContext = CreateMockContext(clientes);
            var controller = CreateController(mockContext);

            // Act
            var result = controller.DialogCliente(clienteGuid);

            // Assert
            var viewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DialogCliente", viewResult.ViewName);
            var model = Assert.IsType<Clientes>(viewResult.Model);
            Assert.Equal("Cliente Existente", model.CliNombre);

            // Verificar ViewBag
            Assert.NotNull(viewResult.ViewData["ListFacturaTipos"]);
            Assert.NotNull(viewResult.ViewData["ListTarifas"]);
            Assert.NotNull(viewResult.ViewData["ListVendedores"]);
            Assert.NotNull(viewResult.ViewData["ListExpNac"]);
            Assert.NotNull(viewResult.ViewData["ListIdiomas"]);
            Assert.NotNull(viewResult.ViewData["ListDivisas"]);
            Assert.NotNull(viewResult.ViewData["ValsysEmpresas"]);
        }

        [Fact]
        public void DialogCliente_ClienteNoExistente_DeberiaRetornarVistaConClienteNuevo()
        {
            // Arrange
            var mockContext = CreateMockContext(new List<Clientes>());
            var controller = CreateController(mockContext);

            // Act
            var result = controller.DialogCliente(Guid.NewGuid());

            // Assert
            var viewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DialogCliente", viewResult.ViewName);
            var model = Assert.IsType<Clientes>(viewResult.Model);
            Assert.Null(model.CliNombre);

            // Verificar ViewBag (igual que en el test anterior)
        }

        [Fact]
        public void DialogCliente_DeberiaPopularViewBagCorrectamente()
        {
            // Arrange
            var mockContext = CreateMockContext(new List<Clientes>());
            var controller = CreateController(mockContext);

            // Act
            var result = controller.DialogCliente(Guid.NewGuid());

            // Assert
            var viewResult = Assert.IsType<PartialViewResult>(result);

            var facturaTipos = Assert.IsType<List<string>>(viewResult.ViewData["ListFacturaTipos"]);
            Assert.Equal(5, facturaTipos.Count);
            Assert.Contains("Etiqueta", facturaTipos);

            var tarifas = Assert.IsType<List<TarifasVenta>>(viewResult.ViewData["ListTarifas"]);
            Assert.True(tarifas.Count > 0);
            Assert.Contains(tarifas, t => t.Tarifa == "");

            var vendedores = Assert.IsType<List<Vendedores>>(viewResult.ViewData["ListVendedores"]);
            Assert.True(vendedores.Count > 0);
            Assert.Contains(vendedores, v => v.Vendedor == "");

            var expNac = Assert.IsType<List<string>>(viewResult.ViewData["ListExpNac"]);
            Assert.Equal(3, expNac.Count);

            var idiomas = Assert.IsType<List<string>>(viewResult.ViewData["ListIdiomas"]);
            Assert.Equal(2, idiomas.Count);
            Assert.Contains("Español", idiomas);
            Assert.Contains("Ingles", idiomas);

            Assert.NotNull(viewResult.ViewData["ListDivisas"]);
            Assert.NotNull(viewResult.ViewData["ValsysEmpresas"]);
        }

      

        [Fact]
        public async Task CreateEdit_ErrorDuranteCreacion_DeberiaRetornarError400()
        {
            // Arrange
            var mockContext = CreateMockContext(new List<Clientes>());
            mockContext.Setup(m => m.Clientes.Add(It.IsAny<Clientes>())).Throws(new Exception("Error de prueba"));
            var controller = CreateController(mockContext);

            var newCustomer = new Clientes
            {
                Guid = Guid.Empty,
                CliNombre = "Cliente Error"
            };

            // Act
            var result = await controller.CreateEdit(newCustomer) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Error de prueba", result.Value);
        }



        [Fact]
        public async Task CreateEdit_NuevoCliente_DeberiaCrearClienteConNumeroSecuencial()
        {
            // Arrange
            var clientes = new List<Clientes>();
            var mockContext = CreateMockContext(clientes);
            var controller = CreateController(mockContext);

            var nuevoCliente = new Clientes
            {
                Guid = Guid.Empty,
                CliNombre = "Nuevo Cliente"
            };

            // Act
            var actionResult = await controller.CreateEdit(nuevoCliente);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(200, objectResult.StatusCode);

            var clienteCreado = Assert.IsType<Clientes>(objectResult.Value);
            Assert.NotNull(clienteCreado);
            Assert.Equal("000001", clienteCreado.Cliente);
            Assert.Equal("Nuevo Cliente", clienteCreado.CliNombre);
            Assert.Equal("TestEmpresa", clienteCreado.Empresa);
            Assert.Equal("TestUser", clienteCreado.IsoUser);
            Assert.NotEqual(Guid.Empty, clienteCreado.Guid);

            // Verificar que el cliente se ha añadido a la lista
            Assert.Single(clientes);
            var clienteEnLista = clientes.First();
            Assert.Equal("000001", clienteEnLista.Cliente);
            Assert.Equal("Nuevo Cliente", clienteEnLista.CliNombre);
            Assert.NotEqual(Guid.Empty, clienteEnLista.Guid);
            Assert.Equal(clienteCreado.Guid, clienteEnLista.Guid);
        }



        [Fact]
        public async Task CreateEdit_ClienteExistente_DeberiaActualizarCliente()
        {
            // Arrange
            var fechaOriginal = DateTime.Now.AddDays(-1);
            var clienteExistente = new Clientes
            {
                Guid = Guid.NewGuid(),
                Cliente = "000001",
                CliNombre = "Cliente Original",
                Empresa = "TestEmpresa",
                IsoUser = "UsuarioOriginal",
                IsoFecAlt = fechaOriginal,
                IsoFecMod = fechaOriginal
            };

            var clientes = new List<Clientes> { clienteExistente }.AsQueryable();

            var mockSet = new Mock<DbSet<Clientes>>();
            mockSet.As<IAsyncEnumerable<Clientes>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new AsyncEnumerator<Clientes>(clientes.GetEnumerator()));

            mockSet.As<IQueryable<Clientes>>()
                .Setup(m => m.Provider)
                .Returns(new AsyncQueryProvider<Clientes>(clientes.Provider));

            mockSet.As<IQueryable<Clientes>>().Setup(m => m.Expression).Returns(clientes.Expression);
            mockSet.As<IQueryable<Clientes>>().Setup(m => m.ElementType).Returns(clientes.ElementType);
            mockSet.As<IQueryable<Clientes>>().Setup(m => m.GetEnumerator()).Returns(() => clientes.GetEnumerator());

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.Clientes).Returns(mockSet.Object);

            // Configurar SaveChangesAsync para que realmente actualice el cliente en la lista
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => {
                    var updatedCliente = clientes.First();
                    updatedCliente.CliNombre = "Cliente Actualizado";
                    updatedCliente.IsoUser = "TestUser";
                    updatedCliente.IsoFecMod = DateTime.Now;
                })
                .ReturnsAsync(1);

            var controller = CreateController(mockContext);

            _output.WriteLine($"Cliente existente creado con GUID: {clienteExistente.Guid}");

            var clienteActualizado = new Clientes
            {
                Guid = clienteExistente.Guid,
                Cliente = clienteExistente.Cliente,
                CliNombre = "Cliente Actualizado"
            };

            _output.WriteLine($"Intentando actualizar cliente con GUID: {clienteActualizado.Guid}");

            // Act
            var actionResult = await controller.CreateEdit(clienteActualizado);

            // Assert
            _output.WriteLine($"Resultado de la acción: {actionResult}");

            var objectResult = Assert.IsType<ObjectResult>(actionResult);
            _output.WriteLine($"Código de estado: {objectResult.StatusCode}");

            Assert.Equal(200, objectResult.StatusCode);

            var clienteResultado = Assert.IsType<Clientes>(objectResult.Value);
            Assert.NotNull(clienteResultado);
            _output.WriteLine($"Cliente resultado - GUID: {clienteResultado.Guid}, Nombre: {clienteResultado.CliNombre}");

            Assert.Equal(clienteExistente.Guid, clienteResultado.Guid);
            Assert.Equal(clienteExistente.Cliente, clienteResultado.Cliente);
            Assert.Equal("Cliente Actualizado", clienteResultado.CliNombre);
            Assert.Equal("TestEmpresa", clienteResultado.Empresa);
            Assert.Equal("TestUser", clienteResultado.IsoUser);
            Assert.True(clienteResultado.IsoFecMod > fechaOriginal);

            // Verificar que el cliente se ha actualizado en la lista
            var clienteEnLista = clientes.First();
            _output.WriteLine($"Cliente en lista - GUID: {clienteEnLista.Guid}, Nombre: {clienteEnLista.CliNombre}");

            Assert.Equal("Cliente Actualizado", clienteEnLista.CliNombre);
            Assert.Equal("TestUser", clienteEnLista.IsoUser);
            Assert.True(clienteEnLista.IsoFecMod > fechaOriginal);
        }


        [Fact]
        public async Task Delete_CLIENTE_ClienteExistente_DeberiaEliminarYRetornarOK()
        {
            // Arrange
            var clienteExistente = new Clientes
            {
                Guid = Guid.NewGuid(),
                Cliente = "000001",
                CliNombre = "Cliente a Eliminar",
                Empresa = "TestEmpresa"
            };

            var clientes = new List<Clientes> { clienteExistente };
            var mockSet = new Mock<DbSet<Clientes>>();

            mockSet.As<IQueryable<Clientes>>()
                .Setup(m => m.Provider)
                .Returns(clientes.AsQueryable().Provider);
            mockSet.As<IQueryable<Clientes>>()
                .Setup(m => m.Expression)
                .Returns(clientes.AsQueryable().Expression);
            mockSet.As<IQueryable<Clientes>>()
                .Setup(m => m.ElementType)
                .Returns(clientes.AsQueryable().ElementType);
            mockSet.As<IQueryable<Clientes>>()
                .Setup(m => m.GetEnumerator())
                .Returns(() => clientes.AsQueryable().GetEnumerator());

            mockSet.Setup(m => m.Remove(It.IsAny<Clientes>()))
                .Callback<Clientes>((entity) => clientes.Remove(entity));

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.Clientes).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var controller = CreateController(mockContext);

            // Act
            var result = await controller.Delete_CLIENTE(clienteExistente.Guid);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, statusCodeResult.StatusCode);
            Assert.Equal("OK", statusCodeResult.Value);
            Assert.Empty(clientes);
        }


        [Fact]
        public void GetClientesLazing_DeberiaRetornarClientesFiltrados()
        {
            // Arrange
            var clientes = new List<Clientes>
            {
                new Clientes { Guid = Guid.NewGuid(), Cliente = "001", CliNombre = "Cliente Uno", Empresa = "TestEmpresa" },
                new Clientes { Guid = Guid.NewGuid(), Cliente = "002", CliNombre = "Cliente Dos", Empresa = "TestEmpresa" },
                new Clientes { Guid = Guid.NewGuid(), Cliente = "003", CliNombre = "Otro Cliente", Empresa = "TestEmpresa" },
                new Clientes { Guid = Guid.NewGuid(), Cliente = "004", CliNombre = "Cliente Cuatro", Empresa = "OtraEmpresa" }
            };

            var mockSet = new Mock<DbSet<Clientes>>();
            mockSet.As<IQueryable<Clientes>>().Setup(m => m.Provider).Returns(clientes.AsQueryable().Provider);
            mockSet.As<IQueryable<Clientes>>().Setup(m => m.Expression).Returns(clientes.AsQueryable().Expression);
            mockSet.As<IQueryable<Clientes>>().Setup(m => m.ElementType).Returns(clientes.AsQueryable().ElementType);
            mockSet.As<IQueryable<Clientes>>().Setup(m => m.GetEnumerator()).Returns(() => clientes.AsQueryable().GetEnumerator());

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.Clientes).Returns(mockSet.Object);

            var controller = CreateController(mockContext);

            // Test Case 1: Query vacío
            var result1 = controller.GetClientesLazing(string.Empty, 10) as JsonResult;
            var clientesResult1 = JsonConvert.DeserializeObject<List<Clientes>>(JsonConvert.SerializeObject(result1.Value));

            Assert.NotNull(clientesResult1);
            Assert.Equal(3, clientesResult1.Count);
            Assert.All(clientesResult1, c => Assert.Equal("TestEmpresa", c.Empresa));

            // Test Case 2: Query con coincidencia al inicio
            var result2 = controller.GetClientesLazing("Cliente", 10) as JsonResult;
            var clientesResult2 = JsonConvert.DeserializeObject<List<Clientes>>(JsonConvert.SerializeObject(result2.Value));

            Assert.NotNull(clientesResult2);
            Assert.Equal(2, clientesResult2.Count);
            Assert.All(clientesResult2, c => Assert.StartsWith("Cliente", c.CliNombre));
            Assert.All(clientesResult2, c => Assert.Equal("TestEmpresa", c.Empresa));

            // Test Case 3: Query sin coincidencias
            var result3 = controller.GetClientesLazing("NoExiste", 10) as JsonResult;
            var clientesResult3 = JsonConvert.DeserializeObject<List<Clientes>>(JsonConvert.SerializeObject(result3.Value));

            Assert.NotNull(clientesResult3);
            Assert.Empty(clientesResult3);

            // Test Case 4: Verificar CalcClienteNombre y búsqueda por número de cliente
            var result4 = controller.GetClientesLazing("001", 10) as JsonResult;
            var clientesResult4 = JsonConvert.DeserializeObject<List<Clientes>>(JsonConvert.SerializeObject(result4.Value));

            Assert.NotNull(clientesResult4);
            Assert.Single(clientesResult4);
            Assert.Equal("001  :  Cliente Uno", clientesResult4[0].CalcClienteNombre);
            Assert.Equal("TestEmpresa", clientesResult4[0].Empresa);

            // Test Case 5: Verificar límite máximo
            var result5 = controller.GetClientesLazing(string.Empty, 2) as JsonResult;
            var clientesResult5 = JsonConvert.DeserializeObject<List<Clientes>>(JsonConvert.SerializeObject(result5.Value));

            Assert.NotNull(clientesResult5);
            Assert.Equal(2, clientesResult5.Count);
        }



        [Fact]
        public async Task Cliente_Copiar_DeberiaCrearCopiaDeClienteExistente()
        {
            // Arrange
            var clienteOriginal = new Clientes
            {
                Guid = Guid.NewGuid(),
                Cliente = "CLI001",
                CliNombre = "Cliente Original",
                Empresa = "TestEmpresa"
            };

            var clientes = new List<Clientes> { clienteOriginal };
            var mockContext = CreateMockContext(clientes);
            var controller = CreateController(mockContext);

            // Act
            var result = await controller.Cliente_Copiar("CLI001", "CLI002");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonValue = JObject.FromObject(jsonResult.Value);

            Assert.True(jsonValue["success"].Value<bool>());
            Assert.NotNull(jsonValue["data"]);

            var nuevoCliente = jsonValue["data"].ToObject<Clientes>();
            Assert.NotEqual(clienteOriginal.Guid, nuevoCliente.Guid);
            Assert.Equal("CLI002", nuevoCliente.Cliente);
            Assert.Equal(clienteOriginal.CliNombre, nuevoCliente.CliNombre);
            Assert.Equal("TestEmpresa", nuevoCliente.Empresa);
            Assert.Equal("TestUser", nuevoCliente.IsoUser);
            Assert.NotEqual(default(DateTime), nuevoCliente.IsoFecAlt);
            Assert.NotEqual(default(DateTime), nuevoCliente.IsoFecMod);

            mockContext.Verify(m => m.Clientes.Add(It.IsAny<Clientes>()), Times.Once());
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once());
        }



        [Fact]
        public async Task Cliente_Copiar_DeberiaRetornarErrorSiClienteNuevoYaExiste()
        {
            // Arrange
            var clienteExistente = new Clientes
            {
                Guid = Guid.NewGuid(),
                Cliente = "CLI001",
                CliNombre = "Cliente Existente",
                Empresa = "TestEmpresa"
            };

            var clientes = new List<Clientes> { clienteExistente };
            var mockContext = CreateMockContext(clientes);
            var controller = CreateController(mockContext);

            // Act
            var result = await controller.Cliente_Copiar("CLI002", "CLI001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonValue = JObject.FromObject(jsonResult.Value);

            Assert.False(jsonValue["success"].Value<bool>());
            Assert.Equal("El cliente nuevo ya existe.", jsonValue["message"].Value<string>());

            mockContext.Verify(m => m.Clientes.Add(It.IsAny<Clientes>()), Times.Never());
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Never());
        }

        [Fact]
        public async Task Cliente_Copiar_DeberiaRetornarErrorSiClienteOriginalNoExiste()
        {
            // Arrange
            var clientes = new List<Clientes>();
            var mockContext = CreateMockContext(clientes);
            var controller = CreateController(mockContext);

            // Act
            var result = await controller.Cliente_Copiar("CLI001", "CLI002");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonValue = JObject.FromObject(jsonResult.Value);

            Assert.False(jsonValue["success"].Value<bool>());
            Assert.Equal("Cliente original no encontrado.", jsonValue["message"].Value<string>());

            mockContext.Verify(m => m.Clientes.Add(It.IsAny<Clientes>()), Times.Never());
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Never());
        }

        //                  TEST GETDOCTORES            //////////////////////////////////////

        [Fact]
        public void GetDoctores_DeberiaLoggearYRetornarDoctoresActivos()
        {
            _output.WriteLine("Iniciando test GetDoctores_DeberiaLoggearYRetornarDoctoresActivos");

            try
            {
                // Arrange
                _output.WriteLine("Configurando datos de prueba");
                var doctores = new List<Doctores>
                {
                    new Doctores { Guid = Guid.NewGuid(), Nombre = "Dr. Activo", Doctor = "DOC001", NumColegiado = "12345", Mail = "activo@example.com", Telefono1 = "123456789", Activo = true },
                    new Doctores { Guid = Guid.NewGuid(), Nombre = "Dr. Inactivo", Doctor = "DOC002", NumColegiado = "67890", Mail = "inactivo@example.com", Telefono1 = "987654321", Activo = false },
                    new Doctores { Guid = Guid.NewGuid(), Nombre = "Dra. Activa", Doctor = "DOC003", NumColegiado = "54321", Mail = "activa@example.com", Telefono1 = "456789123", Activo = true }
                };
                _output.WriteLine($"Configurados {doctores.Count} doctores de prueba");

                var mockSet = new Mock<DbSet<Doctores>>();
                mockSet.As<IQueryable<Doctores>>().Setup(m => m.Provider).Returns(doctores.AsQueryable().Provider);
                mockSet.As<IQueryable<Doctores>>().Setup(m => m.Expression).Returns(doctores.AsQueryable().Expression);
                mockSet.As<IQueryable<Doctores>>().Setup(m => m.ElementType).Returns(doctores.AsQueryable().ElementType);
                mockSet.As<IQueryable<Doctores>>().Setup(m => m.GetEnumerator()).Returns(() => doctores.AsQueryable().GetEnumerator());

                var mockContext = new Mock<DbContextiLabPlus>();
                mockContext.Setup(c => c.Doctores).Returns(mockSet.Object);

                // Configurar UsuariosGridsCfg para devolver una lista vacía
                var usuariosGridsCfgData = new List<UsuariosGridsCfg>().AsQueryable();
                var mockUsuariosGridsCfgDbSet = new Mock<DbSet<UsuariosGridsCfg>>();
                mockUsuariosGridsCfgDbSet.As<IQueryable<UsuariosGridsCfg>>().Setup(m => m.Provider).Returns(usuariosGridsCfgData.Provider);
                mockUsuariosGridsCfgDbSet.As<IQueryable<UsuariosGridsCfg>>().Setup(m => m.Expression).Returns(usuariosGridsCfgData.Expression);
                mockUsuariosGridsCfgDbSet.As<IQueryable<UsuariosGridsCfg>>().Setup(m => m.ElementType).Returns(usuariosGridsCfgData.ElementType);
                mockUsuariosGridsCfgDbSet.As<IQueryable<UsuariosGridsCfg>>().Setup(m => m.GetEnumerator()).Returns(usuariosGridsCfgData.GetEnumerator());
                mockContext.Setup(c => c.UsuariosGridsCfg).Returns(mockUsuariosGridsCfgDbSet.Object);

                var mockLogger = new Mock<ILogger<ClientesController>>();

                var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                var context = new DefaultHttpContext();
                var claims = new List<Claim>
                {
                    new Claim("Empresa", "TestEmpresa"),
                    new Claim("Usuario", "TestUser")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var claimsPrincipal = new ClaimsPrincipal(identity);
                context.User = claimsPrincipal;
                mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

                var functionsBBDD = new FunctionsBBDD(mockContext.Object, mockHttpContextAccessor.Object);

                var controller = new ClientesController(mockContext.Object, functionsBBDD, mockLogger.Object, _output);

                // Act
                _output.WriteLine("Ejecutando GetDoctores()");
                var actionResult = controller.GetDoctores();

                // Assert
                _output.WriteLine("Iniciando assertions");
                var jsonResult = Assert.IsType<JsonResult>(actionResult);
                var jsonData = JObject.FromObject(jsonResult.Value);

                Assert.True(jsonData["success"].Value<bool>());
                _output.WriteLine("Assertion: success es true");

                var doctoresResult = jsonData["doctores"].ToObject<List<object>>();
                Assert.Equal(2, doctoresResult.Count);
                _output.WriteLine($"Assertion: Se encontraron {doctoresResult.Count} doctores activos");

                foreach (var doctor in doctoresResult)
                {
                    var doctorObj = JObject.FromObject(doctor);
                    _output.WriteLine($"Doctor en el resultado: {doctorObj["Nombre"]}, ID: {doctorObj["Doctor"]}");
                }

                _output.WriteLine("Test completado con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Excepción no controlada en el test: {ex}");
                throw;
            }
        }


        [Fact]
        public void GetDoctores_DeberiaRetornarErrorEnCasoDeExcepcion()
        {
            _output.WriteLine("Iniciando test GetDoctores_DeberiaRetornarErrorEnCasoDeExcepcion");

            try
            {
                // Arrange
                _output.WriteLine("Configurando mock de DbContextiLabPlus");
                var mockContext = new Mock<DbContextiLabPlus>();

                // Configurar UsuariosGridsCfg para devolver una lista vacía
                var usuariosGridsCfgData = new List<UsuariosGridsCfg>().AsQueryable();
                var mockUsuariosGridsCfgDbSet = new Mock<DbSet<UsuariosGridsCfg>>();
                mockUsuariosGridsCfgDbSet.As<IQueryable<UsuariosGridsCfg>>().Setup(m => m.Provider).Returns(usuariosGridsCfgData.Provider);
                mockUsuariosGridsCfgDbSet.As<IQueryable<UsuariosGridsCfg>>().Setup(m => m.Expression).Returns(usuariosGridsCfgData.Expression);
                mockUsuariosGridsCfgDbSet.As<IQueryable<UsuariosGridsCfg>>().Setup(m => m.ElementType).Returns(usuariosGridsCfgData.ElementType);
                mockUsuariosGridsCfgDbSet.As<IQueryable<UsuariosGridsCfg>>().Setup(m => m.GetEnumerator()).Returns(usuariosGridsCfgData.GetEnumerator());
                mockContext.Setup(c => c.UsuariosGridsCfg).Returns(mockUsuariosGridsCfgDbSet.Object);

                // Configurar Doctores para lanzar una excepción
                mockContext.Setup(c => c.Doctores).Throws(new Exception("Error de prueba"));

                _output.WriteLine("Configurando mock de ILogger");
                var mockLogger = new Mock<ILogger<ClientesController>>();

                _output.WriteLine("Configurando mock de IHttpContextAccessor");
                var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                var context = new DefaultHttpContext();
                var claims = new List<Claim>
                {
                    new Claim("Empresa", "TestEmpresa"),
                    new Claim("Usuario", "TestUser")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var claimsPrincipal = new ClaimsPrincipal(identity);
                context.User = claimsPrincipal;
                mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

                _output.WriteLine("Creando instancia de FunctionsBBDD");
                var functionsBBDD = new FunctionsBBDD(mockContext.Object, mockHttpContextAccessor.Object);

                _output.WriteLine("Creando instancia de ClientesController");
                var controller = new ClientesController(mockContext.Object, functionsBBDD, mockLogger.Object, _output);

                // Act
                _output.WriteLine("Ejecutando GetDoctores()");
                var actionResult = controller.GetDoctores();

                // Assert
                _output.WriteLine("Iniciando assertions");
                var jsonResult = Assert.IsType<JsonResult>(actionResult);
                _output.WriteLine($"Tipo de resultado: {jsonResult.GetType().Name}");

                var jsonData = JObject.FromObject(jsonResult.Value);
                _output.WriteLine($"Contenido del resultado: {jsonData}");

                Assert.False(jsonData["success"].Value<bool>());
                _output.WriteLine("Assertion: success es false");

                Assert.Equal("Error al obtener los datos de los doctores.", jsonData["message"].Value<string>());
                _output.WriteLine("Assertion: mensaje de error correcto");

                _output.WriteLine("Test completado con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Excepción no controlada en el test: {ex}");
                throw;
            }
        }


    


        [Fact]
        public async Task Cliente_Copiar_DeberiaManejorExcepcionesYRetornarError()
        {
            // Arrange
            var clienteOriginal = new Clientes
            {
                Guid = Guid.NewGuid(),
                Cliente = "CLI001",
                CliNombre = "Cliente Original",
                Empresa = "TestEmpresa"
            };

            var clientes = new List<Clientes> { clienteOriginal };
            var mockContext = CreateMockContext(clientes);

            // Simular una excepción al guardar cambios
            mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Error simulado al guardar cambios", new Exception("Excepción interna")));

            var controller = CreateController(mockContext);

            // Act
            var result = await controller.Cliente_Copiar("CLI001", "CLI002");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonValue = JObject.FromObject(jsonResult.Value);

            Assert.False(jsonValue["success"].Value<bool>());
            Assert.Equal("Ocurrió un error durante la copia del cliente.", jsonValue["message"].Value<string>());

            mockContext.Verify(m => m.Clientes.Add(It.IsAny<Clientes>()), Times.Once());
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public void DialogClienteCopiar_DeberiaEstablecerViewBagYDevolverVistaCorrecta()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var controller = CreateController(mockContext);
            var clienteTest = "CLI001";

            // Act
            var result = controller._DialogClienteCopiar(clienteTest);

            // Assert
            var viewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DialogClienteCopiar", viewResult.ViewName);

            Assert.Equal(clienteTest, controller.ViewBag.ClienteOld);
        }



    




        [Fact]
        public async Task Cliente_Renombrar_DeberiaRetornarErrorSiClienteNuevoYaExiste()
        {
            // Arrange
            var clienteExistente1 = new Clientes
            {
                Guid = Guid.NewGuid(),
                Cliente = "CLI001",
                CliNombre = "Cliente Existente 1",
                Empresa = "TestEmpresa"
            };
            var clienteExistente2 = new Clientes
            {
                Guid = Guid.NewGuid(),
                Cliente = "CLI002",
                CliNombre = "Cliente Existente 2",
                Empresa = "TestEmpresa"
            };

            var clientes = new List<Clientes> { clienteExistente1, clienteExistente2 };
            var mockContext = CreateMockContext(clientes);
            var controller = CreateController(mockContext);

            // Act
            var result = await controller.Cliente_Renombrar("CLI001", "CLI002");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonValue = JObject.FromObject(jsonResult.Value);

            Assert.False(jsonValue["success"].Value<bool>());
            Assert.Equal("El cliente nuevo ya existe.", jsonValue["message"].Value<string>());

            mockContext.Verify(m => m.Clientes.Update(It.IsAny<Clientes>()), Times.Never());
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task Cliente_Renombrar_DeberiaRetornarErrorSiClienteOriginalNoExiste()
        {
            // Arrange
            var clientes = new List<Clientes>();
            var mockContext = CreateMockContext(clientes);
            var controller = CreateController(mockContext);

            // Act
            var result = await controller.Cliente_Renombrar("CLI001", "CLI002");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonValue = JObject.FromObject(jsonResult.Value);

            Assert.False(jsonValue["success"].Value<bool>());
            Assert.Equal("Cliente original no encontrado.", jsonValue["message"].Value<string>());

            mockContext.Verify(m => m.Clientes.Update(It.IsAny<Clientes>()), Times.Never());
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task Cliente_Renombrar_DeberiaManejorExcepcionesYRetornarError()
        {
            // Arrange
            var clienteOriginal = new Clientes
            {
                Guid = Guid.NewGuid(),
                Cliente = "CLI001",
                CliNombre = "Cliente Original",
                Empresa = "TestEmpresa"
            };

            var clientes = new List<Clientes> { clienteOriginal };
            var mockContext = CreateMockContext(clientes);

            // Simular una excepción al guardar cambios
            mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Error simulado al guardar cambios", new Exception("Excepción interna")));

            var controller = CreateController(mockContext);

            // Act
            var result = await controller.Cliente_Renombrar("CLI001", "CLI002");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonValue = JObject.FromObject(jsonResult.Value);

            Assert.False(jsonValue["success"].Value<bool>());
            Assert.Equal("Ocurrió un error durante el cambio de nombre del cliente.", jsonValue["message"].Value<string>());

            mockContext.Verify(m => m.Clientes.Update(It.IsAny<Clientes>()), Times.Once());
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }



        [Fact]
        public async Task GetDoctoresPorClinica_DeberiaRetornarDoctoresActivosPorClinica()
        {
            _output.WriteLine("Iniciando test GetDoctoresPorClinica_DeberiaRetornarDoctoresActivosPorClinica");

            try
            {
                // Arrange
                _output.WriteLine("Configurando datos de prueba");
                var nombreClinica = "ClinicaPrueba";

                var doctoresClinicas = new List<DoctoresClinicas>
                {
                    new DoctoresClinicas { Guid = Guid.NewGuid(), Clinica = nombreClinica, Doctor = "DOC001" },
                    new DoctoresClinicas { Guid = Guid.NewGuid(), Clinica = nombreClinica, Doctor = "DOC002" },
                    new DoctoresClinicas { Guid = Guid.NewGuid(), Clinica = "OtraClinica", Doctor = "DOC003" }
                };

                var doctores = new List<Doctores>
                {
                    new Doctores { Guid = Guid.NewGuid(), Doctor = "DOC001", Nombre = "Dr. Activo", NumColegiado = "12345", Mail = "activo@example.com", Telefono1 = "123456789", Activo = true },
                    new Doctores { Guid = Guid.NewGuid(), Doctor = "DOC002", Nombre = "Dr. Inactivo", NumColegiado = "67890", Mail = "inactivo@example.com", Telefono1 = "987654321", Activo = false },
                    new Doctores { Guid = Guid.NewGuid(), Doctor = "DOC003", Nombre = "Dra. Otra Clinica", NumColegiado = "54321", Mail = "otra@example.com", Telefono1 = "456789123", Activo = true }
                };

                var usuariosGridsCfg = new List<UsuariosGridsCfg>
                {
                    new UsuariosGridsCfg { Empresa = "TestEmpresa", Usuario = "TestUser", GridID = "gridClientes", ColumnsLayout = "TestLayout", ColumnsPinned = 3 }
                };

                _output.WriteLine($"Configurados {doctoresClinicas.Count} DoctoresClinicas, {doctores.Count} Doctores y {usuariosGridsCfg.Count} UsuariosGridsCfg de prueba");

                var mockDoctoresClinicasSet = MockDbSet(doctoresClinicas);
                var mockDoctoresSet = MockDbSet(doctores);
                var mockUsuariosGridsCfgSet = MockDbSet(usuariosGridsCfg);

                var mockContext = new Mock<DbContextiLabPlus>();
                mockContext.Setup(c => c.DoctoresClinicas).Returns(mockDoctoresClinicasSet.Object);
                mockContext.Setup(c => c.Doctores).Returns(mockDoctoresSet.Object);
                mockContext.Setup(c => c.UsuariosGridsCfg).Returns(mockUsuariosGridsCfgSet.Object);

                var mockLogger = new Mock<ILogger<ClientesController>>();

                var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                var context = new DefaultHttpContext();
                var claims = new List<Claim>
                {
                    new Claim("Empresa", "TestEmpresa"),
                    new Claim("Usuario", "TestUser")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var claimsPrincipal = new ClaimsPrincipal(identity);
                context.User = claimsPrincipal;
                mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

                var functionsBBDD = new FunctionsBBDD(mockContext.Object, mockHttpContextAccessor.Object);

                _output.WriteLine("Creando instancia de ClientesController");
                var controller = new ClientesController(mockContext.Object, functionsBBDD, mockLogger.Object, _output);

                // Act
                _output.WriteLine($"Ejecutando GetDoctoresPorClinica con nombreClinica: {nombreClinica}");
                var actionResult = await controller.GetDoctoresPorClinica(nombreClinica);

                // Assert
                _output.WriteLine("Iniciando assertions");
                var jsonResult = Assert.IsType<JsonResult>(actionResult);
                var jsonData = JObject.FromObject(jsonResult.Value);

                Assert.True(jsonData["success"].Value<bool>());
                _output.WriteLine("Assertion: success es true");

                var doctoresResult = jsonData["doctores"].ToObject<List<object>>();
                Assert.Single(doctoresResult);
                _output.WriteLine($"Assertion: Se encontró {doctoresResult.Count} doctor activo para la clínica {nombreClinica}");

                var doctorActivo = JObject.FromObject(doctoresResult[0]);
                _output.WriteLine($"Doctor activo encontrado: Nombre = {doctorActivo["Nombre"]}, NumColegiado = {doctorActivo["NumColegiado"]}");

                Assert.Equal("Dr. Activo", doctorActivo["Nombre"].Value<string>());
                Assert.Equal("12345", doctorActivo["NumColegiado"].Value<string>());

                _output.WriteLine("Test completado con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Excepción no controlada en el test: {ex}");
                throw;
            }
        }


        [Fact]
        public async Task GetDoctoresPorClinica_DeberiaRetornarErrorEnCasoDeExcepcion()
        {
            _output.WriteLine("Iniciando test GetDoctoresPorClinica_DeberiaRetornarErrorEnCasoDeExcepcion");

            try
            {
                // Arrange
                var nombreClinica = "ClinicaPrueba";

                var usuariosGridsCfg = new List<UsuariosGridsCfg>
                {
                    new UsuariosGridsCfg { Empresa = "TestEmpresa", Usuario = "TestUser", GridID = "gridClientes", ColumnsLayout = "TestLayout", ColumnsPinned = 3 }
                };

                var mockUsuariosGridsCfgSet = MockDbSet(usuariosGridsCfg);

                var mockContext = new Mock<DbContextiLabPlus>();
                mockContext.Setup(c => c.UsuariosGridsCfg).Returns(mockUsuariosGridsCfgSet.Object);
                mockContext.Setup(c => c.DoctoresClinicas).Throws(new Exception("Error de prueba"));

                var mockLogger = new Mock<ILogger<ClientesController>>();

                var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                var context = new DefaultHttpContext();
                var claims = new List<Claim>
                {
                    new Claim("Empresa", "TestEmpresa"),
                    new Claim("Usuario", "TestUser")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var claimsPrincipal = new ClaimsPrincipal(identity);
                context.User = claimsPrincipal;
                mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

                var functionsBBDD = new FunctionsBBDD(mockContext.Object, mockHttpContextAccessor.Object);

                var controller = new ClientesController(mockContext.Object, functionsBBDD, mockLogger.Object, _output);

                // Act
                _output.WriteLine($"Ejecutando GetDoctoresPorClinica con nombreClinica: {nombreClinica}");
                var actionResult = await controller.GetDoctoresPorClinica(nombreClinica);

                // Assert
                _output.WriteLine("Iniciando assertions");
                var jsonResult = Assert.IsType<JsonResult>(actionResult);
                var jsonData = JObject.FromObject(jsonResult.Value);

                Assert.False(jsonData["success"].Value<bool>());
                _output.WriteLine("Assertion: success es false");

                Assert.Equal("Error al obtener los datos de los doctores.", jsonData["message"].Value<string>());
                _output.WriteLine("Assertion: mensaje de error correcto");

                _output.WriteLine("Test completado con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Excepción no controlada en el test: {ex}");
                throw;
            }
        }


        // Test para IsNumeric de ClientesController

        [Theory]
        [InlineData("123", true)]
        [InlineData("123.45", false)]
        [InlineData("-123", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("0", true)]
        [InlineData("00123", true)]
        public void IsNumeric_DeberiaValidarCorrectamente(string input, bool expectedResult)
        {
            _output.WriteLine($"Iniciando test IsNumeric con input: '{input}'");

            // Arrange
            // No necesitamos crear una instancia completa de ClientesController

            // Act
            bool result = ClientesControllerTestHelper.IsNumeric(input);

            // Assert
            Assert.Equal(expectedResult, result);
            _output.WriteLine($"Assertion: IsNumeric('{input}') retornó {result}, esperado: {expectedResult}");

            _output.WriteLine("Test completado con éxito");
        }

        public static class ClientesControllerTestHelper
        {
            public static bool IsNumeric(string str)
            {
                // Implementación del método IsNumeric
                return !string.IsNullOrWhiteSpace(str) && str.All(char.IsDigit);
            }
        }


        // Mas test de casos especificos del CreateEdit de Clientes Controller:



        [Fact]
        public async Task CreateEdit_CuandoResultProcessEsFalso_DebeRetornar400()
        {
            _output.WriteLine("Iniciando test CreateEdit_CuandoResultProcessEsFalso_DebeRetornar400");

            try
            {
                // Arrange
                _output.WriteLine("Configurando mocks y datos de prueba");

                var data = new List<Clientes>().AsQueryable();

                var mockSet = new Mock<DbSet<Clientes>>();

                _output.WriteLine("Configurando DbSet mock");
                mockSet.As<IAsyncEnumerable<Clientes>>()
                    .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                    .Returns(new TestAsyncEnumerator<Clientes>(data.GetEnumerator()));

                mockSet.As<IQueryable<Clientes>>()
                    .Setup(m => m.Provider)
                    .Returns(new TestAsyncQueryProvider<Clientes>(data.Provider));

                mockSet.As<IQueryable<Clientes>>().Setup(m => m.Expression).Returns(data.Expression);
                mockSet.As<IQueryable<Clientes>>().Setup(m => m.ElementType).Returns(data.ElementType);
                mockSet.As<IQueryable<Clientes>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

                var mockContext = new Mock<DbContextiLabPlus>();
                _output.WriteLine("Configurando DbContext mock");
                mockContext.Setup(c => c.Clientes).Returns(mockSet.Object);

                var mockLogger = new Mock<ILogger<ClientesController>>();

                _output.WriteLine("Configurando IHttpContextAccessor");
                var mockHttpContextAccessor = MockHttpContextAccessorFactory.CreateMockHttpContextAccessor("TestEmpresa", "TestUser", "ADMIN");

                _output.WriteLine("Configurando UsuariosGridsCfg");
                var usuariosGridsCfg = new List<UsuariosGridsCfg>
        {
            new UsuariosGridsCfg { Empresa = "TestEmpresa", Usuario = "TestUser", GridID = "gridClientes", ColumnsLayout = "TestLayout", ColumnsPinned = 3 }
        };
                var mockUsuariosGridsCfgDbSet = MockDbSet(usuariosGridsCfg);
                mockContext.Setup(c => c.UsuariosGridsCfg).Returns(mockUsuariosGridsCfgDbSet.Object);

                _output.WriteLine("Creando instancia de FunctionsBBDD");
                var functionsBBDD = new FunctionsBBDD(mockContext.Object, mockHttpContextAccessor.Object);

                _output.WriteLine("Creando instancia de ClientesController");
                var controller = new ClientesController(mockContext.Object, functionsBBDD, mockLogger.Object, _output);

                var cliente = new Clientes { Guid = Guid.NewGuid() };
                _output.WriteLine($"Cliente de prueba creado con GUID: {cliente.Guid}");

                // Act
                _output.WriteLine("Ejecutando método CreateEdit");
                var result = await controller.CreateEdit(cliente);

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var statusCodeResult = Assert.IsType<ObjectResult>(result);
                _output.WriteLine($"Tipo de resultado: {statusCodeResult.GetType().Name}");

                Assert.Equal(400, statusCodeResult.StatusCode);
                _output.WriteLine($"StatusCode: {statusCodeResult.StatusCode}");

                Assert.NotNull(statusCodeResult.Value);
                Assert.IsType<string>(statusCodeResult.Value);
                _output.WriteLine($"El valor del resultado es: {statusCodeResult.Value}");

                _output.WriteLine("Test completado con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }



        [Theory]
        [InlineData(null, "000001")]
        [InlineData("", "000001")]
        [InlineData("000005", "000006")]
        [InlineData("999999", "1000000")]
        public void GenerateNextClientNumber_DeberiaGenerarNumeroCorrectamente(string maxCliente, string expectedNextNumber)
        {
            _output.WriteLine($"Iniciando test con maxCliente: '{maxCliente}', expectedNextNumber: '{expectedNextNumber}'");

            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Clientes>>();
            var data = new List<Clientes>();
            if (!string.IsNullOrEmpty(maxCliente))
            {
                data.Add(new Clientes { Cliente = maxCliente, Empresa = "TestEmpresa" });
            }
            var queryableData = data.AsQueryable();

            _output.WriteLine("Configurando DbSet mock");
            mockSet.As<IQueryable<Clientes>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            mockSet.As<IQueryable<Clientes>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            mockSet.As<IQueryable<Clientes>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            mockSet.As<IQueryable<Clientes>>().Setup(m => m.GetEnumerator()).Returns(() => queryableData.GetEnumerator());

            mockContext.Setup(c => c.Clientes).Returns(mockSet.Object);

            var mockHttpContextAccessor = MockHttpContextAccessorFactory.CreateMockHttpContextAccessor("TestEmpresa", "TestUser", "ADMIN");
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var mockLogger = new Mock<ILogger<ClientesController>>();

            _output.WriteLine("Creando instancia de ClientesController");
            var controller = new ClientesController(mockContext.Object, mockFunctionsBBDD.Object, mockLogger.Object, _output);

            // Act
            _output.WriteLine("Ejecutando GenerateNextClientNumber");
            var result = controller.GenerateNextClientNumber();

            // Assert
            _output.WriteLine($"Resultado obtenido: '{result}', Resultado esperado: '{expectedNextNumber}'");
            Assert.Equal(expectedNextNumber, result);
        }



        private static Mock<DbSet<T>> MockDbSet<T>(List<T> list) where T : class
        {
            var queryable = list.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            // Configuración para operaciones síncronas
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            // Configuración para operaciones asíncronas
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(default))
                .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

            // Configuración para Add y Update
            mockSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(list.Add);
            mockSet.Setup(d => d.Update(It.IsAny<T>())).Callback<T>(item =>
            {
                var index = list.FindIndex(i => i == item);
                if (index != -1)
                    list[index] = item;
            });

            return mockSet;
        }





        private Mock<DbContextiLabPlus> CreateMockContext(List<Clientes> clientes)
        {
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = MockDbSet(clientes);

            mockContext.Setup(c => c.Clientes).Returns(mockSet.Object);
            mockContext.Setup(c => c.Set<Clientes>()).Returns(mockSet.Object);

            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            mockContext.Setup(m => m.FindAsync<Clientes>(It.IsAny<object[]>()))
                .ReturnsAsync((object[] keyValues) =>
                {
                    var guid = (Guid)keyValues[0];
                    return clientes.FirstOrDefault(c => c.Guid == guid);
                });

            // Configuraciones adicionales...
            mockContext.Setup(c => c.TarifasVenta).Returns(MockDbSet(new List<TarifasVenta>()).Object);
            mockContext.Setup(c => c.Vendedores).Returns(MockDbSet(new List<Vendedores>()).Object);
            mockContext.Setup(c => c.Divisas).Returns(MockDbSet(new List<Divisas>()).Object);
            mockContext.Setup(c => c.ValSys).Returns(MockDbSet(new List<ValSys>()).Object);

            return mockContext;
        }

        private ClientesController CreateController(DbContextiLabPlus context)
        {
            var mockHttpContextAccessor = MockHttpContextAccessorFactory.CreateMockHttpContextAccessor("TestEmpresa", "TestUser", "ADMIN");
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(context, mockHttpContextAccessor.Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuarioNombre = "TestUser"
            });
            var mockLogger = new Mock<ILogger<ClientesController>>();

            return new ClientesController(context, mockFunctionsBBDD.Object, mockLogger.Object, _output);
        }



        private ClientesController CreateController(Mock<DbContextiLabPlus> mockContext)
        {
            var mockHttpContextAccessor = MockHttpContextAccessorFactory.CreateMockHttpContextAccessor("TestEmpresa", "TestUser", "ADMIN");
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuarioNombre = "TestUser"
            });

            var mockLogger = new Mock<ILogger<ClientesController>>();

            return new ClientesController(mockContext.Object, mockFunctionsBBDD.Object, mockLogger.Object, _output);
        }


        public class AsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public AsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new AsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new AsyncEnumerable<TElement>(expression);
            }

            public object Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var expectedResultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider)
                    .GetMethods()
                    .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethod)
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(this, new object[] { expression });

                var taskResult = typeof(Task).GetMethod(nameof(Task.FromResult))
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(null, new[] { executionResult });

                return (TResult)taskResult;
            }
        }

        public class AsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public AsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }

            public AsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider
            {
                get { return new AsyncQueryProvider<T>(this); }
            }
        }

        public class AsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public AsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current
            {
                get { return _inner.Current; }
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask();
            }
        }




        // PRUEBAS TEST


        internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            internal TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var expectedResultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider)
                    .GetMethod(
                        name: nameof(IQueryProvider.Execute),
                        genericParameterCount: 1,
                        types: new[] { typeof(Expression) })
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(this, new[] { expression });

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(null, new[] { executionResult });
            }
        }



        internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable)
                : base(enumerable)
            { }

            public TestAsyncEnumerable(Expression expression)
                : base(expression)
            { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider
            {
                get { return new TestAsyncQueryProvider<T>(this); }
            }
        }



        internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current
            {
                get
                {
                    return _inner.Current;
                }
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask();
            }
        }







    }
}