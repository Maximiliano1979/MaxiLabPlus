using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Nemesis365.Controllers
{

    public interface IDateTime
    {
        DateTime Now { get; }
    }

    public class SystemDateTime : IDateTime
    {
        public DateTime Now => DateTime.Now;
    }


    [Authorize]
    public class DashboardController : Controller
    {
        private readonly DbContextiLabPlus ctxDB;
        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;
        private readonly ILogger<DashboardController> _logger;
        private readonly ITestOutputHelper _testOutputHelper;
        private IDateTime _dateTime;


        public DashboardController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD, ILogger<DashboardController> logger, ITestOutputHelper testOutputHelper = null)
        {
            ctxDB = Context ?? throw new ArgumentNullException(nameof(Context));
            FunctionsBBDD = _FunctionsBBDD ?? throw new ArgumentNullException(nameof(_FunctionsBBDD));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _testOutputHelper = testOutputHelper;
            GrupoClaims = FunctionsBBDD.GetClaims();
            _dateTime = new SystemDateTime(); // Usa la implementación por defecto

        }

        // Este método se usa solo para testing
        public void SetDateTime(IDateTime dateTime)
        {
            _dateTime = dateTime;
        }

        private void Log(string message)
        {
            _logger.LogInformation(message);
            _testOutputHelper?.WriteLine(message);
        }

        public IActionResult Index()
        {

            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();


            return View("Dashboard");
        }



        [HttpGet]
        public async Task<IActionResult> ObtenerFacturacionClientesAnual(int year)
        {
            Log($"Iniciando ObtenerFacturacionClientesAnual para el año {year}");
            try
            {
                if (ctxDB.FacturasLIN == null || ctxDB.FacturasCAB == null)
                {
                    Log("ctxDB.FacturasLIN o ctxDB.FacturasCAB es nulo");
                    return StatusCode(500, "Error: Contexto de base de datos no inicializado correctamente");
                }

                var grupoClaims = FunctionsBBDD.GetClaims();
                if (grupoClaims == null || string.IsNullOrEmpty(grupoClaims.SessionEmpresa))
                {
                    Log("GrupoClaims.SessionEmpresa es null o vacío");
                    return StatusCode(500, "Error: SessionEmpresa no inicializada correctamente");
                }

                var query = from lin in ctxDB.FacturasLIN
                            join cab in ctxDB.FacturasCAB
                            on new { lin.Empresa, lin.Cliente, lin.Factura } equals new { cab.Empresa, cab.Cliente, cab.Factura }
                            where cab.FacFecha.HasValue &&
                                  cab.FacFecha.Value.Year == year &&
                                  cab.Empresa == GrupoClaims.SessionEmpresa &&
                                  lin.FacPrecioTotal.HasValue
                            select new
                            {
                                Cliente = lin.Cliente,
                                FacPrecioTotal = lin.FacPrecioTotal.Value,
                                Empresa = cab.Empresa,
                                FacFecha = cab.FacFecha.Value
                            };

                var results = await query.ToListAsync();
                Log($"Número de resultados obtenidos: {results.Count}");

                if (results.Count == 0)
                {
                    Log("No se encontraron resultados para la consulta");
                    return Json(new List<EstDashClientesFact>());
                }

                var TotalClientesfact = results.Sum(r => r.FacPrecioTotal);

                var DashClientesFact = results
                    .GroupBy(r => r.Cliente)
                    .Select(g => new EstDashClientesFact
                    {
                        Cliente = g.Key ?? "Sin Cliente",
                        ClienteImporteFact = g.Sum(r => r.FacPrecioTotal),
                        TotalImporteTodosClientes = TotalClientesfact,
                        Porcentaje = TotalClientesfact != 0
                            ? ((double)g.Sum(r => r.FacPrecioTotal) / (double)TotalClientesfact * 100).ToString("N2").Replace(",", ".")
                            : "0.00"
                    })
                    .OrderByDescending(g => g.ClienteImporteFact)
                    .ToList();

                Log($"Finalizando ObtenerFacturacionClientesAnual. Número de clientes: {DashClientesFact.Count}");
                return Json(DashClientesFact);
            }
            catch (Exception ex)
            {
                Log($"Error en ObtenerFacturacionClientesAnual: {ex.Message}");
                Log($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }





        [HttpGet]
        public IActionResult ObtenerArtMasPedidosPrecio(int year)
        {
            Log($"Iniciando ObtenerArtMasPedidosPrecio para el año {year}");
            try
            {
                Log("Creando consulta inicial");
                var query = ctxDB.PedidosLIN
                    .Join(ctxDB.PedidosCAB,
                        lin => new { lin.Empresa, lin.Cliente, lin.Pedido },
                        cab => new { cab.Empresa, cab.Cliente, cab.Pedido },
                        (lin, cab) => new { lin, cab });

                Log("Aplicando filtros");
                query = query.Where(joined => joined.cab.PedFecha.HasValue &&
                                              joined.cab.PedFecha.Value.Year == year &&
                                              joined.cab.Empresa == GrupoClaims.SessionEmpresa);

                Log("Contando registros");
                var count = query.Count();
                Log($"Número de registros en la consulta: {count}");

                Log("Calculando TotalArtsPedPrecio");
                var TotalArtsPedPrecio = query.Sum(joined => joined.lin.PedPrecioTotal ?? 0);
                Log($"TotalArtsPedPrecio: {TotalArtsPedPrecio}");

                if (TotalArtsPedPrecio == 0)
                {
                    Log("No se encontraron pedidos para el año especificado");
                    return Json(new List<EstDashArtPedidos>());
                }

                Log("Calculando ArtMasPedidosPrecio");
                var groupedQuery = query.GroupBy(joined => joined.lin.PedArt);

                var ArtMasPedidosPrecio = groupedQuery
                    .Select(g => new EstDashArtPedidos
                    {
                        Articulo = g.Key,
                        ArticuloPrecioPedido = g.Sum(joined => joined.lin.PedPrecioTotal ?? 0),
                        TotalPrecioTodosArticulos = TotalArtsPedPrecio,
                        Porcentaje = ((double)(g.Sum(joined => joined.lin.PedPrecioTotal ?? 0) / TotalArtsPedPrecio) * 100).ToString("N2").Replace(",", ".")
                    })
                    .OrderByDescending(g => g.ArticuloPrecioPedido)
                    .Take(40)
                    .ToList();

                Log($"Finalizando ObtenerArtMasPedidosPrecio. Número de artículos: {ArtMasPedidosPrecio.Count}");
                return Json(ArtMasPedidosPrecio);
            }
            catch (Exception ex)
            {
                Log($"Error en ObtenerArtMasPedidosPrecio: {ex.Message}");
                Log($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Log($"Inner Exception: {ex.InnerException.Message}");
                    Log($"Inner Exception StackTrace: {ex.InnerException.StackTrace}");
                }
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }




        [HttpGet]
        public async Task<IActionResult> ObtenerArtMasPedidosQty(int year)
        {
            var TotalArtsPedQty = await ctxDB.PedidosLIN
                .Join(ctxDB.PedidosCAB,
                    lin => new { lin.Empresa, lin.Cliente, lin.Pedido },
                    cab => new { cab.Empresa, cab.Cliente, cab.Pedido },
                    (lin, cab) => new { lin, cab })
                .Where(joined => joined.cab.PedFecha.Value.Year == year && joined.cab.Empresa == GrupoClaims.SessionEmpresa)
                .SumAsync(joined => joined.lin.PedQty);

            var ArtMasPedidosQty = await ctxDB.PedidosLIN
                .Join(ctxDB.PedidosCAB,
                    lin => new { lin.Empresa, lin.Cliente, lin.Pedido },
                    cab => new { cab.Empresa, cab.Cliente, cab.Pedido },
                    (lin, cab) => new { lin, cab })
                .Where(joined => joined.cab.PedFecha.Value.Year == year && joined.cab.Empresa == GrupoClaims.SessionEmpresa)
                .GroupBy(joined => joined.lin.PedArt)
                .Select(g => new EstDashArtPedidos
                {
                    Articulo = g.Key,
                    ArticuloQtyPedido = g.Sum(joined => joined.lin.PedQty),
                    TotalQtyTodosArticulos = TotalArtsPedQty,
                    Porcentaje = ((double)g.Sum(joined => joined.lin.PedQty) / (double)TotalArtsPedQty * 100).ToString("N2").Replace(",", ".")
                })
                .OrderByDescending(g => g.ArticuloQtyPedido)
                .Take(40)
                .ToListAsync();

            return Json(ArtMasPedidosQty);
        }



        public class DatosPedidoPorDia
        {
            public int Dia { get; set; }
            public int CantidadPedidos { get; set; }
        }



        [HttpGet]
        public IActionResult ObtenerDatosPedidosPorDia(int year)
        {
            Log($"Iniciando ObtenerDatosPedidosPorDia para el año {year}");
            try
            {
                var inicioMes = new DateTime(year, _dateTime.Now.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);
                Log($"Rango de fechas: {inicioMes} a {finMes}");

                var query = ctxDB.PedidosCAB.AsQueryable();

                // Logs para verificar valores clave
                Log($"ctxDB es nulo: {ctxDB == null}");
                Log($"PedidosCAB es nulo: {ctxDB.PedidosCAB == null}");
                Log($"GrupoClaims es nulo: {GrupoClaims == null}");
                Log($"SessionEmpresa es nulo o vacío: {string.IsNullOrEmpty(GrupoClaims.SessionEmpresa)}");

                Log($"Consulta inicial creada. Cantidad de registros: {query.Count()}");

                // Logs adicionales para seguimiento de la consulta
                Log($"Consulta antes del filtro de empresa: {query.Count()}");
                query = query.Where(p => p.Empresa == GrupoClaims.SessionEmpresa);
                Log($"Consulta después del filtro de empresa: {query.Count()}");

                Log($"Consulta antes del filtro de fecha no nula: {query.Count()}");
                query = query.Where(p => p.PedFecha.HasValue);
                Log($"Consulta después del filtro de fecha no nula: {query.Count()}");

                Log($"Consulta antes del filtro de rango de fechas: {query.Count()}");
                query = query.Where(p => p.PedFecha.Value >= inicioMes && p.PedFecha.Value <= finMes);
                Log($"Consulta después del filtro de rango de fechas: {query.Count()}");

                var datosExistentesPorDia = query
                    .AsEnumerable()
                    .GroupBy(pedido => pedido.PedFecha.Value.Day)
                    .Select(grupo => new
                    {
                        Dia = grupo.Key,
                        CantidadPedidos = grupo.Count()
                    })
                    .ToList();

                Log($"Datos existentes obtenidos. Cantidad: {datosExistentesPorDia.Count}");

                if (datosExistentesPorDia.Count == 0)
                {
                    Log("No se encontraron datos para el período especificado");
                    return Ok(new List<object>());
                }


                var datosPorDia = Enumerable.Range(1, finMes.Day)
                    .Select(day => new DatosPedidoPorDia
                    {
                        Dia = day,
                        CantidadPedidos = datosExistentesPorDia.FirstOrDefault(x => x.Dia == day)?.CantidadPedidos ?? 0
                    })
                    .ToList();


                Log($"Datos procesados. Cantidad total de días: {datosPorDia.Count}");
                return Ok(datosPorDia);
            }
            catch (Exception ex)
            {
                Log($"Error en ObtenerDatosPedidosPorDia: {ex.Message}");
                Log($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }






        [HttpGet]
        public async Task<IActionResult> ObtenerEntregasHoy(int year)
        {
            var hoy = new DateTime(year, _dateTime.Now.Month, _dateTime.Now.Day);

            var cantidadEntregasHoy = await ctxDB.PedidosCAB
                .CountAsync(p => p.Empresa == GrupoClaims.SessionEmpresa && p.PedFechaEnt == hoy);

            return Ok(new { cantidad = cantidadEntregasHoy });
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerEntregasSemana(int year)
        {
            try
            {
                Log($"Iniciando ObtenerEntregasSemana para el año {year}");

                var inicioSemana = new DateTime(year, _dateTime.Now.Month, _dateTime.Now.Day).AddDays(-(int)_dateTime.Now.DayOfWeek + (int)DayOfWeek.Monday);
                var finSemana = inicioSemana.AddDays(7);

                Log($"Rango de fechas: {inicioSemana} a {finSemana}");

                // Primero obtenemos los datos de la base de datos
                var pedidos = await ctxDB.PedidosCAB
                    .Where(p => p.Empresa == GrupoClaims.SessionEmpresa && p.PedFechaEnt >= inicioSemana && p.PedFechaEnt < finSemana)
                    .ToListAsync();

               Log($"Número de pedidos obtenidos: {pedidos.Count}");

                // Luego realizamos la agrupación en el lado del cliente
                var entregasSemana = pedidos
                    .GroupBy(pedido => pedido.PedFechaEnt.Value.DayOfWeek)
                    .Select(grupo => new
                    {
                        DiaSemana = (int)grupo.Key,
                        CantidadEntregas = grupo.Count()
                    })
                    .ToList();

               Log($"Entregas agrupadas por día: {string.Join(", ", entregasSemana.Select(e => $"{e.DiaSemana}:{e.CantidadEntregas}"))}");

                return Ok(entregasSemana);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en ObtenerEntregasSemana: {ex.Message}");
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerEntregasPendSemana(int year)
        {
            try
            {
                var inicioSemana = new DateTime(year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-(int)DateTime.Now.DayOfWeek + (int)DayOfWeek.Monday);
                var finSemana       = inicioSemana.AddDays(6);

                var LineasTotalEntregar = await ctxDB.PedidosCAB
                    .Join(
                        ctxDB.PedidosLIN,
                        cab => new { cab.Empresa, cab.Cliente, cab.Pedido },
                        lin => new { lin.Empresa, lin.Cliente, lin.Pedido },
                        (cab, lin) => new { Cabecera = cab, Linea = lin }
                    )
                    .Where(joined =>
                        joined.Cabecera.Empresa     == GrupoClaims.SessionEmpresa &&
                        joined.Cabecera.PedFechaEnt >= inicioSemana &&
                        joined.Cabecera.PedFechaEnt <= finSemana
                    )
                    .CountAsync();

                var LineasEntregadas = await ctxDB.PedidosCAB
                    .Join(
                        ctxDB.PedidosLIN,
                        cab => new { cab.Empresa, cab.Cliente, cab.Pedido },
                        lin => new { lin.Empresa, lin.Cliente, lin.Pedido },
                        (cab, lin) => new { Cabecera = cab, Linea = lin }
                    )
                    .Where(joined =>
                        joined.Cabecera.Empresa == GrupoClaims.SessionEmpresa &&
                        joined.Cabecera.PedFechaEnt >= inicioSemana &&
                        joined.Cabecera.PedFechaEnt <= finSemana &&
                        joined.Linea.PedEstado == "E"
                    )
                    .CountAsync();

                double Porcentaje = 0;

                // Evitar la división por cero
                if (LineasTotalEntregar > 0)
                {
                    Porcentaje = (double)LineasEntregadas / LineasTotalEntregar * 100;
                }



                return Ok(new
                {
                    Porcentaje,
                    LineasEntregadas,
                    LineasTotalEntregar
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }


        }


        [HttpGet]
        public async Task<IActionResult> ObtenerVentasPorMes(int year)
        {
            var inicioMes = new DateTime(year, DateTime.Now.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var ventasMes = await ctxDB.FacturasCAB
                .Where(factura => factura.FacFecha >= inicioMes && factura.FacFecha <= finMes && factura.Empresa == GrupoClaims.SessionEmpresa)
                .Join(ctxDB.FacturasLIN,
                    cab => new { cab.Factura, cab.Empresa },
                    lin => new { lin.Factura, lin.Empresa },
                    (cab, lin) => new { cab, lin })
                .GroupBy(x => x.cab.FacFecha.Value.Day)
                .Select(grupo => new
                {
                    Dia = grupo.Key,
                    TotalVentas = grupo.Sum(x => x.lin.FacPrecioTotal) // Suma del total de las líneas de pedido
                })
                .ToListAsync();

            return Ok(ventasMes);
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerVentasPorMesesDelAnio(int year)
        {
           // var anoActual = DateTime.Now.Year;

            var ventasMeses = await ctxDB.FacturasCAB
                .Where(factura => factura.FacFecha.HasValue && factura.FacFecha.Value.Year == year && factura.Empresa == GrupoClaims.SessionEmpresa)
                .Join(ctxDB.FacturasLIN,
                    cab => new { cab.Factura, cab.Empresa },
                    lin => new { lin.Factura, lin.Empresa },
                    (cab, lin) => new { cab, lin })
                .GroupBy(x => x.cab.FacFecha.Value.Month)
                .Select(grupo => new
                {
                    Mes = grupo.Key,
                    TotalVentas = grupo.Sum(x => x.lin.FacPrecioTotal) // Asumiendo que PedPrecioTotal es el campo correcto
                })
                .ToListAsync();

            return Ok(ventasMeses);
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerTotalAlbaranesAnual(int year)
        {
            // var añoActual = DateTime.Now.Year;
            var totalAlbaranesAnual = await ctxDB.PedidosLIN
                .Join(ctxDB.PedidosCAB,
                      lin => new { lin.Empresa, lin.Pedido },
                      cab => new { cab.Empresa, cab.Pedido },
                      (lin, cab) => new { lin, cab })
                .Where(x => x.cab.PedAlbaranFecha.HasValue && x.cab.PedAlbaranFecha.Value.Year == year && x.cab.Empresa == GrupoClaims.SessionEmpresa && x.cab.PedAlbaran == true)
                .SumAsync(x => x.lin.PedPrecioTotal ?? 0);

            return Ok(totalAlbaranesAnual);
        }


        [HttpGet]

        public async Task<IActionResult> ObtenerTotalFacturacionAnual(int year)
        {
            // var añoActual = DateTime.Now.Year;
            var totalFacturacionAnual = await ctxDB.FacturasLIN
                .Join(ctxDB.FacturasCAB,
                      lin => new { lin.Empresa, lin.Factura },
                      cab => new { cab.Empresa, cab.Factura },
                      (lin, cab) => new { LIN = lin, CAB =cab })
                .Where(x => x.CAB.FacFecha.Value.Year == year && x.CAB.Empresa == GrupoClaims.SessionEmpresa)
                .SumAsync(x => x.LIN.FacPrecioTotal ?? 0);

            return Ok(totalFacturacionAnual);
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerGastosAnuales(int year)
        {
            var gastosAnuales = await ctxDB.ProvFacturasLIN
                .Join(ctxDB.ProvFacturasCAB,
                    lin => new {lin.Empresa, lin.Factura},
                    cab => new {cab.Empresa, cab.Factura},
                    (lin, cab) => new {LIN = lin, CAB = cab})
                .Where(x => x.CAB.FacFecha.HasValue && x.CAB.FacFecha.Value.Year == year && x.CAB.Empresa == GrupoClaims.SessionEmpresa)
                .SumAsync(x => x.LIN.FacPrecioTotal ?? 0);

            return Ok(gastosAnuales);
        }




    }


}
