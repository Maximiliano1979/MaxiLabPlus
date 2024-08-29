using iLabPlus.Controllers;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using static iLabPlus.Helpers.AsyncTestHelpers;

namespace iLabPlus.Tests
{
    public class ContactosControllerTests
    {
        private readonly Mock<DbContextiLabPlus> _mockContext;
        private readonly Mock<FunctionsBBDD> _mockFunctionsBBDD;
        private readonly Mock<ITestOutputHelper> _mockTestOutputHelper;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ContactosController _controller;

        public ContactosControllerTests(ITestOutputHelper testOutputHelper)
        {
            _mockContext = new Mock<DbContextiLabPlus>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockFunctionsBBDD = new Mock<FunctionsBBDD>(_mockContext.Object, mockHttpContextAccessor.Object);
            _mockTestOutputHelper = new Mock<ITestOutputHelper>();
            _testOutputHelper = testOutputHelper;

            // Setup mocks
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });
            _mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(new List<MenuUser>());
            _mockFunctionsBBDD.Setup(f => f.GetColumnsLayout(It.IsAny<string>())).Returns(new GrupoColumnsLayout
            {
                ColumnsLayoutUser = "TestLayout",
                ColumnsPinnedUser = 3
            });

            _controller = new ContactosController(
                _mockContext.Object,
                _mockFunctionsBBDD.Object,
                _mockTestOutputHelper.Object
            );
        }

        // Método helper para logs en los tests
        private void Log(string message)
        {
            _testOutputHelper.WriteLine(message);
        }




        [Fact]
        public async Task Index_DeberiaRetornarVistaConContactos()
        {
            // Arrange
            _mockTestOutputHelper.Setup(x => x.WriteLine(It.IsAny<string>())).Verifiable();

            var contactos = new List<Contactos>
            {
                new Contactos { Guid = Guid.NewGuid(), Nombre = "Contacto 1", Empresa = "TestEmpresa", Activo = true },
                new Contactos { Guid = Guid.NewGuid(), Nombre = "Contacto 2", Empresa = "TestEmpresa", Activo = true }
            };

            var mockDbSet = new Mock<DbSet<Contactos>>();

            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.Provider).Returns(contactos.AsQueryable().Provider);
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.Expression).Returns(contactos.AsQueryable().Expression);
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.ElementType).Returns(contactos.AsQueryable().ElementType);
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.GetEnumerator()).Returns(contactos.GetEnumerator());

            mockDbSet.As<IAsyncEnumerable<Contactos>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Contactos>(contactos.GetEnumerator()));

            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Contactos>(contactos.AsQueryable().Provider));

            _mockContext.Setup(c => c.Contactos).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<Contactos>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("Contacto 1", model[0].Nombre);
            Assert.Equal("Contacto 2", model[1].Nombre);

            _mockTestOutputHelper.Verify(x => x.WriteLine(It.IsAny<string>()), Times.AtLeastOnce());
        }


        [Fact]
        public async Task Index_CuandoOcurreExcepcion_DeberiaPropagar()
        {
            // Arrange
            _mockContext.Setup(c => c.Contactos).Throws(new Exception("Error de prueba"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Index());
        }



        [Fact]
        public async Task DetallesContacto_CuandoContactoExiste_DeberiaRetornarJson()
        {
            // Arrange
            var contactoId = Guid.NewGuid();
            var contacto = new Contactos { Guid = contactoId, Nombre = "Test Contact", Empresa = "TestEmpresa" };

            var mockDbSet = new Mock<DbSet<Contactos>>();
            mockDbSet.Setup(m => m.FindAsync(It.Is<object[]>(id => id[0].Equals(contactoId))))
                .ReturnsAsync(contacto);

            _mockContext.Setup(c => c.Contactos).Returns(mockDbSet.Object);

            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            // Act
            var result = await _controller.DetallesContacto(contactoId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var returnedContacto = Assert.IsType<Contactos>(jsonResult.Value);
            Assert.Equal(contactoId, returnedContacto.Guid);
            Assert.Equal("Test Contact", returnedContacto.Nombre);
        }


        [Fact]
        public async Task DetallesContacto_CuandoContactoNoExiste_DeberiaRetornarNotFound()
        {
            // Arrange
            var contactoId = Guid.NewGuid();

            var mockDbSet = new Mock<DbSet<Contactos>>();
            mockDbSet.Setup(m => m.FindAsync(It.Is<object[]>(id => id[0].Equals(contactoId))))
                .ReturnsAsync((Contactos)null);

            _mockContext.Setup(c => c.Contactos).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.DetallesContacto(contactoId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }




        [Fact]
        public async Task DialogContacto_CuandoIdEsNull_DeberiaRetornarNuevoContacto()
        {
            // Act
            var result = await _controller.DialogContacto();

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DialogContacto", partialViewResult.ViewName);
            var model = Assert.IsType<Contactos>(partialViewResult.Model);
            Assert.Equal(Guid.Empty, model.Guid);
        }


        [Fact]
        public async Task DialogContacto_CuandoContactoExiste_DeberiaRetornarContacto()
        {
            // Arrange
            var contactoId = Guid.NewGuid();
            var contacto = new Contactos { Guid = contactoId, Nombre = "Test Contact" };

            var mockDbSet = new Mock<DbSet<Contactos>>();
            mockDbSet.Setup(m => m.FindAsync(It.Is<object[]>(id => id[0].Equals(contactoId))))
                .ReturnsAsync(contacto);

            _mockContext.Setup(c => c.Contactos).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.DialogContacto(contactoId);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DialogContacto", partialViewResult.ViewName);
            var model = Assert.IsType<Contactos>(partialViewResult.Model);
            Assert.Equal(contactoId, model.Guid);
            Assert.Equal("Test Contact", model.Nombre);
        }


        [Fact]
        public async Task DialogContacto_CuandoContactoNoExiste_DeberiaRetornarNuevoContacto()
        {
            // Arrange
            var contactoId = Guid.NewGuid();

            var mockDbSet = new Mock<DbSet<Contactos>>();
            mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((Contactos)null);

            _mockContext.Setup(c => c.Contactos).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.DialogContacto(contactoId);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DialogContacto", partialViewResult.ViewName);
            var model = Assert.IsType<Contactos>(partialViewResult.Model);
            Assert.Equal(Guid.Empty, model.Guid);
        }


        // TEST PARA CREAR Y ELIMINAR CONTACTOS


        [Fact]
        public async Task CrearEditarContacto_CuandoEsNuevoContacto_DeberiaCrearContacto()
        {
            // Arrange
            var nuevoContacto = new Contactos
            {
                Guid = Guid.Empty,
                Nombre = "Nuevo Contacto",
                Email = "nuevo@ejemplo.com"
            };

            Log("Configurando mock de DbSet");
            var data = new List<Contactos>().AsQueryable();
            var mockDbSet = new Mock<DbSet<Contactos>>();

            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Contactos>(data.Provider));
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.Expression).Returns(data.Expression);
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            mockDbSet.As<IAsyncEnumerable<Contactos>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Contactos>(data.GetEnumerator()));

            mockDbSet.As<IQueryable<Contactos>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Contactos>(data.Provider));

            Log("Configurando mock de DbContext");
            _mockContext.Setup(c => c.Contactos).Returns(mockDbSet.Object);

            Log("Configurando mock de SaveChangesAsync");
            _mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            Log("Configurando mock de GrupoClaims");
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            Log("Configurando mock de User.Identity");
            var mockPrincipal = new Mock<ClaimsPrincipal>();
            var mockIdentity = new Mock<ClaimsIdentity>();
            mockIdentity.Setup(i => i.Name).Returns("TestUser");
            mockPrincipal.Setup(p => p.Identity).Returns(mockIdentity.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = mockPrincipal.Object }
            };

            // Act
            Log("Ejecutando CrearEditarContacto");
            var result = await _controller.CrearEditarContacto(nuevoContacto);

            // Assert
            Log("Iniciando aserciones");
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic resultValue = jsonResult.Value;

            Log($"Result success: {resultValue.success}");
            Log($"Result message: {resultValue.message}");

            if (!(bool)resultValue.success)
            {
                Log($"Error details: {resultValue.message}");
            }

            Assert.True((bool)resultValue.success, $"Expected success to be true. Actual value: {resultValue.success}. Message: {resultValue.message}");
            Assert.Equal("Contacto creado exitosamente.", (string)resultValue.message);

            Log("Verificando llamadas a métodos mock");
            mockDbSet.Verify(m => m.Add(It.IsAny<Contactos>()), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }



        [Fact]
        public async Task CrearEditarContacto_CuandoContactoYaExiste_DeberiaRetornarError()
        {
            // Arrange
            var contactoExistente = new Contactos
            {
                Guid = Guid.Empty,
                Email = "existente@ejemplo.com",
                Empresa = "TestEmpresa"
            };

            var data = new List<Contactos> { contactoExistente }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Contactos>>();

            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Contactos>(data.Provider));
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.Expression).Returns(data.Expression);
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            mockDbSet.As<IAsyncEnumerable<Contactos>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Contactos>(data.GetEnumerator()));

            mockDbSet.As<IQueryable<Contactos>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Contactos>(data.Provider));

            _mockContext.Setup(c => c.Contactos).Returns(mockDbSet.Object);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            // Act
            var result = await _controller.CrearEditarContacto(contactoExistente);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic resultValue = jsonResult.Value;

            Assert.False((bool)resultValue.success);
            Assert.Equal("El contacto ya existe.", (string)resultValue.message);
        }



        [Fact]
        public async Task EliminarContacto_CuandoContactoExiste_DeberiaEliminarContacto()
        {
            // Arrange
            var contactoId = Guid.NewGuid();
            var contacto = new Contactos { Guid = contactoId };

            var mockDbSet = new Mock<DbSet<Contactos>>();
            mockDbSet.Setup(m => m.FindAsync(It.Is<object[]>(id => id[0].Equals(contactoId))))
                .ReturnsAsync(contacto);

            _mockContext.Setup(c => c.Contactos).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.EliminarContacto(contactoId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<JsonResult>(result).Value as dynamic;
            Assert.True(resultValue.success);
            Assert.Equal("Contacto eliminado Correctamente", resultValue.message);
            mockDbSet.Verify(m => m.Remove(It.IsAny<Contactos>()), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task EliminarContacto_CuandoContactoNoExiste_DeberiaRetornarError()
        {
            // Arrange
            var contactoId = Guid.NewGuid();

            var mockDbSet = new Mock<DbSet<Contactos>>();
            mockDbSet.Setup(m => m.FindAsync(It.Is<object[]>(id => id[0].Equals(contactoId))))
                .ReturnsAsync((Contactos)null);

            _mockContext.Setup(c => c.Contactos).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.EliminarContacto(contactoId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = Assert.IsType<JsonResult>(result).Value as dynamic;
            Assert.False(resultValue.success);
            Assert.Equal("Contacto no encontrado", resultValue.message);
        }



        [Fact]
        public async Task Index_CuandoGrupoColumnsLayoutEsNull_DeberiaLogearMensaje()
        {
            // Arrange
            _mockFunctionsBBDD.Setup(f => f.GetColumnsLayout(It.IsAny<string>())).Returns((GrupoColumnsLayout)null);

            var mockLogger = new Mock<ILogger<ContactosController>>();
            var controller = new ContactosController(
                _mockContext.Object,
                _mockFunctionsBBDD.Object,
                _mockTestOutputHelper.Object,
                mockLogger.Object
            );

            // Act
            await controller.Index();

            // Assert
            _mockTestOutputHelper.Verify(
                x => x.WriteLine("GrupoColumnsLayout es null, no se pudieron configurar ColumnsLayout y ColumnsPinned"),
                Times.Once);
        }

        [Fact]
        public async Task Index_CuandoGrupoClaimsEsNull_DeberiaRetornarBadRequest()
        {
            // Arrange
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns((GrupoClaims)null);

            var controller = new ContactosController(
                _mockContext.Object,
                _mockFunctionsBBDD.Object,
                _mockTestOutputHelper.Object
            );

            // Act
            var result = await controller.Index();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No se pudo obtener la información de la empresa", badRequestResult.Value);

            _mockTestOutputHelper.Verify(
                x => x.WriteLine("GrupoClaims o SessionEmpresa es null o vacío"),
                Times.Once);
        }

        [Fact]
        public async Task Index_CuandoContactosEsNull_DeberiaRetornarBadRequest()
        {
            // Arrange
            _mockContext.Setup(c => c.Contactos).Returns((DbSet<Contactos>)null);

            // Act
            var result = await _controller.Index();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No se pudo acceder a la base de datos de contactos", badRequestResult.Value);

            _mockTestOutputHelper.Verify(
                x => x.WriteLine("ctxDB.Contactos es null"),
                Times.Once);
        }


        [Fact]
        public async Task CrearEditarContacto_CuandoOcurreExcepcionAlCrear_DeberiaRetornarError()
        {
            // Arrange
            var nuevoContacto = new Contactos
            {
                Guid = Guid.Empty,
                Nombre = "Nuevo Contacto",
                Email = "nuevo@ejemplo.com"
            };

            _mockContext.Setup(c => c.Contactos.Add(It.IsAny<Contactos>())).Throws(new Exception("Error de prueba"));

            // Act
            var result = await _controller.CrearEditarContacto(nuevoContacto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic resultValue = jsonResult.Value;
            Assert.False((bool)resultValue.success);
            Assert.Contains("Error al buscar o crear contacto", (string)resultValue.message);

            _mockTestOutputHelper.Verify(
                x => x.WriteLine(It.Is<string>(s => s.StartsWith("Error al buscar o crear contacto"))),
                Times.Once);
        }

        [Fact]
        public async Task CrearEditarContacto_CuandoContactoExistenteNoSeEncuentra_DeberiaRetornarError()
        {
            // Arrange
            var contactoExistente = new Contactos
            {
                Guid = Guid.NewGuid(),
                Nombre = "Contacto Existente",
                Email = "existente@ejemplo.com"
            };

            var mockDbSet = new Mock<DbSet<Contactos>>();
            mockDbSet.As<IQueryable<Contactos>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Contactos>(new List<Contactos>().AsQueryable().Provider));
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.Expression).Returns(new List<Contactos>().AsQueryable().Expression);
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.ElementType).Returns(new List<Contactos>().AsQueryable().ElementType);
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.GetEnumerator()).Returns(new List<Contactos>().GetEnumerator());

            _mockContext.Setup(c => c.Contactos).Returns(mockDbSet.Object);

            // Act
            var result = await _controller.CrearEditarContacto(contactoExistente);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic resultValue = jsonResult.Value;
            Assert.False((bool)resultValue.success);
            Assert.Equal("Contacto no encontrado.", (string)resultValue.message);

            _mockTestOutputHelper.Verify(
                x => x.WriteLine("Contacto no encontrado"),
                Times.Once);
        }




        [Fact]
        public async Task CrearEditarContacto_CuandoOcurreExcepcionAlActualizar_DeberiaRetornarError()
        {
            // Arrange
            var contactoExistente = new Contactos
            {
                Guid = Guid.NewGuid(),
                Nombre = "Contacto Existente",
                Email = "existente@ejemplo.com",
                Empresa = "TestEmpresa"
            };

            var mockDbSet = new Mock<DbSet<Contactos>>();
            mockDbSet.As<IQueryable<Contactos>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Contactos>(new List<Contactos> { contactoExistente }.AsQueryable().Provider));
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.Expression).Returns(new List<Contactos> { contactoExistente }.AsQueryable().Expression);
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.ElementType).Returns(new List<Contactos> { contactoExistente }.AsQueryable().ElementType);
            mockDbSet.As<IQueryable<Contactos>>().Setup(m => m.GetEnumerator()).Returns(new List<Contactos> { contactoExistente }.GetEnumerator());

            // Setup para FirstOrDefaultAsync
            mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(contactoExistente);

            _mockContext.Setup(c => c.Contactos).Returns(mockDbSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Error de prueba"));

            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            // Act
            var result = await _controller.CrearEditarContacto(contactoExistente);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic resultValue = jsonResult.Value;
            Assert.False((bool)resultValue.success);
            Assert.Contains("Error al actualizar contacto", (string)resultValue.message);

            _mockTestOutputHelper.Verify(
                x => x.WriteLine(It.Is<string>(s => s.StartsWith("Error al actualizar contacto"))),
                Times.Once);
        }






        [Fact]
        public async Task CrearEditarContacto_CuandoContactoEsNull_DeberiaRetornarError()
        {
            // Act
            var result = await _controller.CrearEditarContacto(null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic resultValue = jsonResult.Value;
            Assert.False((bool)resultValue.success);
            Assert.Equal("Error: El contacto proporcionado es nulo.", (string)resultValue.message);

            _mockTestOutputHelper.Verify(
                x => x.WriteLine("Error: contacto es null"),
                Times.Once);
        }

        [Fact]
        public async Task CrearEditarContacto_CuandoDbContextEsNull_DeberiaRetornarError()
        {
            // Arrange
            var controller = new ContactosController(
                null,
                _mockFunctionsBBDD.Object,
                _mockTestOutputHelper.Object
            );

            // Act
            var result = await controller.CrearEditarContacto(new Contactos());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic resultValue = jsonResult.Value;
            Assert.False((bool)resultValue.success);
            Assert.Equal("Error interno del servidor: Base de datos no disponible.", (string)resultValue.message);

            _mockTestOutputHelper.Verify(
                x => x.WriteLine("Error: ctxDB o ctxDB.Contactos es null"),
                Times.Once);
        }

        [Fact]
        public async Task CrearEditarContacto_CuandoContactosEsNull_DeberiaRetornarError()
        {
            // Arrange
            _mockContext.Setup(c => c.Contactos).Returns((DbSet<Contactos>)null);

            // Act
            var result = await _controller.CrearEditarContacto(new Contactos());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic resultValue = jsonResult.Value;
            Assert.False((bool)resultValue.success);
            Assert.Equal("Error interno del servidor: Base de datos no disponible.", (string)resultValue.message);

            _mockTestOutputHelper.Verify(
                x => x.WriteLine("Error: ctxDB o ctxDB.Contactos es null"),
                Times.Once);
        }



        [Fact]
        public async Task CrearEditarContacto_CuandoExcepcionEnActualizacion_DeberiaRetornarError()
        {
            // Arrange
            var contactoExistente = new Contactos
            {
                Guid = Guid.NewGuid(),
                Nombre = "Contacto Existente",
                Email = "existente@ejemplo.com",
                Empresa = "TestEmpresa"
            };

            var mockDbSet = new Mock<DbSet<Contactos>>();
            mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(contactoExistente);

            _mockContext.Setup(c => c.Contactos).Returns(mockDbSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Error de prueba en actualización"));

            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            // Act
            var result = await _controller.CrearEditarContacto(contactoExistente);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic resultValue = jsonResult.Value;
            Assert.False((bool)resultValue.success);
            Assert.Contains("Error al actualizar contacto", (string)resultValue.message);

            _mockTestOutputHelper.Verify(
                x => x.WriteLine(It.Is<string>(s => s.StartsWith("Error al actualizar contacto"))),
                Times.Once);
            _mockTestOutputHelper.Verify(
                x => x.WriteLine(It.Is<string>(s => s.StartsWith("StackTrace:"))),
                Times.Once);
        }

        [Fact]
        public async Task CrearEditarContacto_CuandoExcepcionGeneral_DeberiaRetornarError()
        {
            // Arrange
            var contactoExistente = new Contactos
            {
                Guid = Guid.NewGuid(),
                Nombre = "Contacto Existente",
                Email = "existente@ejemplo.com",
                Empresa = "TestEmpresa"
            };

            _mockContext.Setup(c => c.Contactos).Throws(new Exception("Error general de prueba"));

            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            // Act
            var result = await _controller.CrearEditarContacto(contactoExistente);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic resultValue = jsonResult.Value;
            Assert.False((bool)resultValue.success);
            Assert.Contains("Error:", (string)resultValue.message);

            _mockTestOutputHelper.Verify(
                x => x.WriteLine(It.Is<string>(s => s.StartsWith("Error en CrearEditarContacto"))),
                Times.Once);
            _mockTestOutputHelper.Verify(
                x => x.WriteLine(It.Is<string>(s => s.StartsWith("StackTrace:"))),
                Times.Once);
        }



    }

    // Clases de soporte para operaciones asíncronas en los mocks
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

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(expression);
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