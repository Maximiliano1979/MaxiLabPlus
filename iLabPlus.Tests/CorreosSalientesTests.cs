using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Nemesis365.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static iLabPlus.Helpers.AsyncTestHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace iLabPlus.Tests
{
    public class CorreosSalientesTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<FunctionsBBDD> _mockFunctionsBBDD;
        private readonly Mock<ILogger<CorreosSalientesController>> _mockLogger;
        private readonly MockFileSystem _mockFileSystem;


        public CorreosSalientesTests(ITestOutputHelper output)
        {
            _output = output;
            _output.WriteLine("Constructor de CorreosSalientesTests iniciado");

            try
            {
                var mockContext = new Mock<DbContextiLabPlus>();
                _output.WriteLine("Mock de DbContextiLabPlus creado");

                var mockHttpContextAccessor = CreateMockHttpContextAccessor();
                _output.WriteLine("Mock de IHttpContextAccessor creado");

                _mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor);
                _output.WriteLine("Mock de FunctionsBBDD creado");

                _mockLogger = new Mock<ILogger<CorreosSalientesController>>();
                _output.WriteLine("Mock de ILogger creado");

                _mockFileSystem = new MockFileSystem();
                _output.WriteLine("MockFileSystem creado");

                _output.WriteLine("Constructor de CorreosSalientesTests completado");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error en el constructor: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }


        private FunctionsMails CreateFunctionsMails(DbContextiLabPlus context, IHttpContextAccessor contextAccessor)
        {
            var logger = new Mock<ILogger<FunctionsMails>>().Object;
            return new FunctionsMails(context, contextAccessor, logger);
        }

        private IHttpContextAccessor CreateMockHttpContextAccessor()
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim("Empresa", "TestEmpresa"),
                new Claim("Usuario", "TestUsuario"),
                new Claim("EmpresaNombre", "TestEmpresaNombre"),
                new Claim("UsuarioNombre", "TestUsuarioNombre"),
                new Claim("UsuarioTipo", "TestUsuarioTipo")
            });
            var mockPrincipal = new ClaimsPrincipal(mockIdentity);
            mockHttpContext.Setup(c => c.User).Returns(mockPrincipal);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
            return mockHttpContextAccessor.Object;
        }


        public class TestFunctionsMails : FunctionsMails
        {
            public TestFunctionsMails(DbContextiLabPlus Context, IHttpContextAccessor ContextAccessor, ILogger<FunctionsMails> logger)
                : base(Context, ContextAccessor, logger)
            {
            }

            public override string MailSend(string remitente, string destinatario, string CCO, string asunto, string cuerpo, IList<IFormFile> adjuntos)
            {
                // Implementación de prueba que devuelve un MessageId válido
                return "TestMessageId-" + Guid.NewGuid().ToString();
            }
        }
     

        [Fact]
        public async Task Index_RetornaVistaConCorreosDeLaEmpresa()
        {
            _output.WriteLine("Iniciando test Index_RetornaVistaConCorreosDeLaEmpresa");

            try
            {
                // Arrange
                var correosSalientes = new List<CorreosSalientes>
                {
                    new CorreosSalientes { Guid = Guid.NewGuid(), Empresa = "Empresa1", FechaEnv = DateTime.Now, MessageId = "1" },
                    new CorreosSalientes { Guid = Guid.NewGuid(), Empresa = "Empresa1", FechaEnv = DateTime.Now.AddDays(-1), MessageId = "2" },
                    new CorreosSalientes { Guid = Guid.NewGuid(), Empresa = "OtraEmpresa", FechaEnv = DateTime.Now, MessageId = "3" }
                }.AsQueryable();

                var mockSetCorreos = new Mock<DbSet<CorreosSalientes>>();

                mockSetCorreos.As<IAsyncEnumerable<CorreosSalientes>>()
                    .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                    .Returns(new AsyncTestHelpers.TestAsyncEnumerator<CorreosSalientes>(correosSalientes.GetEnumerator()));

                mockSetCorreos.As<IQueryable<CorreosSalientes>>()
                    .Setup(m => m.Provider)
                    .Returns(new AsyncTestHelpers.TestAsyncQueryProvider<CorreosSalientes>(correosSalientes.Provider));

                mockSetCorreos.As<IQueryable<CorreosSalientes>>().Setup(m => m.Expression).Returns(correosSalientes.Expression);
                mockSetCorreos.As<IQueryable<CorreosSalientes>>().Setup(m => m.ElementType).Returns(correosSalientes.ElementType);
                mockSetCorreos.As<IQueryable<CorreosSalientes>>().Setup(m => m.GetEnumerator()).Returns(correosSalientes.GetEnumerator);

                // Agregar esta configuración
                mockSetCorreos.As<IAsyncEnumerable<CorreosSalientes>>()
                    .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                    .Returns(new AsyncTestHelpers.TestAsyncEnumerator<CorreosSalientes>(correosSalientes.GetEnumerator()));

                var correosSalientesAdj = new List<CorreosSalientesAdj>
                {
                    new CorreosSalientesAdj { Empresa = "Empresa1", MessageId = "1" },
                    new CorreosSalientesAdj { Empresa = "Empresa1", MessageId = "2" }
                }.AsQueryable();

                var mockSetCorreosAdj = new Mock<DbSet<CorreosSalientesAdj>>();
                mockSetCorreosAdj.As<IQueryable<CorreosSalientesAdj>>().Setup(m => m.Provider).Returns(correosSalientesAdj.Provider);
                mockSetCorreosAdj.As<IQueryable<CorreosSalientesAdj>>().Setup(m => m.Expression).Returns(correosSalientesAdj.Expression);
                mockSetCorreosAdj.As<IQueryable<CorreosSalientesAdj>>().Setup(m => m.ElementType).Returns(correosSalientesAdj.ElementType);
                mockSetCorreosAdj.As<IQueryable<CorreosSalientesAdj>>().Setup(m => m.GetEnumerator()).Returns(() => correosSalientesAdj.GetEnumerator());

                var mockContext = new Mock<DbContextiLabPlus>();
                mockContext.Setup(c => c.CorreosSalientes).Returns(mockSetCorreos.Object);
                mockContext.Setup(c => c.CorreosSalientesAdj).Returns(mockSetCorreosAdj.Object);

                var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, CreateMockHttpContextAccessor());
                mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(new List<MenuUser>());
                mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "Empresa1" });

                var mockFunctionsMails = new Mock<FunctionsMails>(mockContext.Object, CreateMockHttpContextAccessor(), null);
                var mockLogger = new Mock<ILogger<CorreosSalientesController>>();

                var controller = new CorreosSalientesController(
                    mockContext.Object,
                    mockFunctionsBBDD.Object,
                    mockFunctionsMails.Object,
                    mockLogger.Object,
                    _mockFileSystem,
                    _output
                );

                // Act
                var result = await controller.Index() as ViewResult;

                // Assert
                Assert.NotNull(result);
                _output.WriteLine($"ViewName: {result.ViewName}");
                _output.WriteLine($"Model type: {result?.Model?.GetType().FullName}");

                if (result.ViewName == "Error")
                {
                    _output.WriteLine($"Error message: {result.Model}");
                }

                Assert.Equal("CorreosSalientes", result.ViewName);

                var model = Assert.IsAssignableFrom<List<CorreosSalientes>>(result.Model);
                Assert.Equal(2, model.Count);
                Assert.All(model, correo => Assert.Equal("Empresa1", correo.Empresa));

                _output.WriteLine("Test completado exitosamente");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"Tipo de excepción: {ex.GetType().FullName}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }



        [Fact]
        public async Task Index_CuandoMenuUserListEsNull_RetornaVistaDeError()
        {
            _output.WriteLine("Iniciando test Index_CuandoMenuUserListEsNull_RetornaVistaDeError");

            try
            {
                // Arrange
                var mockContext = new Mock<DbContextiLabPlus>();
                _output.WriteLine("DbContextiLabPlus mock creado");

                var mockHttpContext = new Mock<HttpContext>();
                var mockIdentity = new ClaimsIdentity(new Claim[]
                {
                    new Claim("Empresa", "TestEmpresa"),
                    new Claim("Usuario", "TestUsuario"),
                    new Claim("EmpresaNombre", "TestEmpresaNombre"),
                    new Claim("UsuarioNombre", "TestUsuarioNombre"),
                    new Claim("UsuarioTipo", "TestUsuarioTipo")
                });
                var mockPrincipal = new ClaimsPrincipal(mockIdentity);
                mockHttpContext.Setup(c => c.User).Returns(mockPrincipal);
                _output.WriteLine("HttpContext mock creado con Claims");

                var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
                _output.WriteLine("IHttpContextAccessor mock creado");

                var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
                mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns((List<MenuUser>)null);
                mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });
                _output.WriteLine("FunctionsBBDD mock creado");

                var mockLogger = new Mock<ILogger<FunctionsMails>>();
                _output.WriteLine("ILogger<FunctionsMails> mock creado");

                var functionsMails = new FunctionsMails(mockContext.Object, mockHttpContextAccessor.Object, mockLogger.Object);
                _output.WriteLine("FunctionsMails creado");

                var mockControllerLogger = new Mock<ILogger<CorreosSalientesController>>();
                _output.WriteLine("ILogger<CorreosSalientesController> mock creado");

                var controller = new CorreosSalientesController(
                    mockContext.Object,
                    mockFunctionsBBDD.Object,
                    functionsMails,
                    mockControllerLogger.Object,
                    _mockFileSystem,
                    _output
                );
                _output.WriteLine("CorreosSalientesController creado");

                // Act
                _output.WriteLine("Ejecutando método Index");
                var result = await controller.Index() as ViewResult;

                // Assert
                Assert.NotNull(result);
                _output.WriteLine($"ViewName: {result.ViewName}");
                _output.WriteLine($"Model: {result.Model}");
                Assert.Equal("Error", result.ViewName);
                Assert.Equal("No se pudo obtener la lista de menús de usuario", result.Model);

                _output.WriteLine("Test completado exitosamente");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }


        [Fact]
        public async Task Index_CuandoGrupoClaimsEsNull_RetornaVistaDeError()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var httpContextAccessor = CreateMockHttpContextAccessor();
            var functionsMails = CreateFunctionsMails(mockContext.Object, httpContextAccessor);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, httpContextAccessor);
            mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(new List<MenuUser>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns((GrupoClaims)null);

            var mockLogger = new Mock<ILogger<CorreosSalientesController>>();

            var controller = new CorreosSalientesController(
                mockContext.Object,
                mockFunctionsBBDD.Object,
                functionsMails,
                mockLogger.Object,
                _mockFileSystem,
                _output
            );

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.ViewName);
            Assert.Equal("No se pudo obtener la información de la empresa de la sesión", result.Model);
        }



        [Fact]
        public async Task Index_CuandoCtxDBEsNull_RetornaVistaDeError()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.CorreosSalientes).Returns((DbSet<CorreosSalientes>)null);

            var httpContextAccessor = CreateMockHttpContextAccessor();
            var functionsMails = CreateFunctionsMails(mockContext.Object, httpContextAccessor);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, httpContextAccessor);
            mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(new List<MenuUser>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var mockLogger = new Mock<ILogger<CorreosSalientesController>>();

            var controller = new CorreosSalientesController(
                mockContext.Object,
                mockFunctionsBBDD.Object,
                functionsMails,
                mockLogger.Object,
                _mockFileSystem,
                _output
            );

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.ViewName);
            Assert.Equal("El contexto de la base de datos no está inicializado correctamente", result.Model);
        }



        [Fact]
        public async Task Index_CuandoOcurreExcepcion_RetornaVistaDeError()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.CorreosSalientes).Throws(new Exception("Error de prueba"));

            var httpContextAccessor = CreateMockHttpContextAccessor();
            var functionsMails = CreateFunctionsMails(mockContext.Object, httpContextAccessor);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, httpContextAccessor);
            mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(new List<MenuUser>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var mockLogger = new Mock<ILogger<CorreosSalientesController>>();

            var controller = new CorreosSalientesController(
                mockContext.Object,
                mockFunctionsBBDD.Object,
                functionsMails,
                mockLogger.Object,
                _mockFileSystem,
                _output
            );

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.ViewName);
            Assert.Equal("Error de prueba", result.Model);
        }



        [Fact]
        public void DialogCorreo_CuandoCorreoNoExiste_CreaNuevoCorreo()
        {
            // Arrange
            var mockSet = new Mock<DbSet<CorreosSalientes>>();

            var data = new List<CorreosSalientes>().AsQueryable();

            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.CorreosSalientes).Returns(mockSet.Object);

            var mockEmpresasConfig = new Mock<DbSet<EmpresasConfig>>();
            var empresasConfigData = new List<EmpresasConfig>
    {
        new EmpresasConfig
        {
            Guid = Guid.NewGuid(),
            Empresa = "TestEmpresa",
            TextosFirmaMail = "Firma de prueba"
        }
    }.AsQueryable();

            mockEmpresasConfig.As<IQueryable<EmpresasConfig>>().Setup(m => m.Provider).Returns(empresasConfigData.Provider);
            mockEmpresasConfig.As<IQueryable<EmpresasConfig>>().Setup(m => m.Expression).Returns(empresasConfigData.Expression);
            mockEmpresasConfig.As<IQueryable<EmpresasConfig>>().Setup(m => m.ElementType).Returns(empresasConfigData.ElementType);
            mockEmpresasConfig.As<IQueryable<EmpresasConfig>>().Setup(m => m.GetEnumerator()).Returns(empresasConfigData.GetEnumerator());

            mockContext.Setup(c => c.EmpresasConfig).Returns(mockEmpresasConfig.Object);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockIdentity = new ClaimsIdentity(new Claim[]
                {
            new Claim("Empresa", "TestEmpresa"),
            new Claim("Usuario", "TestUser"),
            new Claim("EmpresaNombre", "Test Empresa"),
            new Claim("UsuarioNombre", "Test User"),
            new Claim("UsuarioTipo", "Admin")
                });
            var mockPrincipal = new ClaimsPrincipal(mockIdentity);
            mockHttpContext.Setup(c => c.User).Returns(mockPrincipal);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var mockLogger = new Mock<ILogger<FunctionsMails>>();
            var functionsMails = new FunctionsMails(mockContext.Object, mockHttpContextAccessor.Object, mockLogger.Object);

            var mockControllerLogger = new Mock<ILogger<CorreosSalientesController>>();

            var controller = new CorreosSalientesController(
                mockContext.Object,
                mockFunctionsBBDD.Object,
                functionsMails,
                mockControllerLogger.Object,
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.DialogCorreo(Guid.NewGuid()) as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("_DialogCorreosSalientes", result.ViewName);
            var model = Assert.IsType<CorreosSalientes>(result.Model);
            Assert.NotNull(model.Cuerpo);
            Assert.Contains("Firma de prueba", model.Cuerpo);
        }



        [Fact]
        public void DialogCorreo_CuandoCorreoExiste_PreparaReenvio()
        {
            // Arrange
            var correoExistente = new CorreosSalientes
            {
                Guid = Guid.NewGuid(),
                Asunto = "Asunto original",
                Cuerpo = "Cuerpo original"
            };

            var data = new List<CorreosSalientes> { correoExistente }.AsQueryable();

            var mockSet = new Mock<DbSet<CorreosSalientes>>();
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.CorreosSalientes).Returns(mockSet.Object);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockIdentity = new ClaimsIdentity(new Claim[]
            {
            new Claim("Empresa", "TestEmpresa"),
            new Claim("Usuario", "TestUser"),
            new Claim("EmpresaNombre", "Test Empresa"),
            new Claim("UsuarioNombre", "Test User"),
            new Claim("UsuarioTipo", "Admin")
            });
            var mockPrincipal = new ClaimsPrincipal(mockIdentity);
            mockHttpContext.Setup(c => c.User).Returns(mockPrincipal);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var mockLogger = new Mock<ILogger<FunctionsMails>>();
            var functionsMails = new FunctionsMails(mockContext.Object, mockHttpContextAccessor.Object, mockLogger.Object);

            var mockControllerLogger = new Mock<ILogger<CorreosSalientesController>>();

            var controller = new CorreosSalientesController(
                mockContext.Object,
                mockFunctionsBBDD.Object,
                functionsMails,
                mockControllerLogger.Object,
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.DialogCorreo(correoExistente.Guid) as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("_DialogCorreosSalientes", result.ViewName);
            var model = Assert.IsType<CorreosSalientes>(result.Model);
            Assert.StartsWith("RV: ", model.Asunto);
            Assert.Contains("-----Mensaje original-----", model.Cuerpo);
            Assert.Contains("Cuerpo original", model.Cuerpo);
        }



        [Fact]
        public void DialogCorreo_CuandoOcurreExcepcion_RetornaVistaDeError()
        {
            // Arrange
            var mockSet = new Mock<DbSet<CorreosSalientes>>();
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Provider).Throws(new Exception("Error de prueba"));

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.CorreosSalientes).Returns(mockSet.Object);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockIdentity = new ClaimsIdentity(new Claim[]
            {
            new Claim("Empresa", "TestEmpresa"),
            new Claim("Usuario", "TestUser"),
            new Claim("EmpresaNombre", "Test Empresa"),
            new Claim("UsuarioNombre", "Test User"),
            new Claim("UsuarioTipo", "Admin")
            });
            var mockPrincipal = new ClaimsPrincipal(mockIdentity);
            mockHttpContext.Setup(c => c.User).Returns(mockPrincipal);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var mockLogger = new Mock<ILogger<FunctionsMails>>();
            var functionsMails = new FunctionsMails(mockContext.Object, mockHttpContextAccessor.Object, mockLogger.Object);

            var mockControllerLogger = new Mock<ILogger<CorreosSalientesController>>();

            var controller = new CorreosSalientesController(
                mockContext.Object,
                mockFunctionsBBDD.Object,
                functionsMails,
                mockControllerLogger.Object,
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.DialogCorreo(Guid.NewGuid()) as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.ViewName);
            Assert.Equal("Error de prueba", result.Model);
        }



        private IHttpContextAccessor CreateMockHttpContextAccessorForMails()
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockIdentity = new ClaimsIdentity(new Claim[]
            {
        new Claim("Empresa", "TestEmpresa"),
        new Claim("Usuario", "TestUser"),
        new Claim("EmpresaNombre", "Test Empresa"),
        new Claim("UsuarioNombre", "Test User"),
        new Claim("UsuarioTipo", "Admin")
            });
            var mockPrincipal = new ClaimsPrincipal(mockIdentity);
            mockHttpContext.Setup(c => c.User).Returns(mockPrincipal);
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
            return mockHttpContextAccessor.Object;
        }

        private FunctionsBBDD CreateFunctionsBBDD(DbContextiLabPlus context)
        {
            return new FunctionsBBDD(context, CreateMockHttpContextAccessorForMails());
        }

        private FunctionsMails CreateFunctionsMails(DbContextiLabPlus context)
        {
            var mockLogger = new Mock<ILogger<FunctionsMails>>();
            return new FunctionsMails(context, CreateMockHttpContextAccessorForMails(), mockLogger.Object);
        }

        [Fact]
        public void BorrarCorreo_CuandoCorreoExiste_RetornaOk()
        {
            // Arrange
            var correoParaBorrar = new CorreosSalientes { Guid = Guid.NewGuid() };
            var data = new List<CorreosSalientes> { correoParaBorrar }.AsQueryable();

            var mockSet = new Mock<DbSet<CorreosSalientes>>();
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.CorreosSalientes).Returns(mockSet.Object);

            var functionsBBDD = CreateFunctionsBBDD(mockContext.Object);
            var functionsMails = CreateFunctionsMails(mockContext.Object);

            var controller = new CorreosSalientesController(
                mockContext.Object,
                functionsBBDD,
                functionsMails,
                Mock.Of<ILogger<CorreosSalientesController>>(),
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.BorrarCorreo(correoParaBorrar.Guid);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        [Fact]
        public void BorrarCorreo_CuandoCorreoNoExiste_RetornaNotFound()
        {
            // Arrange
            var data = new List<CorreosSalientes>().AsQueryable();

            var mockSet = new Mock<DbSet<CorreosSalientes>>();
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.CorreosSalientes).Returns(mockSet.Object);

            var functionsBBDD = CreateFunctionsBBDD(mockContext.Object);
            var functionsMails = CreateFunctionsMails(mockContext.Object);

            var controller = new CorreosSalientesController(
                mockContext.Object,
                functionsBBDD,
                functionsMails,
                Mock.Of<ILogger<CorreosSalientesController>>(),
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.BorrarCorreo(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void BorrarCorreo_CuandoOcurreExcepcion_RetornaStatusCode500()
        {
            // Arrange
            var mockSet = new Mock<DbSet<CorreosSalientes>>();
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Provider).Throws(new Exception("Error de prueba"));

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.CorreosSalientes).Returns(mockSet.Object);

            var functionsBBDD = CreateFunctionsBBDD(mockContext.Object);
            var functionsMails = CreateFunctionsMails(mockContext.Object);

            var controller = new CorreosSalientesController(
                mockContext.Object,
                functionsBBDD,
                functionsMails,
                Mock.Of<ILogger<CorreosSalientesController>>(),
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.BorrarCorreo(Guid.NewGuid());

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Contains("Error de prueba", objectResult.Value.ToString());
        }



        [Fact]
        public void DetalleCorreo_CuandoCorreoExiste_RetornaJsonConDatosCorreo()
        {
            // Arrange
            var correo = new CorreosSalientes
            {
                Guid = Guid.NewGuid(),
                Remitente = "remitente@test.com",
                Destinatario = "destinatario@test.com",
                CCO = "cco@test.com",
                Asunto = "Asunto de prueba",
                FechaEnv = DateTime.Now,
                Cuerpo = "Cuerpo del correo de prueba"
            };
            var data = new List<CorreosSalientes> { correo }.AsQueryable();

            var mockSet = new Mock<DbSet<CorreosSalientes>>();
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.CorreosSalientes).Returns(mockSet.Object);

            var functionsBBDD = CreateFunctionsBBDD(mockContext.Object);
            var functionsMails = CreateFunctionsMails(mockContext.Object);

            var controller = new CorreosSalientesController(
                mockContext.Object,
                functionsBBDD,
                functionsMails,
                Mock.Of<ILogger<CorreosSalientesController>>(),
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.DetalleCorreo(correo.Guid) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.True(jsonData["success"].Value<bool>());
            Assert.Equal(correo.Remitente, jsonData["remitente"].Value<string>());
            Assert.Equal(correo.Destinatario, jsonData["destinatarios"].Value<string>());
            Assert.Equal(correo.CCO, jsonData["CCO"].Value<string>());
            Assert.Equal(correo.Asunto, jsonData["asunto"].Value<string>());
            Assert.Equal(correo.FechaEnv.ToString("dd/MM/yyyy HH:mm"), jsonData["fechaEnvio"].Value<string>());
            Assert.Equal(correo.Cuerpo, jsonData["contenido"].Value<string>());
        }


        [Fact]
        public void DetalleCorreo_CuandoCorreoNoExiste_RetornaJsonConMensajeDeError()
        {
            // Arrange
            var data = new List<CorreosSalientes>().AsQueryable();

            var mockSet = new Mock<DbSet<CorreosSalientes>>();
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.CorreosSalientes).Returns(mockSet.Object);

            var functionsBBDD = CreateFunctionsBBDD(mockContext.Object);
            var functionsMails = CreateFunctionsMails(mockContext.Object);

            var controller = new CorreosSalientesController(
                mockContext.Object,
                functionsBBDD,
                functionsMails,
                Mock.Of<ILogger<CorreosSalientesController>>(),
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.DetalleCorreo(Guid.NewGuid()) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.False(jsonData["success"].Value<bool>());
            Assert.Equal("Correo no encontrado", jsonData["message"].Value<string>());
        }



        [Fact]
        public void DetalleCorreo_CuandoOcurreExcepcion_RetornaJsonConMensajeDeError()
        {
            // Arrange
            var mockSet = new Mock<DbSet<CorreosSalientes>>();
            mockSet.As<IQueryable<CorreosSalientes>>().Setup(m => m.Provider).Throws(new Exception("Error de prueba"));

            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.CorreosSalientes).Returns(mockSet.Object);

            var functionsBBDD = CreateFunctionsBBDD(mockContext.Object);
            var functionsMails = CreateFunctionsMails(mockContext.Object);

            var controller = new CorreosSalientesController(
                mockContext.Object,
                functionsBBDD,
                functionsMails,
                Mock.Of<ILogger<CorreosSalientesController>>(),
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.DetalleCorreo(Guid.NewGuid()) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.False(jsonData["success"].Value<bool>());
            Assert.Contains("Error de prueba", jsonData["message"].Value<string>());
        }


        [Fact]
        public void CreateMail_CuandoSeCreaCorrectamente_RetornaStatusCode200()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();

            var correosSalientes = new List<CorreosSalientes>();
            var mockSetCorreosSalientes = new Mock<DbSet<CorreosSalientes>>();
            mockSetCorreosSalientes.As<IQueryable<CorreosSalientes>>().Setup(m => m.Provider).Returns(correosSalientes.AsQueryable().Provider);
            mockSetCorreosSalientes.As<IQueryable<CorreosSalientes>>().Setup(m => m.Expression).Returns(correosSalientes.AsQueryable().Expression);
            mockSetCorreosSalientes.As<IQueryable<CorreosSalientes>>().Setup(m => m.ElementType).Returns(correosSalientes.AsQueryable().ElementType);
            mockSetCorreosSalientes.As<IQueryable<CorreosSalientes>>().Setup(m => m.GetEnumerator()).Returns(correosSalientes.GetEnumerator());
            mockSetCorreosSalientes.Setup(m => m.Add(It.IsAny<CorreosSalientes>())).Callback<CorreosSalientes>((s) => correosSalientes.Add(s));

            var correosSalientesAdj = new List<CorreosSalientesAdj>();
            var mockSetCorreosSalientesAdj = new Mock<DbSet<CorreosSalientesAdj>>();
            mockSetCorreosSalientesAdj.As<IQueryable<CorreosSalientesAdj>>().Setup(m => m.Provider).Returns(correosSalientesAdj.AsQueryable().Provider);
            mockSetCorreosSalientesAdj.As<IQueryable<CorreosSalientesAdj>>().Setup(m => m.Expression).Returns(correosSalientesAdj.AsQueryable().Expression);
            mockSetCorreosSalientesAdj.As<IQueryable<CorreosSalientesAdj>>().Setup(m => m.ElementType).Returns(correosSalientesAdj.AsQueryable().ElementType);
            mockSetCorreosSalientesAdj.As<IQueryable<CorreosSalientesAdj>>().Setup(m => m.GetEnumerator()).Returns(correosSalientesAdj.GetEnumerator());
            mockSetCorreosSalientesAdj.Setup(m => m.Add(It.IsAny<CorreosSalientesAdj>())).Callback<CorreosSalientesAdj>((s) => correosSalientesAdj.Add(s));

            mockContext.Setup(c => c.CorreosSalientes).Returns(mockSetCorreosSalientes.Object);
            mockContext.Setup(c => c.CorreosSalientesAdj).Returns(mockSetCorreosSalientesAdj.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, CreateMockHttpContextAccessor());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa", SessionUsuario = "TestUser" });

            var mockFunctionsMails = new Mock<FunctionsMails>(mockContext.Object, CreateMockHttpContextAccessor(), Mock.Of<ILogger<FunctionsMails>>());
            mockFunctionsMails.Setup(f => f.MailSend(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<IFormFile>>()))
                .Returns("TestMessageId");

            var mockLogger = new Mock<ILogger<CorreosSalientesController>>();

            var controller = new CorreosSalientesController(
                mockContext.Object,
                mockFunctionsBBDD.Object,
                mockFunctionsMails.Object,
                mockLogger.Object,
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.CreateMail("remitente@test.com", "destinatario@test.com", "cco@test.com", "Asunto de prueba", "Cuerpo de prueba", new List<IFormFile>()) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.IsType<CorreosSalientes>(result.Value);
            var correoCreado = result.Value as CorreosSalientes;
            Assert.Equal("TestEmpresa", correoCreado.Empresa);
            Assert.Equal("Env.", correoCreado.Estado);
            Assert.Equal("TestMessageId", correoCreado.MessageId);

            mockSetCorreosSalientes.Verify(m => m.Add(It.IsAny<CorreosSalientes>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Exactly(2));
        }


        [Fact]
        public void CreateMail_ConAdjuntos_GuardaAdjuntosCorrectamente()
        {
            try { 
                // Arrange
                var mockContext = new Mock<DbContextiLabPlus>();

                // Setup for CorreosSalientes
                var dataCorreos = new List<CorreosSalientes>().AsQueryable();
                var mockSetCorreos = new Mock<DbSet<CorreosSalientes>>();
                mockSetCorreos.As<IQueryable<CorreosSalientes>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<CorreosSalientes>(dataCorreos.Provider));
                mockSetCorreos.As<IQueryable<CorreosSalientes>>().Setup(m => m.Expression).Returns(dataCorreos.Expression);
                mockSetCorreos.As<IQueryable<CorreosSalientes>>().Setup(m => m.ElementType).Returns(dataCorreos.ElementType);
                mockSetCorreos.As<IQueryable<CorreosSalientes>>().Setup(m => m.GetEnumerator()).Returns(dataCorreos.GetEnumerator());
                mockContext.Setup(c => c.CorreosSalientes).Returns(mockSetCorreos.Object);

                // Setup for CorreosSalientesAdj
                var adjuntosList = new List<CorreosSalientesAdj>();
                var mockSetAdjuntos = new Mock<DbSet<CorreosSalientesAdj>>();
                mockSetAdjuntos.As<IQueryable<CorreosSalientesAdj>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<CorreosSalientesAdj>(adjuntosList.AsQueryable().Provider));
                mockSetAdjuntos.As<IQueryable<CorreosSalientesAdj>>().Setup(m => m.Expression).Returns(adjuntosList.AsQueryable().Expression);
                mockSetAdjuntos.As<IQueryable<CorreosSalientesAdj>>().Setup(m => m.ElementType).Returns(adjuntosList.AsQueryable().ElementType);
                mockSetAdjuntos.As<IQueryable<CorreosSalientesAdj>>().Setup(m => m.GetEnumerator()).Returns(() => adjuntosList.GetEnumerator());
                mockSetAdjuntos.Setup(m => m.Add(It.IsAny<CorreosSalientesAdj>()))
                    .Callback<CorreosSalientesAdj>((s) => adjuntosList.Add(s));
                mockContext.Setup(c => c.CorreosSalientesAdj).Returns(mockSetAdjuntos.Object);

                var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, CreateMockHttpContextAccessor());
                mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa", SessionUsuario = "TestUser" });

                var mockLogger = new Mock<ILogger<FunctionsMails>>();
                var testFunctionsMails = new TestFunctionsMails(mockContext.Object, CreateMockHttpContextAccessor(), mockLogger.Object);

                var controllerLogger = new Mock<ILogger<CorreosSalientesController>>();

                // Crear un sistema de archivos simulado
                var mockFileSystem = new MockFileSystem();

                _output.WriteLine("Creando controller");
                var controller = new CorreosSalientesController(
                    mockContext.Object,
                    mockFunctionsBBDD.Object,
                    testFunctionsMails,
                    controllerLogger.Object,
                    mockFileSystem,  // Agregar el sistema de archivos simulado
                    _output
                );
                _output.WriteLine("Controller creado");


                _output.WriteLine("Configurando mockFile");
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.FileName).Returns("test.txt");
                mockFile.Setup(f => f.Length).Returns(1024);
                _output.WriteLine("mockFile configurado");

                // Configurar el mock para simular la creación de archivos
                mockFileSystem.AddDirectory(@"C:\Nemesis365Docs\TestEmpresa\MailsFicAdjuntos");

                // Act
                _output.WriteLine("Llamando a CreateMail");
                var result = controller.CreateMail("remitente@test.com", "destinatario@test.com", "cco@test.com", "Asunto de prueba", "Cuerpo de prueba", new List<IFormFile> { mockFile.Object }) as ObjectResult;
                _output.WriteLine("CreateMail llamado");

                // Assert
                Assert.NotNull(result);
                _output.WriteLine($"Status Code: {result.StatusCode}");
                _output.WriteLine($"Result Value: {result.Value}");

                Assert.Equal(200, result.StatusCode);
                mockSetAdjuntos.Verify(m => m.Add(It.IsAny<CorreosSalientesAdj>()), Times.Once());

                var correoSaliente = result.Value as CorreosSalientes;
                Assert.NotNull(correoSaliente);
                Assert.NotNull(correoSaliente.MessageId);
                Assert.NotEmpty(correoSaliente.Adjuntos);
                Assert.Single(correoSaliente.Adjuntos);
                Assert.Equal("test.txt", correoSaliente.Adjuntos[0].NombreArchivo);

                // Verificar que se haya "creado" el archivo en el sistema de archivos simulado
                Assert.True(mockFileSystem.FileExists(correoSaliente.Adjuntos[0].RutaArchivo));
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Excepción: {ex.Message}");
                    _output.WriteLine($"StackTrace: {ex.StackTrace}");
                    throw;
                }
        }


        [Fact]
        public void CreateMail_CuandoOcurreError_RetornaStatusCode500()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();

            // Simulamos que CorreosSalientes es null para provocar una NullReferenceException
            mockContext.Setup(m => m.CorreosSalientes).Returns((DbSet<CorreosSalientes>)null);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, CreateMockHttpContextAccessor());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa", SessionUsuario = "TestUser" });

            var mockFunctionsMails = new Mock<FunctionsMails>(mockContext.Object, CreateMockHttpContextAccessor(), Mock.Of<ILogger<FunctionsMails>>());

            var mockLogger = new Mock<ILogger<CorreosSalientesController>>();

            var controller = new CorreosSalientesController(
                mockContext.Object,
                mockFunctionsBBDD.Object,
                mockFunctionsMails.Object,
                mockLogger.Object,
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.CreateMail("remitente@test.com", "destinatario@test.com", "cco@test.com", "Asunto de prueba", "Cuerpo de prueba", new List<IFormFile>()) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("Object reference not set to an instance of an object", result.Value.ToString());
        }



        [Fact]
        public void GuardarArchivo_ArchivoNulo_RetornaNulo()
        {
            // Arrange
            var controller = new CorreosSalientesController(
                Mock.Of<DbContextiLabPlus>(),
                _mockFunctionsBBDD.Object,
                null,
                _mockLogger.Object,
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.PublicGuardarArchivo(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GuardarArchivo_ArchivoVacio_RetornaNulo()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);
            var controller = new CorreosSalientesController(
                Mock.Of<DbContextiLabPlus>(),
                _mockFunctionsBBDD.Object,
                null,
                _mockLogger.Object,
                _mockFileSystem,
                _output
            );

            // Act
            var result = controller.PublicGuardarArchivo(mockFile.Object);

            // Assert
            Assert.Null(result);
        }



        [Fact]
        public void GuardarArchivo_ArchivoValido_GuardaYRetornaRuta()
        {
            try
            {
                _output.WriteLine("Iniciando test GuardarArchivo_ArchivoValido_GuardaYRetornaRuta");

                _output.WriteLine("Configurando mockFile");
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(1024);
                mockFile.Setup(f => f.FileName).Returns("test.txt");
                mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
                _output.WriteLine("Mock de IFormFile creado");

                _output.WriteLine("Configurando mockFileSystem");
                _mockFileSystem.AddDirectory(@"C:\Nemesis365Docs\TestEmpresa\MailsFicAdjuntos");
                _mockFileSystem.Directory.SetCurrentDirectory(@"C:\");
                _output.WriteLine("mockFileSystem configurado");

                _output.WriteLine("Creando CorreosSalientesController");

                // Asegurarse de que GrupoClaims.SessionEmpresa tenga un valor
                _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

                var controller = new CorreosSalientesController(
                    Mock.Of<DbContextiLabPlus>(),
                    _mockFunctionsBBDD.Object,
                    null,
                    _mockLogger.Object,
                    _mockFileSystem,
                    _output
                );
                _output.WriteLine("CorreosSalientesController creado");

                _output.WriteLine("Ejecutando PublicGuardarArchivo");
                var result = controller.PublicGuardarArchivo(mockFile.Object);
                _output.WriteLine($"PublicGuardarArchivo ejecutado, resultado: {result ?? "null"}");

                // Assert
                Assert.NotNull(result);
            _output.WriteLine($"Resultado no es null: {result}");
            Assert.Contains("TestEmpresa", result);
            _output.WriteLine("Resultado contiene 'TestEmpresa'");
            Assert.Contains("MailsFicAdjuntos", result);
            _output.WriteLine("Resultado contiene 'MailsFicAdjuntos'");
            Assert.EndsWith(".txt", result);
            _output.WriteLine("Resultado termina con '.txt'");
            Assert.True(_mockFileSystem.File.Exists(result));
            _output.WriteLine("Archivo existe en el sistema de archivos simulado");

            _output.WriteLine("Test completado exitosamente");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error durante la ejecución del test: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }



    }
}