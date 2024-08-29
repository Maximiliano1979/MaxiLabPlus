using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using iLabPlus.Controllers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Newtonsoft.Json;
using Nemesis365.Controllers;
using Xunit.Abstractions;
using iLabPlus.Models.Clases;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using iLabPlus.Tests.TestHelpers;

namespace iLabPlus.Tests
{
    public class ArticulosControllerTests

    {

        private readonly ITestOutputHelper _output;

        public ArticulosControllerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private DbContextiLabPlus CreateMockContext(List<Articulos> articulos)
        {
            var options = new DbContextOptionsBuilder<DbContextiLabPlus>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

            var context = new DbContextiLabPlus(options);

            context.Articulos.AddRange(articulos);
            context.UsuariosGridsCfg.Add(new UsuariosGridsCfg
            {
                Empresa = "TestEmpresa",
                Usuario = "TestUsuario",
                GridID = "gridArticulos",
                ColumnsLayout = "SomeLayout",
                ColumnsPinned = 3
            });
            context.SaveChanges();

            return context;
        }

        private FunctionsBBDD CreateFunctionsBBDD(DbContextiLabPlus context)
        {
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
                {
                    new Claim("Empresa", "TestEmpresa"),
                    new Claim("Usuario", "TestUsuario")
                };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            httpContext.User = claimsPrincipal;

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

            return new FunctionsBBDD(context, httpContextAccessor.Object);
        }

        [Fact]
        public async Task HistorificarArticulos_DeberiaToggleActivoParaArticulosExistentes()
        {
            // Arrange
            var articulos = new List<Articulos>
                {
                    new Articulos { Guid = Guid.NewGuid(), Articulo = "ART001", Activo = true, Empresa = "TestEmpresa" },
                    new Articulos { Guid = Guid.NewGuid(), Articulo = "ART002", Activo = false, Empresa = "TestEmpresa" }
                };
            var context = CreateMockContext(articulos);
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();

            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.HistorificarArticulos(articulos.Select(a => a.Guid).ToList());

            // Debug
            var okResult = Assert.IsType<OkObjectResult>(result);
            Console.WriteLine($"Result type: {okResult.Value.GetType()}");
            Console.WriteLine($"Result value: {JsonConvert.SerializeObject(okResult.Value)}");

            // Assert
            Assert.NotNull(okResult.Value);
            var properties = okResult.Value.GetType().GetProperties();
            foreach (var prop in properties)
            {
                Console.WriteLine($"Property: {prop.Name}, Value: {prop.GetValue(okResult.Value)}");
            }

            // Verificar que Activo se haya cambiado para cada artículo
            var updatedArticulos = await context.Articulos.ToListAsync();
            Assert.False(updatedArticulos[0].Activo);
            Assert.True(updatedArticulos[1].Activo);
        }

        [Fact]
        public async Task HistorificarArticulos_DeberiaRetornarNotFoundSiNoHayArticulos()
        {
            // Arrange
            var context = CreateMockContext(new List<Articulos>());
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();

            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.HistorificarArticulos(new List<Guid> { Guid.NewGuid() });

            // Debug
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Console.WriteLine($"Result type: {notFoundResult.Value.GetType()}");
            Console.WriteLine($"Result value: {JsonConvert.SerializeObject(notFoundResult.Value)}");

            // Assert
            Assert.NotNull(notFoundResult.Value);
            var properties = notFoundResult.Value.GetType().GetProperties();
            foreach (var prop in properties)
            {
                Console.WriteLine($"Property: {prop.Name}, Value: {prop.GetValue(notFoundResult.Value)}");
            }

            // No necesitamos verificar los valores específicos aquí, ya que estamos debuggeando
        }

        [Fact]
        public void Index_DeberiaRetornarVistaConDatosCorrectos()
        {
            // Arrange
            var context = CreateMockContext(new List<Articulos>());
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = controller.Index(Guid.Empty) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Articulos", result.ViewName);
            Assert.NotNull(result.ViewData["MenuUserList"]);
            Assert.NotNull(result.ViewData["ColumnsLayoutUser"]);
            Assert.NotNull(result.ViewData["ColumnsPinnedUser"]);
        }

        [Fact]
        public async Task Articulo_Renombrar_DeberiaRenombrarArticuloCorrectamente()
        {
            // Arrange
            var articuloGuid = Guid.NewGuid();
            var articulos = new List<Articulos>
            {
                new Articulos { Guid = articuloGuid, Articulo = "ART001", Activo = true, Empresa = "TestEmpresa" }
            };
            var context = CreateMockContext(articulos);
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.Articulo_Renombrar("ART001", "ART001_NUEVO") as JsonResult;

            // Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);

            // Deserializar el resultado a un objeto anónimo
            var resultValue = JsonConvert.DeserializeAnonymousType(JsonConvert.SerializeObject(jsonResult.Value), new { success = false, data = new Articulos() });

            _output.WriteLine($"Result: {JsonConvert.SerializeObject(resultValue)}");

            // Verificar la estructura del resultado
            Assert.True(resultValue.success);
            Assert.NotNull(resultValue.data);

            // Verificar los datos del artículo actualizado
            Assert.Equal(articuloGuid, resultValue.data.Guid);
            Assert.Equal("TestEmpresa", resultValue.data.Empresa);
            Assert.Equal("ART001_NUEVO", resultValue.data.Articulo);
            Assert.True(resultValue.data.Activo);

            // Verificar que el artículo se haya actualizado en el contexto
            var updatedArticulo = await context.Articulos.FirstOrDefaultAsync(a => a.Guid == articuloGuid);
            Assert.NotNull(updatedArticulo);
            Assert.Equal("ART001_NUEVO", updatedArticulo.Articulo);

            _output.WriteLine($"Artículo actualizado en el contexto: {JsonConvert.SerializeObject(updatedArticulo)}");
        }


        //[Fact]
        //public async Task GetDataArticulos_DeberiaRetornarDatosCorrectos()
        //{
        //    // Arrange
        //    var articulos = new List<Articulos>
        //        {
        //            new Articulos { Guid = Guid.NewGuid(), Articulo = "ART001", Activo = true, Empresa = "TestEmpresa" },
        //            new Articulos { Guid = Guid.NewGuid(), Articulo = "ART002", Activo = false, Empresa = "TestEmpresa" }
        //        };
        //    var context = CreateMockContext(articulos);
        //    var functionsBBDD = CreateFunctionsBBDD(context);
        //    var mockConfiguration = new Mock<IConfiguration>();
        //    var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

        //    // Act
        //    var actionResult = await controller.GetDataArticulos("Todos");
        //    var result = actionResult as JsonResult;

        //    // Assert
        //    Assert.NotNull(result);
        //    var data = JsonConvert.DeserializeObject<List<Articulos>>(JsonConvert.SerializeObject(result.Value));
        //    Assert.Equal(2, data.Count);
        //    Assert.Contains(data, a => a.Articulo == "ART001");
        //    Assert.Contains(data, a => a.Articulo == "ART002");

        //    // Imprimir para debug
        //    _output.WriteLine($"Resultado: {JsonConvert.SerializeObject(data)}");
        //}

        [Fact]
        public async Task Articulo_Copiar_DeberiaCrearNuevoArticulo()
        {
            // Arrange
            var articuloGuid = Guid.NewGuid();
            var articulos = new List<Articulos>
            {
                new Articulos { Guid = articuloGuid, Articulo = "ART001", Activo = true, Empresa = "TestEmpresa" }
            };
            var context = CreateMockContext(articulos);
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.Articulo_Copiar("ART001", "ART001_COPIA") as JsonResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(result.Value));
            Assert.True(resultValue.ContainsKey("success"));
            Assert.True((bool)resultValue["success"]);
            Assert.True(resultValue.ContainsKey("data"));

            var data = JsonConvert.DeserializeObject<Articulos>(JsonConvert.SerializeObject(resultValue["data"]));
            Assert.Equal("ART001_COPIA", data.Articulo);

            var copiedArticulo = await context.Articulos.FirstOrDefaultAsync(a => a.Articulo == "ART001_COPIA");
            Assert.NotNull(copiedArticulo);
            _output.WriteLine($"Artículo copiado: {JsonConvert.SerializeObject(copiedArticulo)}");
        }


        [Fact]
        public async Task GetArticuloLazing_DeberiaRetornarArticulosFiltrados()
        {
            // Arrange
            var articulos = new List<Articulos>
            {
                new Articulos { Guid = Guid.NewGuid(), Articulo = "ART001", ArtDes = "Descripción 1", Empresa = "TestEmpresa" },
                new Articulos { Guid = Guid.NewGuid(), Articulo = "ART002", ArtDes = "Descripción 2", Empresa = "TestEmpresa" },
                new Articulos { Guid = Guid.NewGuid(), Articulo = "OTR001", ArtDes = "Otra descripción", Empresa = "TestEmpresa" }
            };
            var context = CreateMockContext(articulos);
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.GetArticuloLazing("ART", 10) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(result.Value));
            Assert.True((bool)resultValue["Success"]);
            var data = JsonConvert.DeserializeObject<List<ArtComponentes>>(JsonConvert.SerializeObject(resultValue["Data"]));
            Assert.Equal(2, data.Count);
            _output.WriteLine($"Resultado: {JsonConvert.SerializeObject(data)}");
        }

        [Fact]
        public async Task EliminarMultiplesArticulos_DeberiaEliminarArticulosSeleccionados()
        {
            // Arrange
            var articulos = new List<Articulos>
            {
                new Articulos { Guid = Guid.NewGuid(), Articulo = "ART001", Activo = true, Empresa = "TestEmpresa" },
                new Articulos { Guid = Guid.NewGuid(), Articulo = "ART002", Activo = true, Empresa = "TestEmpresa" },
                new Articulos { Guid = Guid.NewGuid(), Articulo = "ART003", Activo = true, Empresa = "TestEmpresa" }
            };
            var context = CreateMockContext(articulos);
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);
            var guidsToDelete = articulos.Take(2).Select(a => a.Guid).ToList();

            // Act
            var result = await controller.EliminarMultiplesArticulos(guidsToDelete) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(result.Value));
            Assert.True(resultValue.ContainsKey("success"));
            Assert.True((bool)resultValue["success"]);

            var remainingArticulos = await context.Articulos.ToListAsync();
            Assert.Single(remainingArticulos);
            Assert.Equal("ART003", remainingArticulos[0].Articulo);
            _output.WriteLine($"Artículos restantes: {JsonConvert.SerializeObject(remainingArticulos)}");
        }

        [Fact]
        public void Index_ConGuidEspecifico_DeberiaRetornarVistaConRepositionArt()
        {
            // Arrange
            var context = CreateMockContext(new List<Articulos>());
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);
            var guid = Guid.NewGuid();

            // Act
            var result = controller.Index(guid) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Articulos", result.ViewName);
            Assert.Equal(guid, result.ViewData["RepositionArt"]);
        }

        [Theory]
        [InlineData("Todos")]
        [InlineData("Activos")]
        [InlineData("Inactivos")]
        public async Task GetDataArticulos_ConDiferentesValoresDeActivo_DeberiaFiltrarCorrectamente(string activo)
        {
            // Arrange
            var articulos = new List<Articulos>
            {
                new Articulos { Guid = Guid.NewGuid(), Articulo = "ART001", Activo = true, Empresa = "TestEmpresa" },
                new Articulos { Guid = Guid.NewGuid(), Articulo = "ART002", Activo = false, Empresa = "TestEmpresa" },
                new Articulos { Guid = Guid.NewGuid(), Articulo = "ART003", Activo = true, Empresa = "TestEmpresa" }
            };
            var context = CreateMockContext(articulos);
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.GetDataArticulos(activo) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var data = JsonConvert.DeserializeObject<List<Articulos>>(JsonConvert.SerializeObject(result.Value));

            switch (activo)
            {
                case "Todos":
                    Assert.Equal(3, data.Count);
                    break;
                case "Activos":
                    Assert.Equal(2, data.Count);
                    Assert.All(data, item => Assert.True(item.Activo));
                    break;
                case "Inactivos":
                    Assert.Single(data);
                    Assert.All(data, item => Assert.False(item.Activo));
                    break;
            }

            _output.WriteLine($"Resultado para {activo}: {JsonConvert.SerializeObject(data)}");
        }

        [Fact]
        public async Task Articulo_Renombrar_ArticuloNoExistente_DeberiaRetornarError()
        {
            // Arrange
            var context = CreateMockContext(new List<Articulos>());
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.Articulo_Renombrar("NOEXISTE", "NUEVO") as JsonResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(result.Value));
            Assert.False((bool)resultValue["success"]);
            Assert.Equal("Artículo no encontrado.", resultValue["message"]);
        }

        [Fact]
        public async Task Articulo_Copiar_NombreExistente_DeberiaRetornarError()
        {
            // Arrange
            var articulos = new List<Articulos>
            {
                new Articulos { Guid = Guid.NewGuid(), Articulo = "ART001", Activo = true, Empresa = "TestEmpresa" },
                new Articulos { Guid = Guid.NewGuid(), Articulo = "ART002", Activo = true, Empresa = "TestEmpresa" }
            };
            var context = CreateMockContext(articulos);
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.Articulo_Copiar("ART001", "ART002") as JsonResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(result.Value));
            Assert.False((bool)resultValue["success"]);
            Assert.Equal("El artículo nuevo ya existe.", resultValue["message"]);
        }

        [Fact]
        public async Task EliminarMultiplesArticulos_ListaVacia_DeberiaRetornarNotFound()
        {
            // Arrange
            var context = CreateMockContext(new List<Articulos>());
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.EliminarMultiplesArticulos(new List<Guid>());

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var resultValue = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(notFoundResult.Value));
            Assert.False((bool)resultValue["success"]);
            Assert.Equal("No se encontraron artículos para eliminar.", resultValue["message"]);
        }

        [Fact]
        public void DialogArtRENCOP_DeberiaRetornarPartialViewConArticuloOld()
        {
            // Arrange
            var context = CreateMockContext(new List<Articulos>());
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);
            var articuloTest = "ART001";

            // Act
            var result = controller._DialogArtRENCOP(articuloTest) as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("_DialogArtRENCOP", result.ViewName);
            Assert.Equal(articuloTest, result.ViewData["ArticuloOld"]);
        }

        [Fact]
        public void DialogArtNEW_DeberiaRetornarPartialView()
        {
            // Arrange
            var context = CreateMockContext(new List<Articulos>());
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = controller._DialogArtNEW() as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("_DialogArtNEW", result.ViewName);
        }

        [Fact]
        public async Task Articulo_Renombrar_ArticuloYaExiste_DeberiaRetornarError()
        {
            // Arrange
            var articulos = new List<Articulos>
        {
            new Articulos { Guid = Guid.NewGuid(), Articulo = "ART001", Empresa = "TestEmpresa" },
            new Articulos { Guid = Guid.NewGuid(), Articulo = "ART002", Empresa = "TestEmpresa" }
        };
            var context = CreateMockContext(articulos);
            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.Articulo_Renombrar("ART001", "ART002") as JsonResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(result.Value));
            Assert.False((bool)resultValue["success"]);
            Assert.Equal("Artículo ya existe.", resultValue["message"]);
        }



        [Fact]
        public async Task Articulo_Renombrar_DeberiaActualizarTablasRelacionadas()
        {
            // Arrange
            var articuloGuid = Guid.NewGuid();
            var articulos = new List<Articulos>
            {
                new Articulos { Guid = articuloGuid, Articulo = "ART001", Empresa = "TestEmpresa" }
            };
                    var articulosMO = new List<ArticulosMO>
            {
                new ArticulosMO { Guid = Guid.NewGuid(), Articulo = "ART001", Empresa = "TestEmpresa" }
            };
                    var articulosCOMP = new List<ArticulosCOMP>
            {
                new ArticulosCOMP { Guid = Guid.NewGuid(), Articulo = "ART001", Empresa = "TestEmpresa" },
                new ArticulosCOMP { Guid = Guid.NewGuid(), Componente = "ART001", Empresa = "TestEmpresa" }
            };
                    var articulosIMG = new List<ArticulosIMG>
            {
                new ArticulosIMG { Guid = Guid.NewGuid(), Articulo = "ART001", Empresa = "TestEmpresa" }
            };

            var context = CreateMockContext(articulos);
            context.ArticulosMO.AddRange(articulosMO);
            context.ArticulosCOMP.AddRange(articulosCOMP);
            context.ArticulosIMG.AddRange(articulosIMG);
            context.SaveChanges();

            var functionsBBDD = CreateFunctionsBBDD(context);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.Articulo_Renombrar("ART001", "ART001_NUEVO") as JsonResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(result.Value));
            Assert.True((bool)resultValue["success"]);

            // Verificar que se actualizaron todas las tablas relacionadas
            Assert.Empty(await context.Articulos.Where(a => a.Articulo == "ART001").ToListAsync());
            Assert.Single(await context.Articulos.Where(a => a.Articulo == "ART001_NUEVO").ToListAsync());

            Assert.Empty(await context.ArticulosMO.Where(a => a.Articulo == "ART001").ToListAsync());
            Assert.Single(await context.ArticulosMO.Where(a => a.Articulo == "ART001_NUEVO").ToListAsync());

            Assert.Empty(await context.ArticulosCOMP.Where(a => a.Articulo == "ART001").ToListAsync());
            Assert.Single(await context.ArticulosCOMP.Where(a => a.Articulo == "ART001_NUEVO").ToListAsync());

            Assert.Empty(await context.ArticulosCOMP.Where(a => a.Componente == "ART001").ToListAsync());
            Assert.Single(await context.ArticulosCOMP.Where(a => a.Componente == "ART001_NUEVO").ToListAsync());

            Assert.Empty(await context.ArticulosIMG.Where(a => a.Articulo == "ART001").ToListAsync());
            Assert.Single(await context.ArticulosIMG.Where(a => a.Articulo == "ART001_NUEVO").ToListAsync());
        }

        [Fact]
        public async Task Articulo_Renombrar_DeberiaManejearExcepciones()
        {
            // Arrange
            var articulos = new List<Articulos>
            {
                new Articulos { Guid = Guid.NewGuid(), Articulo = "ART001", Empresa = "TestEmpresa" }
            };
            var mockContext = new Mock<DbContextiLabPlus>();
            mockContext.Setup(c => c.Articulos).Returns(MockDbSet(articulos));

            // Configurar otros DbSets necesarios
            mockContext.Setup(c => c.ArticulosMO).Returns(MockDbSet(new List<ArticulosMO>()));
            mockContext.Setup(c => c.ArticulosCOMP).Returns(MockDbSet(new List<ArticulosCOMP>()));
            mockContext.Setup(c => c.ArticulosIMG).Returns(MockDbSet(new List<ArticulosIMG>()));

            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));

            var usuariosGridsCfg = new List<UsuariosGridsCfg>
            {
                new UsuariosGridsCfg { Empresa = "TestEmpresa", Usuario = "TestUsuario", GridID = "gridArticulos", ColumnsLayout = "SomeLayout", ColumnsPinned = 3 }
            };
            mockContext.Setup(c => c.UsuariosGridsCfg).Returns(MockDbSet(usuariosGridsCfg));

            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim("Empresa", "TestEmpresa"),
                new Claim("Usuario", "TestUsuario")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            httpContext.User = claimsPrincipal;

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

            var functionsBBDD = new FunctionsBBDD(mockContext.Object, httpContextAccessor.Object);

            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(mockContext.Object, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.Articulo_Renombrar("ART001", "ART001_NUEVO") as JsonResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(result.Value));
            Assert.False((bool)resultValue["success"]);
            Assert.Equal("Error al renombrar el artículo", resultValue["message"]);

            // Verificar que el error contiene información sobre la excepción
            Assert.Contains("Test exception", (string)resultValue["error"]);

            // Imprimir información de depuración
            _output.WriteLine($"Resultado completo: {JsonConvert.SerializeObject(resultValue)}");
        }

        // Método auxiliar para crear un DbSet mockeado
        private static DbSet<T> MockDbSet<T>(List<T> list) where T : class
        {
            var queryable = list.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => list.Add(s));
            return dbSet.Object;
        }

        [Fact]
        public async Task Articulo_Copiar_DeberiaCrearNuevoArticuloConRelaciones()
        {
            // Arrange
            var articuloGuid = Guid.NewGuid();
            var empresaTest = "TestEmpresa";
            var usuarioTest = "TestUsuario";
            var articulos = new List<Articulos>
            {
                new Articulos { Guid = articuloGuid, Articulo = "ART001", Activo = true, Empresa = empresaTest }
            };

                    var articulosMO = new List<ArticulosMO>
            {
                new ArticulosMO { Guid = Guid.NewGuid(), Articulo = "ART001", Empresa = empresaTest }
            };

                    var articulosCOMP = new List<ArticulosCOMP>
            {
                new ArticulosCOMP { Guid = Guid.NewGuid(), Articulo = "ART001", Empresa = empresaTest },
                new ArticulosCOMP { Guid = Guid.NewGuid(), Componente = "ART001", Empresa = empresaTest }
            };

                    var articulosIMG = new List<ArticulosIMG>
            {
                new ArticulosIMG { Guid = Guid.NewGuid(), Articulo = "ART001", Empresa = empresaTest }
            };

                    var usuariosGridsCfg = new List<UsuariosGridsCfg>
            {
                new UsuariosGridsCfg
                {
                    Empresa = empresaTest,
                    Usuario = usuarioTest,
                    GridID = "gridArticulos",
                    ColumnsLayout = "SomeLayout",
                    ColumnsPinned = 3
                }
            };

            var mockContext = new Mock<DbContextiLabPlus>();

            // Configurar DbSets mockeados
            mockContext.Setup(c => c.Articulos).Returns(MockDbSetCopia(articulos));
            mockContext.Setup(c => c.ArticulosMO).Returns(MockDbSetCopia(articulosMO));
            mockContext.Setup(c => c.ArticulosCOMP).Returns(MockDbSetCopia(articulosCOMP));
            mockContext.Setup(c => c.ArticulosIMG).Returns(MockDbSetCopia(articulosIMG));
            mockContext.Setup(c => c.UsuariosGridsCfg).Returns(MockDbSetCopia(usuariosGridsCfg));

            // Configurar SaveChangesAsync para que no lance excepciones
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1); // Simula que se guardó 1 entidad

            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
    {
        new Claim("Empresa", empresaTest),
        new Claim("Usuario", usuarioTest)
    };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            httpContext.User = claimsPrincipal;

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

            var functionsBBDD = new FunctionsBBDD(mockContext.Object, httpContextAccessor.Object);

            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(mockContext.Object, functionsBBDD, mockConfiguration.Object);

            // Act
            var result = await controller.Articulo_Copiar("ART001", "ART001_COPIA") as JsonResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(result.Value));

            _output.WriteLine($"Resultado completo: {JsonConvert.SerializeObject(resultValue)}");

            Assert.True(resultValue.ContainsKey("success"), "El resultado debe contener una clave 'success'");
            Assert.True((bool)resultValue["success"], "La operación debería ser exitosa");

            if (!(bool)resultValue["success"])
            {
                Assert.True(resultValue.ContainsKey("message"), "En caso de error, debe haber un mensaje");
                Assert.True(resultValue.ContainsKey("error"), "En caso de error, debe haber detalles del error");
                _output.WriteLine($"Error: {resultValue["message"]}");
                _output.WriteLine($"Error detallado: {resultValue["error"]}");
                if (resultValue.ContainsKey("stackTrace"))
                    _output.WriteLine($"Stack Trace: {resultValue["stackTrace"]}");
                if (resultValue.ContainsKey("innerException"))
                    _output.WriteLine($"Inner Exception: {resultValue["innerException"]}");
            }
            else
            {
                Assert.True(resultValue.ContainsKey("data"), "En caso de éxito, debe haber datos del artículo copiado");
                var nuevoArticulo = JsonConvert.DeserializeObject<Articulos>(JsonConvert.SerializeObject(resultValue["data"]));
                Assert.NotNull(nuevoArticulo);
                Assert.Equal("ART001_COPIA", nuevoArticulo.Articulo);
            }

            // Imprimir el estado de los DbSets después de la operación
            _output.WriteLine($"Articulos: {mockContext.Object.Articulos.Count()}");
            _output.WriteLine($"ArticulosMO: {mockContext.Object.ArticulosMO.Count()}");
            _output.WriteLine($"ArticulosCOMP: {mockContext.Object.ArticulosCOMP.Count()}");
            _output.WriteLine($"ArticulosIMG: {mockContext.Object.ArticulosIMG.Count()}");

            // Verificar que se crearon nuevos registros
            mockContext.Verify(m => m.Articulos.Add(It.IsAny<Articulos>()), Times.Once());
            mockContext.Verify(m => m.ArticulosMO.Add(It.IsAny<ArticulosMO>()), Times.Once());
            mockContext.Verify(m => m.ArticulosCOMP.Add(It.IsAny<ArticulosCOMP>()), Times.Exactly(2)); // Una vez para Articulo y otra para Componente
            mockContext.Verify(m => m.ArticulosIMG.Add(It.IsAny<ArticulosIMG>()), Times.Once());

            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2)); // Una vez después de crear el artículo principal y otra después de crear las relaciones
        }

        // Método auxiliar para crear un DbSet mockeado
        private static DbSet<T> MockDbSetCopia<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new AsyncQueryProvider<T>(queryable.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            mockSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => data.Add(s));

            return mockSet.Object;
        }

        // Implementación de IAsyncQueryProvider
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
                var executionResult = ((IQueryProvider)this).Execute(expression);

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(null, new[] { executionResult });
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
        }

        public class AsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public AsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
            public T Current => _inner.Current;
            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask();
            }
        }

        [Fact]
        public async Task GetDataArticulos_DeberiaRetornarDatosCorrectos()
        {
            // Arrange
            var articulos = TestDataFactory.CreateTestArticulos();
            var context = MockDbContextFactory.CreateMockContext(articulos: articulos);
            var mockHttpContextAccessor = MockHttpContextAccessorFactory.CreateMockHttpContextAccessor("TestEmpresa", "TestUser", "ADMIN");
            var functionsBBDD = new FunctionsBBDD(context, mockHttpContextAccessor.Object);
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new ArticulosController(context, functionsBBDD, mockConfiguration.Object);

            // Act
            var actionResult = await controller.GetDataArticulos("Todos");
            var result = actionResult as JsonResult;

            // Assert
            Assert.NotNull(result);
            var data = JsonConvert.DeserializeObject<List<Articulos>>(JsonConvert.SerializeObject(result.Value));
            Assert.Equal(2, data.Count);
            Assert.Contains(data, a => a.Articulo == "ART001");
            Assert.Contains(data, a => a.Articulo == "ART002");
        }

    }
}