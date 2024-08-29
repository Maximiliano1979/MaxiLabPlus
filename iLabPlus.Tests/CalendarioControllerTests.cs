using iLabPlus.Controllers;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using iLabPlus.Tests;
using Microsoft.EntityFrameworkCore; 
using iLabPlus.Models.Clases;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;
using Xunit.Abstractions;
using System.Linq.Expressions;
using Newtonsoft.Json;


namespace iLabPlus.Tests
{
    public class CalendarioControllerTests
    {

        private readonly ITestOutputHelper _output;

        public CalendarioControllerTests(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void ObtenerEmpresaActual_DeberiaRetornarLaEmpresaDelUsuario()
        {
            // Arrange (Preparar)
            var empresaEsperada = "TestEmpresa";
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockContext = new Mock<DbContextiLabPlus>();

            // Configurar el HttpContextAccessor para simular un usuario autenticado
            var claims = new List<Claim>
            {
                new Claim("Empresa", empresaEsperada)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(httpContext);

            var functionsBBDD = new FunctionsBBDD(mockContext.Object, mockHttpContextAccessor.Object);
            var controller = new CalendarioController(mockContext.Object, functionsBBDD);

            // Act (Actuar)
            var result = controller.ObtenerEmpresaActual() as OkObjectResult;

            // Assert (Verificar)
            Assert.NotNull(result);
            var empresaObtenida = Assert.IsType<string>(result.Value.GetType().GetProperty("Empresa").GetValue(result.Value));
            Assert.Equal(empresaEsperada, empresaObtenida);
        }


        [Fact]
        public async Task ObtenerUsuarios_DeberiaRetornarListaDeUsuarios()
        {
            // Arrange (Preparar)
            var empresa = "TestEmpresa";
            var usuariosEsperados = new List<Usuarios>
            {
                new Usuarios { Guid = Guid.NewGuid(), UsuarioNombre = "Usuario 1", Empresa = empresa },
                new Usuarios { Guid = Guid.NewGuid(), UsuarioNombre = "Usuario 2", Empresa = empresa }
            };

            // Crear un contexto de base de datos en memoria (mock)
            var mockDbContext = CreateMockContext(usuariosEsperados);

            // Configurar el HttpContextAccessor para simular un usuario autenticado
            var claims = new List<Claim>
            {
                new Claim("Empresa", empresa)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(httpContext);

            var functionsBBDD = new FunctionsBBDD(mockDbContext, mockHttpContextAccessor.Object);
            var controller = new CalendarioController(mockDbContext, functionsBBDD);

            // Act (Actuar)
            var result = await controller.ObtenerUsuarios() as OkObjectResult;

            // Assert (Verificar)
            Assert.NotNull(result);
            var usuariosObtenidos = Assert.IsAssignableFrom<IEnumerable<object>>(result.Value);
            Assert.Equal(usuariosEsperados.Count, usuariosObtenidos.Count());

            // Verificar que los datos de cada usuario sean correctos
            var usuariosObtenidosList = usuariosObtenidos.ToList();
            for (int i = 0; i < usuariosEsperados.Count; i++)
            {
                var usuarioObtenido = usuariosObtenidosList[i];
                var guidProp = usuarioObtenido.GetType().GetProperty("Guid");
                var nombreProp = usuarioObtenido.GetType().GetProperty("NombreCompleto");

                Assert.NotNull(guidProp);
                Assert.NotNull(nombreProp);

                var guidObtenido = (Guid)guidProp.GetValue(usuarioObtenido);
                var nombreObtenido = (string)nombreProp.GetValue(usuarioObtenido);

                Assert.Equal(usuariosEsperados[i].Guid, guidObtenido);
                Assert.Equal(usuariosEsperados[i].UsuarioNombre, nombreObtenido);
            }
        }

        // Método auxiliar para crear un contexto de base de datos mockeado
        private DbContextiLabPlus CreateMockContext(List<Usuarios> usuarios) // Use the Usuarios class directly
        {
            var options = new DbContextOptionsBuilder<DbContextiLabPlus>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new DbContextiLabPlus(options);
            context.Usuarios.AddRange(usuarios);
            context.SaveChanges();

            return context;
        }

        [Fact]
        public void ObtenerEventos_DeberiaRetornarTodosLosEventos()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Configurar HttpContext mock
            var mockHttpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim("Empresa", "TestEmpresa"),
                new Claim("Usuario", "TestUser"),
                new Claim("UsuarioTipo", "ADMIN"),
                new Claim("EmpresaNombre", "TestEmpresaNombre"),
                new Claim("UsuarioNombre", "TestUserNombre")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            mockHttpContext.User = claimsPrincipal;

            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);

            var functionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);

            functionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(new List<MenuUser>());
            functionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "ADMIN"
            });

            var eventosEsperados = new List<object>
            {
                new { id = Guid.NewGuid(), title = "Evento 1", start = "2023-07-22T10:00:00", end = "2023-07-22T11:00:00" },
                new { id = Guid.NewGuid(), title = "Evento 2", start = "2023-07-23T14:00:00", end = "2023-07-23T15:00:00" }
            };

            var controller = new TestCalendarioController(mockContext.Object, functionsBBDD.Object, eventosEsperados);

            // Act
            var result = controller.ObtenerEventos() as JsonResult;

            // Assert
            Assert.NotNull(result);
            var eventos = Assert.IsType<List<object>>(result.Value);
            Assert.Equal(eventosEsperados.Count, eventos.Count);
        }

        // Clase de prueba que hereda de CalendarioController
        public class TestCalendarioController : CalendarioController
        {
            private readonly List<object> _eventosEsperados;

            public TestCalendarioController(DbContextiLabPlus context, FunctionsBBDD functionsBBDD, List<object> eventosEsperados)
                : base(context, functionsBBDD)
            {
                _eventosEsperados = eventosEsperados;
            }

            protected override List<object> GetEventosAlcanceSoloYo() => _eventosEsperados.Take(1).ToList();
            protected override List<object> GetEventosAlcanceTodos() => _eventosEsperados.Skip(1).Take(1).ToList();
            protected override List<object> GetEventosAlcanceUsuarioMultiples() => new List<object>();
            protected override List<object> GetEventosAlcanceObservador() => new List<object>();
        }


        [Fact]
        public void ObtenerDetallesEvento_DeberiaRetornarEventoExistente()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Calendario>>();
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Configurar HttpContext mock
            var mockHttpContext = new DefaultHttpContext();
            var claims = new List<Claim>
    {
        new Claim("Empresa", "TestEmpresa"),
        new Claim("Usuario", "TestUser"),
        new Claim("UsuarioTipo", "ADMIN"),
        new Claim("EmpresaNombre", "TestEmpresaNombre"),
        new Claim("UsuarioNombre", "TestUserNombre")
    };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            mockHttpContext.User = claimsPrincipal;

            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);

            var eventoGuid = Guid.NewGuid();
            var eventoEsperado = new Calendario
            {
                Guid = eventoGuid,
                Titulo = "Evento de prueba",
                Fecha = DateTime.Now.Date,
                Detalle = "Detalles del evento",
                HoraInicio = new TimeSpan(10, 0, 0),
                HoraFin = new TimeSpan(11, 0, 0),
                Alcance = "Solo Yo",
                Usuario = "TestUser"
            };

            var data = new List<Calendario> { eventoEsperado }.AsQueryable();

            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            mockContext.Setup(c => c.Calendario).Returns(mockSet.Object);

            var functionsBBDD = new FunctionsBBDD(mockContext.Object, mockHttpContextAccessor.Object);
            var controller = new CalendarioController(mockContext.Object, functionsBBDD);

            // Act
            var result = controller.ObtenerDetallesEvento(eventoGuid.ToString()) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var eventoObtenido = Assert.IsType<Dictionary<string, object>>(result.Value);

            Assert.Equal(eventoEsperado.Titulo, eventoObtenido["Titulo"]);
            Assert.Equal(eventoEsperado.Fecha.ToString("yyyy-MM-dd"), eventoObtenido["Fecha"]);
            Assert.Equal(eventoEsperado.Detalle, eventoObtenido["Detalle"]);
            Assert.Equal(eventoEsperado.HoraInicio.ToString(@"hh\:mm"), eventoObtenido["HoraInicio"]);
            Assert.Equal(eventoEsperado.HoraFin.ToString(@"hh\:mm"), eventoObtenido["HoraFin"]);
            Assert.Equal(eventoEsperado.Alcance, eventoObtenido["Alcance"]);
        }


        [Fact]
        public void ObtenerDetallesEvento_DeberiaRetornarNotFoundParaEventoInexistente()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Calendario>>();

            var data = new List<Calendario>().AsQueryable();

            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            mockContext.Setup(c => c.Calendario).Returns(mockSet.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "ADMIN"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object);

            // Act
            var result = controller.ObtenerDetallesEvento(Guid.NewGuid().ToString());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }



        [Fact]
        public async Task AgregarEvento_DeberiaAgregarEventoCorrectamente()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Calendario>>();
            var mockSetUsuarios = new Mock<DbSet<Usuarios>>();
            var mockSetCalendarioMultiUsuarios = new Mock<DbSet<CalendarioMultiUsuarios>>();

            mockContext.Setup(m => m.Calendario).Returns(mockSet.Object);
            mockContext.Setup(m => m.Usuarios).Returns(mockSetUsuarios.Object);
            mockContext.Setup(m => m.CalendarioMultiUsuarios).Returns(mockSetCalendarioMultiUsuarios.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "ADMIN"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object);

            var eventoDto = new CalendarioEventoDto
            {
                Titulo = "Nuevo Evento",
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                Detalle = "Detalles del nuevo evento",
                HoraInicio = "10:00",
                HoraFin = "11:00",
                Alcance = "Solo Yo",
                RecibirMail = true
            };

            // Act
            var result = await controller.AgregarEvento(eventoDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

            var eventoAgregado = Assert.IsType<Calendario>(objectResult.Value);
            Assert.Equal(eventoDto.Titulo, eventoAgregado.Titulo);
            Assert.Equal(DateTime.Parse(eventoDto.Fecha), eventoAgregado.Fecha);
            Assert.Equal(eventoDto.Detalle, eventoAgregado.Detalle);
            Assert.Equal(TimeSpan.Parse(eventoDto.HoraInicio), eventoAgregado.HoraInicio);
            Assert.Equal(TimeSpan.Parse(eventoDto.HoraFin), eventoAgregado.HoraFin);
            Assert.Equal(eventoDto.Alcance, eventoAgregado.Alcance);
            Assert.Equal(eventoDto.RecibirMail, eventoAgregado.RecibirMail);

            mockSet.Verify(m => m.AddAsync(It.IsAny<Calendario>(), It.IsAny<CancellationToken>()), Times.Once());
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }



        [Fact]
        public async Task AgregarEvento_DeberiaRetornarBadRequestEnCasoDeError()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Configurar HttpContext mock
            var mockHttpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim("Empresa", "TestEmpresa"),
                new Claim("Usuario", "TestUser"),
                new Claim("UsuarioTipo", "ADMIN"),
                new Claim("EmpresaNombre", "TestEmpresaNombre"),
                new Claim("UsuarioNombre", "TestUserNombre")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            mockHttpContext.User = claimsPrincipal;

            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);

            var functionsBBDD = new FunctionsBBDD(mockContext.Object, mockHttpContextAccessor.Object);

            mockContext.Setup(c => c.Calendario.AddAsync(It.IsAny<Calendario>(), It.IsAny<CancellationToken>()))
                       .Throws(new Exception("Error al agregar evento"));

            var controller = new CalendarioController(mockContext.Object, functionsBBDD);

            var eventoDto = new CalendarioEventoDto
            {
                Titulo = "Nuevo Evento",
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                Detalle = "Detalles del nuevo evento",
                HoraInicio = "10:00",
                HoraFin = "11:00",
                Alcance = "Solo Yo",
                RecibirMail = true
            };

            // Act
            var result = await controller.AgregarEvento(eventoDto);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Error al agregar evento", badRequestResult.Value);
        }


        [Fact]
        public async Task ActualizarEvento_DeberiaActualizarEventoCorrectamente()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Calendario>>();
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Configurar HttpContext mock
            var mockHttpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim("Empresa", "TestEmpresa"),
                new Claim("Usuario", "TestUser"),
                new Claim("UsuarioTipo", "ADMIN"),
                new Claim("EmpresaNombre", "TestEmpresaNombre"),
                new Claim("UsuarioNombre", "TestUserNombre")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            mockHttpContext.User = claimsPrincipal;

            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);

            var functionsBBDD = new FunctionsBBDD(mockContext.Object, mockHttpContextAccessor.Object);

            var eventoExistente = new Calendario
            {
                Guid = Guid.NewGuid(),
                Titulo = "Evento Original",
                Fecha = DateTime.Now,
                Detalle = "Detalles originales",
                HoraInicio = new TimeSpan(10, 0, 0),
                HoraFin = new TimeSpan(11, 0, 0),
                Alcance = "Solo Yo",
                RecibirMail = false,
                Usuario = "TestUser"
            };

            var data = new List<Calendario> { eventoExistente }.AsQueryable();

            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            mockContext.Setup(c => c.Calendario).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Configurar mock para CalendarioMultiUsuarios
            var mockSetMultiUsuarios = new Mock<DbSet<CalendarioMultiUsuarios>>();
            var dataMultiUsuarios = new List<CalendarioMultiUsuarios>().AsQueryable();
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.Provider).Returns(dataMultiUsuarios.Provider);
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.Expression).Returns(dataMultiUsuarios.Expression);
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.ElementType).Returns(dataMultiUsuarios.ElementType);
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.GetEnumerator()).Returns(dataMultiUsuarios.GetEnumerator());
            mockContext.Setup(c => c.CalendarioMultiUsuarios).Returns(mockSetMultiUsuarios.Object);

            var controller = new CalendarioController(mockContext.Object, functionsBBDD);

            var eventoDto = new CalendarioEventoDto
            {
                Guid = eventoExistente.Guid.ToString(),
                Titulo = "Evento Actualizado",
                Fecha = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
                Detalle = "Detalles actualizados",
                HoraInicio = "11:00",
                HoraFin = "12:00",
                Alcance = "Varios usuarios",
                RecibirMail = true,
                Usuarios = new List<string> { "Usuario1", "Usuario2" }
            };

            // Act
            var result = await controller.ActualizarEvento(eventoDto);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            Assert.Equal(eventoDto.Titulo, eventoExistente.Titulo);
            Assert.Equal(DateTime.Parse(eventoDto.Fecha), eventoExistente.Fecha);
            Assert.Equal(eventoDto.Detalle, eventoExistente.Detalle);
            Assert.Equal(TimeSpan.Parse(eventoDto.HoraInicio), eventoExistente.HoraInicio);
            Assert.Equal(TimeSpan.Parse(eventoDto.HoraFin), eventoExistente.HoraFin);
            Assert.Equal(eventoDto.Alcance, eventoExistente.Alcance);
            Assert.Equal(eventoDto.RecibirMail, eventoExistente.RecibirMail);

            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            mockSetMultiUsuarios.Verify(m => m.AddAsync(It.IsAny<CalendarioMultiUsuarios>(), It.IsAny<CancellationToken>()), Times.Exactly(eventoDto.Usuarios.Count));
        }


        [Fact]
        public async Task ActualizarEvento_DeberiaRetornarNotFoundParaEventoInexistente()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Calendario>>();
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Configurar HttpContext mock
            var mockHttpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim("Empresa", "TestEmpresa"),
                new Claim("Usuario", "TestUser"),
                new Claim("UsuarioTipo", "ADMIN"),
                new Claim("EmpresaNombre", "TestEmpresaNombre"),
                new Claim("UsuarioNombre", "TestUserNombre")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            mockHttpContext.User = claimsPrincipal;

            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);

            // Crear una instancia real de FunctionsBBDD con los mocks
            var functionsBBDD = new FunctionsBBDD(mockContext.Object, mockHttpContextAccessor.Object);

            // Configurar el contexto para devolver una lista vacía de Calendario
            var data = new List<Calendario>().AsQueryable();

            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            mockContext.Setup(c => c.Calendario).Returns(mockSet.Object);

            var controller = new CalendarioController(mockContext.Object, functionsBBDD);

            var eventoDto = new CalendarioEventoDto
            {
                Guid = Guid.NewGuid().ToString(),
                Titulo = "Evento Inexistente",
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                Detalle = "Detalles",
                HoraInicio = "10:00",
                HoraFin = "11:00",
                Alcance = "Solo Yo",
                RecibirMail = true
            };

            // Act
            var result = await controller.ActualizarEvento(eventoDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Evento no encontrado", notFoundResult.Value);
        }


        [Fact]
        public async Task EliminarEvento_DeberiaRetornarNotFoundParaEventoInexistente()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Calendario>>();
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Configurar HttpContext mock
            var mockHttpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim("Empresa", "TestEmpresa"),
                new Claim("Usuario", "TestUser"),
                new Claim("UsuarioTipo", "ADMIN"),
                new Claim("EmpresaNombre", "TestEmpresaNombre"),
                new Claim("UsuarioNombre", "TestUserNombre")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            mockHttpContext.User = claimsPrincipal;

            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);

            // Configurar el contexto para devolver null al buscar el evento
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((Calendario)null);
            mockContext.Setup(c => c.Calendario).Returns(mockSet.Object);

            var functionsBBDD = new FunctionsBBDD(mockContext.Object, mockHttpContextAccessor.Object);

            var controller = new CalendarioController(mockContext.Object, functionsBBDD);

            var guidDTO = new CalendarioController.GuidDTO { Guid = Guid.NewGuid() };

            // Act
            var result = await controller.EliminarEvento(guidDTO);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("Evento a eliminar no encontrado", notFoundResult.Value);
        }

        [Fact]
        public void ModalEvento_DeberiaRetornarPartialView()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Configurar HttpContext mock
            var mockHttpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim("Empresa", "TestEmpresa"),
                new Claim("Usuario", "TestUser"),
                new Claim("UsuarioTipo", "ADMIN"),
                new Claim("EmpresaNombre", "TestEmpresaNombre"),
                new Claim("UsuarioNombre", "TestUserNombre")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            mockHttpContext.User = claimsPrincipal;
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, mockHttpContextAccessor.Object);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "ADMIN"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object);

            // Act
            var result = controller._ModalEvento();

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Null(partialViewResult.ViewName); // Si no se especifica un nombre, se usa el nombre del método por defecto
        }

        [Fact]
        public async Task AgregarEvento_DeberiaAgregarEventoSoloYo()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Calendario>>();
            mockContext.Setup(m => m.Calendario).Returns(mockSet.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "ADMIN"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object);

            var eventoDto = new CalendarioEventoDto
            {
                Titulo = "Evento Solo Yo",
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                HoraInicio = "10:00",
                HoraFin = "11:00",
                Alcance = "Solo yo",
                RecibirMail = true
            };

            // Act
            var result = await controller.AgregarEvento(eventoDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            var eventoAgregado = Assert.IsType<Calendario>(objectResult.Value);
            Assert.Equal("TestUser", eventoAgregado.UsuarioEspecifico);
        }

        [Fact]
        public async Task AgregarEvento_DeberiaAgregarUsuarioSesionParaVariosUsuarios()
        {
            _output.WriteLine("Iniciando test AgregarEvento_DeberiaAgregarUsuarioSesionParaVariosUsuarios");

            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Calendario>>();
            var mockSetUsuarios = new Mock<DbSet<Usuarios>>();
            var mockSetCalendarioMultiUsuarios = new Mock<DbSet<CalendarioMultiUsuarios>>();

            mockContext.Setup(m => m.Calendario).Returns(mockSet.Object);
            mockContext.Setup(m => m.Usuarios).Returns(mockSetUsuarios.Object);
            mockContext.Setup(m => m.CalendarioMultiUsuarios).Returns(mockSetCalendarioMultiUsuarios.Object);

            var usuarioSesion = new Usuarios { Guid = Guid.NewGuid(), Usuario = "TestUser", Empresa = "TestEmpresa" };
            _output.WriteLine($"Usuario de sesión creado: {usuarioSesion.Usuario} ({usuarioSesion.Guid})");

            mockSetUsuarios.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] ids) => {
                    if (ids[0] is string str && str == "TestUser")
                    {
                        _output.WriteLine($"FindAsync llamado con {str}, usuario encontrado: {usuarioSesion.Usuario}");
                        return usuarioSesion;
                    }
                    if (ids[0] is Guid guid && guid == usuarioSesion.Guid)
                    {
                        _output.WriteLine($"FindAsync llamado con {guid}, usuario encontrado: {usuarioSesion.Usuario}");
                        return usuarioSesion;
                    }
                    _output.WriteLine($"FindAsync llamado con {ids[0]}, usuario no encontrado");
                    return null;
                });

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "ADMIN"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            var eventoDto = new CalendarioEventoDto
            {
                Titulo = "Evento Varios Usuarios",
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                HoraInicio = "10:00",
                HoraFin = "11:00",
                Alcance = "Varios usuarios",
                RecibirMail = true,
                Usuarios = new List<string>()
            };

            _output.WriteLine($"EventoDto creado: {JsonConvert.SerializeObject(eventoDto)}");

            var loggedUsers = new List<string>();
            mockSetCalendarioMultiUsuarios.Setup(m => m.AddAsync(It.IsAny<CalendarioMultiUsuarios>(), It.IsAny<CancellationToken>()))
                .Callback<CalendarioMultiUsuarios, CancellationToken>((u, token) =>
                {
                    loggedUsers.Add(u.Usuario);
                    _output.WriteLine($"Usuario agregado a CalendarioMultiUsuarios: {u.Usuario}");
                })
                .ReturnsAsync((CalendarioMultiUsuarios u, CancellationToken token) => null);

            // Act
            _output.WriteLine("Ejecutando AgregarEvento");
            var result = await controller.AgregarEvento(eventoDto);

            // Assert
            _output.WriteLine($"Resultado: {JsonConvert.SerializeObject(result)}");
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            var eventoAgregado = Assert.IsType<Calendario>(objectResult.Value);
            Assert.Equal(eventoDto.Titulo, eventoAgregado.Titulo);
            Assert.Equal(eventoDto.Alcance, eventoAgregado.Alcance);

            _output.WriteLine($"Usuarios agregados: {string.Join(", ", loggedUsers)}");
            Assert.Contains(usuarioSesion.Usuario, loggedUsers);

            mockSetCalendarioMultiUsuarios.Verify(m => m.AddAsync(It.Is<CalendarioMultiUsuarios>(cm => cm.Usuario == usuarioSesion.Usuario), It.IsAny<CancellationToken>()), Times.Once());
            _output.WriteLine("Test completado");
        }

        [Fact]
        public async Task AgregarEvento_DeberiaAgregarEventoComoUsuarioNormal()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Calendario>>();
            mockContext.Setup(m => m.Calendario).Returns(mockSet.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "USUARIO"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object);

            var eventoDto = new CalendarioEventoDto
            {
                Titulo = "Evento Usuario Normal",
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                HoraInicio = "10:00",
                HoraFin = "11:00",
                Alcance = "Todos",
                RecibirMail = true
            };

            // Act
            var result = await controller.AgregarEvento(eventoDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            var eventoAgregado = Assert.IsType<Calendario>(objectResult.Value);
            Assert.Equal("Solo Yo", eventoAgregado.Alcance);
        }

        [Fact]
        public async Task AgregarEvento_DeberiaRetornarBadRequestConFormatoInvalido()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object);

            var eventoDto = new CalendarioEventoDto
            {
                Titulo = "Evento Inválido",
                Fecha = "Fecha inválida",
                HoraInicio = "Hora inválida",
                HoraFin = "11:00",
                Alcance = "Solo yo",
                RecibirMail = true
            };

            // Act
            var result = await controller.AgregarEvento(eventoDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task ActualizarEvento_DeberiaActualizarEventoSoloYo()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Calendario>>();
            var eventoExistente = new Calendario
            {
                Guid = Guid.NewGuid(),
                Usuario = "TestUser",
                Titulo = "Evento Original",
                Fecha = DateTime.Now,
                HoraInicio = TimeSpan.FromHours(9),
                HoraFin = TimeSpan.FromHours(10),
                Alcance = "Solo Yo",
                Empresa = "TestEmpresa"
            };
            var data = new List<Calendario> { eventoExistente }.AsQueryable();
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            // Configura el comportamiento de FirstOrDefault
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(objs => data.FirstOrDefault(d => d.Guid == (Guid)objs[0]));
            mockContext.Setup(c => c.Calendario).Returns(mockSet.Object);

            // Configurar el mock para CalendarioMultiUsuarios
            var mockSetMultiUsuarios = new Mock<DbSet<CalendarioMultiUsuarios>>();
            var dataMultiUsuarios = new List<CalendarioMultiUsuarios>().AsQueryable();
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.Provider).Returns(dataMultiUsuarios.Provider);
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.Expression).Returns(dataMultiUsuarios.Expression);
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.ElementType).Returns(dataMultiUsuarios.ElementType);
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.GetEnumerator()).Returns(dataMultiUsuarios.GetEnumerator());
            mockContext.Setup(c => c.CalendarioMultiUsuarios).Returns(mockSetMultiUsuarios.Object);

            // Configurar SaveChangesAsync
            mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1) // Simula que se guardó 1 cambio
                .Verifiable(); // Nos permite verificar que se llamó a este método

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "ADMIN"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object, _output);
            var eventoDto = new CalendarioEventoDto
            {
                Guid = eventoExistente.Guid.ToString(),
                Titulo = "Evento Actualizado",
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                HoraInicio = "10:00",
                HoraFin = "11:00",
                Alcance = "Solo yo",
                RecibirMail = true
            };

            // Act
            var result = await controller.ActualizarEvento(eventoDto);

            // Assert
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.IsType<OkResult>(result);
            Assert.Equal("TestUser", eventoExistente.UsuarioEspecifico);
            Assert.Equal("Evento Actualizado", eventoExistente.Titulo);
            Assert.Equal(TimeSpan.Parse("10:00"), eventoExistente.HoraInicio);
            Assert.Equal(TimeSpan.Parse("11:00"), eventoExistente.HoraFin);
        }

       
        [Fact]
        public async Task ActualizarEvento_DeberiaActualizarEventoComoUsuarioNormal()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Calendario>>();
            var eventoExistente = new Calendario
            {
                Guid = Guid.NewGuid(),
                Usuario = "TestUser",
                Titulo = "Evento Original",
                Fecha = DateTime.Now,
                HoraInicio = TimeSpan.FromHours(9),
                HoraFin = TimeSpan.FromHours(10),
                Alcance = "Solo Yo",
                Empresa = "TestEmpresa"
            };
            var data = new List<Calendario> { eventoExistente }.AsQueryable();
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    
            // Configura el comportamiento de FirstOrDefault
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(objs => data.FirstOrDefault(d => d.Guid == (Guid)objs[0]));
            mockContext.Setup(c => c.Calendario).Returns(mockSet.Object);
    
            // Configurar el mock para CalendarioMultiUsuarios
            var mockSetMultiUsuarios = new Mock<DbSet<CalendarioMultiUsuarios>>();
            var dataMultiUsuarios = new List<CalendarioMultiUsuarios>().AsQueryable();
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.Provider).Returns(dataMultiUsuarios.Provider);
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.Expression).Returns(dataMultiUsuarios.Expression);
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.ElementType).Returns(dataMultiUsuarios.ElementType);
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.GetEnumerator()).Returns(dataMultiUsuarios.GetEnumerator());
            mockContext.Setup(c => c.CalendarioMultiUsuarios).Returns(mockSetMultiUsuarios.Object);
    
            // Configurar SaveChangesAsync
            mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();
    
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "USUARIO"
            });
    
            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object, _output);
            var eventoDto = new CalendarioEventoDto
            {
                Guid = eventoExistente.Guid.ToString(),
                Titulo = "Evento Actualizado",
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                HoraInicio = "10:00",
                HoraFin = "11:00",
                Alcance = "Todos",
                RecibirMail = true
            };
    
            // Act
            var result = await controller.ActualizarEvento(eventoDto);
    
            // Assert
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.IsType<OkResult>(result);
            Assert.Equal("Solo Yo", eventoExistente.Alcance);
            Assert.Equal("Evento Actualizado", eventoExistente.Titulo);
            Assert.Equal(TimeSpan.Parse("10:00"), eventoExistente.HoraInicio);
            Assert.Equal(TimeSpan.Parse("11:00"), eventoExistente.HoraFin);
        }

        [Fact]
        public void GetEventosAlcanceObservador_DeberiaRetornarEventosCorrectamente()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Calendario>>();

            var eventos = new List<Calendario>
            {
                new Calendario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Usuario = "TestUser", Alcance = "Observador", Titulo = "Evento 1", Fecha = DateTime.Now, HoraInicio = TimeSpan.FromHours(9), HoraFin = TimeSpan.FromHours(10) },
                new Calendario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Usuario = "TestUser", Alcance = "Observador", Titulo = "Evento 2", Fecha = DateTime.Now.AddDays(1), HoraInicio = TimeSpan.FromHours(11), HoraFin = TimeSpan.FromHours(12) },
                new Calendario { Guid = Guid.NewGuid(), Empresa = "OtraEmpresa", Usuario = "TestUser", Alcance = "Observador", Titulo = "Evento 3", Fecha = DateTime.Now, HoraInicio = TimeSpan.FromHours(13), HoraFin = TimeSpan.FromHours(14) }
            }.AsQueryable();

            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Provider).Returns(eventos.Provider);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Expression).Returns(eventos.Expression);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.ElementType).Returns(eventos.ElementType);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.GetEnumerator()).Returns(eventos.GetEnumerator());

            mockContext.Setup(c => c.Calendario).Returns(mockSet.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object);

            // Act
            var result = controller.GetEventosAlcanceObservadorForTesting();

            // Assert
            Assert.Equal(2, result.Count);
            foreach (var item in result)
            {
                var propertyInfo = item.GetType().GetProperty("color");
                Assert.NotNull(propertyInfo);
                var colorValue = propertyInfo.GetValue(item);
                Assert.Equal("#C0C3C7", colorValue);
            }
        }

        [Fact]
        public void GetEventosAlcanceSoloYo_DeberiaRetornarEventosCorrectamente()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Calendario>>();

            var eventos = new List<Calendario>
            {
                new Calendario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Usuario = "TestUser", Alcance = "Solo Yo", Titulo = "Evento 1", Fecha = DateTime.Now, HoraInicio = TimeSpan.FromHours(9), HoraFin = TimeSpan.FromHours(10) },
                new Calendario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Usuario = "TestUser", Alcance = "Solo Yo", Titulo = "Evento 2", Fecha = DateTime.Now.AddDays(1), HoraInicio = TimeSpan.FromHours(11), HoraFin = TimeSpan.FromHours(12) },
                new Calendario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Usuario = "OtroUsuario", Alcance = "Solo Yo", Titulo = "Evento 3", Fecha = DateTime.Now, HoraInicio = TimeSpan.FromHours(13), HoraFin = TimeSpan.FromHours(14) }
            }.AsQueryable();

            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Provider).Returns(eventos.Provider);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Expression).Returns(eventos.Expression);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.ElementType).Returns(eventos.ElementType);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.GetEnumerator()).Returns(eventos.GetEnumerator());

            mockContext.Setup(c => c.Calendario).Returns(mockSet.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object);

            // Act
            var result = controller.GetEventosAlcanceSoloYoForTesting();

            // Assert
            Assert.Equal(2, result.Count);
            foreach (var item in result)
            {
                var propertyInfo = item.GetType().GetProperty("color");
                Assert.NotNull(propertyInfo);
                var colorValue = propertyInfo.GetValue(item);
                Assert.Equal("#FF0000", colorValue);
            }
        }

        [Fact]
        public void GetEventosAlcanceTodos_DeberiaRetornarEventosCorrectamente()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Calendario>>();

            var eventos = new List<Calendario>
    {
        new Calendario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Alcance = "Todos", Titulo = "Evento 1", Fecha = DateTime.Now, HoraInicio = TimeSpan.FromHours(9), HoraFin = TimeSpan.FromHours(10) },
        new Calendario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Alcance = "Todos", Titulo = "Evento 2", Fecha = DateTime.Now.AddDays(1), HoraInicio = TimeSpan.FromHours(11), HoraFin = TimeSpan.FromHours(12) },
        new Calendario { Guid = Guid.NewGuid(), Empresa = "OtraEmpresa", Alcance = "Todos", Titulo = "Evento 3", Fecha = DateTime.Now, HoraInicio = TimeSpan.FromHours(13), HoraFin = TimeSpan.FromHours(14) }
    }.AsQueryable();

            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Provider).Returns(eventos.Provider);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.Expression).Returns(eventos.Expression);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.ElementType).Returns(eventos.ElementType);
            mockSet.As<IQueryable<Calendario>>().Setup(m => m.GetEnumerator()).Returns(eventos.GetEnumerator());

            mockContext.Setup(c => c.Calendario).Returns(mockSet.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "ADMIN"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object);

            // Act
            var result = controller.GetEventosAlcanceTodosForTesting();

            // Assert
            Assert.Equal(2, result.Count);
            foreach (var item in result)
            {
                var propertyInfo = item.GetType().GetProperty("color");
                Assert.NotNull(propertyInfo);
                var colorValue = propertyInfo.GetValue(item);
                Assert.Equal("#3CB371", colorValue);
            }
        }

        [Fact]
        public void GetEventosAlcanceUsuarioMultiples_DeberiaRetornarEventosCorrectamente()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSetCalendario = new Mock<DbSet<Calendario>>();
            var mockSetMultiUsuarios = new Mock<DbSet<CalendarioMultiUsuarios>>();

            var eventos = new List<Calendario>
            {
                new Calendario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Titulo = "Evento 1", Fecha = DateTime.Now, HoraInicio = TimeSpan.FromHours(9), HoraFin = TimeSpan.FromHours(10) },
                new Calendario { Guid = Guid.NewGuid(), Empresa = "TestEmpresa", Titulo = "Evento 2", Fecha = DateTime.Now.AddDays(1), HoraInicio = TimeSpan.FromHours(11), HoraFin = TimeSpan.FromHours(12) },
                new Calendario { Guid = Guid.NewGuid(), Empresa = "OtraEmpresa", Titulo = "Evento 3", Fecha = DateTime.Now, HoraInicio = TimeSpan.FromHours(13), HoraFin = TimeSpan.FromHours(14) }
            }.AsQueryable();

            var multiUsuarios = new List<CalendarioMultiUsuarios>
            {
                new CalendarioMultiUsuarios { GuidEvento = eventos.First().Guid, Usuario = "TestUser" },
                new CalendarioMultiUsuarios { GuidEvento = eventos.Skip(1).First().Guid, Usuario = "TestUser" }
            }.AsQueryable();

            mockSetCalendario.As<IQueryable<Calendario>>().Setup(m => m.Provider).Returns(eventos.Provider);
            mockSetCalendario.As<IQueryable<Calendario>>().Setup(m => m.Expression).Returns(eventos.Expression);
            mockSetCalendario.As<IQueryable<Calendario>>().Setup(m => m.ElementType).Returns(eventos.ElementType);
            mockSetCalendario.As<IQueryable<Calendario>>().Setup(m => m.GetEnumerator()).Returns(eventos.GetEnumerator());

            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.Provider).Returns(multiUsuarios.Provider);
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.Expression).Returns(multiUsuarios.Expression);
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.ElementType).Returns(multiUsuarios.ElementType);
            mockSetMultiUsuarios.As<IQueryable<CalendarioMultiUsuarios>>().Setup(m => m.GetEnumerator()).Returns(multiUsuarios.GetEnumerator());

            mockContext.Setup(c => c.Calendario).Returns(mockSetCalendario.Object);
            mockContext.Setup(c => c.CalendarioMultiUsuarios).Returns(mockSetMultiUsuarios.Object);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object);

            // Act
            var result = controller.GetEventosAlcanceUsuarioMultiplesForTesting();

            // Assert
            Assert.Equal(2, result.Count);
            foreach (var item in result)
            {
                var propertyInfo = item.GetType().GetProperty("color");
                Assert.NotNull(propertyInfo);
                var colorValue = propertyInfo.GetValue(item);
                Assert.Equal("#1B6EC2", colorValue);
            }
        }

        [Fact]
        public void Index_DeberiaRetornarVista()
        {
            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(new List<MenuUser>());
            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["MenuUserList"]);
        }


        [Fact]
        public async Task AgregarEvento_DeberiaAgregarEventoVariosUsuarios()
        {
            _output.WriteLine("Iniciando test AgregarEvento_DeberiaAgregarEventoVariosUsuarios");

            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Calendario>>();
            var mockSetUsuarios = new Mock<DbSet<Usuarios>>();
            var mockSetCalendarioMultiUsuarios = new Mock<DbSet<CalendarioMultiUsuarios>>();

            mockContext.Setup(m => m.Calendario).Returns(mockSet.Object);
            mockContext.Setup(m => m.Usuarios).Returns(mockSetUsuarios.Object);
            mockContext.Setup(m => m.CalendarioMultiUsuarios).Returns(mockSetCalendarioMultiUsuarios.Object);

            var usuarios = new List<Usuarios>
    {
        new Usuarios { Guid = Guid.NewGuid(), Usuario = "TestUser", Empresa = "TestEmpresa" },
        new Usuarios { Guid = Guid.NewGuid(), Usuario = "TestUser1", Empresa = "TestEmpresa" },
        new Usuarios { Guid = Guid.NewGuid(), Usuario = "TestUser2", Empresa = "TestEmpresa" }
    };

            _output.WriteLine($"Usuarios creados: {string.Join(", ", usuarios.Select(u => $"{u.Usuario} ({u.Guid})"))}");

            mockSetUsuarios.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] ids) => {
                    if (ids[0] is Guid guid)
                    {
                        var usuario = usuarios.FirstOrDefault(u => u.Guid == guid);
                        _output.WriteLine($"FindAsync llamado con Guid {guid}, usuario encontrado: {usuario?.Usuario ?? "null"}");
                        return usuario;
                    }
                    else if (ids[0] is string str)
                    {
                        var usuario = usuarios.FirstOrDefault(u => u.Usuario == str);
                        _output.WriteLine($"FindAsync llamado con String {str}, usuario encontrado: {usuario?.Usuario ?? "null"}");
                        return usuario;
                    }
                    _output.WriteLine($"FindAsync llamado con tipo inesperado: {ids[0]?.GetType().Name ?? "null"}");
                    return null;
                });

            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "ADMIN"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            var eventoDto = new CalendarioEventoDto
            {
                Titulo = "Evento Varios Usuarios",
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                HoraInicio = "10:00",
                HoraFin = "11:00",
                Alcance = "Varios usuarios",
                RecibirMail = true,
                Usuarios = usuarios.Select(u => u.Guid.ToString()).ToList()
            };

            _output.WriteLine($"EventoDto creado: {JsonConvert.SerializeObject(eventoDto)}");

            var loggedUsers = new List<string>();
            mockSetCalendarioMultiUsuarios.Setup(m => m.AddAsync(It.IsAny<CalendarioMultiUsuarios>(), It.IsAny<CancellationToken>()))
                .Callback<CalendarioMultiUsuarios, CancellationToken>((u, token) =>
                {
                    loggedUsers.Add(u.Usuario);
                    _output.WriteLine($"Usuario agregado a CalendarioMultiUsuarios: {u.Usuario}");
                })
                .ReturnsAsync((CalendarioMultiUsuarios u, CancellationToken token) => null);

            // Act
            _output.WriteLine("Ejecutando AgregarEvento");
            var result = await controller.AgregarEvento(eventoDto);

            // Assert
            _output.WriteLine($"Resultado: {JsonConvert.SerializeObject(result)}");
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            var eventoAgregado = Assert.IsType<Calendario>(objectResult.Value);
            Assert.Equal(eventoDto.Titulo, eventoAgregado.Titulo);
            Assert.Equal(eventoDto.Alcance, eventoAgregado.Alcance);

            _output.WriteLine($"Usuarios agregados: {string.Join(", ", loggedUsers)}");
            Assert.Equal(3, loggedUsers.Count);
            Assert.Contains("TestUser", loggedUsers);
            Assert.Contains("TestUser1", loggedUsers);
            Assert.Contains("TestUser2", loggedUsers);

            mockSetCalendarioMultiUsuarios.Verify(m => m.AddAsync(It.IsAny<CalendarioMultiUsuarios>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            _output.WriteLine("Test completado");
        }


        [Fact]
        public async Task AgregarEvento_DeberiaAgregarUsuarioSesionCuandoListaUsuariosEsNula()
        {
            _output.WriteLine("Iniciando test AgregarEvento_DeberiaAgregarUsuarioSesionCuandoListaUsuariosEsNula");

            // Arrange
            var mockContext = new Mock<DbContextiLabPlus>();
            var mockSet = new Mock<DbSet<Calendario>>();
            var mockSetUsuarios = new Mock<DbSet<Usuarios>>();
            var mockSetCalendarioMultiUsuarios = new Mock<DbSet<CalendarioMultiUsuarios>>();

            mockContext.Setup(m => m.Calendario).Returns(mockSet.Object);
            mockContext.Setup(m => m.Usuarios).Returns(mockSetUsuarios.Object);
            mockContext.Setup(m => m.CalendarioMultiUsuarios).Returns(mockSetCalendarioMultiUsuarios.Object);

            var usuarioSesion = new Usuarios { Guid = Guid.NewGuid(), Usuario = "TestUser", Empresa = "TestEmpresa" };
            _output.WriteLine($"Usuario de sesión creado: {usuarioSesion.Usuario} ({usuarioSesion.Guid})");

            mockSetUsuarios.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] ids) => {
                    if (ids[0] is string str && str == "TestUser")
                    {
                        _output.WriteLine($"FindAsync llamado con {str}, usuario encontrado: {usuarioSesion.Usuario}");
                        return usuarioSesion;
                    }
                    if (ids[0] is Guid guid && guid == usuarioSesion.Guid)
                    {
                        _output.WriteLine($"FindAsync llamado con {guid}, usuario encontrado: {usuarioSesion.Usuario}");
                        return usuarioSesion;
                    }
                    _output.WriteLine($"FindAsync llamado con {ids[0]}, usuario no encontrado");
                    return null;
                });

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext.Object, Mock.Of<IHttpContextAccessor>());
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "ADMIN"
            });

            var controller = new CalendarioController(mockContext.Object, mockFunctionsBBDD.Object, _output);

            var eventoDto = new CalendarioEventoDto
            {
                Titulo = "Evento Varios Usuarios",
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                HoraInicio = "10:00",
                HoraFin = "11:00",
                Alcance = "Varios usuarios",
                RecibirMail = true,
                Usuarios = null  // Establecemos Usuarios como null para probar este caso
            };

            _output.WriteLine($"EventoDto creado: {JsonConvert.SerializeObject(eventoDto)}");

            var usuariosAgregados = new List<string>();
            mockSetCalendarioMultiUsuarios.Setup(m => m.AddAsync(It.IsAny<CalendarioMultiUsuarios>(), It.IsAny<CancellationToken>()))
                .Callback<CalendarioMultiUsuarios, CancellationToken>((u, token) =>
                {
                    usuariosAgregados.Add(u.Usuario);
                    _output.WriteLine($"Usuario agregado a CalendarioMultiUsuarios: {u.Usuario}");
                })
                .ReturnsAsync((CalendarioMultiUsuarios u, CancellationToken token) => null);

            // Act
            _output.WriteLine("Ejecutando AgregarEvento");
            var result = await controller.AgregarEvento(eventoDto);

            // Assert
            _output.WriteLine($"Resultado: {JsonConvert.SerializeObject(result)}");
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            var eventoAgregado = Assert.IsType<Calendario>(objectResult.Value);
            Assert.Equal(eventoDto.Titulo, eventoAgregado.Titulo);
            Assert.Equal(eventoDto.Alcance, eventoAgregado.Alcance);

            _output.WriteLine($"Usuarios agregados a CalendarioMultiUsuarios: {string.Join(", ", usuariosAgregados)}");
            _output.WriteLine($"EventoDto.Usuarios después de AgregarEvento: {string.Join(", ", eventoDto.Usuarios ?? new List<string>())}");

            Assert.NotNull(eventoDto.Usuarios);
            Assert.Contains(usuarioSesion.Guid.ToString(), eventoDto.Usuarios);

            // Verificamos que se haya llamado a AddAsync para CalendarioMultiUsuarios
            mockSetCalendarioMultiUsuarios.Verify(m => m.AddAsync(It.Is<CalendarioMultiUsuarios>(cm => cm.Usuario == usuarioSesion.Usuario), It.IsAny<CancellationToken>()), Times.Once());

            _output.WriteLine("Test completado");
        }




    }

    // Método de extensión para crear un mock de DbSet
    public static class MockDbSetExtensions
    {
        public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }

    
}
