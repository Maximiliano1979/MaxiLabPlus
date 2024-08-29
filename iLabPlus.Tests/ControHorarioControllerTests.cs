using iLabPlus.Controllers;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace iLabPlus.Tests
{


    public class ControHorarioControllerTests
    {
        private readonly ITestOutputHelper _output;

        public ControHorarioControllerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GetEmpleados_DebeRetornarListaDeEmpleados()
        {
            _output.WriteLine("Iniciando test GetEmpleados_DebeRetornarListaDeEmpleados");

            try
            {
                // Arrange
                var mockContext = new Mock<DbContextiLabPlus>();
                _output.WriteLine("Mock de DbContextiLabPlus creado");

                var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                _output.WriteLine("Mock de IHttpContextAccessor creado");

                var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
                _output.WriteLine("Mock de FunctionsBBDD creado");

                mockFunctionsBBDD.Setup(f => f.GetClaims())
                    .Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });
                _output.WriteLine("GetClaims configurado para FunctionsBBDD");

                var empleadosData = new List<Empleados>
            {
                new Empleados { Empleado = "1", EmpNombre = "Juan Pérez", Empresa = "TestEmpresa" },
                new Empleados { Empleado = "2", EmpNombre = "Ana García", Empresa = "TestEmpresa" },
                new Empleados { Empleado = "3", EmpNombre = "Carlos López", Empresa = "OtraEmpresa" }
            }.AsQueryable();
                _output.WriteLine("Datos de prueba de empleados creados");

                var mockSet = new Mock<DbSet<Empleados>>();
                mockSet.As<IQueryable<Empleados>>().Setup(m => m.Provider).Returns(empleadosData.Provider);
                mockSet.As<IQueryable<Empleados>>().Setup(m => m.Expression).Returns(empleadosData.Expression);
                mockSet.As<IQueryable<Empleados>>().Setup(m => m.ElementType).Returns(empleadosData.ElementType);
                mockSet.As<IQueryable<Empleados>>().Setup(m => m.GetEnumerator()).Returns(empleadosData.GetEnumerator());
                _output.WriteLine("Mock de DbSet<Empleados> configurado");

                mockContext.Setup(c => c.Empleados).Returns(mockSet.Object);
                _output.WriteLine("DbSet<Empleados> configurado en el contexto mock");

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando GetEmpleados");
                var result = controller.GetEmpleados();

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var jsonResult = Assert.IsType<JsonResult>(result);
                _output.WriteLine("Resultado verificado como JsonResult");

                var empleados = Assert.IsAssignableFrom<IEnumerable<dynamic>>(jsonResult.Value);
                _output.WriteLine($"Número de empleados obtenidos: {empleados.Count()}");

                Assert.Equal(2, empleados.Count());
                Assert.Contains(empleados, e => e.Empleado == "1" && e.EmpNombre == "Juan Pérez");
                Assert.Contains(empleados, e => e.Empleado == "2" && e.EmpNombre == "Ana García");
                Assert.DoesNotContain(empleados, e => e.Empleado == "3" && e.EmpNombre == "Carlos López");
                _output.WriteLine("Todas las aserciones pasadas con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }


        [Fact]
        public void Index_DebeEstablecerViewBagYDevolverVistaCorrecta()
        {
            _output.WriteLine("Iniciando test Index_DebeEstablecerViewBagYDevolverVistaCorrecta");

            try
            {
                // Arrange
                var mockContext = new Mock<DbContextiLabPlus>();
                var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
                _output.WriteLine("Mocks creados");

                var menuAccesos = new List<MenuUser>
        {
            new MenuUser(
                Menu: "TestMenu",
                Menu_Tooltip: "Test Tooltip",
                Accesos: new List<MenuUserAccesos>(),
                Menu_Icono: "test-icon"
            )
        };
                mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(menuAccesos);
                _output.WriteLine("GetMenuAccesos configurado");

                mockFunctionsBBDD.Setup(f => f.GetColumnsLayout(It.IsAny<string>()))
                    .Returns(new GrupoColumnsLayout { ColumnsLayoutUser = "TestLayout", ColumnsPinnedUser = 3 });
                _output.WriteLine("GetColumnsLayout configurado");

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando método Index");
                var result = controller.Index();

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal("ControlHorario", viewResult.ViewName);
                _output.WriteLine("Vista verificada");

                Assert.Equal(menuAccesos, controller.ViewBag.MenuUserList);
                Assert.Equal("TestLayout", controller.ViewBag.ColumnsLayoutUser);
                Assert.Equal(3, controller.ViewBag.ColumnsPinnedUser);
                _output.WriteLine("ViewBag verificado");

                // Verificación adicional para MenuUser
                var menuUserList = Assert.IsType<List<MenuUser>>(controller.ViewBag.MenuUserList);
                Assert.Single(menuUserList);
                Assert.Equal("TestMenu", menuUserList[0].Menu);
                Assert.Equal("Test Tooltip", menuUserList[0].Menu_Tooltip);
                Assert.Equal("test-icon", menuUserList[0].Menu_Icono);
                Assert.Empty(menuUserList[0].Accesos);
                _output.WriteLine("MenuUser verificado");

                _output.WriteLine("Test completado con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }



        [Fact]
        public void ControlHorarioAdmin_DebeFiltrarPorEmpresaYOrdenarCorrectamente()
        {
            _output.WriteLine("Iniciando test ControlHorarioAdmin_DebeFiltrarPorEmpresaYOrdenarCorrectamente");

            try
            {
                // Arrange
                var mockContext = new Mock<DbContextiLabPlus>();
                var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
                _output.WriteLine("Mocks creados");

                mockFunctionsBBDD.Setup(f => f.GetClaims())
                    .Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });
                _output.WriteLine("GetClaims configurado");

                // Mock para GetMenuAccesos
                mockFunctionsBBDD.Setup(f => f.GetMenuAccesos())
                    .Returns(new List<MenuUser>());
                _output.WriteLine("GetMenuAccesos configurado");

                // Mock para GetColumnsLayout
                mockFunctionsBBDD.Setup(f => f.GetColumnsLayout(It.IsAny<string>()))
                    .Returns(new GrupoColumnsLayout { ColumnsLayoutUser = "TestLayout", ColumnsPinnedUser = 3 });
                _output.WriteLine("GetColumnsLayout configurado");

                var controlHorarioData = new List<ControlHorario>
                {
                    new ControlHorario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Empleado = "1", Fecha = DateTime.Today, HoraEntrada = new TimeSpan(9, 0, 0) },
                    new ControlHorario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Empleado = "2", Fecha = DateTime.Today.AddDays(-1), HoraEntrada = new TimeSpan(8, 0, 0) },
                    new ControlHorario { Guid = Guid.NewGuid(), Empresa = "OtraEmpresa", Empleado = "3", Fecha = DateTime.Today, HoraEntrada = new TimeSpan(10, 0, 0) }
                }.AsQueryable();

                var mockSetControlHorario = new Mock<DbSet<ControlHorario>>();
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Provider).Returns(controlHorarioData.Provider);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Expression).Returns(controlHorarioData.Expression);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.ElementType).Returns(controlHorarioData.ElementType);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.GetEnumerator()).Returns(controlHorarioData.GetEnumerator());

                mockContext.Setup(c => c.ControlHorario).Returns(mockSetControlHorario.Object);
                _output.WriteLine("Mock de ControlHorario configurado");

                var empleadosData = new List<Empleados>
                {
                    new Empleados { Empleado = "1", EmpNombre = "Juan Pérez", Empresa = "TestEmpresa" },
                    new Empleados { Empleado = "2", EmpNombre = "Ana García", Empresa = "TestEmpresa" }
                }.AsQueryable();

                var mockSetEmpleados = new Mock<DbSet<Empleados>>();
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Provider).Returns(empleadosData.Provider);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Expression).Returns(empleadosData.Expression);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.ElementType).Returns(empleadosData.ElementType);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.GetEnumerator()).Returns(empleadosData.GetEnumerator());

                mockContext.Setup(c => c.Empleados).Returns(mockSetEmpleados.Object);
                _output.WriteLine("Mock de Empleados configurado");

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando método ControlHorarioAdmin");
                var result = controller.ControlHorarioAdmin();

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal("ControlHorarioAdmin", viewResult.ViewName);
                _output.WriteLine("Vista verificada");

                var model = Assert.IsAssignableFrom<List<ControlHorario>>(viewResult.Model);
                Assert.Equal(2, model.Count);
                Assert.All(model, item => Assert.Equal("TestEmpresa", item.Empresa));
                Assert.True(model[0].Fecha >= model[1].Fecha);
                _output.WriteLine("Filtrado y ordenamiento verificados");

                _output.WriteLine("Test completado con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }




        [Fact]
        public void ControlHorarioAdmin_DebeRellenarNombreEmpleadoCorrectamente()
        {
            _output.WriteLine("Iniciando test ControlHorarioAdmin_DebeRellenarNombreEmpleadoCorrectamente");

            try
            {
                // Arrange
                var mockContext = new Mock<DbContextiLabPlus>();
                var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
                _output.WriteLine("Mocks creados");

                mockFunctionsBBDD.Setup(f => f.GetClaims())
                    .Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });
                _output.WriteLine("GetClaims configurado");

                // Mock para GetMenuAccesos
                mockFunctionsBBDD.Setup(f => f.GetMenuAccesos())
                    .Returns(new List<MenuUser>());
                _output.WriteLine("GetMenuAccesos configurado");

                // Mock para GetColumnsLayout
                mockFunctionsBBDD.Setup(f => f.GetColumnsLayout(It.IsAny<string>()))
                    .Returns(new GrupoColumnsLayout { ColumnsLayoutUser = "TestLayout", ColumnsPinnedUser = 3 });
                _output.WriteLine("GetColumnsLayout configurado");

                var controlHorarioData = new List<ControlHorario>
                {
                    new ControlHorario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Empleado = "1", Fecha = DateTime.Today },
                    new ControlHorario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Empleado = "2", Fecha = DateTime.Today }
                }.AsQueryable();

                var mockSetControlHorario = new Mock<DbSet<ControlHorario>>();
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Provider).Returns(controlHorarioData.Provider);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Expression).Returns(controlHorarioData.Expression);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.ElementType).Returns(controlHorarioData.ElementType);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.GetEnumerator()).Returns(controlHorarioData.GetEnumerator());

                mockContext.Setup(c => c.ControlHorario).Returns(mockSetControlHorario.Object);
                _output.WriteLine("Mock de ControlHorario configurado");

                var empleadosData = new List<Empleados>
                {
                    new Empleados { Empleado = "1", EmpNombre = "Juan Pérez", Empresa = "TestEmpresa" },
                    new Empleados { Empleado = "2", EmpNombre = "Ana García", Empresa = "TestEmpresa" }
                }.AsQueryable();

                var mockSetEmpleados = new Mock<DbSet<Empleados>>();
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Provider).Returns(empleadosData.Provider);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Expression).Returns(empleadosData.Expression);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.ElementType).Returns(empleadosData.ElementType);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.GetEnumerator()).Returns(empleadosData.GetEnumerator());

                mockContext.Setup(c => c.Empleados).Returns(mockSetEmpleados.Object);
                _output.WriteLine("Mock de Empleados configurado");

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando método ControlHorarioAdmin");
                var result = controller.ControlHorarioAdmin();

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<List<ControlHorario>>(viewResult.Model);

                Assert.Equal("Juan Pérez", model.First(r => r.Empleado == "1").EmpleadoNombre);
                Assert.Equal("Ana García", model.First(r => r.Empleado == "2").EmpleadoNombre);
                _output.WriteLine("Nombres de empleados verificados");

                _output.WriteLine("Test completado con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }


        [Fact]
        public void DialogControlHorarioAdmin_ConRegistroExistente_DebeRetornarPartialViewConModeloCorrecto()
        {
            _output.WriteLine("Iniciando test DialogControlHorarioAdmin_ConRegistroExistente_DebeRetornarPartialViewConModeloCorrecto");

            try
            {
                // Arrange
                var guid = Guid.NewGuid();
                var mockContext = new Mock<DbContextiLabPlus>();
                var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                _output.WriteLine("Mocks creados");

                // Configurar HttpContextAccessor
                var mockHttpContext = new Mock<HttpContext>();
                var mockIdentity = new Mock<System.Security.Claims.ClaimsIdentity>();
                mockIdentity.Setup(i => i.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("", ""));
                var mockPrincipal = new Mock<System.Security.Claims.ClaimsPrincipal>();
                mockPrincipal.Setup(p => p.Identity).Returns(mockIdentity.Object);
                mockHttpContext.Setup(c => c.User).Returns(mockPrincipal.Object);
                mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
                _output.WriteLine("HttpContextAccessor configurado");

                var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
                _output.WriteLine("FunctionsBBDD mock creado");

                var controlHorarioData = new List<ControlHorario>
                {
                    new ControlHorario
                    {
                        Guid = guid,
                        Empresa = "TestEmpresa",
                        Empleado = "Emp1",
                        Fecha = DateTime.Today,
                        HoraEntrada = new TimeSpan(9, 0, 0),
                        HoraSalida = new TimeSpan(17, 0, 0),
                        HorasTrabajadas = new TimeSpan(8, 0, 0),
                        Cierre = "N",
                        Observaciones = "Test"
                    }
                }.AsQueryable();

                var mockSetControlHorario = new Mock<DbSet<ControlHorario>>();
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Provider).Returns(controlHorarioData.Provider);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Expression).Returns(controlHorarioData.Expression);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.ElementType).Returns(controlHorarioData.ElementType);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.GetEnumerator()).Returns(controlHorarioData.GetEnumerator());

                mockContext.Setup(c => c.ControlHorario).Returns(mockSetControlHorario.Object);
                _output.WriteLine("Mock de ControlHorario configurado");

                var empleadosData = new List<Empleados>
                {
                    new Empleados { Empleado = "Emp1", EmpNombre = "Juan Pérez", Empresa = "TestEmpresa" }
                }.AsQueryable();

                var mockSetEmpleados = new Mock<DbSet<Empleados>>();
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Provider).Returns(empleadosData.Provider);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Expression).Returns(empleadosData.Expression);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.ElementType).Returns(empleadosData.ElementType);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.GetEnumerator()).Returns(empleadosData.GetEnumerator());

                mockContext.Setup(c => c.Empleados).Returns(mockSetEmpleados.Object);
                _output.WriteLine("Mock de Empleados configurado");

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando DialogControlHorarioAdmin");
                var result = controller.DialogControlHorarioAdmin(guid);

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var partialViewResult = Assert.IsType<PartialViewResult>(result);
                Assert.Equal("_DialogControlHorarioAdmin", partialViewResult.ViewName);
                _output.WriteLine("PartialViewResult verificado");

                var model = Assert.IsType<ControlHorario>(partialViewResult.Model);
                Assert.Equal(guid, model.Guid);
                Assert.Equal("TestEmpresa", model.Empresa);
                Assert.Equal("Emp1", model.Empleado);
                Assert.Equal("Juan Pérez", model.EmpleadoNombre);
                _output.WriteLine("Modelo verificado");

                _output.WriteLine("Test completado con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public void DialogControlHorarioAdmin_ConRegistroInexistente_DebeRetornarPartialViewConModeloVacio()
        {
            _output.WriteLine("Iniciando test DialogControlHorarioAdmin_ConRegistroInexistente_DebeRetornarPartialViewConModeloVacio");

            try
            {
                // Arrange
                var guid = Guid.NewGuid();
                var mockContext = new Mock<DbContextiLabPlus>();
                var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                _output.WriteLine("Mocks creados");

                // Configurar HttpContextAccessor
                var mockHttpContext = new Mock<HttpContext>();
                var mockIdentity = new Mock<System.Security.Claims.ClaimsIdentity>();
                mockIdentity.Setup(i => i.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("", ""));
                var mockPrincipal = new Mock<System.Security.Claims.ClaimsPrincipal>();
                mockPrincipal.Setup(p => p.Identity).Returns(mockIdentity.Object);
                mockHttpContext.Setup(c => c.User).Returns(mockPrincipal.Object);
                mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
                _output.WriteLine("HttpContextAccessor configurado");

                var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
                _output.WriteLine("FunctionsBBDD mock creado");

                var controlHorarioData = new List<ControlHorario>().AsQueryable();

                var mockSetControlHorario = new Mock<DbSet<ControlHorario>>();
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Provider).Returns(controlHorarioData.Provider);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Expression).Returns(controlHorarioData.Expression);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.ElementType).Returns(controlHorarioData.ElementType);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.GetEnumerator()).Returns(controlHorarioData.GetEnumerator());

                mockContext.Setup(c => c.ControlHorario).Returns(mockSetControlHorario.Object);
                _output.WriteLine("Mock de ControlHorario configurado");

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando DialogControlHorarioAdmin");
                var result = controller.DialogControlHorarioAdmin(guid);

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var partialViewResult = Assert.IsType<PartialViewResult>(result);
                Assert.Equal("_DialogControlHorarioAdmin", partialViewResult.ViewName);
                _output.WriteLine("PartialViewResult verificado");

                var model = Assert.IsType<ControlHorario>(partialViewResult.Model);
                Assert.Equal(Guid.Empty, model.Guid);
                _output.WriteLine("Modelo verificado");

                _output.WriteLine("Test completado con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }



        [Fact]
        public async Task RegistroControlHorario_MensajeQRInvalido_DebeRetornarError()
        {
            _output.WriteLine("Iniciando test RegistroControlHorario_MensajeQRInvalido_DebeRetornarError");

            try
            {
                // Arrange
                var (mockContext, mockFunctionsBBDD) = SetupMocks();
                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando RegistroControlHorario con mensaje QR inválido");
                var result = await controller.RegistroControlHorario("InvalidQR");

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var jsonResult = Assert.IsType<JsonResult>(result);
                _output.WriteLine("Verificando tipo de resultado");

                dynamic value = jsonResult.Value;
                _output.WriteLine("Obteniendo valor dinámico del resultado");

                Assert.False((bool)value.success);
                _output.WriteLine("Verificado: success es false");

                Assert.Equal("No es un empleado registrado por la empresa", (string)value.message);
                _output.WriteLine("Verificado: mensaje de error correcto");

                _output.WriteLine("Aserciones completadas con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }


        [Fact]
        public async Task RegistroControlHorario_CierreAutomaticoDeRegistrosAnteriores()
        {
            _output.WriteLine("Iniciando test RegistroControlHorario_CierreAutomaticoDeRegistrosAnteriores");

            try
            {
                // Arrange
                var (mockContext, mockFunctionsBBDD) = SetupMocks();

                var empleadosData = new List<Empleados>
                {
                    new Empleados { Empleado = "Emp1", EmpNombre = "Juan Pérez", Empresa = "TestEmpresa" }
                }.AsQueryable();
                var mockSetEmpleados = new Mock<DbSet<Empleados>>();
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Provider).Returns(empleadosData.Provider);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Expression).Returns(empleadosData.Expression);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.ElementType).Returns(empleadosData.ElementType);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.GetEnumerator()).Returns(empleadosData.GetEnumerator());
                mockContext.Setup(c => c.Empleados).Returns(mockSetEmpleados.Object);

                var fechaHoy = DateTime.Today;
                var controlHorarioData = new List<ControlHorario>
                {
                    new ControlHorario
                    {
                        Empleado = "Emp1",
                        Empresa = "TestEmpresa",
                        Fecha = fechaHoy.AddDays(-2),
                        HoraEntrada = new TimeSpan(9, 0, 0),
                        HoraSalida = null
                    },
                    new ControlHorario
                    {
                        Empleado = "Emp1",
                        Empresa = "TestEmpresa",
                        Fecha = fechaHoy.AddDays(-1),
                        HoraEntrada = new TimeSpan(9, 0, 0),
                        HoraSalida = null
                    }
                }.AsQueryable();

                var mockSetControlHorario = new Mock<DbSet<ControlHorario>>();
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Provider).Returns(controlHorarioData.Provider);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Expression).Returns(controlHorarioData.Expression);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.ElementType).Returns(controlHorarioData.ElementType);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.GetEnumerator()).Returns(controlHorarioData.GetEnumerator());
                mockContext.Setup(c => c.ControlHorario).Returns(mockSetControlHorario.Object);

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando RegistroControlHorario para nueva entrada");
                var result = await controller.RegistroControlHorario("25%Emp1");

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var jsonResult = Assert.IsType<JsonResult>(result);
                dynamic value = jsonResult.Value;

                Assert.True((bool)value.success);
                Assert.Contains("Hora de Entrada", (string)value.message);

                // Verificar que los registros anteriores se cerraron
                mockContext.Verify(m => m.ControlHorario.Update(It.Is<ControlHorario>(ch =>
                    ch.Empleado == "Emp1" &&
                    ch.Empresa == "TestEmpresa" &&
                    ch.Fecha < fechaHoy &&
                    ch.HoraSalida == new TimeSpan(20, 0, 0) &&
                    ch.Cierre == "AUTO"
                )), Times.Exactly(2));

                mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once());
                _output.WriteLine("Verificado: registros anteriores cerrados y SaveChangesAsync llamado");

                _output.WriteLine("Aserciones completadas con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }
 

        [Fact]
        public async Task RegistroControlHorario_RegistroEntrada_DebeCrearNuevoRegistro()
        {
            _output.WriteLine("Iniciando test RegistroControlHorario_RegistroEntrada_DebeCrearNuevoRegistro");

            try
            {
                // Arrange
                var (mockContext, mockFunctionsBBDD) = SetupMocks();

                var empleadosData = new List<Empleados>
                {
                    new Empleados { Empleado = "Emp1", EmpNombre = "Juan Pérez", Empresa = "TestEmpresa" }
                }.AsQueryable();
                var mockSetEmpleados = new Mock<DbSet<Empleados>>();
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Provider).Returns(empleadosData.Provider);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Expression).Returns(empleadosData.Expression);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.ElementType).Returns(empleadosData.ElementType);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.GetEnumerator()).Returns(empleadosData.GetEnumerator());
                mockContext.Setup(c => c.Empleados).Returns(mockSetEmpleados.Object);

                var controlHorarioData = new List<ControlHorario>().AsQueryable();
                var mockSetControlHorario = new Mock<DbSet<ControlHorario>>();
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Provider).Returns(controlHorarioData.Provider);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Expression).Returns(controlHorarioData.Expression);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.ElementType).Returns(controlHorarioData.ElementType);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.GetEnumerator()).Returns(controlHorarioData.GetEnumerator());
                mockContext.Setup(c => c.ControlHorario).Returns(mockSetControlHorario.Object);

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando RegistroControlHorario para registro de entrada");
                var result = await controller.RegistroControlHorario("25%Emp1");

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var jsonResult = Assert.IsType<JsonResult>(result);
                _output.WriteLine("Verificando tipo de resultado");

                dynamic value = jsonResult.Value;
                _output.WriteLine("Obteniendo valor dinámico del resultado");

                Assert.True((bool)value.success);
                _output.WriteLine("Verificado: success es true");

                Assert.Contains("Hora de Entrada", (string)value.message);
                _output.WriteLine("Verificado: mensaje contiene 'Hora de Entrada'");

                Assert.NotNull((string)value.hora);
                _output.WriteLine("Verificado: hora no es null");

                mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once());
                _output.WriteLine("Verificado: SaveChangesAsync fue llamado una vez");

                _output.WriteLine("Aserciones completadas con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }



        [Fact]
        public async Task RegistroControlHorario_RegistroSalida_DebeActualizarRegistroExistente()
        {
            _output.WriteLine("Iniciando test RegistroControlHorario_RegistroSalida_DebeActualizarRegistroExistente");

            try
            {
                // Arrange
                var (mockContext, mockFunctionsBBDD) = SetupMocks();

                var empleadosData = new List<Empleados>
                {
                    new Empleados { Empleado = "Emp1", EmpNombre = "Juan Pérez", Empresa = "TestEmpresa" }
                }.AsQueryable();
                var mockSetEmpleados = new Mock<DbSet<Empleados>>();
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Provider).Returns(empleadosData.Provider);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Expression).Returns(empleadosData.Expression);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.ElementType).Returns(empleadosData.ElementType);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.GetEnumerator()).Returns(empleadosData.GetEnumerator());
                mockContext.Setup(c => c.Empleados).Returns(mockSetEmpleados.Object);

                var controlHorarioData = new List<ControlHorario>
                {
                    new ControlHorario
                    {
                        Empleado = "Emp1",
                        Empresa = "TestEmpresa",
                        Fecha = DateTime.Today,
                        HoraEntrada = new TimeSpan(9, 0, 0),
                        HoraSalida = null
                    }
                }.AsQueryable();
                var mockSetControlHorario = new Mock<DbSet<ControlHorario>>();
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Provider).Returns(controlHorarioData.Provider);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.Expression).Returns(controlHorarioData.Expression);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.ElementType).Returns(controlHorarioData.ElementType);
                mockSetControlHorario.As<IQueryable<ControlHorario>>().Setup(m => m.GetEnumerator()).Returns(controlHorarioData.GetEnumerator());
                mockContext.Setup(c => c.ControlHorario).Returns(mockSetControlHorario.Object);

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando RegistroControlHorario para registro de salida");
                var result = await controller.RegistroControlHorario("25%Emp1");

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var jsonResult = Assert.IsType<JsonResult>(result);
                _output.WriteLine("Verificando tipo de resultado");

                dynamic value = jsonResult.Value;
                _output.WriteLine("Obteniendo valor dinámico del resultado");

                Assert.True((bool)value.success);
                _output.WriteLine("Verificado: success es true");

                Assert.Contains("Hora de Salida", (string)value.message);
                _output.WriteLine("Verificado: mensaje contiene 'Hora de Salida'");

                Assert.NotNull((string)value.hora);
                _output.WriteLine("Verificado: hora no es null");

                mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once());
                _output.WriteLine("Verificado: SaveChangesAsync fue llamado una vez");

                _output.WriteLine("Aserciones completadas con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }


        [Fact]
        public async Task UpdateRegistroHorario_RegistroNuloOGuidVacio_DebeRetornarError()
        {
            _output.WriteLine("Iniciando test UpdateRegistroHorario_RegistroNuloOGuidVacio_DebeRetornarError");

            try
            {
                // Arrange
                var (mockContext, mockFunctionsBBDD) = SetupMocks();
                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act - Caso 1: Registro nulo
                _output.WriteLine("Ejecutando UpdateRegistroHorario con registro nulo");
                var resultNull = await controller.UpdateRegistroHorario(null);

                // Assert - Caso 1
                _output.WriteLine("Iniciando aserciones para caso de registro nulo");
                var jsonResultNull = Assert.IsType<JsonResult>(resultNull);
                dynamic valueNull = jsonResultNull.Value;
                Assert.False((bool)valueNull.success);
                Assert.Equal("Registro no válido", (string)valueNull.message);

                // Act - Caso 2: Guid vacío
                _output.WriteLine("Ejecutando UpdateRegistroHorario con Guid vacío");
                var resultEmptyGuid = await controller.UpdateRegistroHorario(new ControlHorario { Guid = Guid.Empty });

                // Assert - Caso 2
                _output.WriteLine("Iniciando aserciones para caso de Guid vacío");
                var jsonResultEmptyGuid = Assert.IsType<JsonResult>(resultEmptyGuid);
                dynamic valueEmptyGuid = jsonResultEmptyGuid.Value;
                Assert.False((bool)valueEmptyGuid.success);
                Assert.Equal("Registro no válido", (string)valueEmptyGuid.message);

                _output.WriteLine("Aserciones completadas con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task UpdateRegistroHorario_RegistroNoEncontrado_DebeRetornarError()
        {
            _output.WriteLine("Iniciando test UpdateRegistroHorario_RegistroNoEncontrado_DebeRetornarError");

            try
            {
                // Arrange
                var (mockContext, mockFunctionsBBDD) = SetupMocks();
                var controlHorarioData = new List<ControlHorario>().AsQueryable();
                var mockSet = new Mock<DbSet<ControlHorario>>();
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.Provider).Returns(controlHorarioData.Provider);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.Expression).Returns(controlHorarioData.Expression);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.ElementType).Returns(controlHorarioData.ElementType);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.GetEnumerator()).Returns(controlHorarioData.GetEnumerator());
                mockContext.Setup(c => c.ControlHorario).Returns(mockSet.Object);

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando UpdateRegistroHorario con Guid no existente");
                var result = await controller.UpdateRegistroHorario(new ControlHorario { Guid = Guid.NewGuid() });

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var jsonResult = Assert.IsType<JsonResult>(result);
                dynamic value = jsonResult.Value;
                Assert.False((bool)value.success);
                Assert.Equal("Registro no encontrado", (string)value.message);

                _output.WriteLine("Aserciones completadas con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task UpdateRegistroHorario_ActualizacionExitosa_DebeRetornarRegistroActualizado()
        {
            _output.WriteLine("Iniciando test UpdateRegistroHorario_ActualizacionExitosa_DebeRetornarRegistroActualizado");

            try
            {
                // Arrange
                var (mockContext, mockFunctionsBBDD) = SetupMocks();
                var guid = Guid.NewGuid();
                var registroExistente = new ControlHorario
                {
                    Guid = guid,
                    Empleado = "Emp1",
                    Fecha = DateTime.Today,
                    HoraEntrada = new TimeSpan(9, 0, 0),
                    HoraSalida = new TimeSpan(17, 0, 0),
                    Empresa = "TestEmpresa",
                    Cierre = "N",
                    Observaciones = "Observación inicial"
                };

                var controlHorarioData = new List<ControlHorario> { registroExistente }.AsQueryable();
                var mockSet = new Mock<DbSet<ControlHorario>>();
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.Provider).Returns(controlHorarioData.Provider);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.Expression).Returns(controlHorarioData.Expression);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.ElementType).Returns(controlHorarioData.ElementType);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.GetEnumerator()).Returns(controlHorarioData.GetEnumerator());
                mockContext.Setup(c => c.ControlHorario).Returns(mockSet.Object);

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                var registroActualizado = new ControlHorario
                {
                    Guid = guid,
                    Empleado = "Emp1",
                    Fecha = DateTime.Today,
                    HoraEntrada = new TimeSpan(8, 0, 0),
                    HoraSalida = new TimeSpan(18, 0, 0),
                    Empresa = "TestEmpresa",
                    Cierre = "S",
                    Observaciones = "Observación actualizada"
                };

                // Act
                _output.WriteLine("Ejecutando UpdateRegistroHorario con registro actualizado");
                var result = await controller.UpdateRegistroHorario(registroActualizado);

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var jsonResult = Assert.IsType<JsonResult>(result);
                dynamic value = jsonResult.Value;
                Assert.True((bool)value.success);
                Assert.Equal("Registro actualizado correctamente", (string)value.message);

                var data = value.data;
                Assert.Equal("Emp1", data.Empleado);
                Assert.Equal(DateTime.Today.ToString("dd/MM/yyyy"), data.Fecha);
                Assert.Equal(new TimeSpan(8, 0, 0), data.HoraEntrada);
                Assert.Equal(new TimeSpan(18, 0, 0), data.HoraSalida);
                Assert.Equal("TestEmpresa", data.Empresa);
                Assert.Equal(new TimeSpan(10, 0, 0), data.HorasTrabajadas);
                Assert.Equal("S", data.Cierre);
                Assert.Equal("Observación actualizada", data.Observaciones);

                mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once());
                _output.WriteLine("Aserciones completadas con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }


        [Fact]
        public async Task RegistroControlHorario_EmpleadoNoExiste_DebeRetornarError()
        {
            _output.WriteLine("Iniciando test RegistroControlHorario_EmpleadoNoExiste_DebeRetornarError");

            try
            {
                // Arrange
                var (mockContext, mockFunctionsBBDD) = SetupMocks();

                // Configurar una base de datos vacía de empleados
                var empleadosData = new List<Empleados>().AsQueryable();
                var mockSetEmpleados = new Mock<DbSet<Empleados>>();
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Provider).Returns(empleadosData.Provider);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.Expression).Returns(empleadosData.Expression);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.ElementType).Returns(empleadosData.ElementType);
                mockSetEmpleados.As<IQueryable<Empleados>>().Setup(m => m.GetEnumerator()).Returns(empleadosData.GetEnumerator());
                mockContext.Setup(c => c.Empleados).Returns(mockSetEmpleados.Object);

                mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando RegistroControlHorario con un empleado que no existe");
                var result = await controller.RegistroControlHorario("25%EmpleadoInexistente");

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var jsonResult = Assert.IsType<JsonResult>(result);
                dynamic value = jsonResult.Value;

                Assert.False((bool)value.success);
                Assert.Equal("El empleado no existe", (string)value.message);

                // Verificar que no se hicieron cambios en la base de datos
                mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Never());

                _output.WriteLine("Aserciones completadas con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }



        [Fact]
        public async Task DeleteRegistroHorario_EliminacionExitosa_DebeRetornarOk()
        {
            _output.WriteLine("Iniciando test DeleteRegistroHorario_EliminacionExitosa_DebeRetornarOk");

            try
            {
                // Arrange
                var guid = Guid.NewGuid();
                var (mockContext, mockFunctionsBBDD) = SetupMocks();
                var registroExistente = new ControlHorario
                {
                    Guid = guid,
                    Empleado = "Emp1",
                    Fecha = DateTime.Today,
                    HoraEntrada = new TimeSpan(9, 0, 0),
                    HoraSalida = new TimeSpan(17, 0, 0),
                    Empresa = "TestEmpresa"
                };

                var controlHorarioData = new List<ControlHorario> { registroExistente }.AsQueryable();
                var mockSet = new Mock<DbSet<ControlHorario>>();
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.Provider).Returns(controlHorarioData.Provider);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.Expression).Returns(controlHorarioData.Expression);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.ElementType).Returns(controlHorarioData.ElementType);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.GetEnumerator()).Returns(controlHorarioData.GetEnumerator());
                mockContext.Setup(c => c.ControlHorario).Returns(mockSet.Object);

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando DeleteRegistroHorario");
                var result = await controller.DeleteRegistroHorario(guid);

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var objectResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal(200, objectResult.StatusCode);
                Assert.Equal("Registro eliminado correctamente", objectResult.Value);

                mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once());
                mockSet.Verify(m => m.Remove(It.Is<ControlHorario>(ch => ch.Guid == guid)), Times.Once());

                _output.WriteLine("Aserciones completadas con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }



        [Fact]
        public async Task DeleteRegistroHorario_RegistroNoEncontrado_DebeRetornarOk()
        {
            _output.WriteLine("Iniciando test DeleteRegistroHorario_RegistroNoEncontrado_DebeRetornarOk");

            try
            {
                // Arrange
                var (mockContext, mockFunctionsBBDD) = SetupMocks();
                var controlHorarioData = new List<ControlHorario>().AsQueryable();
                var mockSet = new Mock<DbSet<ControlHorario>>();
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.Provider).Returns(controlHorarioData.Provider);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.Expression).Returns(controlHorarioData.Expression);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.ElementType).Returns(controlHorarioData.ElementType);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.GetEnumerator()).Returns(controlHorarioData.GetEnumerator());
                mockContext.Setup(c => c.ControlHorario).Returns(mockSet.Object);

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando DeleteRegistroHorario con Guid no existente");
                var result = await controller.DeleteRegistroHorario(Guid.NewGuid());

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var objectResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal(200, objectResult.StatusCode);
                Assert.Equal("Registro no encontrado", objectResult.Value);

                mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Never());
                mockSet.Verify(m => m.Remove(It.IsAny<ControlHorario>()), Times.Never());

                _output.WriteLine("Aserciones completadas con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task DeleteRegistroHorario_ErrorDuranteEliminacion_DebeRetornarBadRequest()
        {
            _output.WriteLine("Iniciando test DeleteRegistroHorario_ErrorDuranteEliminacion_DebeRetornarBadRequest");

            try
            {
                // Arrange
                var guid = Guid.NewGuid();
                var (mockContext, mockFunctionsBBDD) = SetupMocks();
                var registroExistente = new ControlHorario
                {
                    Guid = guid,
                    Empleado = "Emp1",
                    Fecha = DateTime.Today,
                    HoraEntrada = new TimeSpan(9, 0, 0),
                    HoraSalida = new TimeSpan(17, 0, 0),
                    Empresa = "TestEmpresa"
                };

                var controlHorarioData = new List<ControlHorario> { registroExistente }.AsQueryable();
                var mockSet = new Mock<DbSet<ControlHorario>>();
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.Provider).Returns(controlHorarioData.Provider);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.Expression).Returns(controlHorarioData.Expression);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.ElementType).Returns(controlHorarioData.ElementType);
                mockSet.As<IQueryable<ControlHorario>>().Setup(m => m.GetEnumerator()).Returns(controlHorarioData.GetEnumerator());
                mockContext.Setup(c => c.ControlHorario).Returns(mockSet.Object);

                mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()))
                    .ThrowsAsync(new Exception("Error de prueba"));

                var controller = new ControlHorarioController(mockContext.Object, mockFunctionsBBDD.Object);
                _output.WriteLine("ControlHorarioController creado");

                // Act
                _output.WriteLine("Ejecutando DeleteRegistroHorario");
                var result = await controller.DeleteRegistroHorario(guid);

                // Assert
                _output.WriteLine("Iniciando aserciones");
                var objectResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal(400, objectResult.StatusCode);
                Assert.Equal("Error de prueba", objectResult.Value);

                mockSet.Verify(m => m.Remove(It.Is<ControlHorario>(ch => ch.Guid == guid)), Times.Once());

                _output.WriteLine("Aserciones completadas con éxito");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }



        private (Mock<DbContextiLabPlus>, Mock<FunctionsBBDD>) SetupMocks()
        {
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var mockHttpContext = new Mock<HttpContext>();
            var mockIdentity = new Mock<System.Security.Claims.ClaimsIdentity>();
            mockIdentity.Setup(i => i.FindFirst(It.IsAny<string>())).Returns(new System.Security.Claims.Claim("", ""));
            var mockPrincipal = new Mock<System.Security.Claims.ClaimsPrincipal>();
            mockPrincipal.Setup(p => p.Identity).Returns(mockIdentity.Object);
            mockHttpContext.Setup(c => c.User).Returns(mockPrincipal.Object);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            return (mockContext, mockFunctionsBBDD);
        }




    }





}
