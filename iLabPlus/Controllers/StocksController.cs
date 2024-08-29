using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using C1.Web.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Newtonsoft.Json;

//using NLog;



namespace Nemesis365.Controllers
{
    [Authorize]
    public class StocksController : Controller
    {
        private readonly DbContextiLabPlus   ctxDB;
        private readonly ILogger<EstadisticasController> _logger;
        private readonly FunctionsBBDD      _functionsBBDD;
        private readonly GrupoClaims        GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;

        public StocksController(DbContextiLabPlus Context, FunctionsBBDD functionsBBDD, ILogger<EstadisticasController> logger)
        {
            ctxDB                = Context;
            _logger              = logger;
            _functionsBBDD       = functionsBBDD;
            GrupoClaims          = functionsBBDD.GetClaims();
            GrupoColumnsLayout   = functionsBBDD.GetColumnsLayout("gridStocks");
        }


        public IActionResult Index()
        {
            ViewBag.MenuUserList        = _functionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser   = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser   = GrupoColumnsLayout.ColumnsPinnedUser;

            var Almacenes = new List<string>();
            Almacenes.Add("Almacen");
            Almacenes.AddRange(ctxDB.StocksALM.Where(x => x.Empresa == GrupoClaims.SessionEmpresa ).OrderBy(x => x.StkOrden).ThenBy(x => x.StkAlmacen).Select(x => x.StkAlmacen).ToList());
            ViewBag.ListAlmacenes = Almacenes;

            var FiltroQty = new List<string>();
            FiltroQty.Add("Filtro Cantidades");
            FiltroQty.Add("Físico +/-");
            FiltroQty.Add("Físico +");
            FiltroQty.Add("Físico -");
            FiltroQty.Add("Mínimo +/-");
            FiltroQty.Add("Máximo +/-");
            FiltroQty.Add("Reservado +/-");

            ViewBag.FiltroQty = FiltroQty;

            var Stocks = ctxDB.Stocks.Where(x=>x.Empresa == GrupoClaims.SessionEmpresa)
                .Select(c => new Stocks
                {
                    Guid        = c.Guid,
                    Empresa     = c.Empresa,
                    StkAlmacen  = c.StkAlmacen,
                    StkArticulo = c.StkArticulo,

                    StkKilates      = c.StkKilates,
                    StkColor        = c.StkColor,
                    StkFisico       = c.StkFisico,
                    StkMinimo       = c.StkMinimo,
                    StkMaximo       = c.StkMaximo,
                    StkReservado    = c.StkReservado,
                    StkUbicacion    = c.StkUbicacion,

                    ArtDescrip  = ctxDB.Articulos.Where(x => x.Empresa == c.Empresa && x.Articulo == c.StkArticulo).FirstOrDefault().ArtDes,

                    StkAlmOrden = ctxDB.StocksALM.Where(x => x.Empresa == c.Empresa && x.StkAlmacen == c.StkAlmacen).FirstOrDefault().StkOrden,

                    IsoUser     = c.IsoUser,
                    IsoFecAlt   = c.IsoFecAlt,
                    IsoFecMod   = c.IsoFecMod,

                })
                .OrderBy(x=>x.StkAlmOrden).ThenBy(x => x.StkArticulo)
                .ToList();

            return View("Stocks", Stocks);
        }

        public IActionResult DialogStocks(Guid Guid)
        {
            var Stocks = ctxDB.Stocks.Where(x => x.Guid == Guid).FirstOrDefault();
            if (Stocks == null)
            {
                Stocks = new Stocks();
            }

            var Almacenes = new List<string>();
            Almacenes.Add("Almacen");
            Almacenes.AddRange(ctxDB.StocksALM.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.StkOrden).ThenBy(x => x.StkAlmacen).Select(x => x.StkAlmacen).ToList());
            ViewBag.ListAlmacenes = Almacenes;

            var ValsysVacio = new ValSys();
            ValsysVacio.Indice  = "";
            ValsysVacio.Clave   = "";
            ValsysVacio.Valor1  = "";

            var ValsysKilates = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "Kilates").OrderBy(x => x.Indice).ThenBy(x => x.Clave).ToList();
            ValsysKilates.Insert(0, ValsysVacio);
            ViewBag.ValsysKilates = ValsysKilates;

            var ValsysColores = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "Colores").OrderBy(x => x.Indice).ThenBy(x => x.Clave).ToList();
            ValsysColores.Insert(0, ValsysVacio);
            ViewBag.ValsysColores = ValsysColores;


            return PartialView("_DialogStocks", Stocks);
        }

        public IActionResult DialogAlmacenes()
        {
            var Almacenes = ctxDB.StocksALM.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x=>x.StkOrden);

            var UserConfigGrid = ctxDB.UsuariosGridsCfg.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Usuario == GrupoClaims.SessionUsuario && x.GridID == "gridAlmacenes").FirstOrDefault();
            if (UserConfigGrid != null)
            {
                ViewBag.ColumnsLayoutUser = UserConfigGrid.ColumnsLayout;

                if (UserConfigGrid.ColumnsPinned == null)
                {
                    ViewBag.ColumnsPinnedUser = 3;
                }
                else
                {
                    ViewBag.ColumnsPinnedUser = UserConfigGrid.ColumnsPinned;
                }
            }
            else
            {
                ViewBag.ColumnsPinnedUser = 3;
            }

            return PartialView("_DialogAlmacenes", Almacenes);
        }

        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> CreateEdit(Stocks RowStocks)
        {
            bool resultProcess = false;

            try
            {
                if (RowStocks.Guid == Guid.Empty)
                {
                    var FindStk = ctxDB.Stocks.Where(x => x.Empresa     == GrupoClaims.SessionEmpresa           && 
                                                        x.StkAlmacen    == RowStocks.StkAlmacen     &&
                                                        x.StkArticulo   == RowStocks.StkArticulo    &&
                                                        x.StkKilates    == RowStocks.StkKilates     &&
                                                        x.StkColor      == RowStocks.StkColor       &&
                                                        x.StkAcabado    == RowStocks.StkAcabado     &&
                                                        x.StkBano       == RowStocks.StkBano
                    ).FirstOrDefault();                    
                    if (FindStk == null)
                    {
                        // Creacion
                        RowStocks.Empresa       = GrupoClaims.SessionEmpresa;
                        RowStocks.IsoUser       = GrupoClaims.SessionUsuarioNombre;
                        RowStocks.IsoFecAlt     = DateTime.Now;
                        RowStocks.IsoFecMod     = DateTime.Now;

                        if (RowStocks.StkKilates == null)
                        {
                            RowStocks.StkKilates = "";
                        }
                        if (RowStocks.StkColor == null)
                        {
                            RowStocks.StkColor = "";
                        }
                        if (RowStocks.StkAcabado == null)
                        {
                            RowStocks.StkAcabado = "";
                        }
                        if (RowStocks.StkBano == null)
                        {
                            RowStocks.StkBano = "";
                        }

                        ctxDB.Stocks.Add(RowStocks);
                        await ctxDB.SaveChangesAsync();
                        resultProcess = true;
                    }
                    else
                    {
                        return StatusCode(200, "EXIST");
                    }

                }
                else
                {
                    if (RowStocks.StkKilates == null)
                    {
                        RowStocks.StkKilates = "";
                    }
                    if (RowStocks.StkColor == null)
                    {
                        RowStocks.StkColor = "";
                    }
                    if (RowStocks.StkAcabado == null)
                    {
                        RowStocks.StkAcabado = "";
                    }
                    if (RowStocks.StkBano == null)
                    {
                        RowStocks.StkBano = "";
                    }

                    // Edicion
                    RowStocks.IsoUser    = GrupoClaims.SessionUsuarioNombre;
                    RowStocks.IsoFecMod  = DateTime.Now;

                    ctxDB.Stocks.Update(RowStocks);
                    await ctxDB.SaveChangesAsync();
                    resultProcess = true;
                }

            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);  //  Send "Error"
            }


            var Result = ctxDB.Stocks.Where(x => x.Guid == RowStocks.Guid)
                .Select(c => new Stocks
                {
                    Guid = c.Guid,
                    Empresa = c.Empresa,
                    StkAlmacen = c.StkAlmacen,
                    StkArticulo = c.StkArticulo,

                    StkKilates = c.StkKilates,
                    StkColor = c.StkColor,
                    StkFisico = c.StkFisico,
                    StkMinimo = c.StkMinimo,
                    StkMaximo = c.StkMaximo,
                    StkReservado = c.StkReservado,
                    StkUbicacion = c.StkUbicacion,

                    ArtDescrip = ctxDB.Articulos.Where(x => x.Empresa == c.Empresa && x.Articulo == c.StkArticulo).FirstOrDefault().ArtDes,
                    StkAlmOrden = ctxDB.StocksALM.Where(x => x.Empresa == c.Empresa && x.StkAlmacen == c.StkAlmacen).FirstOrDefault().StkOrden,

                    IsoUser = c.IsoUser,
                    IsoFecAlt = c.IsoFecAlt,
                    IsoFecMod = c.IsoFecMod,

                })
                .OrderBy(x => x.StkAlmOrden).ThenBy(x => x.StkArticulo)
                .FirstOrDefault();

            if (resultProcess)
            {
                return StatusCode(200, Result);
            }
            else
            {
                return StatusCode(400, null);
            }

        }


        [HttpPost]
        public async Task<IActionResult> Delete_STOCKS(Guid Guid)
        {
            var FindStk = ctxDB.Stocks.Where(x => x.Guid == Guid).FirstOrDefault();
            if (FindStk != null)
            {
                try
                {
                    ctxDB.Stocks.Remove(FindStk);
                    await ctxDB.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return StatusCode(400, e.Message);
                }

                return StatusCode(200, "OK");
            }

            return StatusCode(200, null);
        }

        [HttpPost]
        public async Task<IActionResult> AlmacenesCreateEdit(string StkAlmacen, string StkNombre, string StkDescripcion, int StkOrden)
        {
            bool resultProcess = false;
            Guid SGuid;

            try
            {
                var FindAlm = ctxDB.StocksALM.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.StkAlmacen == StkAlmacen).FirstOrDefault();
                if (FindAlm == null)
                {
                    var NewAlm              = new StocksALM();
                    NewAlm.Empresa          = GrupoClaims.SessionEmpresa;
                    NewAlm.StkAlmacen       = StkAlmacen;
                    NewAlm.StkNombre        = StkNombre;
                    NewAlm.StkDescripcion   = StkDescripcion;

                    var Orden = 1;
                    if (StkOrden == 0)
                    {                        
                        var FindLast = ctxDB.StocksALM.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x=>x.StkOrden).LastOrDefault();
                        if (FindLast != null)
                        {
                            if (FindLast.StkOrden != 0)
                            {
                                Orden = (int)FindLast.StkOrden + 1;
                            }                            
                        }
                    }
                    else
                    {
                        Orden = StkOrden;

                    }
                    NewAlm.StkOrden         = Orden;

                    ctxDB.StocksALM.Add(NewAlm);
                    await ctxDB.SaveChangesAsync();
                    resultProcess = true;

                    SGuid = NewAlm.Guid;
                }
                else
                {
                    FindAlm.StkNombre       = StkNombre;
                    FindAlm.StkDescripcion  = StkDescripcion;

                    FindAlm.StkOrden = StkOrden;

                    ctxDB.StocksALM.Update(FindAlm);
                    await ctxDB.SaveChangesAsync();
                    resultProcess = true;

                    SGuid = FindAlm.Guid;
                }



            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);  //  Send "Error"
            }

            var Result = ctxDB.StocksALM.Where(x => x.Guid == SGuid).FirstOrDefault();


            if (resultProcess)
            {
                return StatusCode(200, Result);
            }
            else
            {
                return StatusCode(400, null);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Delete_ALMACEN(string StkAlmacen)
        {
            var FindStk = ctxDB.StocksALM.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.StkAlmacen == StkAlmacen).FirstOrDefault();
            if (FindStk != null)
            {
                try
                {
                    var Stocks = ctxDB.Stocks.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.StkAlmacen == StkAlmacen).ToList();
                    foreach (var itemSTK in Stocks)
                    {
                        ctxDB.Stocks.Remove(itemSTK);
                    }
                    ctxDB.SaveChanges();

                    ctxDB.StocksALM.Remove(FindStk);
                    await ctxDB.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return StatusCode(400, e.Message);
                }

                return StatusCode(200, "OK");
            }

            return StatusCode(200, null);
        }

        [HttpPost]
        public IActionResult GetAlmacenes()
        {
            var Almacenes = new List<string>();
            Almacenes.Add("Almacen");
            Almacenes.AddRange(ctxDB.StocksALM.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.StkOrden).ThenBy(x => x.StkAlmacen).Select(x => x.StkAlmacen).ToList());
            ViewBag.ListAlmacenes = Almacenes;

            return StatusCode(200, Almacenes);

        }

        [HttpGet]

        public IActionResult ObtenerRupturaDeStocks()
        {
            try
            {
                var stocksRuptura = ctxDB.Stocks
                    .Where(stock => stock.Empresa == GrupoClaims.SessionEmpresa &&
                                    stock.StkFisico < stock.StkMinimo &&
                                    stock.StkMinimo != 0)
                    .Select(stock => new
                    {
                        Articulo = stock.StkArticulo,
                        StockActual = stock.StkFisico,
                        StockMinimo = stock.StkMinimo
                        // Podes agregar más campos aquí si necesitás
                    })
                    .ToList();

                return Ok(stocksRuptura);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al obtener la ruptura de stocks: {Error}", ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet]
        private List<Stocks> ObtenerDatosRupturaDeStocks()
        {
            try
            {
                return ctxDB.Stocks
                    .Where(stock => stock.Empresa == GrupoClaims.SessionEmpresa &&
                                    stock.StkFisico < stock.StkMinimo &&
                                    stock.StkMinimo != 0)
                    .Select(stock => new Stocks
                    {
                        StkArticulo = stock.StkArticulo,
                        StkFisico = stock.StkFisico,
                        StkMinimo = stock.StkMinimo
                        // Puedo agregar más campos aquí si necesito
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                return new List<Stocks>();
            }
        }


        [HttpGet]
        public IActionResult RupturaDeStocks()
        {
            ViewBag.MenuUserList = _functionsBBDD.GetMenuAccesos();

            var stocksRuptura = ObtenerDatosRupturaDeStocks();


            var Almacenes = new List<string>();
            Almacenes.Add("Almacen");
            Almacenes.AddRange(ctxDB.StocksALM.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.StkOrden).ThenBy(x => x.StkAlmacen).Select(x => x.StkAlmacen).ToList());
            ViewBag.ListAlmacenes = Almacenes;

            ViewBag.ColumnsPinnedUser = ViewBag.ColumnsPinnedUser ?? 3; // Valor predeterminado si es nulo


            var FiltroQty = new List<string>();
            FiltroQty.Add("Filtro Cantidades");
            FiltroQty.Add("Físico +/-");
            FiltroQty.Add("Físico +");
            FiltroQty.Add("Físico -");
            FiltroQty.Add("Mínimo +/-");
            FiltroQty.Add("Máximo +/-");
            FiltroQty.Add("Reservado +/-");

            ViewBag.FiltroQty = FiltroQty;


            return View("RupturaDeStocks", stocksRuptura);
        }

    }

}