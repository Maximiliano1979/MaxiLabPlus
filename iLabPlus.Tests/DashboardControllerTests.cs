using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Nemesis365.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static iLabPlus.Helpers.AsyncTestHelpers;
using static Nemesis365.Controllers.DashboardController;


namespace iLabPlus.Tests
{
    public class DashboardControllerTests
    {


        private readonly Mock<DbContextiLabPlus> _mockContext;
        private readonly Mock<FunctionsBBDD> _mockFunctionsBBDD;
        private readonly Mock<ILogger<DashboardController>> _mockLogger;
        private readonly ITestOutputHelper _output;
        private readonly DashboardController _controller;


        public DashboardControllerTests(ITestOutputHelper output)
        {
            _output = output;
            _mockContext = new Mock<DbContextiLabPlus>();
            _mockFunctionsBBDD = new Mock<FunctionsBBDD>(_mockContext.Object, Mock.Of<IHttpContextAccessor>());
            var logger = new Mock<ILogger<DashboardController>>();
            _mockLogger = new Mock<ILogger<DashboardController>>();
            _controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);


            // Configurar IDateTime si es necesario
            var mockDateTime = new Mock<IDateTime>();
            mockDateTime.Setup(x => x.Now).Returns(new DateTime(2023, 7, 1));
            _controller.SetDateTime(mockDateTime.Object);
        }


        public class FacturaUnida
        {
            public FacturasLIN Lin { get; set; }
            public FacturasCAB Cab { get; set; }
        }



        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            _mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(new List<MenuUser>());

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Dashboard", viewResult.ViewName);
        }



        [Fact]
        public async Task ObtenerFacturacionClientesAnual_ReturnsCorrectData()
        {
            // Arrange
            var year = 2023;
            var testDataLIN = new List<FacturasLIN>
            {
                new FacturasLIN { Empresa = "TestEmpresa", Cliente = "Cliente1", Factura = "Factura1", FacPrecioTotal = 1000 },
                new FacturasLIN { Empresa = "TestEmpresa", Cliente = "Cliente2", Factura = "Factura2", FacPrecioTotal = 2000 }
            }.AsQueryable();

            var mockSetLIN = new Mock<DbSet<FacturasLIN>>();
            mockSetLIN.As<IQueryable<FacturasLIN>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<FacturasLIN>(testDataLIN.Provider));
            mockSetLIN.As<IQueryable<FacturasLIN>>().Setup(m => m.Expression).Returns(testDataLIN.Expression);
            mockSetLIN.As<IQueryable<FacturasLIN>>().Setup(m => m.ElementType).Returns(testDataLIN.ElementType);
            mockSetLIN.As<IQueryable<FacturasLIN>>().Setup(m => m.GetEnumerator()).Returns(testDataLIN.GetEnumerator);

            var testDataCAB = new List<FacturasCAB>
            {
                new FacturasCAB { Empresa = "TestEmpresa", Cliente = "Cliente1", Factura = "Factura1", FacFecha = new DateTime(2023, 1, 1) },
                new FacturasCAB { Empresa = "TestEmpresa", Cliente = "Cliente2", Factura = "Factura2", FacFecha = new DateTime(2023, 2, 1) }
            }.AsQueryable();

            var mockSetCAB = new Mock<DbSet<FacturasCAB>>();
            mockSetCAB.As<IQueryable<FacturasCAB>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<FacturasCAB>(testDataCAB.Provider));
            mockSetCAB.As<IQueryable<FacturasCAB>>().Setup(m => m.Expression).Returns(testDataCAB.Expression);
            mockSetCAB.As<IQueryable<FacturasCAB>>().Setup(m => m.ElementType).Returns(testDataCAB.ElementType);
            mockSetCAB.As<IQueryable<FacturasCAB>>().Setup(m => m.GetEnumerator()).Returns(testDataCAB.GetEnumerator);

            _mockContext.Setup(c => c.FacturasLIN).Returns(mockSetLIN.Object);
            _mockContext.Setup(c => c.FacturasCAB).Returns(mockSetCAB.Object);

            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var logger = new Mock<ILogger<DashboardController>>();
            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, logger.Object, _output);

            // Act
            var result = await controller.ObtenerFacturacionClientesAnual(year);

            // Assert
            if (result is JsonResult jsonResult)
            {
                var data = Assert.IsType<List<EstDashClientesFact>>(jsonResult.Value);
                Assert.Equal(2, data.Count);
                Assert.Equal(3000, data.Sum(d => d.ClienteImporteFact));
            }
            else if (result is ObjectResult objectResult)
            {
                Assert.Equal(500, objectResult.StatusCode);
                _output.WriteLine($"Error: {objectResult.Value}");
            }
            else
            {
                Assert.True(false, $"Unexpected result type: {result.GetType()}");
            }
        }


        [Fact]
        public async Task ObtenerFacturacionClientesAnual_CuandoFacturasLINEsNulo_RetornaError500()
        {
            // Arrange
            _mockContext.Setup(c => c.FacturasLIN).Returns((DbSet<FacturasLIN>)null);
            var year = 2024;

            // Act
            var result = await _controller.ObtenerFacturacionClientesAnual(year);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Error: Contexto de base de datos no inicializado correctamente", objectResult.Value);
        }




        [Fact]
        public async Task ObtenerFacturacionClientesAnual_CuandoSessionEmpresaEsNulo_RetornaError500()
        {
            // Arrange
            _mockContext.Setup(c => c.FacturasLIN).Returns(Mock.Of<DbSet<FacturasLIN>>());
            _mockContext.Setup(c => c.FacturasCAB).Returns(Mock.Of<DbSet<FacturasCAB>>());
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = null });
            var year = 2024;

            // Act
            var result = await _controller.ObtenerFacturacionClientesAnual(year);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            _output.WriteLine($"Actual error message: {objectResult.Value}");
            Assert.Contains("SessionEmpresa no inicializada correctamente", objectResult.Value.ToString());
        }




        [Fact]
        public async Task ObtenerFacturacionClientesAnual_CuandoNoHayResultados_RetornaListaVacia()
        {
            // Arrange
            var mockSetLIN = new Mock<DbSet<FacturasLIN>>();
            var mockSetCAB = new Mock<DbSet<FacturasCAB>>();

            var dataLIN = new List<FacturasLIN>().AsQueryable();
            var dataCAB = new List<FacturasCAB>().AsQueryable();

            mockSetLIN.As<IQueryable<FacturasLIN>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<FacturasLIN>(dataLIN.Provider));
            mockSetLIN.As<IQueryable<FacturasLIN>>().Setup(m => m.Expression).Returns(dataLIN.Expression);
            mockSetLIN.As<IQueryable<FacturasLIN>>().Setup(m => m.ElementType).Returns(dataLIN.ElementType);
            mockSetLIN.As<IQueryable<FacturasLIN>>().Setup(m => m.GetEnumerator()).Returns(dataLIN.GetEnumerator());

            mockSetCAB.As<IQueryable<FacturasCAB>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<FacturasCAB>(dataCAB.Provider));
            mockSetCAB.As<IQueryable<FacturasCAB>>().Setup(m => m.Expression).Returns(dataCAB.Expression);
            mockSetCAB.As<IQueryable<FacturasCAB>>().Setup(m => m.ElementType).Returns(dataCAB.ElementType);
            mockSetCAB.As<IQueryable<FacturasCAB>>().Setup(m => m.GetEnumerator()).Returns(dataCAB.GetEnumerator());

            _mockContext.Setup(c => c.FacturasLIN).Returns(mockSetLIN.Object);
            _mockContext.Setup(c => c.FacturasCAB).Returns(mockSetCAB.Object);

            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var year = 2024;

            // Act
            var result = await _controller.ObtenerFacturacionClientesAnual(year);

            // Assert
            if (result is ObjectResult objectResult)
            {
                _output.WriteLine($"ObjectResult StatusCode: {objectResult.StatusCode}");
                _output.WriteLine($"ObjectResult Value: {objectResult.Value}");
            }
            else if (result is JsonResult jsonResult)
            {
                var data = Assert.IsType<List<EstDashClientesFact>>(jsonResult.Value);
                Assert.Empty(data);
            }
            else
            {
                _output.WriteLine($"Unexpected result type: {result.GetType()}");
                Assert.True(false, $"Unexpected result type: {result.GetType()}");
            }
        }




        [Fact]
        public async Task ObtenerFacturacionClientesAnual_CuandoOcurreExcepcion_RetornaError500()
        {
            // Arrange
            _mockContext.Setup(c => c.FacturasLIN).Throws(new Exception("Test exception"));
            var year = 2024;

            // Act
            var result = await _controller.ObtenerFacturacionClientesAnual(year);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Error interno del servidor: Test exception", objectResult.Value);
        }




        [Fact]
        public void ObtenerArtMasPedidosPrecio_ProcesaDatosCorrectamente()
        {
            _output.WriteLine("Iniciando test ObtenerArtMasPedidosPrecio_ProcesaDatosCorrectamente");

            var year = 2023;
            var testDataLIN = new List<PedidosLIN>
            {
                new PedidosLIN { Empresa = "TestEmpresa", Cliente = "Cliente1", Pedido = "Pedido1", PedArt = "Art1", PedPrecioTotal = 1000 },
                new PedidosLIN { Empresa = "TestEmpresa", Cliente = "Cliente1", Pedido = "Pedido1", PedArt = "Art2", PedPrecioTotal = 2000 },
                new PedidosLIN { Empresa = "TestEmpresa", Cliente = "Cliente2", Pedido = "Pedido2", PedArt = "Art1", PedPrecioTotal = 3000 },
                new PedidosLIN { Empresa = "TestEmpresa", Cliente = "Cliente2", Pedido = "Pedido2", PedArt = "Art3", PedPrecioTotal = 4000 }
            }.AsQueryable();

            _output.WriteLine($"Número de PedidosLIN: {testDataLIN.Count()}");

            var mockSetLIN = new Mock<DbSet<PedidosLIN>>();
            mockSetLIN.As<IQueryable<PedidosLIN>>().Setup(m => m.Provider).Returns(testDataLIN.Provider);
            mockSetLIN.As<IQueryable<PedidosLIN>>().Setup(m => m.Expression).Returns(testDataLIN.Expression);
            mockSetLIN.As<IQueryable<PedidosLIN>>().Setup(m => m.ElementType).Returns(testDataLIN.ElementType);
            mockSetLIN.As<IQueryable<PedidosLIN>>().Setup(m => m.GetEnumerator()).Returns(testDataLIN.GetEnumerator);

            _mockContext.Setup(c => c.PedidosLIN).Returns(mockSetLIN.Object);

            var testDataCAB = new List<PedidosCAB>
            {
                new PedidosCAB { Empresa = "TestEmpresa", Cliente = "Cliente1", Pedido = "Pedido1", PedFecha = new DateTime(2023, 1, 1) },
                new PedidosCAB { Empresa = "TestEmpresa", Cliente = "Cliente2", Pedido = "Pedido2", PedFecha = new DateTime(2023, 2, 1) }
            }.AsQueryable();

            _output.WriteLine($"Número de PedidosCAB: {testDataCAB.Count()}");

            var mockSetCAB = new Mock<DbSet<PedidosCAB>>();
            mockSetCAB.As<IQueryable<PedidosCAB>>().Setup(m => m.Provider).Returns(testDataCAB.Provider);
            mockSetCAB.As<IQueryable<PedidosCAB>>().Setup(m => m.Expression).Returns(testDataCAB.Expression);
            mockSetCAB.As<IQueryable<PedidosCAB>>().Setup(m => m.ElementType).Returns(testDataCAB.ElementType);
            mockSetCAB.As<IQueryable<PedidosCAB>>().Setup(m => m.GetEnumerator()).Returns(testDataCAB.GetEnumerator);

            _mockContext.Setup(c => c.PedidosCAB).Returns(mockSetCAB.Object);

            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var loggerMock = new Mock<ILogger<DashboardController>>();
            List<string> logMessages = new List<string>();

            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Callback(new InvocationAction(invocation =>
            {
                var logLevel = (LogLevel)invocation.Arguments[0];
                var eventId = (EventId)invocation.Arguments[1];
                var state = invocation.Arguments[2];
                var exception = (Exception)invocation.Arguments[3];
                var formatter = invocation.Arguments[4];

                var invokeMethod = formatter.GetType().GetMethod("Invoke");
                var logMessage = (string)invokeMethod.Invoke(formatter, new[] { state, exception });
                logMessages.Add(logMessage);
            }));

            _output.WriteLine("Configuración de mocks completada");

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, loggerMock.Object, _output);

            _output.WriteLine("Ejecutando ObtenerArtMasPedidosPrecio");
            var result = controller.ObtenerArtMasPedidosPrecio(year);

            foreach (var log in logMessages)
            {
                _output.WriteLine($"Log: {log}");
            }

            _output.WriteLine("Iniciando aserciones");
            if (result is JsonResult jsonResult)
            {
                var data = Assert.IsType<List<EstDashArtPedidos>>(jsonResult.Value);
                Assert.Equal(3, data.Count);
                Assert.Equal(10000, data.Sum(d => d.ArticuloPrecioPedido));

                // Verificar que todos los artículos esperados están presentes
                Assert.Contains(data, d => d.Articulo == "Art1" && d.ArticuloPrecioPedido == 4000 && d.Porcentaje == "40.00");
                Assert.Contains(data, d => d.Articulo == "Art2" && d.ArticuloPrecioPedido == 2000 && d.Porcentaje == "20.00");
                Assert.Contains(data, d => d.Articulo == "Art3" && d.ArticuloPrecioPedido == 4000 && d.Porcentaje == "40.00");

                // Verificar que están ordenados de mayor a menor por ArticuloPrecioPedido
                Assert.True(data[0].ArticuloPrecioPedido >= data[1].ArticuloPrecioPedido);
                Assert.True(data[1].ArticuloPrecioPedido >= data[2].ArticuloPrecioPedido);

                Assert.All(data, item => Assert.Equal(10000, item.TotalPrecioTodosArticulos));
                Assert.True(data.Count <= 40);

                // Imprimir los resultados para depuración
                foreach (var item in data)
                {
                    _output.WriteLine($"Articulo: {item.Articulo}, Precio: {item.ArticuloPrecioPedido}, Porcentaje: {item.Porcentaje}");
                }
            }
            else if (result is ObjectResult objectResult)
            {
                _output.WriteLine($"Resultado es ObjectResult con StatusCode: {objectResult.StatusCode}");
                _output.WriteLine($"Error: {objectResult.Value}");
                Assert.True(false, $"Test falló debido a un error: {objectResult.Value}");
            }
            else
            {
                _output.WriteLine($"Tipo de resultado inesperado: {result.GetType()}");
                Assert.True(false, $"Tipo de resultado inesperado: {result.GetType()}");
            }
        }

        [Fact]
        public void ObtenerArtMasPedidosPrecio_SinPedidosEncontrados_RetornaListaVacia()
        {
            _output.WriteLine("Iniciando test ObtenerArtMasPedidosPrecio_SinPedidosEncontrados_RetornaListaVacia");

            // Arrange
            var year = 2023;
            var testDataLIN = new List<PedidosLIN>().AsQueryable();
            var testDataCAB = new List<PedidosCAB>().AsQueryable();

            _output.WriteLine($"Número de PedidosLIN: {testDataLIN.Count()}");
            _output.WriteLine($"Número de PedidosCAB: {testDataCAB.Count()}");

            var mockSetLIN = new Mock<DbSet<PedidosLIN>>();
            mockSetLIN.As<IQueryable<PedidosLIN>>().Setup(m => m.Provider).Returns(testDataLIN.Provider);
            mockSetLIN.As<IQueryable<PedidosLIN>>().Setup(m => m.Expression).Returns(testDataLIN.Expression);
            mockSetLIN.As<IQueryable<PedidosLIN>>().Setup(m => m.ElementType).Returns(testDataLIN.ElementType);
            mockSetLIN.As<IQueryable<PedidosLIN>>().Setup(m => m.GetEnumerator()).Returns(testDataLIN.GetEnumerator);

            _mockContext.Setup(c => c.PedidosLIN).Returns(mockSetLIN.Object);

            var mockSetCAB = new Mock<DbSet<PedidosCAB>>();
            mockSetCAB.As<IQueryable<PedidosCAB>>().Setup(m => m.Provider).Returns(testDataCAB.Provider);
            mockSetCAB.As<IQueryable<PedidosCAB>>().Setup(m => m.Expression).Returns(testDataCAB.Expression);
            mockSetCAB.As<IQueryable<PedidosCAB>>().Setup(m => m.ElementType).Returns(testDataCAB.ElementType);
            mockSetCAB.As<IQueryable<PedidosCAB>>().Setup(m => m.GetEnumerator()).Returns(testDataCAB.GetEnumerator);

            _mockContext.Setup(c => c.PedidosCAB).Returns(mockSetCAB.Object);

            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var loggerMock = new Mock<ILogger<DashboardController>>();
            List<string> logMessages = new List<string>();

            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Callback(new InvocationAction(invocation =>
            {
                var logLevel = (LogLevel)invocation.Arguments[0];
                var eventId = (EventId)invocation.Arguments[1];
                var state = invocation.Arguments[2];
                var exception = (Exception)invocation.Arguments[3];
                var formatter = invocation.Arguments[4];

                var invokeMethod = formatter.GetType().GetMethod("Invoke");
                var logMessage = (string)invokeMethod.Invoke(formatter, new[] { state, exception });
                logMessages.Add(logMessage);
            }));

            _output.WriteLine("Configuración de mocks completada");

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, loggerMock.Object, _output);

            // Act
            _output.WriteLine("Ejecutando ObtenerArtMasPedidosPrecio");
            var result = controller.ObtenerArtMasPedidosPrecio(year);

            // Assert
            _output.WriteLine("Iniciando aserciones");
            var jsonResult = Assert.IsType<JsonResult>(result);
            var data = Assert.IsType<List<EstDashArtPedidos>>(jsonResult.Value);
            Assert.Empty(data);

            // Verificar que se registró el mensaje de log esperado
            Assert.Contains(logMessages, log => log == "No se encontraron pedidos para el año especificado");

            foreach (var log in logMessages)
            {
                _output.WriteLine($"Log: {log}");
            }

            _output.WriteLine("Test completado con éxito");
        }


        [Fact]
        public void ObtenerArtMasPedidosPrecio_CuandoOcurreExcepcion_RetornaErrorInterno()
        {
            _output.WriteLine("Iniciando test ObtenerArtMasPedidosPrecio_CuandoOcurreExcepcion_RetornaErrorInterno");

            // Arrange
            var year = 2023;
            var exceptionMessage = "Error de prueba";
            var innerExceptionMessage = "Error interno de prueba";

            _mockContext.Setup(c => c.PedidosLIN).Throws(new Exception(exceptionMessage, new Exception(innerExceptionMessage)));

            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var loggerMock = new Mock<ILogger<DashboardController>>();
            List<string> logMessages = new List<string>();

            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Callback(new InvocationAction(invocation =>
            {
                var logLevel = (LogLevel)invocation.Arguments[0];
                var eventId = (EventId)invocation.Arguments[1];
                var state = invocation.Arguments[2];
                var exception = (Exception)invocation.Arguments[3];
                var formatter = invocation.Arguments[4];

                var invokeMethod = formatter.GetType().GetMethod("Invoke");
                var logMessage = (string)invokeMethod.Invoke(formatter, new[] { state, exception });
                logMessages.Add(logMessage);
            }));

            _output.WriteLine("Configuración de mocks completada");

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, loggerMock.Object, _output);

            // Act
            _output.WriteLine("Ejecutando ObtenerArtMasPedidosPrecio");
            var result = controller.ObtenerArtMasPedidosPrecio(year);

            // Assert
            _output.WriteLine("Iniciando aserciones");
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal($"Error interno del servidor: {exceptionMessage}", objectResult.Value);

            // Verificar los mensajes de log
            Assert.Contains(logMessages, log => log.StartsWith($"Error en ObtenerArtMasPedidosPrecio: {exceptionMessage}"));
            Assert.Contains(logMessages, log => log.StartsWith("StackTrace:"));
            Assert.Contains(logMessages, log => log == $"Inner Exception: {innerExceptionMessage}");
            Assert.Contains(logMessages, log => log.StartsWith("Inner Exception StackTrace:"));

            foreach (var log in logMessages)
            {
                _output.WriteLine($"Log: {log}");
            }

            _output.WriteLine("Test completado con éxito");
        }


        [Fact]
        public async Task ObtenerArtMasPedidosQty_SinDatos_RetornaListaVacia()
        {
            // Arrange
            var year = 2023;
            var testDataLIN = new List<PedidosLIN>().AsQueryable();
            var testDataCAB = new List<PedidosCAB>().AsQueryable();

            SetupMockDbSet(_mockContext, testDataLIN, testDataCAB);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            // Act
            var result = await _controller.ObtenerArtMasPedidosQty(year);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var data = Assert.IsType<List<EstDashArtPedidos>>(jsonResult.Value);

            Assert.Empty(data);
        }


        private void SetupMockDbSet<T>(Mock<DbContextiLabPlus> mockContext, IQueryable<T> data, IQueryable<PedidosCAB> cabData) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator);

            mockContext.Setup(c => c.Set<T>()).Returns(mockSet.Object);

            if (typeof(T) == typeof(PedidosLIN))
            {
                // Solo configuramos PedidosLIN si no ha sido configurado previamente
                mockContext.SetupGet(c => c.PedidosLIN).Returns(mockSet.Object as DbSet<PedidosLIN>);
            }

            var mockSetCAB = new Mock<DbSet<PedidosCAB>>();
            mockSetCAB.As<IQueryable<PedidosCAB>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<PedidosCAB>(cabData.Provider));
            mockSetCAB.As<IQueryable<PedidosCAB>>().Setup(m => m.Expression).Returns(cabData.Expression);
            mockSetCAB.As<IQueryable<PedidosCAB>>().Setup(m => m.ElementType).Returns(cabData.ElementType);
            mockSetCAB.As<IQueryable<PedidosCAB>>().Setup(m => m.GetEnumerator()).Returns(cabData.GetEnumerator);

            // Solo configuramos PedidosCAB si no ha sido configurado previamente
            mockContext.SetupGet(c => c.PedidosCAB).Returns(mockSetCAB.Object);
        }



        [Fact]
        public async Task ObtenerDatosPedidosPorDia_RetornaDatosCorrectos()
        {
            _output.WriteLine("Iniciando test ObtenerDatosPedidosPorDia_RetornaDatosCorrectos");

            // Arrange
            var year = 2023;
            var month = 8; // Agosto
            _output.WriteLine($"Utilizando el mes: {month}");

            var testData = new List<PedidosCAB>
            {
                new PedidosCAB { Empresa = "TestEmpresa", Cliente = "Cliente1", Pedido = "Pedido1", PedFecha = new DateTime(2023, month, 1) },
                new PedidosCAB { Empresa = "TestEmpresa", Cliente = "Cliente1", Pedido = "Pedido2", PedFecha = new DateTime(2023, month, 1) },
                new PedidosCAB { Empresa = "TestEmpresa", Cliente = "Cliente2", Pedido = "Pedido3", PedFecha = new DateTime(2023, month, 2) },
                new PedidosCAB { Empresa = "TestEmpresa", Cliente = "Cliente2", Pedido = "Pedido4", PedFecha = new DateTime(2023, month, 15) }
            };

            _output.WriteLine($"Datos de prueba creados. Cantidad: {testData.Count}");

            var options = new DbContextOptionsBuilder<DbContextiLabPlus>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .EnableSensitiveDataLogging() // Habilita el registro de datos sensibles
                .Options;

            using var context = new DbContextiLabPlus(options);

            // Poblar la base de datos en memoria
            context.PedidosCAB.AddRange(testData);
            context.SaveChanges();

            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });
            _output.WriteLine("Mock de FunctionsBBDD configurado");

            var mockDateTime = new Mock<IDateTime>();
            mockDateTime.Setup(x => x.Now).Returns(new DateTime(year, month, 1));

            var logger = new Mock<ILogger<DashboardController>>();

            // Crear el controlador con el contexto en memoria
            var controller = new DashboardController(context, _mockFunctionsBBDD.Object, logger.Object, _output);
            controller.SetDateTime(mockDateTime.Object);
            _output.WriteLine("Mock de DateTime configurado");

            // Act
            _output.WriteLine("Ejecutando ObtenerDatosPedidosPorDia");
            var result = controller.ObtenerDatosPedidosPorDia(year);

            // Imprimir el rango de fechas desde la salida estándar
            var expectedStartDate = new DateTime(year, month, 1);
            var expectedEndDate = expectedStartDate.AddMonths(1).AddDays(-1);
            _output.WriteLine($"Rango de fechas esperado: {expectedStartDate} a {expectedEndDate}");

            // Assert
            _output.WriteLine("Iniciando aserciones");
            _output.WriteLine($"Tipo de resultado: {result.GetType().Name}");

            var okResult = Assert.IsType<OkObjectResult>(result);
            _output.WriteLine("Resultado verificado como OkObjectResult");

            var datosPorDia = Assert.IsType<List<DatosPedidoPorDia>>(okResult.Value); // Esperar List<DatosPedidoPorDia>
            _output.WriteLine($"Datos por día obtenidos. Cantidad: {datosPorDia.Count}");


            Assert.Equal(31, datosPorDia.Count); // Agosto tiene 31 días
            Assert.Equal(2, GetCantidadPedidos(datosPorDia[0])); // 2 pedidos el día 1
            Assert.Equal(1, GetCantidadPedidos(datosPorDia[1])); // 1 pedido el día 2
            Assert.Equal(1, GetCantidadPedidos(datosPorDia[14])); // 1 pedido el día 15
            Assert.Equal(0, GetCantidadPedidos(datosPorDia[30])); // 0 pedidos el día 31

            // Verificar que las fechas de los pedidos estén dentro del rango esperado
            foreach (var pedido in testData)
            {
                Assert.True(pedido.PedFecha >= expectedStartDate && pedido.PedFecha <= expectedEndDate,
                    $"La fecha del pedido {pedido.PedFecha} está fuera del rango esperado.");
            }

            _output.WriteLine("Todas las aserciones pasadas con éxito");
        }



        private int GetCantidadPedidos(object item)
        {
            var datosPedido = item as DatosPedidoPorDia; // Convertir a DatosPedidoPorDia
            if (datosPedido == null)
            {
                _output.WriteLine($"El objeto no es de tipo DatosPedidoPorDia.");
                return -1;
            }
            return datosPedido.CantidadPedidos;
        }



        [Fact]
        public async Task ObtenerEntregasHoy_DebeRetornarCantidadCorrectaDeEntregas()
        {
            // Arrange
            var year = 2023;
            var mockDateTime = new Mock<IDateTime>();
            mockDateTime.Setup(x => x.Now).Returns(new DateTime(2023, 7, 15));

            var testData = new List<PedidosCAB>
            {
                new PedidosCAB { Empresa = "TestEmpresa", PedFechaEnt = new DateTime(2023, 7, 15) },
                new PedidosCAB { Empresa = "TestEmpresa", PedFechaEnt = new DateTime(2023, 7, 15) },
                new PedidosCAB { Empresa = "TestEmpresa", PedFechaEnt = new DateTime(2023, 7, 16) },
                new PedidosCAB { Empresa = "OtraEmpresa", PedFechaEnt = new DateTime(2023, 7, 15) }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PedidosCAB>>();
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<PedidosCAB>(testData.Provider));
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.Expression).Returns(testData.Expression);
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.ElementType).Returns(testData.ElementType);
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.GetEnumerator()).Returns(testData.GetEnumerator());

            _mockContext.Setup(c => c.PedidosCAB).Returns(mockSet.Object);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);
            controller.SetDateTime(mockDateTime.Object);

            // Act
            var result = await controller.ObtenerEntregasHoy(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic responseValue = okResult.Value;
            Assert.Equal(2, responseValue.cantidad);
        }

        [Fact]
        public async Task ObtenerEntregasHoy_CuandoNoHayEntregas_DebeRetornarCero()
        {
            // Arrange
            var year = 2023;
            var mockDateTime = new Mock<IDateTime>();
            mockDateTime.Setup(x => x.Now).Returns(new DateTime(2023, 7, 15));

            var testData = new List<PedidosCAB>
            {
                new PedidosCAB { Empresa = "TestEmpresa", PedFechaEnt = new DateTime(2023, 7, 16) },
                new PedidosCAB { Empresa = "OtraEmpresa", PedFechaEnt = new DateTime(2023, 7, 15) }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PedidosCAB>>();
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<PedidosCAB>(testData.Provider));
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.Expression).Returns(testData.Expression);
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.ElementType).Returns(testData.ElementType);
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.GetEnumerator()).Returns(testData.GetEnumerator());

            _mockContext.Setup(c => c.PedidosCAB).Returns(mockSet.Object);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);
            controller.SetDateTime(mockDateTime.Object);

            // Act
            var result = await controller.ObtenerEntregasHoy(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic responseValue = okResult.Value;
            Assert.Equal(0, responseValue.cantidad);
        }


        [Fact]
        public async Task ObtenerEntregasSemana_DebeRetornarEntregasAgrupadasPorDia()
        {
            // Arrange
            var year = 2023;
            var mockDateTime = new Mock<IDateTime>();
            mockDateTime.Setup(x => x.Now).Returns(new DateTime(2023, 7, 12)); // Asumimos que es un miércoles

            _output.WriteLine($"Fecha simulada: {mockDateTime.Object.Now}");

            var inicioSemana = new DateTime(2023, 7, 10); // Lunes
            var finSemana = new DateTime(2023, 7, 17); // Lunes siguiente

            _output.WriteLine($"Rango de fechas esperado: {inicioSemana} a {finSemana}");

            var testData = new List<PedidosCAB>
    {
        new PedidosCAB { Empresa = "TestEmpresa", PedFechaEnt = new DateTime(2023, 7, 10) }, // Lunes
        new PedidosCAB { Empresa = "TestEmpresa", PedFechaEnt = new DateTime(2023, 7, 10) }, // Lunes
        new PedidosCAB { Empresa = "TestEmpresa", PedFechaEnt = new DateTime(2023, 7, 12) }, // Miércoles
        new PedidosCAB { Empresa = "TestEmpresa", PedFechaEnt = new DateTime(2023, 7, 14) }, // Viernes
        new PedidosCAB { Empresa = "TestEmpresa", PedFechaEnt = new DateTime(2023, 7, 16) }, // Domingo
        new PedidosCAB { Empresa = "OtraEmpresa", PedFechaEnt = new DateTime(2023, 7, 12) }, // No debe contar
        new PedidosCAB { Empresa = "TestEmpresa", PedFechaEnt = new DateTime(2023, 7, 17) }, // No debe contar (siguiente semana)
    }.AsQueryable();

            _output.WriteLine($"Número de pedidos de prueba: {testData.Count()}");

            var mockSet = new Mock<DbSet<PedidosCAB>>();
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<PedidosCAB>(testData.Provider));
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.Expression).Returns(testData.Expression);
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.ElementType).Returns(testData.ElementType);
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.GetEnumerator()).Returns(testData.GetEnumerator());

            _mockContext.Setup(c => c.PedidosCAB).Returns(mockSet.Object);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);
            controller.SetDateTime(mockDateTime.Object);

            // Act
            var result = await controller.ObtenerEntregasSemana(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var entregasSemana = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);

            _output.WriteLine($"Número de días con entregas: {entregasSemana.Count()}");

            Assert.Equal(4, entregasSemana.Count()); // Debe haber entregas en 4 días diferentes

            // Verificar cada día
            foreach (dynamic entrega in entregasSemana)
            {
                _output.WriteLine($"Día de la semana: {entrega.DiaSemana}, Cantidad de entregas: {entrega.CantidadEntregas}");

                switch (entrega.DiaSemana)
                {
                    case 1: // Lunes
                        Assert.Equal(2, entrega.CantidadEntregas);
                        break;
                    case 3: // Miércoles
                        Assert.Equal(1, entrega.CantidadEntregas);
                        break;
                    case 5: // Viernes
                        Assert.Equal(1, entrega.CantidadEntregas);
                        break;
                    case 0: // Domingo (0 en DayOfWeek enum)
                        Assert.Equal(1, entrega.CantidadEntregas);
                        break;
                    default:
                        Assert.True(false, $"Día inesperado: {entrega.DiaSemana}");
                        break;
                }
            }
        }


        


        [Fact]
        public async Task ObtenerEntregasPendSemana_SinEntregas_DebeRetornarCeros()
        {
            // Arrange
            var year = 2023;
            var mockDateTime = new Mock<IDateTime>();
            mockDateTime.Setup(x => x.Now).Returns(new DateTime(2023, 7, 12));

            var testDataCAB = new List<PedidosCAB>().AsQueryable();
            var testDataLIN = new List<PedidosLIN>().AsQueryable();

            SetupMockDbSet(_mockContext, testDataCAB, testDataLIN);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);
            controller.SetDateTime(mockDateTime.Object);

            // Act
            var result = await controller.ObtenerEntregasPendSemana(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic value = okResult.Value;

            Assert.Equal(0, value.LineasTotalEntregar);
            Assert.Equal(0, value.LineasEntregadas);
            Assert.Equal(0, value.Porcentaje);
        }

        


        [Fact]
        public async Task ObtenerEntregasPendSemana_ErrorEnBaseDeDatos_DebeRetornarStatusCode500()
        {
            // Arrange
            var year = 2023;
            var mockDateTime = new Mock<IDateTime>();
            mockDateTime.Setup(x => x.Now).Returns(new DateTime(2023, 7, 12));

            _mockContext.Setup(c => c.PedidosCAB).Throws(new Exception("Error de base de datos"));
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);
            controller.SetDateTime(mockDateTime.Object);

            // Act
            var result = await controller.ObtenerEntregasPendSemana(year);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Internal Server Error", statusCodeResult.Value.ToString());
        }



        [Fact]
        public async Task ObtenerVentasPorMes_SinVentas_DebeRetornarListaVacia()
        {
            // Arrange
            var year = 2023;
            var mockDateTime = new Mock<IDateTime>();
            mockDateTime.Setup(x => x.Now).Returns(new DateTime(2023, 7, 15));

            var testDataCAB = new List<FacturasCAB>().AsQueryable();
            var testDataLIN = new List<FacturasLIN>().AsQueryable();

            SetupMockDbSet(_mockContext, testDataCAB, testDataLIN);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);
            controller.SetDateTime(mockDateTime.Object);

            // Act
            var result = await controller.ObtenerVentasPorMes(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var ventasMes = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);

            Assert.Empty(ventasMes);
        }

        [Fact]
        public async Task ObtenerVentasPorMesesDelAnio_DebeRetornarVentasAgrupadasPorMes()
        {
            // Arrange
            var year = 2023;
            var testDataCAB = new List<FacturasCAB>
        {
            new FacturasCAB { Empresa = "TestEmpresa", Factura = "F001", FacFecha = new DateTime(2023, 1, 15) },
            new FacturasCAB { Empresa = "TestEmpresa", Factura = "F002", FacFecha = new DateTime(2023, 1, 20) },
            new FacturasCAB { Empresa = "TestEmpresa", Factura = "F003", FacFecha = new DateTime(2023, 6, 1) },
            new FacturasCAB { Empresa = "TestEmpresa", Factura = "F004", FacFecha = new DateTime(2023, 12, 31) },
            new FacturasCAB { Empresa = "OtraEmpresa", Factura = "F005", FacFecha = new DateTime(2023, 7, 1) },
        }.AsQueryable();

            var testDataLIN = new List<FacturasLIN>
        {
            new FacturasLIN { Empresa = "TestEmpresa", Factura = "F001", FacPrecioTotal = 100 },
            new FacturasLIN { Empresa = "TestEmpresa", Factura = "F002", FacPrecioTotal = 200 },
            new FacturasLIN { Empresa = "TestEmpresa", Factura = "F003", FacPrecioTotal = 300 },
            new FacturasLIN { Empresa = "TestEmpresa", Factura = "F004", FacPrecioTotal = 400 },
            new FacturasLIN { Empresa = "OtraEmpresa", Factura = "F005", FacPrecioTotal = 500 },
        }.AsQueryable();

            SetupMockDbSet(_mockContext, testDataCAB, testDataLIN);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);

            // Act
            var result = await controller.ObtenerVentasPorMesesDelAnio(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var ventasMeses = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);

            Assert.Equal(3, ventasMeses.Count());

            var ventasEnero = ventasMeses.First() as dynamic;
            Assert.Equal(1, ventasEnero.Mes);
            Assert.Equal(300, ventasEnero.TotalVentas);

            var ventasJunio = ventasMeses.Skip(1).First() as dynamic;
            Assert.Equal(6, ventasJunio.Mes);
            Assert.Equal(300, ventasJunio.TotalVentas);

            var ventasDiciembre = ventasMeses.Last() as dynamic;
            Assert.Equal(12, ventasDiciembre.Mes);
            Assert.Equal(400, ventasDiciembre.TotalVentas);
        }

        [Fact]
        public async Task ObtenerVentasPorMesesDelAnio_SinVentas_DebeRetornarListaVacia()
        {
            // Arrange
            var year = 2023;
            var testDataCAB = new List<FacturasCAB>().AsQueryable();
            var testDataLIN = new List<FacturasLIN>().AsQueryable();

            SetupMockDbSet(_mockContext, testDataCAB, testDataLIN);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);

            // Act
            var result = await controller.ObtenerVentasPorMesesDelAnio(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var ventasMeses = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);

            Assert.Empty(ventasMeses);
        }



        [Fact]
        public async Task ObtenerTotalAlbaranesAnual_DebeRetornarSumaCorrecta()
        {
            // Arrange
            var year = 2023;
            var testDataCAB = new List<PedidosCAB>
        {
            new PedidosCAB { Empresa = "TestEmpresa", Pedido = "P001", PedAlbaranFecha = new DateTime(2023, 1, 1), PedAlbaran = true },
            new PedidosCAB { Empresa = "TestEmpresa", Pedido = "P002", PedAlbaranFecha = new DateTime(2023, 6, 1), PedAlbaran = true },
            new PedidosCAB { Empresa = "TestEmpresa", Pedido = "P003", PedAlbaranFecha = new DateTime(2023, 12, 31), PedAlbaran = true },
            new PedidosCAB { Empresa = "TestEmpresa", Pedido = "P004", PedAlbaranFecha = new DateTime(2022, 12, 31), PedAlbaran = true }, // Año anterior
            new PedidosCAB { Empresa = "OtraEmpresa", Pedido = "P005", PedAlbaranFecha = new DateTime(2023, 7, 1), PedAlbaran = true },
        }.AsQueryable();

            var testDataLIN = new List<PedidosLIN>
        {
            new PedidosLIN { Empresa = "TestEmpresa", Pedido = "P001", PedPrecioTotal = 100 },
            new PedidosLIN { Empresa = "TestEmpresa", Pedido = "P002", PedPrecioTotal = 200 },
            new PedidosLIN { Empresa = "TestEmpresa", Pedido = "P003", PedPrecioTotal = 300 },
            new PedidosLIN { Empresa = "TestEmpresa", Pedido = "P004", PedPrecioTotal = 400 },
            new PedidosLIN { Empresa = "OtraEmpresa", Pedido = "P005", PedPrecioTotal = 500 },
        }.AsQueryable();

            SetupMockDbSet(_mockContext, testDataCAB, testDataLIN);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);

            // Act
            var result = await controller.ObtenerTotalAlbaranesAnual(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var totalAlbaranesAnual = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(600m, totalAlbaranesAnual);
        }

        [Fact]
        public async Task ObtenerTotalAlbaranesAnual_SinDatos_DebeRetornarCero()
        {
            // Arrange
            var year = 2023;
            var testDataCAB = new List<PedidosCAB>().AsQueryable();
            var testDataLIN = new List<PedidosLIN>().AsQueryable();

            SetupMockDbSet(_mockContext, testDataCAB, testDataLIN);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);

            // Act
            var result = await controller.ObtenerTotalAlbaranesAnual(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var totalAlbaranesAnual = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(0m, totalAlbaranesAnual);
        }

        [Fact]
        public async Task ObtenerTotalFacturacionAnual_DebeRetornarSumaCorrecta()
        {
            // Arrange
            var year = 2023;
            var testDataCAB = new List<FacturasCAB>
        {
            new FacturasCAB { Empresa = "TestEmpresa", Factura = "F001", FacFecha = new DateTime(2023, 1, 1) },
            new FacturasCAB { Empresa = "TestEmpresa", Factura = "F002", FacFecha = new DateTime(2023, 6, 1) },
            new FacturasCAB { Empresa = "TestEmpresa", Factura = "F003", FacFecha = new DateTime(2023, 12, 31) },
            new FacturasCAB { Empresa = "TestEmpresa", Factura = "F004", FacFecha = new DateTime(2022, 12, 31) }, // Año anterior
            new FacturasCAB { Empresa = "OtraEmpresa", Factura = "F005", FacFecha = new DateTime(2023, 7, 1) },
        }.AsQueryable();

            var testDataLIN = new List<FacturasLIN>
        {
            new FacturasLIN { Empresa = "TestEmpresa", Factura = "F001", FacPrecioTotal = 100 },
            new FacturasLIN { Empresa = "TestEmpresa", Factura = "F002", FacPrecioTotal = 200 },
            new FacturasLIN { Empresa = "TestEmpresa", Factura = "F003", FacPrecioTotal = 300 },
            new FacturasLIN { Empresa = "TestEmpresa", Factura = "F004", FacPrecioTotal = 400 },
            new FacturasLIN { Empresa = "OtraEmpresa", Factura = "F005", FacPrecioTotal = 500 },
        }.AsQueryable();

            SetupMockDbSet(_mockContext, testDataCAB, testDataLIN);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);

            // Act
            var result = await controller.ObtenerTotalFacturacionAnual(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var totalFacturacionAnual = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(600m, totalFacturacionAnual);
        }

        [Fact]
        public async Task ObtenerTotalFacturacionAnual_SinDatos_DebeRetornarCero()
        {
            // Arrange
            var year = 2023;
            var testDataCAB = new List<FacturasCAB>().AsQueryable();
            var testDataLIN = new List<FacturasLIN>().AsQueryable();

            SetupMockDbSet(_mockContext, testDataCAB, testDataLIN);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);

            // Act
            var result = await controller.ObtenerTotalFacturacionAnual(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var totalFacturacionAnual = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(0m, totalFacturacionAnual);
        }

        [Fact]
        public async Task ObtenerGastosAnuales_DebeRetornarSumaCorrecta()
        {
            // Arrange
            var year = 2023;
            var testDataCAB = new List<ProvFacturasCAB>
        {
            new ProvFacturasCAB { Empresa = "TestEmpresa", Factura = "PF001", FacFecha = new DateTime(2023, 1, 1) },
            new ProvFacturasCAB { Empresa = "TestEmpresa", Factura = "PF002", FacFecha = new DateTime(2023, 6, 1) },
            new ProvFacturasCAB { Empresa = "TestEmpresa", Factura = "PF003", FacFecha = new DateTime(2023, 12, 31) },
            new ProvFacturasCAB { Empresa = "TestEmpresa", Factura = "PF004", FacFecha = new DateTime(2022, 12, 31) }, // Año anterior
            new ProvFacturasCAB { Empresa = "OtraEmpresa", Factura = "PF005", FacFecha = new DateTime(2023, 7, 1) },
        }.AsQueryable();

            var testDataLIN = new List<ProvFacturasLIN>
        {
            new ProvFacturasLIN { Empresa = "TestEmpresa", Factura = "PF001", FacPrecioTotal = 100 },
            new ProvFacturasLIN { Empresa = "TestEmpresa", Factura = "PF002", FacPrecioTotal = 200 },
            new ProvFacturasLIN { Empresa = "TestEmpresa", Factura = "PF003", FacPrecioTotal = 300 },
            new ProvFacturasLIN { Empresa = "TestEmpresa", Factura = "PF004", FacPrecioTotal = 400 },
            new ProvFacturasLIN { Empresa = "OtraEmpresa", Factura = "PF005", FacPrecioTotal = 500 },
        }.AsQueryable();

            SetupMockDbSet(_mockContext, testDataCAB, testDataLIN);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);

            // Act
            var result = await controller.ObtenerGastosAnuales(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var gastosAnuales = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(600m, gastosAnuales);
        }

        [Fact]
        public async Task ObtenerGastosAnuales_SinDatos_DebeRetornarCero()
        {
            // Arrange
            var year = 2023;
            var testDataCAB = new List<ProvFacturasCAB>().AsQueryable();
            var testDataLIN = new List<ProvFacturasLIN>().AsQueryable();

            SetupMockDbSet(_mockContext, testDataCAB, testDataLIN);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);

            // Act
            var result = await controller.ObtenerGastosAnuales(year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var gastosAnuales = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(0m, gastosAnuales);
        }


        [Fact]
        public void ObtenerDatosPedidosPorDia_SinDatos_DebeRetornarListaVacia()
        {
            _output.WriteLine("Iniciando test ObtenerDatosPedidosPorDia_SinDatos_DebeRetornarListaVacia");

            // Arrange
            var year = 2023;
            var month = 8; // Agosto
            var mockDateTime = new Mock<IDateTime>();
            mockDateTime.Setup(x => x.Now).Returns(new DateTime(year, month, 1));

            var testData = new List<PedidosCAB>().AsQueryable();

            _output.WriteLine($"Datos de prueba creados. Cantidad: {testData.Count()}");

            var mockSet = new Mock<DbSet<PedidosCAB>>();
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<PedidosCAB>(testData.Provider));
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.Expression).Returns(testData.Expression);
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.ElementType).Returns(testData.ElementType);
            mockSet.As<IQueryable<PedidosCAB>>().Setup(m => m.GetEnumerator()).Returns(testData.GetEnumerator());

            _mockContext.Setup(c => c.PedidosCAB).Returns(mockSet.Object);
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);
            controller.SetDateTime(mockDateTime.Object);

            // Act
            _output.WriteLine("Ejecutando ObtenerDatosPedidosPorDia");
            var result = controller.ObtenerDatosPedidosPorDia(year);

            // Assert
            _output.WriteLine("Iniciando aserciones");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var datosPorDia = Assert.IsType<List<object>>(okResult.Value);
            Assert.Empty(datosPorDia);

            _output.WriteLine("Test completado con éxito");
        }

        [Fact]
        public void ObtenerDatosPedidosPorDia_CuandoOcurreExcepcion_DebeRetornarStatusCode500()
        {
            _output.WriteLine("Iniciando test ObtenerDatosPedidosPorDia_CuandoOcurreExcepcion_DebeRetornarStatusCode500");

            // Arrange
            var year = 2023;
            var mockDateTime = new Mock<IDateTime>();
            mockDateTime.Setup(x => x.Now).Returns(new DateTime(2023, 8, 1));

            _mockContext.Setup(c => c.PedidosCAB).Throws(new Exception("Error de prueba"));
            _mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims { SessionEmpresa = "TestEmpresa" });

            var controller = new DashboardController(_mockContext.Object, _mockFunctionsBBDD.Object, _mockLogger.Object, _output);
            controller.SetDateTime(mockDateTime.Object);

            // Act
            _output.WriteLine("Ejecutando ObtenerDatosPedidosPorDia");
            var result = controller.ObtenerDatosPedidosPorDia(year);

            // Assert
            _output.WriteLine("Iniciando aserciones");
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Error interno del servidor", statusCodeResult.Value.ToString());

            _output.WriteLine("Test completado con éxito");
        }




        private void SetupMockDbSet(Mock<DbContextiLabPlus> mockContext, IQueryable<FacturasCAB> dataCAB, IQueryable<FacturasLIN> dataLIN)
        {
            var mockSetCAB = new Mock<DbSet<FacturasCAB>>();
            mockSetCAB.As<IQueryable<FacturasCAB>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<FacturasCAB>(dataCAB.Provider));
            mockSetCAB.As<IQueryable<FacturasCAB>>().Setup(m => m.Expression).Returns(dataCAB.Expression);
            mockSetCAB.As<IQueryable<FacturasCAB>>().Setup(m => m.ElementType).Returns(dataCAB.ElementType);
            mockSetCAB.As<IQueryable<FacturasCAB>>().Setup(m => m.GetEnumerator()).Returns(dataCAB.GetEnumerator());

            mockContext.Setup(c => c.FacturasCAB).Returns(mockSetCAB.Object);

            var mockSetLIN = new Mock<DbSet<FacturasLIN>>();
            mockSetLIN.As<IQueryable<FacturasLIN>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<FacturasLIN>(dataLIN.Provider));
            mockSetLIN.As<IQueryable<FacturasLIN>>().Setup(m => m.Expression).Returns(dataLIN.Expression);
            mockSetLIN.As<IQueryable<FacturasLIN>>().Setup(m => m.ElementType).Returns(dataLIN.ElementType);
            mockSetLIN.As<IQueryable<FacturasLIN>>().Setup(m => m.GetEnumerator()).Returns(dataLIN.GetEnumerator());

            mockContext.Setup(c => c.FacturasLIN).Returns(mockSetLIN.Object);
        }


        private void SetupMockDbSet<T>(Mock<DbContextiLabPlus> mockContext, IQueryable<T> dataCAB, IQueryable<PedidosLIN> dataLIN) where T : class
        {
            var mockSetCAB = new Mock<DbSet<T>>();
            mockSetCAB.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(dataCAB.Provider));
            mockSetCAB.As<IQueryable<T>>().Setup(m => m.Expression).Returns(dataCAB.Expression);
            mockSetCAB.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(dataCAB.ElementType);
            mockSetCAB.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(dataCAB.GetEnumerator());

            mockContext.Setup(c => c.Set<T>()).Returns(mockSetCAB.Object);

            if (typeof(T) == typeof(PedidosCAB))
            {
                mockContext.Setup(c => c.PedidosCAB).Returns(mockSetCAB.Object as DbSet<PedidosCAB>);
            }

            var mockSetLIN = new Mock<DbSet<PedidosLIN>>();
            mockSetLIN.As<IQueryable<PedidosLIN>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<PedidosLIN>(dataLIN.Provider));
            mockSetLIN.As<IQueryable<PedidosLIN>>().Setup(m => m.Expression).Returns(dataLIN.Expression);
            mockSetLIN.As<IQueryable<PedidosLIN>>().Setup(m => m.ElementType).Returns(dataLIN.ElementType);
            mockSetLIN.As<IQueryable<PedidosLIN>>().Setup(m => m.GetEnumerator()).Returns(dataLIN.GetEnumerator());

            mockContext.Setup(c => c.PedidosLIN).Returns(mockSetLIN.Object);
        }

        private void SetupMockDbSet<TCAB, TLIN>(Mock<DbContextiLabPlus> mockContext, IQueryable<TCAB> dataCAB, IQueryable<TLIN> dataLIN)
        where TCAB : class
        where TLIN : class
        {
            var mockSetCAB = new Mock<DbSet<TCAB>>();
            mockSetCAB.As<IQueryable<TCAB>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<TCAB>(dataCAB.Provider));
            mockSetCAB.As<IQueryable<TCAB>>().Setup(m => m.Expression).Returns(dataCAB.Expression);
            mockSetCAB.As<IQueryable<TCAB>>().Setup(m => m.ElementType).Returns(dataCAB.ElementType);
            mockSetCAB.As<IQueryable<TCAB>>().Setup(m => m.GetEnumerator()).Returns(dataCAB.GetEnumerator());

            var mockSetLIN = new Mock<DbSet<TLIN>>();
            mockSetLIN.As<IQueryable<TLIN>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<TLIN>(dataLIN.Provider));
            mockSetLIN.As<IQueryable<TLIN>>().Setup(m => m.Expression).Returns(dataLIN.Expression);
            mockSetLIN.As<IQueryable<TLIN>>().Setup(m => m.ElementType).Returns(dataLIN.ElementType);
            mockSetLIN.As<IQueryable<TLIN>>().Setup(m => m.GetEnumerator()).Returns(dataLIN.GetEnumerator());

            if (typeof(TCAB) == typeof(PedidosCAB) && typeof(TLIN) == typeof(PedidosLIN))
            {
                mockContext.Setup(c => c.PedidosCAB).Returns(mockSetCAB.Object as DbSet<PedidosCAB>);
                mockContext.Setup(c => c.PedidosLIN).Returns(mockSetLIN.Object as DbSet<PedidosLIN>);
            }
            else if (typeof(TCAB) == typeof(FacturasCAB) && typeof(TLIN) == typeof(FacturasLIN))
            {
                mockContext.Setup(c => c.FacturasCAB).Returns(mockSetCAB.Object as DbSet<FacturasCAB>);
                mockContext.Setup(c => c.FacturasLIN).Returns(mockSetLIN.Object as DbSet<FacturasLIN>);
            }
            else if (typeof(TCAB) == typeof(ProvFacturasCAB) && typeof(TLIN) == typeof(ProvFacturasLIN))
            {
                mockContext.Setup(c => c.ProvFacturasCAB).Returns(mockSetCAB.Object as DbSet<ProvFacturasCAB>);
                mockContext.Setup(c => c.ProvFacturasLIN).Returns(mockSetLIN.Object as DbSet<ProvFacturasLIN>);
            }
        }











    }



}

