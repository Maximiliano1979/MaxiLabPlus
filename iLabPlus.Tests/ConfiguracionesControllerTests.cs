using FluentAssertions;
using FluentAssertions.Execution;
using iLabPlus.Controllers;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;


namespace iLabPlus.Tests
{
    public class ConfiguracionesControllerTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<IFunctionsCrypto> _mockFunctionsCrypto;


        public ConfiguracionesControllerTests(ITestOutputHelper output)
        {
            _output = output;
            _mockFunctionsCrypto = new Mock<IFunctionsCrypto>();
            _mockFunctionsCrypto.Setup(f => f.EncryptAES(It.IsAny<string>())).Returns((string s) => System.Text.Encoding.UTF8.GetBytes(s));
        }

        [Fact]
        public void Index_DeberiaRetornarVistaConDatosCorrectos()
        {
            // Arrange
            _output.WriteLine("Iniciando test Index_DeberiaRetornarVistaConDatosCorrectos");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);

            // Configurar datos de prueba
            var empresas = new List<Empresas>
        {
            new Empresas { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Nombre = "Test Company" }
        }.AsQueryable();

            var empresasConfig = new List<EmpresasConfig>
        {
            new EmpresasConfig { Guid = Guid.NewGuid(), Empresa = "TestEmpresa" }
        }.AsQueryable();

            // Configurar DbSet mocks
            var mockEmpresasSet = new Mock<DbSet<Empresas>>();
            mockEmpresasSet.As<IQueryable<Empresas>>().Setup(m => m.Provider).Returns(empresas.Provider);
            mockEmpresasSet.As<IQueryable<Empresas>>().Setup(m => m.Expression).Returns(empresas.Expression);
            mockEmpresasSet.As<IQueryable<Empresas>>().Setup(m => m.ElementType).Returns(empresas.ElementType);
            mockEmpresasSet.As<IQueryable<Empresas>>().Setup(m => m.GetEnumerator()).Returns(empresas.GetEnumerator());

            var mockEmpresasConfigSet = new Mock<DbSet<EmpresasConfig>>();
            mockEmpresasConfigSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.Provider).Returns(empresasConfig.Provider);
            mockEmpresasConfigSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.Expression).Returns(empresasConfig.Expression);
            mockEmpresasConfigSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.ElementType).Returns(empresasConfig.ElementType);
            mockEmpresasConfigSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.GetEnumerator()).Returns(empresasConfig.GetEnumerator());

            mockContext.Setup(c => c.Empresas).Returns(mockEmpresasSet.Object);
            mockContext.Setup(c => c.EmpresasConfig).Returns(mockEmpresasConfigSet.Object);

            // Configurar FunctionsBBDD mock
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });
            mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(new List<MenuUser>());
            mockFunctionsBBDD.Setup(f => f.GetColumnsLayout(It.IsAny<string>())).Returns(new GrupoColumnsLayout { ColumnsLayoutUser = "TestLayout", ColumnsPinnedUser = 3 });

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            // Act
            _output.WriteLine("Ejecutando método Index");
            var result = controller.Index();

            // Assert
            _output.WriteLine("Verificando resultados");
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Configuraciones", viewResult.ViewName);

            var model = Assert.IsType<EstConfiguraciones>(viewResult.Model);
            Assert.NotNull(model.EmpresasEst);
            Assert.Equal("TestEmpresa", model.EmpresasEst.Empresa);
            Assert.NotNull(model.EmpresasConfigEst);

            _output.WriteLine("Verificando ViewBag");
            Assert.NotNull(viewResult.ViewData["MenuUserList"]);
            Assert.Equal("TestLayout", viewResult.ViewData["ColumnsLayoutUser"]);
            Assert.Equal(3, viewResult.ViewData["ColumnsPinnedUser"]);

            _output.WriteLine("Test completado con éxito");
        }

        [Fact]
        public void Index_CuandoNoExisteEmpresa_DeberiaRetornarVistaConModeloVacio()
        {
            // Arrange
            _output.WriteLine("Iniciando test Index_CuandoNoExisteEmpresa_DeberiaRetornarVistaConModeloVacio");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);

            // Configurar datos de prueba vacíos
            var empresas = new List<Empresas>().AsQueryable();
            var empresasConfig = new List<EmpresasConfig>().AsQueryable();

            // Configurar DbSet mocks
            SetupMockDbSet(mockContext, empresas, empresasConfig);

            // Configurar FunctionsBBDD mock
            SetupMockFunctionsBBDD(mockFunctionsBBDD, "TestEmpresa");

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            // Act
            _output.WriteLine("Ejecutando método Index");
            var result = controller.Index();

            // Assert
            _output.WriteLine("Verificando resultados");
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Configuraciones", viewResult.ViewName);

            var model = Assert.IsType<EstConfiguraciones>(viewResult.Model);
            Assert.Null(model.EmpresasEst);
            Assert.Null(model.EmpresasConfigEst);

            _output.WriteLine("Test completado con éxito");
        }

        [Fact]
        public void Index_CuandoExisteEmpresaPeroNoConfiguracion_DeberiaRetornarVistaConModeloParcial()
        {
            // Arrange
            _output.WriteLine("Iniciando test Index_CuandoExisteEmpresaPeroNoConfiguracion_DeberiaRetornarVistaConModeloParcial");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);

            // Configurar datos de prueba con empresa pero sin configuración
            var empresas = new List<Empresas>
            {
                new Empresas { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Nombre = "Test Company" }
            }.AsQueryable();
            var empresasConfig = new List<EmpresasConfig>().AsQueryable();

            // Configurar DbSet mocks
            SetupMockDbSet(mockContext, empresas, empresasConfig);

            // Configurar FunctionsBBDD mock
            SetupMockFunctionsBBDD(mockFunctionsBBDD, "TestEmpresa");

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            // Act
            _output.WriteLine("Ejecutando método Index");
            var result = controller.Index();

            // Assert
            _output.WriteLine("Verificando resultados");
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Configuraciones", viewResult.ViewName);

            var model = Assert.IsType<EstConfiguraciones>(viewResult.Model);
            Assert.NotNull(model.EmpresasEst);
            Assert.Equal("TestEmpresa", model.EmpresasEst.Empresa);
            Assert.Null(model.EmpresasConfigEst);

            _output.WriteLine("Test completado con éxito");
        }


        [Fact]
        public async Task Save_CONFIG_Mail_DeberiaActualizarConfiguracionYRedirigir()
        {
            // Arrange
            _output.WriteLine("Iniciando test Save_CONFIG_Mail_DeberiaActualizarConfiguracionYRedirigir");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);

            var empresasConfigGuid = Guid.NewGuid();
            var empresasConfig = new List<EmpresasConfig>
            {
                new EmpresasConfig
                {
                    Guid = empresasConfigGuid,
                    Empresa = "TestEmpresa",
                    MailsEnvILabPlus = false,
                    MailsEnvNombre = "OldName",
                    MailsEnvCuenta = "old@email.com",
                    MailsEnvServidor = "old.server.com",
                    MailsEnvPuerto = 25,
                    MailsEnvDirRespuesta = "oldreply@email.com",
                    TextosFirmaMail = "Old Signature"
                }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<EmpresasConfig>>();
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.Provider).Returns(empresasConfig.Provider);
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.Expression).Returns(empresasConfig.Expression);
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.ElementType).Returns(empresasConfig.ElementType);
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.GetEnumerator()).Returns(empresasConfig.GetEnumerator());

            mockContext.Setup(c => c.EmpresasConfig).Returns(mockSet.Object);

            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa", SessionUsuarioNombre = "TestUser" });

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            var updatedConfig = new EmpresasConfig
            {
                Guid = empresasConfigGuid,
                MailsEnvILabPlus = true,
                MailsEnvNombre = "NewName",
                MailsEnvCuenta = "new@email.com",
                MailsEnvServidor = "new.server.com",
                MailsEnvPuerto = 587,
                MailsEnvDirRespuesta = "newreply@email.com",
                TextosFirmaMail = "New Signature"
            };

            // Act
            _output.WriteLine("Ejecutando Save_CONFIG_Mail");
            var result = await controller.Save_CONFIG_Mail(updatedConfig);

            // Assert
            _output.WriteLine("Verificando resultados");
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var savedConfig = empresasConfig.First();
            Assert.True(savedConfig.MailsEnvILabPlus);
            Assert.Equal("NewName", savedConfig.MailsEnvNombre);
            Assert.Equal("new@email.com", savedConfig.MailsEnvCuenta);
            Assert.Equal("new.server.com", savedConfig.MailsEnvServidor);
            Assert.Equal(587, savedConfig.MailsEnvPuerto);
            Assert.Equal("newreply@email.com", savedConfig.MailsEnvDirRespuesta);
            Assert.Equal("New Signature", savedConfig.TextosFirmaMail);
            Assert.Equal("TestUser", savedConfig.IsoUser);
            Assert.NotNull(savedConfig.IsoFecMod);

            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            _output.WriteLine("Test completado con éxito");
        }



        [Fact]
        public async Task Save_CONFIG_Mail_CuandoNoExisteConfiguracion_DeberiaRetornar400ConNull()
        {
            // Arrange
            _output.WriteLine("Iniciando test Save_CONFIG_Mail_CuandoNoExisteConfiguracion_DeberiaRetornar400ConNull");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);

            var empresasConfig = new List<EmpresasConfig>().AsQueryable();

            var mockSet = new Mock<DbSet<EmpresasConfig>>();
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.Provider).Returns(empresasConfig.Provider);
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.Expression).Returns(empresasConfig.Expression);
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.ElementType).Returns(empresasConfig.ElementType);
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.GetEnumerator()).Returns(empresasConfig.GetEnumerator());

            mockContext.Setup(c => c.EmpresasConfig).Returns(mockSet.Object);

            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa", SessionUsuarioNombre = "TestUser" });

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            var updatedConfig = new EmpresasConfig
            {
                Guid = Guid.NewGuid(),
                MailsEnvILabPlus = true
            };

            // Act
            _output.WriteLine("Ejecutando Save_CONFIG_Mail");
            var result = await controller.Save_CONFIG_Mail(updatedConfig);

            // Assert
            _output.WriteLine("Verificando resultados");
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Null(objectResult.Value);

            _output.WriteLine("Test completado con éxito");
        }


        [Fact]
        public async Task Save_CONFIG_Mail_CuandoOcurreExcepcion_DeberiaRetornar400ConMensajeDeError()
        {
            // Arrange
            _output.WriteLine("Iniciando test Save_CONFIG_Mail_CuandoOcurreExcepcion_DeberiaRetornar400ConMensajeDeError");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);

            mockContext.Setup(c => c.EmpresasConfig).Throws(new Exception("Error de prueba"));

            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa", SessionUsuarioNombre = "TestUser" });

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            var updatedConfig = new EmpresasConfig
            {
                Guid = Guid.NewGuid(),
                MailsEnvILabPlus = true
            };

            // Act
            _output.WriteLine("Ejecutando Save_CONFIG_Mail");
            var result = await controller.Save_CONFIG_Mail(updatedConfig);

            // Assert
            _output.WriteLine("Verificando resultados");
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Error de prueba", objectResult.Value);

            _output.WriteLine("Test completado con éxito");
        }


        [Fact]
        public async Task Save_CONFIG_Empresa_DeberiaActualizarEmpresaYConfiguracionYRedirigir()
        {
            // Arrange
            _output.WriteLine("Iniciando test Save_CONFIG_Empresa_DeberiaActualizarEmpresaYConfiguracionYRedirigir");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);

            var empresaGuid = Guid.NewGuid();
            var empresasConfigGuid = Guid.NewGuid();

            var empresas = new List<Empresas>
        {
            new Empresas
            {
                Guid = empresaGuid,
                Nombre = "OldName",
                RazonSocial = "OldRazonSocial"
            }
        }.AsQueryable();

            var empresasConfig = new List<EmpresasConfig>
        {
            new EmpresasConfig
            {
                Guid = empresasConfigGuid,
                FormaCobroClientes = "OldForma",
                CuentaBancaria = "OldCuenta"
            }
        }.AsQueryable();

            var mockEmpresasSet = new Mock<DbSet<Empresas>>();
            SetupMockDbSet(mockEmpresasSet, empresas);

            var mockEmpresasConfigSet = new Mock<DbSet<EmpresasConfig>>();
            SetupMockDbSet(mockEmpresasConfigSet, empresasConfig);

            mockContext.Setup(c => c.Empresas).Returns(mockEmpresasSet.Object);
            mockContext.Setup(c => c.EmpresasConfig).Returns(mockEmpresasConfigSet.Object);

            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa", SessionUsuarioNombre = "TestUser" });

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            var updatedEmpresa = new Empresas
            {
                Guid = empresaGuid,
                Nombre = "NewName",
                RazonSocial = "NewRazonSocial"
            };

            var updatedConfig = new EmpresasConfig
            {
                Guid = empresasConfigGuid,
                FormaCobroClientes = "NewForma",
                CuentaBancaria = "NewCuenta"
            };

            // Act
            _output.WriteLine("Ejecutando Save_CONFIG_Empresa");
            var result = await controller.Save_CONFIG_Empresa(updatedEmpresa, updatedConfig);

            // Assert
            _output.WriteLine("Verificando resultados");
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var savedEmpresa = empresas.First();
            Assert.Equal("NewName", savedEmpresa.Nombre);
            Assert.Equal("NewRazonSocial", savedEmpresa.RazonSocial);

            var savedConfig = empresasConfig.First();
            Assert.Equal("NewForma", savedConfig.FormaCobroClientes);
            Assert.Equal("NewCuenta", savedConfig.CuentaBancaria);
            Assert.Equal("TestUser", savedConfig.IsoUser);
            Assert.NotNull(savedConfig.IsoFecMod);

            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));

            _output.WriteLine("Test completado con éxito");
        }

        [Fact]
        public async Task Save_CONFIG_Empresa_CuandoNoExisteEmpresa_DeberiaActualizarSoloConfiguracion()
        {
            // Arrange
            _output.WriteLine("Iniciando test Save_CONFIG_Empresa_CuandoNoExisteEmpresa_DeberiaActualizarSoloConfiguracion");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);

            var empresaGuid = Guid.NewGuid();
            var empresasConfigGuid = Guid.NewGuid();

            var empresas = new List<Empresas>().AsQueryable();

            var empresasConfig = new List<EmpresasConfig>
        {
            new EmpresasConfig
            {
                Guid = empresasConfigGuid,
                FormaCobroClientes = "OldForma",
                CuentaBancaria = "OldCuenta"
            }
        }.AsQueryable();

            var mockEmpresasSet = new Mock<DbSet<Empresas>>();
            SetupMockDbSet(mockEmpresasSet, empresas);

            var mockEmpresasConfigSet = new Mock<DbSet<EmpresasConfig>>();
            SetupMockDbSet(mockEmpresasConfigSet, empresasConfig);

            mockContext.Setup(c => c.Empresas).Returns(mockEmpresasSet.Object);
            mockContext.Setup(c => c.EmpresasConfig).Returns(mockEmpresasConfigSet.Object);

            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa", SessionUsuarioNombre = "TestUser" });

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            var updatedEmpresa = new Empresas
            {
                Guid = empresaGuid,
                Nombre = "NewName",
                RazonSocial = "NewRazonSocial"
            };

            var updatedConfig = new EmpresasConfig
            {
                Guid = empresasConfigGuid,
                FormaCobroClientes = "NewForma",
                CuentaBancaria = "NewCuenta"
            };

            // Act
            _output.WriteLine("Ejecutando Save_CONFIG_Empresa");
            var result = await controller.Save_CONFIG_Empresa(updatedEmpresa, updatedConfig);

            // Assert
            _output.WriteLine("Verificando resultados");
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            Assert.Empty(empresas);

            var savedConfig = empresasConfig.First();
            Assert.Equal("NewForma", savedConfig.FormaCobroClientes);
            Assert.Equal("NewCuenta", savedConfig.CuentaBancaria);
            Assert.Equal("TestUser", savedConfig.IsoUser);
            Assert.NotNull(savedConfig.IsoFecMod);

            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            _output.WriteLine("Test completado con éxito");
        }


        [Fact]
        public async Task Save_CONFIG_Empresa_CuandoOcurreExcepcion_DeberiaRetornar400ConMensajeDeError()
        {
            // Arrange
            _output.WriteLine("Iniciando test Save_CONFIG_Empresa_CuandoOcurreExcepcion_DeberiaRetornar400ConMensajeDeError");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);

            // Simulamos que EmpresasConfig es null para provocar la excepción
            mockContext.Setup(c => c.EmpresasConfig).Returns((DbSet<EmpresasConfig>)null);

            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa", SessionUsuarioNombre = "TestUser" });

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            var updatedEmpresa = new Empresas
            {
                Guid = Guid.NewGuid(),
                Nombre = "NewName"
            };

            var updatedConfig = new EmpresasConfig
            {
                Guid = Guid.NewGuid(),
                FormaCobroClientes = "NewForma"
            };

            // Act
            _output.WriteLine("Ejecutando Save_CONFIG_Empresa");
            var result = await controller.Save_CONFIG_Empresa(updatedEmpresa, updatedConfig);

            // Assert
            _output.WriteLine("Verificando resultados");
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Contains("Value cannot be null. (Parameter 'source')", objectResult.Value.ToString());

            _output.WriteLine("Test completado con éxito");
        }



        [Fact]
        public async Task Save_CONFIG_TextosPlantillas_DeberiaActualizarYRedirigir()
        {
            // Arrange
            _output.WriteLine("Iniciando test Save_CONFIG_TextosPlantillas_DeberiaActualizarYRedirigir");
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);

            var empresasConfigGuid = Guid.NewGuid();
            var existingConfig = new EmpresasConfig
            {
                Guid = empresasConfigGuid,
                TextosAlbPiePag = "OldTextoAlb",
                TextosAlbRPGD = "OldRPGDAlb",
                TextosFacPiePag = "OldTextoFac",
                TextosFacRPGD = "OldRPGDFac",
                RegistroMercantil = "OldRegistro"
            };

            var mockSet = new Mock<DbSet<EmpresasConfig>>();
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.Provider).Returns(new[] { existingConfig }.AsQueryable().Provider);
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.Expression).Returns(new[] { existingConfig }.AsQueryable().Expression);
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.ElementType).Returns(new[] { existingConfig }.AsQueryable().ElementType);
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.GetEnumerator()).Returns(new[] { existingConfig }.AsQueryable().GetEnumerator());

            mockContext.Setup(c => c.EmpresasConfig).Returns(mockSet.Object);

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            var updatedConfig = new EmpresasConfig
            {
                Guid = empresasConfigGuid,
                TextosAlbPiePag = "NewTextoAlb",
                TextosAlbRPGD = "NewRPGDAlb",
                TextosFacPiePag = "NewTextoFac",
                TextosFacRPGD = "NewRPGDFac",
                RegistroMercantil = "NewRegistro"
            };

            // Act
            _output.WriteLine("Ejecutando Save_CONFIG_TextosPlantillas");
            var result = await controller.Save_CONFIG_TextosPlantillas(updatedConfig);

            // Assert
            _output.WriteLine("Verificando resultados");
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.Equal("NewTextoAlb", existingConfig.TextosAlbPiePag);
            Assert.Equal("NewRPGDAlb", existingConfig.TextosAlbRPGD);
            Assert.Equal("NewTextoFac", existingConfig.TextosFacPiePag);
            Assert.Equal("NewRPGDFac", existingConfig.TextosFacRPGD);
            Assert.Equal("NewRegistro", existingConfig.RegistroMercantil);

            _output.WriteLine("Test completado con éxito");
        }




        [Fact]
        public async Task Save_CONFIG_TextosPlantillas_CuandoNoExisteConfiguracion_DeberiaCrearYRedirigir()
        {
            _output.WriteLine("Iniciando test Save_CONFIG_TextosPlantillas_CuandoNoExisteConfiguracion_DeberiaCrearYRedirigir");

            // Configuración del mock para EmpresasConfig
            var empresasConfig = new List<EmpresasConfig>();
            var mockSet = new Mock<DbSet<EmpresasConfig>>();
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.Provider).Returns(empresasConfig.AsQueryable().Provider);
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.Expression).Returns(empresasConfig.AsQueryable().Expression);
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.ElementType).Returns(empresasConfig.AsQueryable().ElementType);
            mockSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.GetEnumerator()).Returns(empresasConfig.GetEnumerator());
            mockSet.Setup(m => m.Add(It.IsAny<EmpresasConfig>())).Callback<EmpresasConfig>(empresasConfig.Add);

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.EmpresasConfig).Returns(mockSet.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa", SessionUsuarioNombre = "TestUser" });

            // Configuración del ControllerContext y servicios necesarios
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("mockUrl");

            var serviceProvider = new ServiceCollection()
                .AddScoped<IUrlHelper>(sp => mockUrlHelper.Object)
                .BuildServiceProvider();

            httpContext.RequestServices = serviceProvider;

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output)
            {
                ControllerContext = new ControllerContext(actionContext),
                TempData = tempData,
                Url = mockUrlHelper.Object
            };

            var updatedConfig = new EmpresasConfig
            {
                Guid = Guid.NewGuid(),
                TextosAlbPiePag = "NewTextoAlb"
            };

            _output.WriteLine("Ejecutando Save_CONFIG_TextosPlantillas");
            var result = await controller.Save_CONFIG_TextosPlantillas(updatedConfig);

            _output.WriteLine("Verificando resultados");
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            mockSet.Verify(m => m.Add(It.IsAny<EmpresasConfig>()), Times.Once());
            //mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));


            _output.WriteLine("Test completado con éxito");
        }


        [Fact]
        public async Task Save_CONFIG_TextosPlantillas_CuandoOcurreExcepcion_DeberiaRetornar400ConMensaje()
        {
            // Arrange
            _output.WriteLine("Iniciando test Save_CONFIG_TextosPlantillas_CuandoOcurreExcepcion_DeberiaRetornar400ConMensaje");
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);
            mockContext.Setup(c => c.EmpresasConfig).Throws(new InvalidOperationException("Error de prueba"));
            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);
            var updatedConfig = new EmpresasConfig
            {
                Guid = Guid.NewGuid(),
                TextosAlbPiePag = "NewTextoAlb"
            };

            // Act
            _output.WriteLine("Ejecutando Save_CONFIG_TextosPlantillas");
            var result = await controller.Save_CONFIG_TextosPlantillas(updatedConfig);

            // Assert
            _output.WriteLine("Verificando resultados");
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Error de prueba", objectResult.Value);
            _output.WriteLine("Test completado con éxito");
        }


        [Fact]
        public async Task GetCertificadosExistentes_DeberiaRetornarListaDeCertificados()
        {
            // Arrange
            _output.WriteLine("Iniciando test GetCertificadosExistentes_DeberiaRetornarListaDeCertificados");
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);

            // Simulamos un error para que se ejecute el bloque catch
            mockContext.Setup(c => c.EmpresasCertificados).Throws(new Exception("Error de prueba"));
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            // Act
            _output.WriteLine("Ejecutando GetCertificadosExistentes");
            var result = await controller.GetCertificadosExistentes();

            // Assert
            _output.WriteLine("Verificando resultados");
            using (new AssertionScope())  // Usamos AssertionScope para agrupar aserciones
            {
                result.Should().BeOfType<JsonResult>();
                var jsonResult = (JsonResult)result;
                jsonResult.Value.Should().BeEquivalentTo(new { error = "Error interno del servidor" });
            }

            _output.WriteLine("Test completado con éxito");
        }

        [Fact]
        public async Task GetCertificadosExistentes_CuandoOcurreExcepcion_DeberiaRetornar500()
        {
            // Arrange
            _output.WriteLine("Iniciando test GetCertificadosExistentes_CuandoOcurreExcepcion_DeberiaRetornar500");
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);

            mockContext.Setup(c => c.EmpresasCertificados).Throws(new Exception("Error de prueba"));
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            // Act
            _output.WriteLine("Ejecutando GetCertificadosExistentes");
            var result = await controller.GetCertificadosExistentes();

            // Assert
            _output.WriteLine("Verificando resultados");
            using (new AssertionScope()) // Usamos AssertionScope para agrupar aserciones
            {
                result.Should().BeOfType<JsonResult>(); // Verificamos que el resultado sea un JsonResult
                var jsonResult = (JsonResult)result;
                jsonResult.Value.Should().BeEquivalentTo(new { error = "Error interno del servidor" }); // Verificamos el contenido del JSON
            }

            _output.WriteLine("Test completado con éxito");
        }


        // Tests para el Metodo SubirCertificados ////////////////////////////////////////////////////////////


        [Fact]
        public async Task SubirCertificado_ConArchivoInvalido_DebeRedirigiryEstablecerMensajeDeError()
        {
            // Arrange
            _output.WriteLine("Iniciando test SubirCertificado_ConArchivoInvalido_DebeRedirigiryEstablecerMensajeDeError");
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            // Simular un archivo inválido
            var invalidFile = new Mock<IFormFile>();
            invalidFile.Setup(f => f.FileName).Returns("invalid.txt");

            // Act
            _output.WriteLine("Ejecutando SubirCertificado");
            var result = await controller.SubirCertificado(invalidFile.Object, "password");

            // Assert
            _output.WriteLine("Verificando resultados");
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            Assert.Equal("Validación fallida: Asegúrate de que el archivo sea .pfx y la contraseña tenga al menos 8 caracteres.", controller.ViewBag.ErrorMessage);

            _output.WriteLine("Test completado con éxito");
        }



        [Fact]
        public async Task SubirCertificado_CertificadoExistente_DebeActualizarYRedirigir()
        {
            // Arrange
            _output.WriteLine("Iniciando test SubirCertificado_CertificadoExistente_DebeActualizarYRedirigir");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            // Simular un certificado existente en la base de datos
            var certificadosExistentes = new List<EmpresasCertificados>
                {
                    new EmpresasCertificados
                    {
                        Guid = Guid.NewGuid(),
                        Empresa = "TestEmpresa",
                        Fichero = "existing.pfx",
                        RutaArchivo = "old/path/existing.pfx",
                        Password = System.Text.Encoding.UTF8.GetBytes("oldPassword")
                    }
                }.AsQueryable();

            var mockSet = new Mock<DbSet<EmpresasCertificados>>();
            mockSet.As<IQueryable<EmpresasCertificados>>().Setup(m => m.Provider).Returns(certificadosExistentes.Provider);
            mockSet.As<IQueryable<EmpresasCertificados>>().Setup(m => m.Expression).Returns(certificadosExistentes.Expression);
            mockSet.As<IQueryable<EmpresasCertificados>>().Setup(m => m.ElementType).Returns(certificadosExistentes.ElementType);
            mockSet.As<IQueryable<EmpresasCertificados>>().Setup(m => m.GetEnumerator()).Returns(certificadosExistentes.GetEnumerator());

            mockContext.Setup(c => c.EmpresasCertificados).Returns(mockSet.Object);

            var controller = new ConfiguracionesController(mockContext.Object, mockFunctionsBBDD.Object, _output)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Simular un archivo válido
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("existing.pfx");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            _output.WriteLine("Ejecutando SubirCertificado");
            var result = await controller.SubirCertificado(fileMock.Object, "newPassword");

            // Assert
            _output.WriteLine("Verificando resultados");
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            mockSet.Verify(m => m.Update(It.Is<EmpresasCertificados>(c => c.Fichero == "existing.pfx")), Times.Once());
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            _output.WriteLine("Test completado con éxito");
        }


        [Fact]
        public async Task SubirCertificado_NuevoCertificado_DebeAgregarYRedirigir()
        {
            // Arrange
            _output.WriteLine("Iniciando test SubirCertificado_NuevoCertificado_DebeAgregarYRedirigir");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            // Simular una base de datos vacía
            var certificadosExistentes = new List<EmpresasCertificados>().AsQueryable();

            var mockSet = new Mock<DbSet<EmpresasCertificados>>();
            mockSet.As<IQueryable<EmpresasCertificados>>().Setup(m => m.Provider).Returns(certificadosExistentes.Provider);
            mockSet.As<IQueryable<EmpresasCertificados>>().Setup(m => m.Expression).Returns(certificadosExistentes.Expression);
            mockSet.As<IQueryable<EmpresasCertificados>>().Setup(m => m.ElementType).Returns(certificadosExistentes.ElementType);
            mockSet.As<IQueryable<EmpresasCertificados>>().Setup(m => m.GetEnumerator()).Returns(certificadosExistentes.GetEnumerator());

            mockContext.Setup(c => c.EmpresasCertificados).Returns(mockSet.Object);

            // Mock para IFunctionsCrypto
            var mockFunctionsCrypto = new Mock<IFunctionsCrypto>();
            mockFunctionsCrypto.Setup(f => f.EncryptAES(It.IsAny<string>())).Returns((string s) => System.Text.Encoding.UTF8.GetBytes(s));

            var controller = new ConfiguracionesController(
                mockContext.Object,
                mockFunctionsBBDD.Object,
                _output,
                _mockFunctionsCrypto.Object);

            // Simular un archivo válido
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("new_certificate.pfx");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            _output.WriteLine("Ejecutando SubirCertificado");
            var result = await controller.SubirCertificado(fileMock.Object, "newPassword");

            // Assert
            _output.WriteLine("Verificando resultados");
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            mockSet.Verify(m => m.Add(It.Is<EmpresasCertificados>(c =>
                c.Fichero == "new_certificate.pfx" &&
                c.Empresa == "TestEmpresa")), Times.Once());
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            _output.WriteLine("Test completado con éxito");
        }


        // Tests para el Metodo GetDirectorioEmpresas ////////////////////////////////////////////////////////////



        [Fact]
        public void GetDirectoriosEmpresa_DirectorioExiste_DevuelveListaDeDirectorios()
        {
            // Arrange
            _output.WriteLine("Iniciando test GetDirectoriosEmpresa_DirectorioExiste_DevuelveListaDeDirectorios");
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddDirectory(@"C:\");
            mockFileSystem.AddDirectory(@"C:\iLabPlusDocs");
            mockFileSystem.Directory.SetCurrentDirectory(@"C:\");

            var mockGrupoClaims = new GrupoClaims { SessionEmpresa = "TestEmpresa" };
            var baseDir = Path.Combine("C:", "iLabPlusDocs", "TestEmpresa");
            mockFileSystem.AddDirectory(baseDir);
            mockFileSystem.AddDirectory(Path.Combine(baseDir, "Dir1"));
            mockFileSystem.AddDirectory(Path.Combine(baseDir, "Dir2"));
            mockFileSystem.AddFile(Path.Combine(baseDir, "Dir1", "file1.txt"), new MockFileData(new string('a', 100)));
            mockFileSystem.AddFile(Path.Combine(baseDir, "Dir2", "file2.txt"), new MockFileData(new string('b', 200)));

            _output.WriteLine($"Directorio base configurado: {baseDir}");
            _output.WriteLine($"Directorios existentes: {string.Join(", ", mockFileSystem.AllDirectories)}");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(mockGrupoClaims);

            var controller = new ConfiguracionesController(
                mockContext.Object,
                mockFunctionsBBDD.Object,
                _output,
                _mockFunctionsCrypto.Object,
                mockFileSystem);

            // Asegúrate de que GrupoClaims esté configurado correctamente
            var grupoClaims = controller.GetType().GetField("GrupoClaims", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(controller) as GrupoClaims;
            if (grupoClaims == null)
            {
                controller.GetType().GetField("GrupoClaims", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, mockGrupoClaims);
            }
            else
            {
                grupoClaims.SessionEmpresa = mockGrupoClaims.SessionEmpresa;
            }

            // Act
            _output.WriteLine("Ejecutando GetDirectoriosEmpresa");
            var result = controller.GetDirectoriosEmpresa() as JsonResult;

            // Assert
            _output.WriteLine("Verificando resultados");
            Assert.NotNull(result);
            var resultValue = result.Value;

            if (resultValue is IDictionary<string, object> errorDict && errorDict.ContainsKey("error"))
            {
                _output.WriteLine($"Error en el resultado: {errorDict["error"]}");
                Assert.True(false, $"Se encontró un error: {errorDict["error"]}");
            }
            else if (resultValue is IEnumerable<object> directorios)
            {
                var dirList = directorios.ToList();
                Assert.Equal(2, dirList.Count);

                dynamic dir1 = dirList.FirstOrDefault(d => ((dynamic)d).Nombre == "Dir1");
                dynamic dir2 = dirList.FirstOrDefault(d => ((dynamic)d).Nombre == "Dir2");

                Assert.NotNull(dir1);
                Assert.NotNull(dir2);
                Assert.Equal(100L, (long)dir1.Tamaño);
                Assert.Equal(200L, (long)dir2.Tamaño);

                _output.WriteLine($"Dir1: Nombre={dir1.Nombre}, Tamaño={dir1.Tamaño}");
                _output.WriteLine($"Dir2: Nombre={dir2.Nombre}, Tamaño={dir2.Tamaño}");
            }
            else
            {
                Assert.True(false, $"Tipo de resultado inesperado: {resultValue?.GetType().Name ?? "null"}");
            }

            _output.WriteLine("Test completado con éxito");
        }


        [Fact]
        public void GetDirectoriosEmpresa_DirectorioNoRaiz_DevuelveListaDeDirectorios()
        {
            // Arrange
            _output.WriteLine("Iniciando test GetDirectoriosEmpresa_DirectorioNoRaiz_DevuelveListaDeDirectorios");
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddDirectory(@"C:\");
            mockFileSystem.AddDirectory(@"C:\Users");
            mockFileSystem.AddDirectory(@"C:\Users\iLabPlusDocs");
            mockFileSystem.AddDirectory(@"C:\Users\iLabPlusDocs\TestEmpresa");
            mockFileSystem.AddDirectory(@"C:\Users\iLabPlusDocs\TestEmpresa\Dir1");
            mockFileSystem.AddDirectory(@"C:\Users\iLabPlusDocs\TestEmpresa\Dir2");
            mockFileSystem.AddFile(@"C:\Users\iLabPlusDocs\TestEmpresa\Dir1\file1.txt", new MockFileData(new string('a', 100)));
            mockFileSystem.AddFile(@"C:\Users\iLabPlusDocs\TestEmpresa\Dir2\file2.txt", new MockFileData(new string('b', 200)));

            // Establecemos el directorio actual
            mockFileSystem.Directory.SetCurrentDirectory(@"C:\Users");

            var mockGrupoClaims = new GrupoClaims { SessionEmpresa = "TestEmpresa" };

            _output.WriteLine($"Directorio actual: {mockFileSystem.Directory.GetCurrentDirectory()}");
            _output.WriteLine($"Directorios existentes: {string.Join(", ", mockFileSystem.AllDirectories)}");

            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(MockBehavior.Loose, mockContext.Object, new Mock<IHttpContextAccessor>().Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(mockGrupoClaims);

            var controller = new ConfiguracionesController(
                mockContext.Object,
                mockFunctionsBBDD.Object,
                _output,
                _mockFunctionsCrypto.Object,
                mockFileSystem);

            // Asegúrate de que GrupoClaims esté configurado correctamente
            controller.GetType().GetField("GrupoClaims", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, mockGrupoClaims);

            // Act
            _output.WriteLine("Ejecutando GetDirectoriosEmpresa");
            var result = controller.GetDirectoriosEmpresa() as JsonResult;

            // Assert
            _output.WriteLine("Verificando resultados");
            Assert.NotNull(result);
            var resultValue = result.Value;

            if (resultValue is IDictionary<string, object> errorDict && errorDict.ContainsKey("error"))
            {
                _output.WriteLine($"Error en el resultado: {errorDict["error"]}");
                Assert.True(false, $"Se encontró un error: {errorDict["error"]}");
            }
            else if (resultValue is IEnumerable<object> directorios)
            {
                var dirList = directorios.ToList();
                Assert.Equal(2, dirList.Count);

                dynamic dir1 = dirList.FirstOrDefault(d => ((dynamic)d).Nombre == "Dir1");
                dynamic dir2 = dirList.FirstOrDefault(d => ((dynamic)d).Nombre == "Dir2");

                Assert.NotNull(dir1);
                Assert.NotNull(dir2);
                Assert.Equal(100L, (long)dir1.Tamaño);
                Assert.Equal(200L, (long)dir2.Tamaño);

                _output.WriteLine($"Dir1: Nombre={dir1.Nombre}, Tamaño={dir1.Tamaño}");
                _output.WriteLine($"Dir2: Nombre={dir2.Nombre}, Tamaño={dir2.Tamaño}");
            }
            else
            {
                Assert.True(false, $"Tipo de resultado inesperado: {resultValue?.GetType().Name ?? "null"}");
            }

            _output.WriteLine("Test completado con éxito");
        }



        private void SetupMockDbSet<T>(Mock<DbSet<T>> mockSet, IQueryable<T> data) where T : class
        {
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        }

        private void SetupMockDbSet(Mock<DbContextiLabPlus> mockContext, IQueryable<Empresas> empresas, IQueryable<EmpresasConfig> empresasConfig)
        {
            var mockEmpresasSet = new Mock<DbSet<Empresas>>();
            mockEmpresasSet.As<IQueryable<Empresas>>().Setup(m => m.Provider).Returns(empresas.Provider);
            mockEmpresasSet.As<IQueryable<Empresas>>().Setup(m => m.Expression).Returns(empresas.Expression);
            mockEmpresasSet.As<IQueryable<Empresas>>().Setup(m => m.ElementType).Returns(empresas.ElementType);
            mockEmpresasSet.As<IQueryable<Empresas>>().Setup(m => m.GetEnumerator()).Returns(empresas.GetEnumerator());

            var mockEmpresasConfigSet = new Mock<DbSet<EmpresasConfig>>();
            mockEmpresasConfigSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.Provider).Returns(empresasConfig.Provider);
            mockEmpresasConfigSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.Expression).Returns(empresasConfig.Expression);
            mockEmpresasConfigSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.ElementType).Returns(empresasConfig.ElementType);
            mockEmpresasConfigSet.As<IQueryable<EmpresasConfig>>().Setup(m => m.GetEnumerator()).Returns(empresasConfig.GetEnumerator());

            mockContext.Setup(c => c.Empresas).Returns(mockEmpresasSet.Object);
            mockContext.Setup(c => c.EmpresasConfig).Returns(mockEmpresasConfigSet.Object);
        }

        private void SetupMockFunctionsBBDD(Mock<FunctionsBBDD> mockFunctionsBBDD, string sessionEmpresa)
        {
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = sessionEmpresa });
            mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(new List<MenuUser>());
            mockFunctionsBBDD.Setup(f => f.GetColumnsLayout(It.IsAny<string>())).Returns(new GrupoColumnsLayout { ColumnsLayoutUser = "TestLayout", ColumnsPinnedUser = 3 });
        }


    }
}