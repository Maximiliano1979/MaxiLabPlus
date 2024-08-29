using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using iLabPlus.Controllers;
using iLabPlus.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using iLabPlus.Models.BDiLabPlus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;
using System.Security.Cryptography;
using System.Text;
using AccountController.Controllers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Threading.Tasks;
using System;
using System.IO.Abstractions; 
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using static iLabPlus.Tests.ClientesControllerTests;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;
using Xunit.Sdk;
using iLabPlus.Models.Clases;




namespace iLabPlus.Tests
{


    public class TestableAccountController : AccountController.Controllers.AccountController
    {
        public bool CheckUserCalled { get; private set; }
        public LoginModel CheckUserLoginModel { get; private set; }
        public Usuarios CheckUserLookupUser { get; private set; }

        public TestableAccountController(
            DbContextiLabPlus context,
            IWebHostEnvironment ienvironment,
            IHttpContextAccessor ihttpContext,
            IFileSystem fileSystem,
            Notifications notifications,
            ITestOutputHelper testOutputHelper = null)
            : base(context, ienvironment, ihttpContext, fileSystem, notifications, testOutputHelper)
        {
        }

        public virtual void CheckUser(LoginModel LoginUser, Usuarios LookupUser)
        {
            CheckUserCalled = true;
            CheckUserLoginModel = LoginUser;
            CheckUserLookupUser = LookupUser;
            base.CheckUser(LoginUser, LookupUser);
        }
    }



    public class AccountControllerTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<DbContextiLabPlus> _mockContext;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<Notifications> _mockNotifications;
        private readonly AccountController.Controllers.AccountController _controller;

        public AccountControllerTests(ITestOutputHelper output)
        {
            _output = output;
            _mockContext = new Mock<DbContextiLabPlus>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockNotifications = new Mock<Notifications>();


            // Configuración básica del mockFileSystem
            _mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(true);
            _mockFileSystem.Setup(fs => fs.File.OpenText(It.IsAny<string>()))
                .Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("Mock email template content"))));

            // Configuración básica del mockNotifications
            _mockNotifications.Setup(n => n.EmailNotification(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("200");

            _controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                output);
        }



        [Fact]
        public async Task Login_ConContrasenaValida_YControllerInitVacio_DebeRedirigirAHome()
        {
            // Arrange
            _output.WriteLine("Iniciando test Login_ConContrasenaValida_YControllerInitVacio_DebeRedirigirAHome");

            var loginUser = new LoginModel { Usuario = "test@example.com", Password = "password" };
            var lookupUser = new Usuarios
            {
                Usuario = "test@example.com",
                Password = "$2a$11$QYk3tEn0uyycGPxD3RlJUew3G7PVKLzjkLlZpqCnZf8yMRQ7jkZYa", // BCrypt hash for "password"
                ControllerInit = string.Empty
            };

            var mockSet = new Mock<DbSet<Usuarios>>();
            var data = new List<Usuarios> { lookupUser }.AsQueryable();

            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Usuarios>(data.Provider));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            mockSet.As<IAsyncEnumerable<Usuarios>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Usuarios>(data.GetEnumerator()));

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            // Configurar FunctionsCrypto para que la verificación de contraseña sea exitosa
            FunctionsCrypto.VerifyPasswordImplementation = (password, hashedPassword) => true;

            // Configurar HttpContext y servicios mock
            var mockHttpContext = new Mock<HttpContext>();
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService
                .Setup(x => x.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var mockTempDataDictionaryFactory = new Mock<ITempDataDictionaryFactory>();
            mockTempDataDictionaryFactory
                .Setup(x => x.GetTempData(It.IsAny<HttpContext>()))
                .Returns(new TempDataDictionary(mockHttpContext.Object, Mock.Of<ITempDataProvider>()));

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("http://localhost/Home/Index");

            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            mockUrlHelperFactory
                .Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(mockUrlHelper.Object);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthService.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(ITempDataDictionaryFactory)))
                .Returns(mockTempDataDictionaryFactory.Object);
            serviceProvider
                .Setup(x => x.GetService(typeof(IUrlHelperFactory)))
                .Returns(mockUrlHelperFactory.Object);

            mockHttpContext.Setup(x => x.RequestServices).Returns(serviceProvider.Object);

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            controller.Url = mockUrlHelper.Object;

            // Configurar ModelState para que sea válido
            controller.ModelState.Clear();

            _output.WriteLine("Configuración completada. Ejecutando Login...");

            // Act
            var result = await controller.Login(loginUser);

            // Assert
            _output.WriteLine($"Tipo de resultado: {result.GetType().Name}");

            if (result is ViewResult viewResult)
            {
                _output.WriteLine($"Returned ViewResult. ViewName: {viewResult.ViewName}");
                _output.WriteLine($"Model: {viewResult.Model}");
                _output.WriteLine($"ModelState.IsValid: {controller.ModelState.IsValid}");
                foreach (var modelState in controller.ModelState)
                {
                    _output.WriteLine($"Key: {modelState.Key}, Errors: {string.Join(", ", modelState.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                Assert.True(false, "Expected RedirectToActionResult, but got ViewResult");
            }
            else if (result is RedirectToActionResult redirectResult)
            {
                _output.WriteLine($"ActionName: {redirectResult.ActionName}, ControllerName: {redirectResult.ControllerName}");
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Equal("Home", redirectResult.ControllerName);
            }
            else
            {
                Assert.True(false, $"Unexpected result type: {result.GetType()}");
            }
        }



        [Fact]
        public void Login_DeberiaRetornarVistaLogin_CuandoSeAccedePorGET()
        {
            // Arrange
            var actionDescriptor = new ControllerActionDescriptor
            {
                ControllerName = "Account",
                ActionName = "Login"
            };

            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), actionDescriptor);
            _controller.ControllerContext = new ControllerContext(actionContext);

            // Act
            var result = _controller.Login() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ViewName);
            Assert.IsType<LoginModel>(result.Model);
            var model = result.Model as LoginModel;
            Assert.Equal("", model.Usuario);
            Assert.Equal("", model.Password);
        }




        [Fact]
        public void Logout_DeberiaRedirigirALogin_CuandoSeLlamaAlMetodo()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns("http://testurl.com");

            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelperMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactoryMock.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object
            };

            var actionDescriptor = new ControllerActionDescriptor
            {
                ControllerName = "Account",
                ActionName = "Logout"
            };

            var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);
            _controller.ControllerContext = new ControllerContext(actionContext);

            // Act
            var result = _controller.Logout() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }



        [Fact]
        public void ResetPassword_DeberiaRetornarVistaNewPassword_CuandoTokenEsValido()
        {
            // Arrange
            var userToken = "validtoken";
            var usuario = new Usuarios
            {
                tokenid = userToken,
                tokenexpiredutc = DateTime.UtcNow.AddMinutes(10),
                UsuarioNombre = "Test User"
            };
            var usuarios = new List<Usuarios> { usuario }.AsQueryable();

            var mockSet = new Mock<DbSet<Usuarios>>();
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(usuarios.Provider);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(usuarios.Expression);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(usuarios.ElementType);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(usuarios.GetEnumerator());

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            // Act
            var result = _controller.ResetPassword(userToken) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NewPassword", result.ViewName);
            Assert.Equal("Test User", result.ViewData["username"]);
        }

        [Fact]
        public void ChangePassword_DeberiaRetornarVistaChangePassword()
        {
            // Act
            var result = _controller.ChangePassword() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ChangePassword", result.ViewName);
        }



        [Fact]
        public async Task SendPasswordResetLink_ReturnsOkForValidUser()
        {
            _output.WriteLine("Iniciando test SendPasswordResetLink_ReturnsOkForValidUser");

            try
            {
                // Arrange
                var mockContext = new Mock<DbContextiLabPlus>();
                var mockEnvironment = new Mock<IWebHostEnvironment>();
                var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                var mockFileSystem = new Mock<IFileSystem>();

                _output.WriteLine("Configurando mock de DbContext para Usuarios");
                var usuarios = new List<Usuarios>
                {
                    new Usuarios { Usuario = "test@example.com" }
                }.AsQueryable();

                var mockSet = new Mock<DbSet<Usuarios>>();
                mockSet.As<IAsyncEnumerable<Usuarios>>()
                    .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                    .Returns(new TestAsyncEnumerator<Usuarios>(usuarios.GetEnumerator()));

                mockSet.As<IQueryable<Usuarios>>()
                    .Setup(m => m.Provider)
                    .Returns(new TestAsyncQueryProvider<Usuarios>(usuarios.Provider));

                mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(usuarios.Expression);
                mockSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(usuarios.ElementType);
                mockSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(usuarios.GetEnumerator());

                mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

                _output.WriteLine("Configurando mock de DbContext para ValSys");
                var valSys = new List<ValSys>
                {
                    new ValSys { Empresa = "iLabPlus", Clave = "Smtp", Valor1 = "test@email.com", Valor2 = "password", Valor3 = "smtp.test.com", Valor4 = 587m }
                }.AsQueryable();

                var mockValSysSet = new Mock<DbSet<ValSys>>();
                mockValSysSet.As<IAsyncEnumerable<ValSys>>()
                    .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                    .Returns(new TestAsyncEnumerator<ValSys>(valSys.GetEnumerator()));

                mockValSysSet.As<IQueryable<ValSys>>()
                    .Setup(m => m.Provider)
                    .Returns(new TestAsyncQueryProvider<ValSys>(valSys.Provider));

                mockValSysSet.As<IQueryable<ValSys>>().Setup(m => m.Expression).Returns(valSys.Expression);
                mockValSysSet.As<IQueryable<ValSys>>().Setup(m => m.ElementType).Returns(valSys.ElementType);
                mockValSysSet.As<IQueryable<ValSys>>().Setup(m => m.GetEnumerator()).Returns(valSys.GetEnumerator());

                mockContext.Setup(c => c.ValSys).Returns(mockValSysSet.Object);

                _output.WriteLine("Configurando mock de IWebHostEnvironment");
                mockEnvironment.Setup(e => e.WebRootPath).Returns("testpath");

                _output.WriteLine("Configurando mock de HttpContext");
                var mockHttpContext = new Mock<HttpContext>();
                var mockRequest = new Mock<HttpRequest>();
                mockRequest.Setup(r => r.Scheme).Returns("https");
                mockRequest.Setup(r => r.Host).Returns(new HostString("example.com"));
                mockRequest.Setup(r => r.PathBase).Returns(new PathString(""));
                mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
                mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

                _output.WriteLine("Configurando mock de FileSystem");
                mockFileSystem.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns(true);
                mockFileSystem.Setup(fs => fs.File.OpenText(It.IsAny<string>())).Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("Mock email template content"))));

                _output.WriteLine("Configurando mock de Notifications");
                var mockNotifications = new Mock<Notifications>();
                mockNotifications.Setup(n => n.EmailNotification(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync("200");

                _output.WriteLine("Creando instancia del controlador");
                var controller = new AccountController.Controllers.AccountController(
                    mockContext.Object,
                    mockEnvironment.Object,
                    mockHttpContextAccessor.Object,
                    mockFileSystem.Object,
                    mockNotifications.Object, // Pasando el mock de Notifications
                    _output)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext()
                    }
                };

                if (controller == null)
                {
                    _output.WriteLine("Error: El controlador es null");
                    Assert.Fail("El controlador es null");
                    return;
                }

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

            

                // Act
                _output.WriteLine("Llamando al método SendPasswordResetLink");
                var result = await controller.SendPasswordResetLink("test@example.com");

                // Assert
                _output.WriteLine($"Tipo de resultado: {result?.GetType().FullName ?? "null"}");

                if (result is ObjectResult objectResult)
                {
                    _output.WriteLine($"StatusCode: {objectResult.StatusCode}");
                    _output.WriteLine($"Value: {objectResult.Value}");
                    Assert.Equal(200, objectResult.StatusCode);
                }
                else if (result is StatusCodeResult statusCodeResult)
                {
                    _output.WriteLine($"StatusCode: {statusCodeResult.StatusCode}");
                    Assert.Equal(200, statusCodeResult.StatusCode);
                }
                else
                {
                    _output.WriteLine($"Tipo de resultado inesperado: {result?.GetType().FullName ?? "null"}");
                    Assert.True(false, $"Tipo de resultado inesperado: {result?.GetType().FullName ?? "null"}");
                }

                _output.WriteLine("Test completado");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Excepción no controlada: {ex.GetType().Name}");
                _output.WriteLine($"Mensaje: {ex.Message}");
                _output.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }



        [Fact]
        public async Task SendPasswordResetLink_ReturnsErrorWhenExceptionOccursDuringUserSearch()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Usuarios>>();
            mockSet.As<IQueryable<Usuarios>>()
                .Setup(m => m.Provider)
                .Throws(new Exception("Error de base de datos"));

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);

            // Act
            var result = await controller.SendPasswordResetLink("test@example.com");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Error de base de datos", statusCodeResult.Value);
        }




        private Mock<DbSet<T>> MockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

            return mockSet;
        }





        [Fact]
        public async Task SendPasswordResetLink_ReturnsErrorWhenEmailConfigNotFound()
        {
            // Configurar el mock del DbContext
            var options = new DbContextOptionsBuilder<DbContextiLabPlus>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var context = new DbContextiLabPlus(options);

            // Agregar un usuario de prueba
            context.Usuarios.Add(new Usuarios { Usuario = "test@example.com" });
            await context.SaveChangesAsync();

            // Configurar los mocks necesarios
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(mockHttpContext);
            var mockFileSystem = new Mock<IFileSystem>();
            var notifications = new Mock<Notifications>();

            // Crear el controlador
            var controller = new AccountController.Controllers.AccountController(
                context,
                mockEnvironment.Object,
                mockHttpContextAccessor.Object,
                mockFileSystem.Object,
                notifications.Object,
                _output);

            // Actuar
            var result = await controller.SendPasswordResetLink("test@example.com");

            // Afirmar
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Configuración de correo no encontrada", objectResult.Value);
        }



        [Fact]
        public async Task SendPasswordResetLink_ReturnsNotFoundWhenUserDoesNotExist()
        {
            // Arrange
            var emptyUsuarios = new List<Usuarios>().AsQueryable();
            var mockSet = MockDbSet(emptyUsuarios);
            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);

            // Act
            var result = await controller.SendPasswordResetLink("nonexistent@example.com");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, statusCodeResult.StatusCode);
            Assert.Equal("Usuario no encontrado", statusCodeResult.Value);
        }


        [Fact]
        public void ResetPassword_ReturnsLinkErrorView_WhenTokenExpired()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockDbSet = new Mock<DbSet<Usuarios>>();
            var expiredToken = "expiredToken";
            var testUser = new Usuarios
            {
                tokenid = expiredToken,
                tokenexpiredutc = DateTime.UtcNow.AddMinutes(-5) // Token expirado hace 5 minutos
            };

            var queryable = new List<Usuarios> { testUser }.AsQueryable();
            mockDbSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            mockContext.Setup(c => c.Usuarios).Returns(mockDbSet.Object);

            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockFileSystem = new Mock<IFileSystem>();
            var notifications = new Mock<Notifications>();
            var testOutputHelper = new Mock<ITestOutputHelper>();

            var controller = new AccountController.Controllers.AccountController(
                mockContext.Object,
                mockEnvironment.Object,
                mockHttpContextAccessor.Object,
                mockFileSystem.Object,
                notifications.Object,
                testOutputHelper.Object);

            // Act
            var result = controller.ResetPassword(expiredToken);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("LinkError", viewResult.ViewName);
        }



        [Fact]
        public void ResetPassword_ReturnsLinkErrorView_WhenTokenNotFound()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockDbSet = new Mock<DbSet<Usuarios>>();
            var nonExistentToken = "nonExistentToken";

            var queryable = new List<Usuarios>().AsQueryable(); // Lista vacía
            mockDbSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            mockContext.Setup(c => c.Usuarios).Returns(mockDbSet.Object);

            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockFileSystem = new Mock<IFileSystem>();
            var notifications = new Mock<Notifications>();
            var testOutputHelper = new Mock<ITestOutputHelper>();

            var controller = new AccountController.Controllers.AccountController(
                mockContext.Object,
                mockEnvironment.Object,
                mockHttpContextAccessor.Object,
                mockFileSystem.Object,
                notifications.Object,
                testOutputHelper.Object);

            // Act
            var result = controller.ResetPassword(nonExistentToken);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("LinkError", viewResult.ViewName);
        }



        [Fact]
        public async Task RenewPassword_ReturnsStatusCode400_WhenInputIsEmpty()
        {
            // Arrange
            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);

            // Act
            var result = await controller.RenewPassword("", "");

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }



        [Fact]
        public async Task RenewPassword_ReturnsStatusCode400_WhenUserNotFound()
        {
            // Arrange
            var data = new List<Usuarios>().AsQueryable();
            var mockSet = new Mock<DbSet<Usuarios>>();
            mockSet.As<IAsyncEnumerable<Usuarios>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Usuarios>(data.GetEnumerator()));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Usuarios>(data.Provider));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);

            // Act
            var result = await controller.RenewPassword("test@example.com", "newpassword");

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task RenewPassword_ReturnsStatusCode200_WhenPasswordUpdatedSuccessfully()
        {
            // Arrange
            var testUser = new Usuarios { Usuario = "test@example.com" };
            var data = new List<Usuarios> { testUser }.AsQueryable();
            var mockSet = new Mock<DbSet<Usuarios>>();
            mockSet.As<IAsyncEnumerable<Usuarios>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Usuarios>(data.GetEnumerator()));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Usuarios>(data.Provider));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);

            // Act
            var result = await controller.RenewPassword("test@example.com", "newpassword");

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(200, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task RenewPassword_ReturnsStatusCode500_WhenExceptionOccurs()
        {
            // Arrange
            var testUser = new Usuarios { Usuario = "test@example.com" };
            var data = new List<Usuarios> { testUser }.AsQueryable();
            var mockSet = new Mock<DbSet<Usuarios>>();
            mockSet.As<IAsyncEnumerable<Usuarios>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Usuarios>(data.GetEnumerator()));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Usuarios>(data.Provider));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);

            // Act
            var result = await controller.RenewPassword("test@example.com", "newpassword");

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }



        [Fact]
        public void CheckUser_AddsClaimsCorrectly()
        {
            // Arrange
            var loginUser = new LoginModel { Usuario = "test@example.com", Recordar = true };
            var lookupUser = new Usuarios
            {
                Usuario = "test@example.com",
                Empresa = "TestCompany",
                UsuarioNombre = "Test User",
                UsuarioTipo = "Admin"
            };

            var mockHttpContext = new Mock<HttpContext>();
            var mockAuthenticationService = new Mock<IAuthenticationService>();
            mockAuthenticationService
                .Setup(x => x.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthenticationService.Object);

            mockHttpContext.Setup(x => x.RequestServices).Returns(serviceProvider.Object);

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            // Mock Empresas DbSet
            var mockEmpresasSet = new Mock<DbSet<Empresas>>();
            var empresas = new List<Empresas>
    {
        new Empresas { Empresa = "TestCompany", Nombre = "Test Company Name" }
    }.AsQueryable();

            mockEmpresasSet.As<IQueryable<Empresas>>().Setup(m => m.Provider).Returns(empresas.Provider);
            mockEmpresasSet.As<IQueryable<Empresas>>().Setup(m => m.Expression).Returns(empresas.Expression);
            mockEmpresasSet.As<IQueryable<Empresas>>().Setup(m => m.ElementType).Returns(empresas.ElementType);
            mockEmpresasSet.As<IQueryable<Empresas>>().Setup(m => m.GetEnumerator()).Returns(empresas.GetEnumerator());

            _mockContext.Setup(c => c.Empresas).Returns(mockEmpresasSet.Object);

            // Act
            controller.CheckUser(loginUser, lookupUser);

            // Assert
            mockAuthenticationService.Verify(x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(s => s == CookieAuthenticationDefaults.AuthenticationScheme),
                It.Is<ClaimsPrincipal>(p =>
                    p.HasClaim(c => c.Type == ClaimTypes.Name && c.Value == "test@example.com") &&
                    p.HasClaim(c => c.Type == "Empresa" && c.Value == "TestCompany") &&
                    p.HasClaim(c => c.Type == "Usuario" && c.Value == "test@example.com") &&
                    p.HasClaim(c => c.Type == "UsuarioNombre" && c.Value == "Test User") &&
                    p.HasClaim(c => c.Type == "UsuarioTipo" && c.Value == "Admin") &&
                    p.HasClaim(c => c.Type == "EmpresaNombre" && c.Value == "Test Company Name")
                ),
                It.Is<AuthenticationProperties>(ap => ap.IsPersistent == true)
            ), Times.Once);
        }



        [Fact]
        public async Task Login_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);
            controller.ModelState.AddModelError("", "Test error");
            var loginModel = new LoginModel { Usuario = "test@example.com", Password = "password" };

            // Act
            var result = await controller.Login(loginModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Login", viewResult.ViewName);
            Assert.IsType<LoginModel>(viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Login_ReturnsViewResult_WhenUserNotFound()
        {
            // Arrange
            var data = new List<Usuarios>().AsQueryable();
            var mockSet = new Mock<DbSet<Usuarios>>();
            mockSet.As<IAsyncEnumerable<Usuarios>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Usuarios>(data.GetEnumerator()));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Usuarios>(data.Provider));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);
            var loginModel = new LoginModel { Usuario = "test@example.com", Password = "password" };

            // Act
            var result = await controller.Login(loginModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Login", viewResult.ViewName);
            Assert.IsType<LoginModel>(viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState.Values, v => v.Errors.Any(e => e.ErrorMessage == "Cuenta desactivada. Pongase en contacto con soporte técnico."));
        }



        [Fact]
        public async Task Login_ReturnsViewResult_WhenPasswordIsInvalid()
        {
            // Arrange
            var testUser = new Usuarios { Usuario = "test@example.com", Password = "$2a$11$QYk3tEn0uyycGPxD3RlJUew3G7PVKLzjkLlZpqCnZf8yMRQ7jkZYa" }; // BCrypt hash for "password"
            var data = new List<Usuarios> { testUser }.AsQueryable();
            var mockSet = new Mock<DbSet<Usuarios>>();
            mockSet.As<IAsyncEnumerable<Usuarios>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Usuarios>(data.GetEnumerator()));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Usuarios>(data.Provider));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            // Asegúrate de que FunctionsCrypto.VerifyPassword devuelve false para una contraseña incorrecta
            FunctionsCrypto.VerifyPasswordImplementation = (password, hashedPassword) => false;

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);
            var loginModel = new LoginModel { Usuario = "test@example.com", Password = "wrongpassword" };

            // Act
            var result = await controller.Login(loginModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Login", viewResult.ViewName);
            Assert.IsType<LoginModel>(viewResult.Model);
            Assert.False(controller.ModelState.IsValid);

            // Verifica que haya un error en ModelState
            Assert.True(controller.ModelState.ErrorCount > 0);

            // Obtiene el mensaje de error real
            var errorMessage = controller.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault();

            // Imprime el mensaje de error para depuración
            _output.WriteLine($"Error message: {errorMessage}");

            // Verifica que el mensaje de error contenga ciertas palabras clave
            Assert.Contains("Datos de acceso incorrectos", errorMessage);

            // Restaura el comportamiento original de VerifyPassword
            FunctionsCrypto.VerifyPasswordImplementation = BCrypt.Net.BCrypt.Verify;
        }



        [Fact]
        public async Task SendPasswordResetLink_ReturnsStatusCode500_WhenEmailSendingFails()
        {
            // Arrange
            var testUser = new Usuarios { Usuario = "test@example.com" };
            var data = new List<Usuarios> { testUser }.AsQueryable();
            var mockSet = new Mock<DbSet<Usuarios>>();
            mockSet.As<IAsyncEnumerable<Usuarios>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Usuarios>(data.GetEnumerator()));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Usuarios>(data.Provider));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            var mockValSys = new Mock<DbSet<ValSys>>();
            var valSysData = new List<ValSys>
    {
        new ValSys { Empresa = "iLabPlus", Clave = "Smtp", Valor1 = "test", Valor2 = "test", Valor3 = "test", Valor4 = 587m }
    }.AsQueryable();
            mockValSys.As<IAsyncEnumerable<ValSys>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<ValSys>(valSysData.GetEnumerator()));
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<ValSys>(valSysData.Provider));
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.Expression).Returns(valSysData.Expression);
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.ElementType).Returns(valSysData.ElementType);
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.GetEnumerator()).Returns(valSysData.GetEnumerator());

            _mockContext.Setup(c => c.ValSys).Returns(mockValSys.Object);

            _mockEnvironment.Setup(e => e.WebRootPath).Returns("test_path");
            _mockFileSystem.Setup(f => f.File.Exists(It.IsAny<string>())).Returns(true);
            _mockFileSystem.Setup(f => f.File.OpenText(It.IsAny<string>())).Returns(new StreamReader(new MemoryStream()));

            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Scheme).Returns("https");
            mockRequest.Setup(r => r.Host).Returns(HostString.FromUriComponent("localhost"));
            mockRequest.Setup(r => r.PathBase).Returns(PathString.FromUriComponent("/"));
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            _mockNotifications
                .Setup(n => n.EmailNotification(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("Error");

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);

            // Act
            var result = await controller.SendPasswordResetLink("test@example.com");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Error al enviar el correo electrónico", objectResult.Value);
        }



        [Fact]
        public async Task SendPasswordResetLink_ReturnsStatusCode500_WhenEmailTemplateNotFound()
        {
            // Arrange
            var testUser = new Usuarios { Usuario = "test@example.com" };
            var data = new List<Usuarios> { testUser }.AsQueryable();
            var mockSet = new Mock<DbSet<Usuarios>>();
            mockSet.As<IAsyncEnumerable<Usuarios>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Usuarios>(data.GetEnumerator()));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Usuarios>(data.Provider));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            var mockValSys = new Mock<DbSet<ValSys>>();
            var valSysData = new List<ValSys>
    {
        new ValSys { Empresa = "iLabPlus", Clave = "Smtp", Valor1 = "test", Valor2 = "test", Valor3 = "test", Valor4 = 587m }
    }.AsQueryable();
            mockValSys.As<IAsyncEnumerable<ValSys>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<ValSys>(valSysData.GetEnumerator()));
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<ValSys>(valSysData.Provider));
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.Expression).Returns(valSysData.Expression);
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.ElementType).Returns(valSysData.ElementType);
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.GetEnumerator()).Returns(valSysData.GetEnumerator());

            _mockContext.Setup(c => c.ValSys).Returns(mockValSys.Object);

            _mockEnvironment.Setup(e => e.WebRootPath).Returns("test_path");
            _mockFileSystem.Setup(f => f.File.Exists(It.IsAny<string>())).Returns(false);

            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Scheme).Returns("https");
            mockRequest.Setup(r => r.Host).Returns(HostString.FromUriComponent("localhost"));
            mockRequest.Setup(r => r.PathBase).Returns(PathString.FromUriComponent("/"));
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);

            // Act
            var result = await controller.SendPasswordResetLink("test@example.com");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Plantilla de correo no encontrada", objectResult.Value);
        }




        [Fact]
        public async Task Login_ReturnsViewResult_WhenExceptionOccurs()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Usuarios>>();
            mockSet.As<IQueryable<Usuarios>>()
                .Setup(m => m.Provider)
                .Throws(new Exception("Test exception"));

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);
            var loginModel = new LoginModel { Usuario = "test@example.com", Password = "password" };

            // Act
            var result = await controller.Login(loginModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Login", viewResult.ViewName);
            Assert.IsType<LoginModel>(viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState.Values, v => v.Errors.Any(e => e.ErrorMessage.StartsWith("Error de conexión con el servidor iLabPlus")));
        }





        [Fact]
        public async Task SendPasswordResetLink_ReturnsStatusCode500_WhenWebRootPathIsNull()
        {
            // Arrange
            var testUser = new Usuarios { Usuario = "test@example.com" };
            var data = new List<Usuarios> { testUser }.AsQueryable();
            var mockSet = new Mock<DbSet<Usuarios>>();
            mockSet.As<IAsyncEnumerable<Usuarios>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Usuarios>(data.GetEnumerator()));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Usuarios>(data.Provider));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            var mockValSys = new Mock<DbSet<ValSys>>();
            var valSysData = new List<ValSys>
            {
                new ValSys { Empresa = "iLabPlus", Clave = "Smtp", Valor1 = "test", Valor2 = "test", Valor3 = "test", Valor4 = 587m }
            }.AsQueryable();
            mockValSys.As<IAsyncEnumerable<ValSys>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<ValSys>(valSysData.GetEnumerator()));
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<ValSys>(valSysData.Provider));
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.Expression).Returns(valSysData.Expression);
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.ElementType).Returns(valSysData.ElementType);
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.GetEnumerator()).Returns(valSysData.GetEnumerator());

            _mockContext.Setup(c => c.ValSys).Returns(mockValSys.Object);

            _mockEnvironment.Setup(e => e.WebRootPath).Returns((string)null);

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);

            // Act
            var result = await controller.SendPasswordResetLink("test@example.com");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Error en la configuración del servidor", objectResult.Value);
        }


        [Fact]
        public async Task SendPasswordResetLink_ReturnsStatusCode500_WhenHttpContextIsNull()
        {
            // Arrange
            var testUser = new Usuarios { Usuario = "test@example.com" };
            var data = new List<Usuarios> { testUser }.AsQueryable();
            var mockSet = new Mock<DbSet<Usuarios>>();
            mockSet.As<IAsyncEnumerable<Usuarios>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Usuarios>(data.GetEnumerator()));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Usuarios>(data.Provider));
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Usuarios>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            _mockContext.Setup(c => c.Usuarios).Returns(mockSet.Object);

            var mockValSys = new Mock<DbSet<ValSys>>();
            var valSysData = new List<ValSys>
            {
                new ValSys { Empresa = "iLabPlus", Clave = "Smtp", Valor1 = "test", Valor2 = "test", Valor3 = "test", Valor4 = 587m }
            }.AsQueryable();
            mockValSys.As<IAsyncEnumerable<ValSys>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<ValSys>(valSysData.GetEnumerator()));
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<ValSys>(valSysData.Provider));
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.Expression).Returns(valSysData.Expression);
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.ElementType).Returns(valSysData.ElementType);
            mockValSys.As<IQueryable<ValSys>>().Setup(m => m.GetEnumerator()).Returns(valSysData.GetEnumerator());

            _mockContext.Setup(c => c.ValSys).Returns(mockValSys.Object);

            _mockEnvironment.Setup(e => e.WebRootPath).Returns("test_path");
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns((HttpContext)null);

            var controller = new AccountController.Controllers.AccountController(
                _mockContext.Object,
                _mockEnvironment.Object,
                _mockHttpContextAccessor.Object,
                _mockFileSystem.Object,
                _mockNotifications.Object,
                _output);

            // Act
            var result = await controller.SendPasswordResetLink("test@example.com");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Error en la configuración del servidor", objectResult.Value);
        }


        
        //[Fact]
        //public async Task Login_ConContrasenaValida_DebeInvocarCheckUser()
        //{
        //    // Arrange
        //    var loginUser = new LoginModel { Usuario = "test@example.com", Password = "password" };
        //    var lookupUser = new Usuarios
        //    {
        //        Usuario = "test@example.com",
        //        Password = "$2a$11$QYk3tEn0uyycGPxD3RlJUew3G7PVKLzjkLlZpqCnZf8yMRQ7jkZYa", // BCrypt hash for "password"
        //    };

        //    _mockContext.Setup(c => c.Usuarios.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuarios, bool>>>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(lookupUser);

        //    // Configurar FunctionsCrypto para que la verificación de contraseña sea exitosa
        //    FunctionsCrypto.VerifyPasswordImplementation = (password, hashedPassword) => true;

        //    bool checkUserInvoked = false;
        //    // Configurar mock para CheckUser
        //    _controller.CheckUser = (user, lookup) => { checkUserInvoked = true; };

        //    // Act
        //    await _controller.Login(loginUser);

        //    // Assert
        //    Assert.True(checkUserInvoked, "CheckUser debería haber sido invocado");
        //}



        public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner)
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

        public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
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

        public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
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