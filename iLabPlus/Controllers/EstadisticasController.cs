using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nemesis365.Controllers
{
    public class EstadisticasController : Controller
    {

        private readonly DbContextiLabPlus   ctxDB;

        private readonly FunctionsBBDD      FunctionsBBDD;
        private readonly GrupoClaims        GrupoClaims;

        private readonly ILogger<EstadisticasController> _logger;

        public EstadisticasController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD, ILogger<EstadisticasController> logger)
        {
            _logger = logger;
            ctxDB = Context;

            FunctionsBBDD   = _FunctionsBBDD;
            GrupoClaims     = FunctionsBBDD.GetClaims();
        }


        public IActionResult Index()
        {

            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();

            return View("Estadisticas");
        }

        [HttpGet]
        public IActionResult ObtenerDatosGrafica(int anio, string clienteId = null) // Parámetro opcional
        {
            try
            {
                clienteId = clienteId?.Trim();
                var query = ctxDB.PedidosCAB
                                            .Join(
                                                ctxDB.PedidosLIN,
                                                cab => new { cab.Pedido, cab.Empresa, cab.Cliente },
                                                lin => new { lin.Pedido, lin.Empresa, lin.Cliente },
                                                (cab, lin) => new { cab, lin }
                                            )
                                            .Where(x => x.cab.PedFecha.HasValue && x.cab.PedFecha.Value.Year == anio && x.cab.Empresa == GrupoClaims.SessionEmpresa);

                // Si se pasa el clienteId, filtramos la consulta
                if (!string.IsNullOrEmpty(clienteId))
                {
                    query = query.Where(x => x.cab.Cliente == clienteId);
                }

                var datosAgrupados = query
                                        .GroupBy(q => new { Mes = q.cab.PedFecha.Value.Month })
                                        .Select(g => new
                                        {
                                            Mes = g.Key.Mes,
                                            Total = g.Sum(x => x.lin.PedPrecioTotal)
                                        })
                                        .OrderBy(x => x.Mes);


                var datosGrafica = datosAgrupados.ToList();
                return Ok(new { Datos = datosGrafica });
            }
            catch (Exception Ex)
            {
                return BadRequest(new { Error = Ex.Message });
            }
        }


        [HttpGet]
        public IActionResult ObtenerClientesConGastosPorAnio(int anio)
        {
            try
            {
                _logger.LogInformation($"Solicitando clientes con gastos para el año: {anio}");

                var clientesConGastos = ctxDB.PedidosCAB
                    .Where(cab => cab.PedFecha.HasValue &&
                                  cab.PedFecha.Value.Year == anio &&
                                  cab.Empresa == GrupoClaims.SessionEmpresa)
                    .Join(ctxDB.PedidosLIN,
                        cab => new { cab.Pedido, cab.Empresa, cab.Cliente },
                        lin => new { lin.Pedido, lin.Empresa, lin.Cliente },
                        (cab, lin) => new { cab.Cliente, lin.PedPrecioTotal })
                    .GroupBy(x => x.Cliente)
                    .Select(g => new
                    {
                        ClienteId = g.Key,
                        TotalGasto = g.Sum(x => x.PedPrecioTotal)
                    })
                    .Where(x => x.TotalGasto > 0)
                    .OrderByDescending(x => x.TotalGasto)
                    .Join(ctxDB.Clientes,
                        c => c.ClienteId,
                        cli => cli.Cliente,
                        (c, cli) => new
                        {
                            ClienteId = c.ClienteId,
                            CliNombre = cli.CliNombre,
                            TotalGasto = c.TotalGasto
                        })
                    .ToList();

                _logger.LogInformation($"Clientes encontrados: {clientesConGastos.Count}");

                return Ok(new { Clientes = clientesConGastos });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener clientes con gastos para el año {anio}: {ex.Message}");
                return BadRequest(new { Error = $"Error al obtener clientes con gastos para el año {anio}: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult ObtenerClientesConGastos(DateTime? inicio, DateTime? fin)
        {
            try
            {
                var query = ctxDB.PedidosCAB
                    .Where(cab => cab.Empresa == GrupoClaims.SessionEmpresa);

                // Filtramos por fecha si se proporcionan
                if (inicio.HasValue && fin.HasValue)
                {
                    query = query.Where(cab => cab.PedFecha.Value.Year >= inicio.Value.Year && cab.PedFecha.Value.Year <= fin.Value.Year);
                }

                // Obtener los clientes que tienen pedidos en el rango de fechas
                var clientesConGastos = query
                    .Join(ctxDB.PedidosLIN,
                          cab => new { cab.Pedido, cab.Empresa, cab.Cliente },
                          lin => new { lin.Pedido, lin.Empresa, lin.Cliente },
                          (cab, lin) => new { cab.Cliente, cab.Empresa, lin.PedPrecioTotal })
                    .Join(ctxDB.Clientes, // Hacemos un join adicional aquí con la tabla Clientes
                          pedidos => pedidos.Cliente,
                          clientes => clientes.Cliente,
                          (pedidos, clientes) => new { pedidos.Cliente, clientes.CliNombre, pedidos.Empresa, pedidos.PedPrecioTotal })
                    .GroupBy(x => new { x.Cliente, x.CliNombre }) // Agrupamos por Cliente y CliNombre
                    .Select(group => new
                    {
                        Cliente = group.Key.Cliente,
                        CliNombre = group.Key.CliNombre, // Agregamos el CliNombre
                        TotalGasto = group.Sum(x => x.PedPrecioTotal) // Sumamos el total de PedPrecioTotal
                    })
                    .Where(x => x.TotalGasto > 0) // Filtrar clientes con gastos
                    .OrderBy(x => x.Cliente)
                    .ToList();

                return Ok(new { Clientes = clientesConGastos });
            }
            catch (Exception Ex)
            {
                return BadRequest(new { Error = Ex.Message });
            }
        }


        [HttpGet]
        public IActionResult ObtenerDatosGraficaPastel(int anio, int inicio, int fin)
        {
            try
            {
                var datosPastel = ctxDB.PedidosCAB
                    .Where(cab => cab.Empresa == GrupoClaims.SessionEmpresa)
                    .Join(ctxDB.PedidosLIN,
                        cab => new { cab.Pedido, cab.Empresa, cab.Cliente },
                        lin => new { lin.Pedido, lin.Empresa, lin.Cliente },
                        (cab, lin) => new { cab, lin }
                    )
                    .Join(
                        ctxDB.Clientes,
                        cabLin => cabLin.cab.Cliente,
                        cliente => cliente.Cliente,
                        (cabLin, cliente) => new { cabLin.cab, cabLin.lin, cliente.CliNombre }
                    )
                    .Where(x => x.cab.PedFecha.HasValue && x.cab.PedFecha.Value.Year == anio && x.lin.PedPrecioTotal > 0)
                    .GroupBy(x => new { x.cab.Cliente, x.CliNombre },
                        x => x.lin.PedPrecioTotal,
                        (key, group) => new
                        {
                            ClienteId = key.Cliente,
                            ClienteNombre = key.CliNombre,
                            Total = group.Sum()
                        }
                    )
                    .Where(x => x.Total > 0)
                    .OrderByDescending(x => x.Total)
                    .Skip(inicio - 1)
                    .Take(fin - inicio + 1)
                    .ToList();

                return Ok(new { DatosPastel = datosPastel });
            }
            catch (Exception Ex)
            {
                return BadRequest(new { Error = Ex.Message });
            }
        }





        [HttpGet]
        public IActionResult TotalClientesConPedidos(int anio)
        {
            try
            {
                // Filtrar los clientes que tienen pedidos con total mayor a 0 para el año 2022
                int totalClientesConPedidos = ctxDB.PedidosCAB
                    .Join(ctxDB.PedidosLIN, cab => new { cab.Pedido, cab.Empresa, cab.Cliente },
                          lin => new { lin.Pedido, lin.Empresa, lin.Cliente },
                          (cab, lin) => new { cab, lin })
                    .Where(x => x.cab.PedFecha.HasValue && x.cab.PedFecha.Value.Year == anio && x.lin.PedPrecioTotal > 0)
                    .Select(x => x.cab.Cliente)
                    .Distinct() // Usar Distinct para contar cada cliente solo una vez;
                    .Count();


                return Ok(new { totalClientesConPedidos });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Ocurrió un error al obtener el total de clientes con pedidos.", ex });
            }
        }

        [HttpGet]
        public IActionResult ObtenerAniosDisponibles()
        {
            try
            {

                var aniosConGastos = ctxDB.PedidosCAB
                            .Join(ctxDB.PedidosLIN,
                                cab => new { cab.Pedido, cab.Empresa, cab.Cliente },
                                lin => new { lin.Pedido, lin.Empresa, lin.Cliente },
                                (cab, lin) => new { cab, lin }
                                )
                            .Where(x => x.cab.PedFecha.HasValue
                                         && x.lin.PedPrecioTotal > 0
                                         && x.cab.Empresa == GrupoClaims.SessionEmpresa)
                            .Select(x => x.cab.PedFecha.Value.Year)
                            .Distinct()
                            .OrderBy(year => year)
                            .ToList();



                return Ok(aniosConGastos);
            }

            catch (Exception Ex)

            {
                _logger.LogError("Ocurrió un error al obtener los años disponibles: {Error}", Ex.Message);
                return BadRequest(new { Error = Ex.Message });
            }
        }

        [HttpGet]

        public IActionResult ObtenerTotalGastosPorAnio()
        {
            try
            {
                var gastosPorAnio = ctxDB.PedidosCAB
                                    .Where(cab => cab.Empresa == GrupoClaims.SessionEmpresa && cab.PedFecha.HasValue)
                                    .Join(ctxDB.PedidosLIN,
                                    cab => new { cab.Pedido, cab.Empresa },
                                    lin => new { lin.Pedido, lin.Empresa },
                                    (cab, lin) => new { cab.PedFecha, lin.PedPrecioTotal })
                                    .GroupBy(x => x.PedFecha.Value.Year)
                                    .Select(group => new
                                    {
                                        Anio = group.Key,
                                        TotalGasto = group.Sum(x => x.PedPrecioTotal)
                                    })
                                    .OrderBy(x => x.Anio)
                                    .ToList();

                return Ok(new { GastosPorAnio = gastosPorAnio });
            }
            catch (Exception Ex)
            {
                _logger.LogError("Ocurrió un error al obtener el total de gastos por año: {Error}", Ex.Message);
                return BadRequest(new { Error = Ex.Message });
            }
        }


        public async Task<IActionResult> ObtenerDatosParaGraficoBurbujaConAnios()
        {
            try
            {
                var datosBurbuja = await ctxDB.PedidosCAB
                                    .Where(cab => cab.PedFecha.HasValue)
                                    .Join(ctxDB.PedidosLIN,
                                        cab => new { cab.Empresa, cab.Pedido },
                                        lin => new { lin.Empresa, lin.Pedido },
                                        (cab, lin) => new { cab.Cliente, cab.PedFecha, lin.PedPeso, lin.PedPrecioTotal })
                                    .Join(ctxDB.Clientes,
                                        ped => ped.Cliente,
                                        cli => cli.Cliente,
                                        (ped, cli) => new { ped.Cliente, cli.CliNombre, ped.PedPeso, ped.PedPrecioTotal, ped.PedFecha })
                                    .GroupBy(x => new { x.PedFecha.Value.Year, x.Cliente, x.CliNombre })
                                    .Select(grp => new
                                    {
                                        Anio = grp.Key.Year,
                                        ClienteId = grp.Key.Cliente,
                                        CliNombre = grp.Key.CliNombre,
                                        CantidadPedidos = grp.Count(),
                                        ValorTotal = grp.Sum(p => p.PedPrecioTotal),
                                        PesoTotal = grp.Sum(p => p.PedPeso)
                                    })
                                    .OrderBy(x => x.Anio).ThenBy(x => x.CliNombre)
                                    .ToListAsync();

                var aniosConGastos = datosBurbuja.Select(x => x.Anio).Distinct().ToList();

                return Json(new { Anios = aniosConGastos, Datos = datosBurbuja });
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult ObtenerProductosMasVendidos(int anio, string tipo = "unidades")
        {
            try
            {
                var productosQuery = ctxDB.PedidosLIN
                    .Join(ctxDB.PedidosCAB,
                          lin => lin.Pedido,
                          cab => cab.Pedido,
                          (lin, cab) => new { lin, cab })
                    .Where(x => x.cab.PedFecha.HasValue && x.cab.PedFecha.Value.Year == anio);

                var productos = productosQuery
                    .GroupBy(x => x.lin.PedArt)
                    .Select(group => new
                    {
                        Articulo = group.Key,
                        TotalVendido = group.Sum(x => x.lin.PedQty),
                        TotalVendidoImporte = group.Sum(x => x.lin.PedPrecioTotal)
                    })
                    .Join(ctxDB.Articulos,
                          vendidos => vendidos.Articulo,
                          articulo => articulo.Articulo,
                          (vendidos, articulo) => new
                          {
                              Articulo = vendidos.Articulo,
                              Descripcion = articulo.ArtDes,
                              TotalVendido = vendidos.TotalVendido,
                              TotalVendidoImporte = vendidos.TotalVendidoImporte
                          });

                if (tipo == "importe")
                {
                    productos = productos.OrderByDescending(x => x.TotalVendidoImporte);
                }
                else
                {
                    productos = productos.OrderByDescending(x => x.TotalVendido);
                }

                var xxx = productos.Take(50).ToList();

                return Ok(new { Productos = productos.Take(50).ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al obtener los productos más vendidos: {Error}", ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
        }




        [HttpGet]
        public IActionResult ObtenerTotalStocks()
        {
            try
            {

                var stocksQuery = ctxDB.Stocks
                               .Where(stock => stock.Empresa == GrupoClaims.SessionEmpresa)
                               .Where(stock => stock.StkFisico != 0 && stock.StkMinimo != 0);

                int totalStocks = stocksQuery.Count();

                return Ok(new { Total = totalStocks });
            } 
            catch (Exception ex)
            {
                _logger.LogError("Error al obtener el total de stocks: {Error}", ex.Message);
                return BadRequest(new { Error = ex.Message }); 
            }
        }


      


        [HttpGet]
        public IActionResult ObtenerStocks(int inicio = 1, int fin = 15)
        {
            try
            {
                var stocksQuery = ctxDB.Stocks
                    .Where(stock => stock.Empresa == GrupoClaims.SessionEmpresa)
                    .Where(stock => stock.StkFisico != 0 && stock.StkMinimo != 0) // Agrega esta línea
                    .Select(stock => new
                    {
                        Articulo = stock.StkArticulo,
                        StockActual = stock.StkFisico,
                        StockMinimo = stock.StkMinimo,
                        StockMaximo = stock.StkMaximo,
                        DiferenciaMinimo = stock.StkFisico - stock.StkMinimo
                    });

                var totalRegistros = stocksQuery.Count();

                var stocksData = stocksQuery
                    .OrderBy(stock => stock.DiferenciaMinimo)
                    .ThenByDescending(stock => stock.StockActual < 0)
                    .Skip(inicio - 1)
                    .Take(fin - inicio + 1)
                    .ToList();

                return Ok(new { Datos = stocksData, TotalRegistros = totalRegistros });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al obtener datos de stocks: {Error}", ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult BuscarStocks(string articulo)
        {
            try
            {
                var stocksQuery = ctxDB.Stocks
                    .Where(stock => stock.Empresa == GrupoClaims.SessionEmpresa &&
                                    stock.StkArticulo.Contains(articulo) &&
                                    stock.StkFisico != 0 && stock.StkMinimo != 0);

                var stocksData = stocksQuery
                    .Select(stock => new
                    {
                        Articulo = stock.StkArticulo,
                        StockActual = stock.StkFisico,
                        StockMinimo = stock.StkMinimo,
                        StockMaximo = stock.StkMaximo
                    })
                    .ToList();

                return Ok(new { Datos = stocksData });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al buscar datos de stocks: {Error}", ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
        }



        [HttpGet]
        public IActionResult ObtenerTotalStocksRuptura()
        {
            try
            {
                var totalStocksRuptura = ctxDB.Stocks
                    .Where(stock => stock.Empresa == GrupoClaims.SessionEmpresa && stock.StkFisico < stock.StkMinimo && stock.StkMinimo != 0)
                    .Count();

                return Ok(new { Total = totalStocksRuptura });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al obtener el total de stocks de ruptura: {Error}", ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
        }




        [HttpGet] 

        public IActionResult ObtenerRupturaDeStocks(int inicio = 1, int fin = 15)
        {
            try
            {
                var stocksRupturaQuery = ctxDB.Stocks
                           .Where(stock => stock.Empresa == GrupoClaims.SessionEmpresa && stock.StkFisico < stock.StkMinimo && stock.StkMinimo != 0);

                int totalRegistros = stocksRupturaQuery.Count();

                var stocksRuptura = stocksRupturaQuery
                                        .OrderByDescending(stock => stock.StkFisico == 0) // Priorizar artículos con Stock Fisico igual a 0
                                        .ThenByDescending(stock => stock.StkFisico == 0 ? stock.StkMinimo : 0) // Dentro de los que tienen Stock Fisico igual a 0, ordenar por Stock Minimo descendente
                                        .ThenByDescending(stock => stock.StkMinimo - stock.StkFisico) // Para los que tienen Stock Fisico, ordenar por la cercanía a 0 de Stock Fisico
                                        .ThenBy(stock => stock.StkArticulo) // Ordenar por Articulo para los que tienen Stock Fisico
                                        .Skip(inicio - 1)
                                        .Take(fin)
                                        .Select(stock => new
                                        {
                                            Articulo = stock.StkArticulo,
                                            StockActual = stock.StkFisico,
                                            StockMinimo = stock.StkMinimo
                                        })
                                        .ToList();


                return Ok(new { Datos = stocksRuptura, TotalRegistros = totalRegistros });

            }

            catch (Exception ex)

            {

                _logger.LogError("Error al obtener datos de ruptura de stocks: {Error}", ex.Message);
                return BadRequest(new { Error = ex.Message });

            }
        }

    }

}


