using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Newtonsoft.Json;
using System.Reflection;


namespace Nemesis365.Controllers
{
    [Authorize]
    public class ArticulosController : Controller
    {
        private readonly IConfiguration     _configuration;
        private readonly DbContextiLabPlus   ctxDB;
        private readonly FunctionsBBDD      FunctionsBBDD;
        private readonly GrupoClaims        GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;

        public ArticulosController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD, /*FunctionsNemesis _FunctionsNemesis,*/ IConfiguration configuration)
        {
            _configuration      = configuration;
            ctxDB               = Context;
            FunctionsBBDD       = _FunctionsBBDD;
            // FunctionsNemesis    = _FunctionsNemesis;
            GrupoClaims         = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridArticulos");

        }

        //[HttpGet("/Articulos")]
        //[HttpGet("/Articulos/{Guid}")]
        public IActionResult Index(Guid Guid)
        {
            ViewBag.MenuUserList        = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser   = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser   = GrupoColumnsLayout.ColumnsPinnedUser;

            if (Guid == Guid.Empty)
            {
                ViewBag.RepositionArt = null;
            }
            else
            {
                ViewBag.RepositionArt = Guid;
            }

            var ArtActivos = new List<string>();
            ArtActivos.Add("Todos");
            ArtActivos.Add("Activos");
            ArtActivos.Add("Inactivos");
            ViewBag.ArtActivos = ArtActivos;

            var ArtTipos = new List<string>();
            ArtTipos.Add("Tipo");
            ArtTipos.AddRange(ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "ArtTipos").OrderBy(x => x.Indice).ThenBy(x => x.Clave).Select(x => x.Clave).ToList());
            ViewBag.FartTipos = ArtTipos;

            var Empresas = new List<string>();
            Empresas.Add("Empresa");
            Empresas.AddRange(ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "Empresas").OrderBy(x => x.Indice).ThenBy(x => x.Clave).Select(x => x.Clave).ToList());
            ViewBag.Empresas = Empresas;

            var Familias = new List<string>();
            Familias.Add("Familia");
            Familias.AddRange(ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "Familias").OrderBy(x => x.Indice).ThenBy(x => x.Clave).Select(x => x.Clave).ToList());
            ViewBag.FartFamilias = Familias;



            var Articulos = new List<Articulos>();

            return View("Articulos", Articulos);
        }

        [HttpPost]
        // public ActionResult GetDataArticulos(string Activo)
       
        public async Task<ActionResult> GetDataArticulos(string Activo)
        {
            var ListaArticulos = new List<Articulos>();

            if (Activo == "Todos" || Activo == null)
            {
                ListaArticulos = await ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa /*&& x.ArtTipo == "FAB"*/)
                        .Select(c => new Articulos
                        {
                            Guid = c.Guid,
                            Empresa = c.Empresa,
                            Articulo = c.Articulo,
                            MultiEmpresa = c.MultiEmpresa,
                            ArtDes = c.ArtDes,
                            ArtTipo = c.ArtTipo,
                            ArtFamilia = c.ArtFamilia,
                            ArtTipoMat = c.ArtTipoMat,
                            ArtCalidades = c.ArtCalidades,
                            Activo = c.Activo,
                            ArtPrecioPVP = c.ArtPrecioPVP
                        })
                    .OrderBy(x => x.Articulo).ToListAsync();
            }
            else
            {
                if (Activo == "Activos")
                {
                    ListaArticulos = await ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Activo == true)
                            .Select(c => new Articulos
                            {
                                Guid = c.Guid,
                                Empresa = c.Empresa,
                                Articulo = c.Articulo,
                                MultiEmpresa = c.MultiEmpresa,
                                ArtDes = c.ArtDes,
                                ArtTipo = c.ArtTipo,
                                ArtFamilia = c.ArtFamilia,
                                ArtTipoMat = c.ArtTipoMat,
                                ArtCalidades = c.ArtCalidades,
                                Activo = c.Activo,
                                ArtPrecioPVP = c.ArtPrecioPVP
                            })
                        .OrderBy(x => x.Articulo).ToListAsync();
                }
                else
                {
                    ListaArticulos = await ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Activo == false)
                            .Select(c => new Articulos
                            {
                                Guid = c.Guid,
                                Empresa = c.Empresa,
                                Articulo = c.Articulo,
                                MultiEmpresa = c.MultiEmpresa,
                                ArtDes = c.ArtDes,
                                ArtTipo = c.ArtTipo,
                                ArtFamilia = c.ArtFamilia,
                                ArtTipoMat = c.ArtTipoMat,
                                ArtCalidades = c.ArtCalidades,
                                Activo = c.Activo,
                                ArtPrecioPVP = c.ArtPrecioPVP
                            })
                        .OrderBy(x => x.Articulo).ToListAsync();
                }
            }


            return Json(ListaArticulos);

            //var Jsonxx = Json(Articulos.ToList());

            //return StatusCode(200, Articulos.ToList());

            //var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd-MM-yyyy" };
            //var Resultado = JsonConvert.SerializeObject(Articulos, dateTimeConverter).ToString();

            //return Content(Resultado, "application/json");


        }

        /********************************************************************************************************************************/
        /*                                                  LLAMADAS A DIALOGOS                                                         */
        /********************************************************************************************************************************/
        //public class ArtComponentes
        //{
        //    public Guid     Guid                { get; set; }
        //    public string   Articulo            { get; set; }
        //    public string   ArtDes              { get; set; }
        //    public decimal? ArtPrecioCoste      { get; set; }
        //    public decimal? ArtPrecioPVP        { get; set; }
        //    public string   CalcArticuloDesc    { get; set; }
        //    public string   ArtTipoVenta        { get; set; }

        //    public decimal? Peso                { get; set; }
        //    public decimal? PesoSinMerma        { get; set; }
        //}

        //public IActionResult DialogArtMet(Guid Guid, string Articulo)
        //{
        //    var Row = new ArticulosMET();

        //    if (Guid == Guid.Empty)
        //    {
        //        Row.Empresa     = GrupoClaims.SessionEmpresa;
        //        Row.Articulo    = Articulo;
        //    }
        //    else
        //    {
        //        Row = ctxDB.ArticulosMET.Where(x => x.Guid == Guid).FirstOrDefault();
        //        if (Row == null)
        //        {
        //            Row = new ArticulosMET();
        //        }
        //    }

        //    var ValsysVacio = new ValSys();
        //    ValsysVacio.Indice = "";
        //    ValsysVacio.Clave = "";
        //    ValsysVacio.Valor1 = "";

        //    var ValsysMetales = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "Metales").OrderBy(x => x.Indice).ThenBy(x => x.Clave).ToList();
        //    ValsysMetales.Insert(0, ValsysVacio);
        //    ViewBag.ValsysMetales = ValsysMetales;

        //    //var ValKilates = new List<int>();
        //    //ValKilates.Add(24);
        //    //ValKilates.Add(19);
        //    //ValKilates.Add(18);
        //    //ValKilates.Add(14);
        //    //ValKilates.Add(10);
        //    //ValKilates.Add(9);
        //    //ValKilates.Add(8);
        //    //ViewBag.ValKilates = ValKilates;


        //    var ValKilates = new List<string>();
        //    ValKilates.Add("");
        //    ValKilates.Add("24");
        //    ValKilates.Add("19");
        //    ValKilates.Add("18");
        //    ValKilates.Add("14");
        //    ValKilates.Add("10");
        //    ValKilates.Add("9");
        //    ValKilates.Add("8");
        //    ViewBag.ValKilates = ValKilates;




        //    return PartialView("_DialogArtMET", Row);
        //}

        //public IActionResult DialogArtPIE(Guid Guid, string Articulo)
        //{
        //    var Row = new ArticulosCOMP();

        //    if (Guid == Guid.Empty)
        //    {
        //        Row.Empresa = GrupoClaims.SessionEmpresa;
        //        Row.Articulo = Articulo;
        //    }
        //    else
        //    {
        //        Row = ctxDB.ArticulosCOMP.Where(x => x.Guid == Guid).FirstOrDefault();
        //        if (Row == null)
        //        {
        //            Row = new ArticulosCOMP();
        //        }
        //        else
        //        {
        //            Row.Precio = (decimal)ctxDB.Articulos.Where(x => x.Empresa == Row.Empresa && x.Articulo == Row.Componente).FirstOrDefault().ArtPrecioPVP;
        //            Row.Valor = Row.Cantidad * Row.Precio;
        //        }

        //        // Peso componentes
        //        var PesoComp = 0m;
        //        var PesoCompSinMerma = 0m;
        //        var CompMet = ctxDB.ArticulosMET.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Row.Componente);
        //        foreach (var itemPC in CompMet)
        //        {
        //            PesoComp = PesoComp + (itemPC.Peso + (itemPC.Peso * itemPC.Merma / 100));
        //            PesoCompSinMerma = PesoCompSinMerma + (itemPC.Peso);
        //        }
        //        if (PesoComp != 0)
        //        {
        //            Row.Peso = PesoComp * Row.Cantidad;
        //            Row.PesoSinMerma = PesoCompSinMerma * Row.Cantidad;
        //        }

        //    }


        //    var ListComponentes = ctxDB.Articulos.Where(x => x.Empresa == Row.Empresa && x.ArtTipo == "COM")
        //        .Select(c => new ArtComponentes
        //        {
        //            Guid = c.Guid,
        //            Articulo = c.Articulo,
        //            ArtDes = c.ArtDes,
        //            ArtPrecioCoste = c.ArtPrecioCoste,
        //            ArtPrecioPVP = c.ArtPrecioPVP
        //        })
        //        .ToList();
        //    ViewBag.ListComponentes = JsonConvert.SerializeObject(ListComponentes);


        //    var ValsysTipoPrecio = new List<ValSys>();
        //    ValsysTipoPrecio.Add(new ValSys
        //    {
        //        Clave = "V",
        //        Valor1 = "Venta"
        //    });
        //    ValsysTipoPrecio.Add(new ValSys
        //    {
        //        Clave = "C",
        //        Valor1 = "Coste"
        //    });
        //    ViewBag.ValsysTipoPrecio = ValsysTipoPrecio;


        //    return PartialView("_DialogArtPIE", Row);
        //}

        //public IActionResult DialogArtCOMP(Guid Guid, string Articulo)
        //{
        //    var Row = new ArticulosCOMP();

        //    if (Guid == Guid.Empty)
        //    {
        //        Row.Empresa     = GrupoClaims.SessionEmpresa;
        //        Row.Articulo    = Articulo;
        //    }
        //    else
        //    {
        //        Row = ctxDB.ArticulosCOMP.Where(x => x.Guid == Guid).FirstOrDefault();
        //        if (Row == null)
        //        {
        //            Row = new ArticulosCOMP();
        //        }
        //        else
        //        {
        //            Row.Precio  = (decimal)ctxDB.Articulos.Where(x => x.Empresa == Row.Empresa && x.Articulo == Row.Componente).FirstOrDefault().ArtPrecioPVP;
        //            Row.Valor   = Row.Cantidad * Row.Precio;
        //        }

        //        // Peso componentes
        //        var PesoComp = 0m;
        //        var PesoCompSinMerma = 0m;
        //        var CompMet = ctxDB.ArticulosMET.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Row.Componente);
        //        foreach (var itemPC in CompMet)
        //        {
        //            PesoComp = PesoComp + (itemPC.Peso + (itemPC.Peso * itemPC.Merma / 100));
        //            PesoCompSinMerma = PesoCompSinMerma + (itemPC.Peso);
        //        }
        //        if (PesoComp != 0)
        //        {
        //            Row.Peso = PesoComp * Row.Cantidad;
        //            Row.PesoSinMerma = PesoCompSinMerma * Row.Cantidad;
        //        }

        //    }


        //    var ListComponentes = ctxDB.Articulos.Where(x => x.Empresa == Row.Empresa && x.ArtTipo == "COM")
        //        .Select(c => new ArtComponentes
        //        {
        //            Guid            = c.Guid,
        //            Articulo        = c.Articulo,
        //            ArtDes          = c.ArtDes,
        //            ArtPrecioCoste  = c.ArtPrecioCoste,
        //            ArtPrecioPVP    = c.ArtPrecioPVP
        //        })
        //        .ToList();
        //    ViewBag.ListComponentes = JsonConvert.SerializeObject(ListComponentes);


        //    var ValsysTipoPrecio = new List<ValSys>();
        //    ValsysTipoPrecio.Add(new ValSys
        //    {
        //        Clave = "V",
        //        Valor1 = "Venta"
        //    });
        //    ValsysTipoPrecio.Add(new ValSys
        //    {
        //        Clave = "C",
        //        Valor1 = "Coste"
        //    });
        //    ViewBag.ValsysTipoPrecio = ValsysTipoPrecio;


        //    return PartialView("_DialogArtCOMP", Row);
        //}

        //public IActionResult DialogArtMO(Guid Guid, string Articulo)
        //{

        //    var Row = new ArticulosMO();

        //    if (Guid == Guid.Empty)
        //    {
        //        Row.Empresa = GrupoClaims.SessionEmpresa;
        //        Row.Articulo = Articulo;
        //    }
        //    else
        //    {
        //        Row = ctxDB.ArticulosMO.Where(x => x.Guid == Guid).FirstOrDefault();
        //        if (Row == null)
        //        {
        //            Row = new ArticulosMO();
        //        }
        //    }

        //    var ValsysVacio = new ValSys();
        //    ValsysVacio.Indice = "";
        //    ValsysVacio.Clave = "";
        //    ValsysVacio.Valor1 = "";

        //    var ValsysMO = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "Mano de obra").OrderBy(x => x.Indice).ThenBy(x => x.Clave).ToList();
        //    ValsysMO.Insert(0, ValsysVacio);
        //    ViewBag.ValsysMO = ValsysMO;


        //    var ValMOExtInt = new List<ValSys>();
        //    ValMOExtInt.Add(new ValSys
        //    {
        //        Clave = "",
        //        Valor1 = ""
        //    });
        //    ValMOExtInt.Add(new ValSys
        //    {
        //        Clave   = "E",
        //        Valor1  = "Externa"
        //    });
        //    ValMOExtInt.Add(new ValSys
        //    {
        //        Clave   = "I",
        //        Valor1  = "Interna"
        //    });
        //    ViewBag.ValMOExtInt = ValMOExtInt;


        //    var ValTipoCalculot = new List<ValSys>();
        //    ValTipoCalculot.Add(new ValSys
        //    {
        //        Clave = "",
        //        Valor1 = ""
        //    });
        //    ValTipoCalculot.Add(new ValSys
        //    {
        //        Clave = "C",
        //        Valor1 = "Cantidad"
        //    });
        //    ValTipoCalculot.Add(new ValSys
        //    {
        //        Clave = "G",
        //        Valor1 = "Gramos (Peso Art.)"
        //    });
        //    ValTipoCalculot.Add(new ValSys
        //    {
        //        Clave = "P",
        //        Valor1 = "Peso"
        //    });
        //    ValTipoCalculot.Add(new ValSys
        //    {
        //        Clave = "U",
        //        Valor1 = "Unidad"
        //    });
        //    ValTipoCalculot.Add(new ValSys
        //    {
        //        Clave = "T",
        //        Valor1 = "Tiempo"
        //    });
        //    ViewBag.ValTipoCalculot = ValTipoCalculot;

        //    var PesoTotalSinMerma   = 0m;
        //    var PesComp             = 0m;
        //    var SumPes          = ctxDB.ArticulosMET.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Articulo).Sum(x => x.Peso);
        //    var ListComp        = ctxDB.ArticulosCOMP.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Articulo).ToList();
        //    foreach (var itemCOMP in ListComp)
        //    {
        //        var PesoCompSinMerma = 0m;
        //        var CompMet = ctxDB.ArticulosMET.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == itemCOMP.Componente).ToList();
        //        foreach (var itemPC in CompMet)
        //        {
        //            PesoCompSinMerma = PesoCompSinMerma + (itemPC.Peso * itemCOMP.Cantidad);
        //        }
        //        if (PesoCompSinMerma != 0)
        //        {
        //            PesComp = PesComp + PesoCompSinMerma;
        //        }             
        //    }

        //    if (Row.MOUniMed == "G")
        //    {
        //        PesoTotalSinMerma   = PesoTotalSinMerma + SumPes + PesComp;
        //        Row.MOCantidad      = PesoTotalSinMerma;
        //    }

        //    ViewBag.ArtPeso = PesoTotalSinMerma;


        //    return PartialView("_DialogArtMO", Row);
        //}

        //public IActionResult DialogSimuladorPrecios(Guid GuidArt, string SelectArticulo, decimal SelectOroFino, string SelectKilates, string SelectColorOro, string SelectTarifa)
        //{
        //    var Articulo = new Articulos();

        //    if (GuidArt != Guid.Empty)
        //    {
        //        Articulo = ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Guid == GuidArt).FirstOrDefault();
        //    }
        //    else
        //    {
        //        if (SelectArticulo != "" && SelectArticulo != null)
        //        {
        //            Articulo = ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == SelectArticulo).FirstOrDefault();
        //        }
        //    }

        //    if (SelectOroFino != 0)
        //    {
        //        ViewBag.SelectOroFino = SelectOroFino;
        //    }
        //    if (SelectKilates != "undefined")
        //    {
        //        ViewBag.SelectKilates = SelectKilates;
        //    }
        //    if (SelectColorOro != "undefined")
        //    {
        //        ViewBag.SelectColorOro = SelectColorOro;
        //    }
        //    if (SelectTarifa != "undefined")
        //    {
        //        ViewBag.SelectTarifa = SelectTarifa;
        //    }


        //    var ListArticulos = new List<ArtComponentes>();

        //    var OroKilatesPieza = 18;
        //    if (Articulo != null)
        //    {
        //        var ListArticuloMET = ctxDB.ArticulosMET.Where(x => x.Empresa == Articulo.Empresa && x.Articulo == Articulo.Articulo).FirstOrDefault();
        //        if (ListArticuloMET != null)
        //        {
        //            OroKilatesPieza = ListArticuloMET.Kilates;
        //        }

        //        //var ListArticulos = ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.ArtTipo == "FAB" && x.Guid == GuidArt)
        //        ListArticulos = ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.ArtTipo == "FAB" && x.Articulo == Articulo.Articulo)
        //            .Select(c => new ArtComponentes
        //            {
        //                Guid = c.Guid,
        //                Articulo = c.Articulo,
        //                ArtDes = c.ArtDes,
        //                ArtTipoVenta = c.ArtTipoVenta,
        //                CalcArticuloDesc = c.Articulo + "  :  " + c.ArtDes
        //            })
        //            .ToList();

        //    }
        //    else
        //    {
        //        Articulo = new Articulos();
        //    }

        //    ViewBag.OroKilatesPieza = OroKilatesPieza;
        //    ViewBag.ListArticulos = JsonConvert.SerializeObject(ListArticulos);


        //    var ListClientes = ctxDB.Clientes.Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
        //        .Select(c => new Clientes
        //        {
        //            Guid                = c.Guid,
        //            Cliente             = c.Cliente,
        //            CliNombre           = c.CliNombre,
        //            CliTarifaVenta      = c.CliTarifaVenta,
        //            CalcClienteNombre   = c.Cliente + "  :  " + c.CliNombre
        //        })
        //        .ToList();
        //    ViewBag.ListClientes = JsonConvert.SerializeObject(ListClientes);


        //    var TarifaVacia = new TarifasVenta();
        //    TarifaVacia.Tarifa = "";
        //    TarifaVacia.TarDescripcion = "";

        //    var Tarifas = ctxDB.TarifasVenta.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Tarifa).ToList();
        //    Tarifas.Insert(0, TarifaVacia);
        //    ViewBag.ListTarifas = Tarifas;

        //    var _DocumentsDirectory = _configuration.GetSection("URLStrings").GetSection("DocumentsDirectory").Value;
        //    var PathArtImg = _DocumentsDirectory + "/" + GrupoClaims.SessionEmpresa + "/" + "Articulos" + "/";
        //    ViewBag.RutaArtImg = PathArtImg;
        //    var PathArtImg3D = _DocumentsDirectory + "/" + GrupoClaims.SessionEmpresa + "/" + "Articulos3D" + "/";
        //    ViewBag.RutaArtImg3D = PathArtImg3D;


        //    var ListFixing = ctxDB.Fixing.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Tipo == "Oro").OrderByDescending(x => x.Fecha).ToList();
        //    foreach (var item in ListFixing)
        //    {
        //        item.CalcMetalCombo = item.Fecha.ToString("dd-MM-yyyy") + "        " + item.Metal + "     Fino: " + item.OroFinoVenta;
        //    }
        //    ViewBag.ListFixing = ListFixing;

        //    var TipoVenta = new List<string>();
        //    //TipoVenta.Add("");
        //    TipoVenta.Add("Etiqueta");
        //    TipoVenta.Add("Hechura");
        //    TipoVenta.Add("Peso");
        //    TipoVenta.Add("PesoHechura");
        //    ViewBag.ListTipoVenta = TipoVenta;

        //    var ValKilates = new List<string>();
        //    ValKilates.Add("");
        //    ValKilates.Add("24");
        //    ValKilates.Add("19");
        //    ValKilates.Add("18");
        //    ValKilates.Add("14");
        //    ValKilates.Add("10");
        //    ValKilates.Add("9");
        //    ValKilates.Add("8");
        //    ViewBag.ValKilates = ValKilates;

        //    var ValTipoOro = new List<string>();
        //    //ValTipoOro.Add("");
        //    ValTipoOro.Add("AM");
        //    ValTipoOro.Add("BL");
        //    ValTipoOro.Add("RS");
        //    ValTipoOro.Add("BC");
        //    ValTipoOro.Add("AM-BL");
        //    ValTipoOro.Add("BL-AM");
        //    ValTipoOro.Add("RS-BL");
        //    ValTipoOro.Add("BL-RS");
        //    ValTipoOro.Add("AM-RS");
        //    ViewBag.ValTipoOro = ValTipoOro;


        //    var PrecioEstructura = FunctionsNemesis.GetPreciosEstructura();

        //    ViewBag.PrecioEstructura = PrecioEstructura;

        //    return PartialView("_DialogSimuladorPrecios", Articulo);
        //}

        //public IActionResult DialogImplosionArt(Guid GuidArt)
        //{
        //    var Articulo = new Articulos();

        //    if (GuidArt != Guid.Empty)
        //    {
        //        Articulo = ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Guid == GuidArt).FirstOrDefault();

        //        var Componente  = Articulo.Articulo;
        //        var CompoDesc   = Articulo.ArtDes;

        //        ViewBag.Componente  = Componente;
        //        ViewBag.CompoDesc   = CompoDesc;

        //        var ListImplosiones = new List<ArticulosCOMP>();

        //        var ListCompoArt = ctxDB.ArticulosCOMP.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Componente == Componente && x.Tipo == "C")
        //            .Select(c => new ArticulosCOMP
        //            {
        //                Guid        = c.Guid,
        //                Empresa     = c.Empresa,
        //                Articulo    = c.Articulo,
        //                Componente  = c.Componente,
        //                Secuencia   = c.Secuencia,

        //                Cantidad        = c.Cantidad,
        //                Descripcion     = ctxDB.Articulos.Where(x => x.Empresa == Articulo.Empresa && x.Articulo == c.Componente).FirstOrDefault().ArtDes,
        //                DescripcionArt  = ctxDB.Articulos.Where(x => x.Empresa == Articulo.Empresa && x.Articulo == c.Articulo).FirstOrDefault().ArtDes,
        //                TipoPrecio      = c.TipoPrecio,
        //            }).OrderBy(x => x.Articulo).ToList();

        //        var ListCompoArtPIE = ctxDB.ArticulosCOMP.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Componente == Componente && x.Tipo == "P")
        //            .Select(c => new ArticulosCOMP
        //            {
        //                Guid = c.Guid,
        //                Empresa = c.Empresa,
        //                Articulo = c.Articulo,
        //                Componente = c.Componente,
        //                Secuencia = c.Secuencia,

        //                Cantidad = c.Cantidad,
        //                Descripcion = ctxDB.Articulos.Where(x => x.Empresa == Articulo.Empresa && x.Articulo == c.Componente).FirstOrDefault().ArtDes,
        //                DescripcionArt = ctxDB.Articulos.Where(x => x.Empresa == Articulo.Empresa && x.Articulo == c.Articulo).FirstOrDefault().ArtDes,
        //                TipoPrecio = c.TipoPrecio,
        //            }).OrderBy(x => x.Articulo).ToList();

        //        ListImplosiones.AddRange(ListCompoArt);
        //        ListImplosiones.AddRange(ListCompoArtPIE);

        //        return PartialView("_DialogImplosionArt", ListImplosiones.OrderBy(x => x.Articulo).ToList());
        //    }
        //    else
        //    {
        //        return PartialView("_DialogImplosionArt", null);
        //    }




        //}

        ///********************************************************************************************************************************/
        ///*                                                  PROCESOS SIMULADOR DE PRECIOS                                               */
        ///********************************************************************************************************************************/
        //[HttpPost]
        //public IActionResult GetPartialSimDetailCOMP(string Articulo, string Tipo /*, string EstCOMP_Json*/)
        //{
        //    //var EstCOMP = System.Text.Json.JsonSerializer.Deserialize<List<ArticulosCOMP>>(EstCOMP_Json);
        //    //ViewBag.EstCOMP_Mod = JsonConvert.SerializeObject(EstCOMP);

        //    ViewBag.Tipo = Tipo;

        //    return PartialView("_DialogSimDetailCOMP", null);
        //}

        //[HttpPost]
        //public IActionResult GetPartialSimDetailMO(string Articulo)
        //{

        //    return PartialView("_DialogSimDetailMO", null);
        //}


        ///********************************************************************************************************************************/
        ///*                                                  FUNCIONES DE PRECIOS                                                        */
        ///********************************************************************************************************************************/
        //public class PrecioEstructura
        //{
        //    public string Id                    { get; set; }
        //    public string Header                { get; set; }            
        //    public List<PrecioEstructura> Items { get; set; }
        //}

        //[HttpPost]
        //public IActionResult GetPrecioEstDetalle(string Articulo, string TipoEstDetalle)
        //{
        //    var PrecioEstDetail = new List<PrecioEstructura>();

        //    if (TipoEstDetalle == "Oro")
        //    {
        //        PrecioEstDetail = new List<PrecioEstructura>{
        //            new PrecioEstructura{Id="Peso",             Header = "Peso <span style='position: absolute;right: 15px;'>0.00€</span>"},
        //            new PrecioEstructura{Id="Merma",            Header = "Merma <span style='position: absolute;right: 15px;'>0.00€</span>"},
        //            new PrecioEstructura{Id="Peso Total",       Header = "Peso Total <span style='position: absolute;right: 15px;'>0.00€</span>"},
        //            new PrecioEstructura{Id="Separador",        Header = "______________________________________________"},
        //            new PrecioEstructura{Id="Kilates",          Header = "Kilates <span style='position: absolute;right: 15px;'>0.00€</span>"},
        //            new PrecioEstructura{Id="Oro Ley",          Header = "Oro Ley <span style='position: absolute;right: 15px;'>0.00€</span>"},
        //            new PrecioEstructura{Id="Precio Total",     Header = "Precio Total <span style='position: absolute;right: 15px;'>0.00€</span>"}
        //        };


        //    }

        //    return Json(PrecioEstDetail);
        //}

        //[HttpPost]
        //public IActionResult GetSimulacionPrecios(string Articulo, int SelectKilates, string SelectTipoVenta, string SelectTarifa, decimal SelectOroFino, string SelectTipoOro,
        //                                            string EstructuraCOMP, string EstructuraPIE
        //                                          )
        //{

        //    if (EstructuraCOMP != null)
        //    {
        //        var EstCOMP = System.Text.Json.JsonSerializer.Deserialize<List<ArticulosCOMP>>(EstructuraCOMP);
        //        var EstPIE = System.Text.Json.JsonSerializer.Deserialize<List<ArticulosCOMP>>(EstructuraPIE);

        //        var ArticuloPrecioEstructura = FunctionsNemesis.GetArticuloEstructuraPrecios(
        //                                                                                    Articulo, SelectKilates, SelectTipoVenta, SelectTarifa, SelectOroFino, SelectTipoOro, EstCOMP, EstPIE
        //                                                                                    );

        //        return Json(ArticuloPrecioEstructura);
        //    }
        //    else
        //    {
        //        var ArticuloPrecioEstructura = FunctionsNemesis.GetArticuloEstructuraPrecios(
        //                                                                                    Articulo, SelectKilates, SelectTipoVenta, SelectTarifa, SelectOroFino, SelectTipoOro, null, null
        //                                                                                    );

        //        return Json(ArticuloPrecioEstructura);
        //    }


        //}

        //public IActionResult GetArticuloImagenes(string Articulo)
        //{


        //    var ListArticuloIMG = ctxDB.ArticulosIMG.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Articulo).OrderBy(x => x.Tipo).ThenBy(x => x.Secuencia).ToList();

        //    return Json(ListArticuloIMG);
        //}




        ///********************************************************************************************************************************/
        ///*                                                  FUNCIONES FICHA ARTICULO : CREATE, EDIT                                     */
        ///********************************************************************************************************************************/
        //[HttpGet("/Articulos/Ficha/{Guid}")]
        //public IActionResult Ficha(Guid Guid)
        //{
        //    ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();

        //    /********************************************************/
        //    /* VALORES AUXILIARES                                   */
        //    /********************************************************/
        //    var ValsysVacio = new ValSys();
        //    ValsysVacio.Indice = "";
        //    ValsysVacio.Clave = "";
        //    ValsysVacio.Valor1 = "";

        //    var ValArtTipoMat = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "TipoMaterial").OrderBy(x => x.Indice).ThenBy(x => x.Clave).ToList();
        //    ValArtTipoMat.Insert(0, ValsysVacio);
        //    ViewBag.ValsysArtTipoMat = ValArtTipoMat;

        //    var ValsysEmpresas = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "Empresas").OrderBy(x => x.Indice).ThenBy(x => x.Clave).ToList();
        //    ValsysEmpresas.Insert(0, ValsysVacio);
        //    ViewBag.ValsysEmpresas = ValsysEmpresas;

        //    var ValFamilias = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "Familias").OrderBy(x => x.Indice).ThenBy(x => x.Clave).ToList();
        //    ViewBag.ValsysFamilias = ValFamilias;

        //    var ArtTipos = new List<string>();
        //    ArtTipos.AddRange(ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "ArtTipos").OrderBy(x => x.Indice).ThenBy(x => x.Clave).Select(x => x.Clave).ToList());
        //    ViewBag.ValsysArtTipos = ArtTipos;

        //    var _DocumentsDirectory = _configuration.GetSection("URLStrings").GetSection("DocumentsDirectory").Value;
        //    var PathArtImg          = _DocumentsDirectory + "/" + GrupoClaims.SessionEmpresa + "/" + "Articulos" + "/";
        //    ViewBag.RutaArtImg      = PathArtImg;
        //    var PathArtImg3D        = _DocumentsDirectory + "/" + GrupoClaims.SessionEmpresa + "/" + "Articulos3D" + "/";
        //    ViewBag.RutaArtImg3D    = PathArtImg3D;

        //    var ArtTipoVenComp = new List<string>();
        //    ArtTipoVenComp.Add("");
        //    ArtTipoVenComp.Add("Etiqueta");
        //    ArtTipoVenComp.Add("Hechura");
        //    ArtTipoVenComp.Add("Peso");
        //    ArtTipoVenComp.Add("PesoHechura");
        //    ViewBag.ArtTipoVenComp = ArtTipoVenComp;

        //    var ListProveedores = ctxDB.Proveedores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
        //        .Select(c => new Proveedores
        //        {
        //            Proveedor   = c.Proveedor,
        //            ProNombre   = c.ProNombre,
        //            ProTipo     = c.ProTipo
        //        })
        //        .ToList();
        //    ViewBag.ListProveedores = JsonConvert.SerializeObject(ListProveedores);

        //    var ValsysBriTalla = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "BriTalla").OrderBy(x => x.Indice).ThenBy(x => x.Clave).ToList();
        //    ValsysBriTalla.Insert(0, ValsysVacio);
        //    ViewBag.ValsysBriTalla = ValsysBriTalla;

        //    var ValsysBriColor = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "BriColor").OrderBy(x => x.Indice).ThenBy(x => x.Clave).ToList();
        //    ValsysBriColor.Insert(0, ValsysVacio);
        //    ViewBag.ValsysBriColor = ValsysBriColor;

        //    var ValsysBriPureza = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "BriPureza").OrderBy(x => x.Indice).ThenBy(x => x.Clave).ToList();
        //    ValsysBriPureza.Insert(0, ValsysVacio);
        //    ViewBag.ValsysBriPureza = ValsysBriPureza;


        //    var Monedas = new List<string>();
        //    Monedas.Add("");
        //    Monedas.Add("Euro");
        //    Monedas.Add("Libra");
        //    Monedas.Add("Dolar");
        //    ViewBag.Monedas = Monedas;

        //    var ValTipoOro = new List<string>();
        //    ValTipoOro.Add("");
        //    ValTipoOro.Add("AM");
        //    ValTipoOro.Add("BL");
        //    ValTipoOro.Add("RS");
        //    ValTipoOro.Add("BC");
        //    ValTipoOro.Add("AM-BL");
        //    ValTipoOro.Add("BL-AM");
        //    ValTipoOro.Add("RS-BL");
        //    ValTipoOro.Add("BL-RS");
        //    ValTipoOro.Add("AM-RS");
        //    ViewBag.ValTipoOro = ValTipoOro;

        //    var ValMedidas = new List<string>();
        //    ValMedidas.Add("Todas");
        //    ValMedidas.Add("06");
        //    ValMedidas.Add("07");
        //    ValMedidas.Add("08");
        //    ValMedidas.Add("09");
        //    ValMedidas.Add("10");
        //    ValMedidas.Add("11");
        //    ValMedidas.Add("12");
        //    ValMedidas.Add("13");
        //    ValMedidas.Add("14");
        //    ValMedidas.Add("15");
        //    ValMedidas.Add("16");
        //    ValMedidas.Add("17");
        //    ValMedidas.Add("18");
        //    ViewBag.ValMedidas = ValMedidas;

        //    var RodioVacio      = new Fixing();
        //    RodioVacio.Metal    = "";
        //    RodioVacio.Precio   = 0;

        //    var ValTiposRod         = ctxDB.Fixing.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Tipo == "RODIO").ToList();
        //    ValTiposRod.Insert(0, RodioVacio);
        //    ViewBag.ValTiposRod     = ValTiposRod;

        //    var ValTiposLiga        = ctxDB.Fixing.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Tipo == "LIGA").ToList();
        //    ValTiposLiga.Insert(0, RodioVacio);
        //    ViewBag.ValTiposLiga    = ValTiposLiga;

        //    var ListFixing = ctxDB.Fixing.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Tipo == "Oro").OrderByDescending(x => x.Fecha).ToList();
        //    foreach (var item in ListFixing)
        //    {
        //        item.CalcMetalCombo = item.Fecha.ToString("dd-MM-yyyy") + "        " + item.Metal + "     Fino: " + item.OroFinoVenta;
        //    }
        //    ViewBag.ListFixing = ListFixing;


        //    /********************************************************/
        //    /********************************************************/

        //    var ArticuloEstructuraPrecio = new ArticuloEstructuraPrecio();

        //    var Articulo = ctxDB.Articulos.Where(x => x.Guid == Guid).OrderBy(x => x.Articulo).FirstOrDefault();

        //    if (Articulo == null && Guid != Guid.Empty)
        //    {
        //        // Comprobar que el Guid no sea que venga de la tabla de componentes
        //        var ArticuloComp = ctxDB.ArticulosCOMP.Where(x => x.Guid == Guid).OrderBy(x => x.Articulo).FirstOrDefault();
        //        if (ArticuloComp != null)
        //        {
        //            Articulo = ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == ArticuloComp.Componente).FirstOrDefault();
        //        }
        //        else
        //        {
        //            var ArticuloCompPIE = ctxDB.ArticulosCOMP.Where(x => x.Guid == Guid).OrderBy(x => x.Articulo).FirstOrDefault();
        //            if (ArticuloCompPIE != null)
        //            {
        //                Articulo = ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == ArticuloCompPIE.Componente).FirstOrDefault();
        //            }
        //        }
        //    }


        //    if (Articulo != null)
        //    {
        //        var ValLigaAMStd = ctxDB.Fixing.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Tipo == "LIGA" && x.Metal.Contains(Articulo.ArtTipoOro)).OrderBy(x => x.Fecha).LastOrDefault();
        //        if (ValLigaAMStd != null)
        //        {
        //            ViewBag.LigaFixing = ValLigaAMStd.Precio;
        //        }
        //        else
        //        {
        //            ViewBag.LigaFixing = 0;
        //        }

        //        var ListArticulosAlt = new List<Articulos>();
        //        ViewBag.ListArticulosAlt = JsonConvert.SerializeObject(ListArticulosAlt);


        //        var OroKilatesPieza = 18;
        //        var FindArticuloMET = ctxDB.ArticulosMET.Where(x => x.Empresa == Articulo.Empresa && x.Articulo == Articulo.Articulo).FirstOrDefault();
        //        if (FindArticuloMET != null)
        //        {
        //            OroKilatesPieza = FindArticuloMET.Kilates;
        //        }
        //        ViewBag.OroKilatesPieza = OroKilatesPieza;

        //        // BUSQUEDA DE PRECIO DEL RODIO - DEPENDIENTE DEL TIPO DE ORO DE LA PIEZA
        //        ViewBag.RodioFixing = 0;
        //        var FindRodio = ctxDB.Fixing.Where(x => x.Empresa == Articulo.Empresa && x.Metal == Articulo.ArtTipoRodioAM).FirstOrDefault();
        //        if (FindRodio != null)
        //        {
        //            ViewBag.RodioFixing = FindRodio.Precio;
        //        }

        //        var OroFinoToday = 0m;
        //        var FindFixing = ctxDB.Fixing.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Metal == "ORO").OrderBy(x => x.Fecha).LastOrDefault();
        //        if (FindFixing != null)
        //        {
        //            switch (OroKilatesPieza)
        //            {
        //                case 24:
        //                    OroFinoToday = FindFixing.K24Ley;
        //                    break;
        //                case 19:
        //                    OroFinoToday = FindFixing.K19Ley;
        //                    break;
        //                case 18:
        //                    OroFinoToday = FindFixing.K18Ley;
        //                    break;
        //                case 14:
        //                    OroFinoToday = FindFixing.K14Ley;
        //                    break;
        //                case 10:
        //                    OroFinoToday = FindFixing.K10Ley;
        //                    break;
        //                case 9:
        //                    OroFinoToday = FindFixing.K9Ley;
        //                    break;
        //                case 8:
        //                    OroFinoToday = FindFixing.K8Ley;
        //                    break;
        //            }

        //        }

        //        ArticuloEstructuraPrecio = FunctionsNemesis.GetArticuloEstructuraPrecios(Articulo.Articulo, OroKilatesPieza, Articulo.ArtTipoVenta, null, OroFinoToday, Articulo.ArtTipoOro, null, null);

        //    }




        //    return View("ArtFicha", ArticuloEstructuraPrecio);
        //}



        //[HttpPost]
        //public async Task<IActionResult> Get_File_Base64(string Fichero)
        //{
        //    string StrImgBase64 = "";

        //    var ParentPathFic = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();
        //    var PathDocImg = Path.Combine(ParentPathFic, "Nemesis365Docs", GrupoClaims.SessionEmpresa, "Articulos3D", Fichero);

        //    if (System.IO.File.Exists(PathDocImg))
        //    {
        //        var memory = new MemoryStream();
        //        using (var stream = new FileStream(PathDocImg, FileMode.Open))
        //        {
        //            await stream.CopyToAsync(memory);
        //        }
        //        memory.Position = 0;
        //        StrImgBase64            = Convert.ToBase64String(memory.ToArray());
        //        var StrImgBase64length  = StrImgBase64.Length;

        //    }

        //    return Json(StrImgBase64);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Img_Delete(string Articulo, int Secuencia)
        //{
        //    var FindImg = ctxDB.ArticulosIMG.Where(x=> x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Articulo && x.Secuencia == Secuencia).FirstOrDefault();
        //    if (FindImg != null)
        //    {
        //        try
        //        {
        //            ctxDB.ArticulosIMG.Remove(FindImg);
        //            await ctxDB.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            var error = e.InnerException;
        //        }
        //    }

        //    var ListArticuloIMG = ctxDB.ArticulosIMG.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Articulo).OrderBy(x => x.Tipo).ThenBy(x => x.Secuencia).ToList();

        //    return Json(ListArticuloIMG);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Img_Add(string Articulo, string Tipo, IFormFile file)
        //{
        //    var Secuencia       = 1;

        //    if (Tipo == null)
        //    {
        //        Tipo = "";
        //    }

        //    if (file != null)
        //    {
        //        if (file.Length > 0)
        //        {
        //            // SE COMPRUEBAN DIRECTORIO DE SUBIDA DE DOCUMENTOS - USUARIOS PERFILES
        //            var ParentPathFic = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();

        //            var PathDoc = Path.Combine(ParentPathFic, "Nemesis365Docs");
        //            if (!Directory.Exists(PathDoc))
        //            {
        //                Directory.CreateDirectory(PathDoc);
        //            }
        //            var PathDocEmp = Path.Combine(ParentPathFic, "Nemesis365Docs", GrupoClaims.SessionEmpresa);
        //            if (!Directory.Exists(PathDocEmp))
        //            {
        //                Directory.CreateDirectory(PathDocEmp);
        //            }
        //            var PathDirFic = Path.Combine(ParentPathFic, "Nemesis365Docs", GrupoClaims.SessionEmpresa, "Articulos");
        //            if (!Directory.Exists(PathDirFic))
        //            {
        //                Directory.CreateDirectory(PathDirFic);
        //            }
        //            var PathDirFi3D = Path.Combine(ParentPathFic, "Nemesis365Docs", GrupoClaims.SessionEmpresa, "Articulos3D");
        //            if (!Directory.Exists(PathDirFic))
        //            {
        //                Directory.CreateDirectory(PathDirFic);
        //            }

        //            var FileName        = file.FileName;
        //            var FileNameSINEXT  = Path.GetFileNameWithoutExtension(FileName);
        //            var FileExt         = Path.GetExtension(FileName).ToLower();


        //            var ticks = new DateTime(2016, 1, 1).Ticks;
        //            var ans = DateTime.Now.Ticks - ticks;
        //            var uniqueId = ans.ToString("x");

        //            var Fichero_New = FileNameSINEXT + "_" + uniqueId + FileExt;

        //            var Fichero = "";
        //            if (Tipo == "" || Tipo == null)
        //            {
        //                Fichero = Path.Combine(PathDirFic, Fichero_New);
        //            }
        //            else
        //            {
        //                Fichero = Path.Combine(PathDirFi3D, Fichero_New);
        //            }


        //            if (file.Length > 0)
        //            {
        //                using (var fileStream = new FileStream(Fichero, FileMode.Create))
        //                {
        //                    await file.CopyToAsync(fileStream);
        //                }
        //            }

        //            var FindImgNext = ctxDB.ArticulosIMG.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Articulo).OrderBy(x => x.Articulo).ThenBy(x => x.Secuencia).LastOrDefault();
        //            if (FindImgNext != null)
        //            {
        //                Secuencia = FindImgNext.Secuencia + 1;
        //            }

        //            var ImgNew          = new ArticulosIMG();
        //            ImgNew.Empresa      = GrupoClaims.SessionEmpresa;
        //            ImgNew.Articulo     = Articulo;
        //            ImgNew.Tipo         = Tipo;
        //            ImgNew.Secuencia    = Secuencia;
        //            ImgNew.Imagen       = Fichero_New;

        //            ImgNew.IsoUser      = GrupoClaims.SessionUsuarioNombre;
        //            ImgNew.IsoFecAlt    = DateTime.Now;
        //            ImgNew.IsoFecMod    = DateTime.Now;

        //            try
        //            {
        //                ctxDB.ArticulosIMG.Add(ImgNew);
        //                await ctxDB.SaveChangesAsync();
        //            }
        //            catch (Exception e)
        //            {
        //                var errorImg = e.InnerException;
        //            }

        //        }
        //    }


        //    var ListArticuloIMG = ctxDB.ArticulosIMG.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Articulo).OrderBy(x => x.Tipo).ThenBy(x => x.Secuencia).ToList();

        //    return Json(ListArticuloIMG);
        //}

        //[HttpPost]
        //public async Task<IActionResult> CreateEdit_MET(ArticulosMET Row)
        //{
        //    bool resultProcess = false;            

        //    try
        //    {
        //        if (Row.Guid == Guid.Empty)
        //        {
        //            var Sec = 1;

        //            var LastRow = ctxDB.ArticulosMET.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Row.Articulo).OrderBy(x => x.Secuencia).LastOrDefault();
        //            if (LastRow != null)
        //            {
        //                Sec = LastRow.Secuencia + 1;
        //            }

        //            // Creacion
        //            Row.Empresa = GrupoClaims.SessionEmpresa;
        //            Row.Secuencia = Sec;
        //            Row.IsoUser = GrupoClaims.SessionUsuarioNombre;
        //            Row.IsoFecAlt = DateTime.Now;
        //            Row.IsoFecMod = DateTime.Now;

        //            ctxDB.ArticulosMET.Add(Row);
        //            await ctxDB.SaveChangesAsync();
        //            resultProcess = true;

        //            //var FindRow = ctxDB.ArticulosMET.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Row.Articulo && x.Metal == Row.Metal).FirstOrDefault();
        //            //if (FindRow == null)
        //            //{

        //            //}
        //            //else
        //            //{
        //            //    return StatusCode(200, "EXIST");
        //            //}


        //        }
        //        else
        //        {
        //            // Edicion
        //            Row.IsoUser     = GrupoClaims.SessionUsuarioNombre;
        //            Row.IsoFecMod   = DateTime.Now;

        //            ctxDB.ArticulosMET.Update(Row);
        //            await ctxDB.SaveChangesAsync();
        //            resultProcess = true;

        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(400, e.Message);  //  Send "Error"
        //    }

        //    var EstFichaArt = new EstFichaArt();

        //    var ListaMET = new List<ArticulosMET>();
        //    var RowMET = ctxDB.ArticulosMET.Where(x => x.Guid == Row.Guid).FirstOrDefault();

        //    RowMET.PesoTotal = RowMET.Peso + (RowMET.Peso * RowMET.Merma / 100);

        //    if (RowMET.Metal == "ORO")
        //    {
        //        var FindFixing = ctxDB.Fixing.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Metal == "ORO").OrderBy(x => x.Fecha).LastOrDefault();
        //        if (FindFixing != null)
        //        {
        //            switch (RowMET.Kilates)
        //            {
        //                case 24:
        //                    RowMET.Precio = RowMET.PesoTotal * FindFixing.K24Ley; RowMET.OroLey = FindFixing.K24Ley;
        //                    break;
        //                case 19:
        //                    RowMET.Precio = RowMET.PesoTotal * FindFixing.K19Ley; RowMET.OroLey = FindFixing.K19Ley;
        //                    break;
        //                case 18:
        //                    RowMET.Precio = RowMET.PesoTotal * FindFixing.K18Ley; RowMET.OroLey = FindFixing.K18Ley;
        //                    break;
        //                case 14:
        //                    RowMET.Precio = RowMET.PesoTotal * FindFixing.K14Ley; RowMET.OroLey = FindFixing.K14Ley;
        //                    break;
        //                case 10:
        //                    RowMET.Precio = RowMET.PesoTotal * FindFixing.K10Ley; RowMET.OroLey = FindFixing.K10Ley;
        //                    break;
        //                case 9:
        //                    RowMET.Precio = RowMET.PesoTotal * FindFixing.K9Ley; RowMET.OroLey = FindFixing.K9Ley;
        //                    break;
        //                case 8:
        //                    RowMET.Precio = RowMET.PesoTotal * FindFixing.K8Ley; RowMET.OroLey = FindFixing.K8Ley;
        //                    break;

        //            }
        //        }


        //    }


        //    ListaMET.Add(RowMET);

        //    EstFichaArt.ListArticuloMET = ListaMET;

        //    var ListArticuloMO = ctxDB.ArticulosMO.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Row.Articulo).OrderBy(x => x.MOSecuencia).ToList();
        //    foreach (var itemMO in ListArticuloMO)
        //    {
        //        if (itemMO.MOUniMed == "G")
        //        {
        //            // Sum Peso ArticulosMET
        //            decimal PesoART = 0;
        //            var SumPes = ctxDB.ArticulosMET.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Row.Articulo).Sum(x => x.Peso);
        //            PesoART = SumPes;

        //            itemMO.MOCantidad = PesoART;
        //            itemMO.Valor = PesoART * itemMO.MOPrecio;
        //        }
        //        else
        //        {
        //            itemMO.Valor = itemMO.MOCantidad * itemMO.MOPrecio;
        //        }
        //    }

        //    EstFichaArt.ListArticuloMO  = ListArticuloMO;

        //    if (resultProcess)
        //    {
        //        return StatusCode(200, EstFichaArt);
        //    }
        //    else
        //    {
        //        return StatusCode(400, null);
        //    }

        //}

        //[HttpPost]
        //public async Task<IActionResult> CreateEdit_PIE(ArticulosCOMP Row)
        //{
        //    bool resultProcess = false;

        //    try
        //    {
        //        if (Row.Guid == Guid.Empty)
        //        {

        //            var FindRow = ctxDB.ArticulosCOMP.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Row.Articulo && x.Componente == Row.Componente && x.Tipo == "P").FirstOrDefault();
        //            if (FindRow == null)
        //            {
        //                var Secuencia = 1;
        //                var FindSec = ctxDB.ArticulosCOMP.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Row.Articulo && x.Tipo == "P").OrderBy(x => x.Secuencia).LastOrDefault();
        //                if (FindSec != null)
        //                {
        //                    Secuencia = FindSec.Secuencia + 1;
        //                }

        //                // Creacion
        //                Row.Empresa = GrupoClaims.SessionEmpresa;
        //                Row.Secuencia = Secuencia;
        //                Row.IsoUser = GrupoClaims.SessionUsuarioNombre;
        //                Row.IsoFecAlt = DateTime.Now;
        //                Row.IsoFecMod = DateTime.Now;

        //                ctxDB.ArticulosCOMP.Add(Row);
        //                await ctxDB.SaveChangesAsync();
        //                resultProcess = true;
        //            }
        //            else
        //            {
        //                return StatusCode(200, "EXIST");
        //            }


        //        }
        //        else
        //        {
        //            // Edicion
        //            Row.IsoUser = GrupoClaims.SessionUsuarioNombre;
        //            Row.IsoFecMod = DateTime.Now;

        //            ctxDB.ArticulosCOMP.Update(Row);
        //            await ctxDB.SaveChangesAsync();
        //            resultProcess = true;

        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(400, e.Message);  //  Send "Error"
        //    }


        //    var Result = ctxDB.ArticulosCOMP.Where(x => x.Guid == Row.Guid).FirstOrDefault();
        //    if (Result != null)
        //    {
        //        // Peso componentes
        //        var PesoComp = 0m;
        //        var PesoCompSinMerma = 0m;
        //        var CompMet = ctxDB.ArticulosMET.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Result.Componente);
        //        foreach (var itemPC in CompMet)
        //        {
        //            PesoComp = PesoComp + (itemPC.Peso + (itemPC.Peso * itemPC.Merma / 100));
        //            PesoCompSinMerma = PesoCompSinMerma + (itemPC.Peso);
        //        }
        //        if (PesoComp != 0)
        //        {
        //            Result.Peso = PesoComp * Row.Cantidad;
        //            Result.PesoSinMerma = PesoCompSinMerma * Row.Cantidad;
        //        }
        //    }



        //    var Componente = ctxDB.Articulos.Where(x => x.Empresa == Row.Empresa && x.Articulo == Row.Componente).FirstOrDefault();
        //    if (Componente != null)
        //    {
        //        Result.Precio = Componente.ArtPrecioPVP;
        //        Result.Valor = Row.Cantidad * Row.Precio;
        //        Result.Descripcion = Componente.ArtDes;
        //    }

        //    if (resultProcess)
        //    {
        //        return StatusCode(200, Result);
        //    }
        //    else
        //    {
        //        return StatusCode(400, null);
        //    }

        //}

        //[HttpPost]
        //public async Task<IActionResult> CreateEdit_COMP(ArticulosCOMP Row)
        //{
        //    bool resultProcess = false;

        //    try
        //    {
        //        if (Row.Guid == Guid.Empty)
        //        {

        //            var FindRow = ctxDB.ArticulosCOMP.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Row.Articulo && x.Componente == Row.Componente).FirstOrDefault();
        //            if (FindRow == null)
        //            {
        //                var Secuencia = 1;
        //                var FindSec = ctxDB.ArticulosCOMP.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Row.Articulo).OrderBy(x => x.Secuencia).LastOrDefault();
        //                if (FindSec != null)
        //                {
        //                    Secuencia = FindSec.Secuencia + 1;
        //                }

        //                if (Row.Tipo == null)
        //                {
        //                    Row.Tipo = "C";
        //                }
        //                // Creacion
        //                Row.Empresa = GrupoClaims.SessionEmpresa;
        //                Row.Secuencia = Secuencia;
        //                Row.IsoUser = GrupoClaims.SessionUsuarioNombre;
        //                Row.IsoFecAlt = DateTime.Now;
        //                Row.IsoFecMod = DateTime.Now;

        //                ctxDB.ArticulosCOMP.Add(Row);
        //                await ctxDB.SaveChangesAsync();
        //                resultProcess = true;
        //            }
        //            else
        //            {
        //                return StatusCode(200, "EXIST");
        //            }


        //        }
        //        else
        //        {
        //            // Edicion
        //            Row.IsoUser = GrupoClaims.SessionUsuarioNombre;
        //            Row.IsoFecMod = DateTime.Now;

        //            ctxDB.ArticulosCOMP.Update(Row);
        //            await ctxDB.SaveChangesAsync();
        //            resultProcess = true;

        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(400, e.Message);  //  Send "Error"
        //    }


        //    var Result = ctxDB.ArticulosCOMP.Where(x => x.Guid == Row.Guid).FirstOrDefault();
        //    if (Result != null)
        //    {
        //        // Peso componentes
        //        var PesoComp = 0m;
        //        var PesoCompSinMerma = 0m;
        //        var CompMet = ctxDB.ArticulosMET.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Result.Componente);
        //        foreach (var itemPC in CompMet)
        //        {
        //            PesoComp = PesoComp + (itemPC.Peso + (itemPC.Peso * itemPC.Merma / 100));
        //            PesoCompSinMerma = PesoCompSinMerma + (itemPC.Peso);
        //        }
        //        if (PesoComp != 0)
        //        {
        //            Result.Peso = PesoComp * Row.Cantidad;
        //            Result.PesoSinMerma = PesoCompSinMerma * Row.Cantidad;
        //        }
        //    }



        //    var Componente = ctxDB.Articulos.Where(x => x.Empresa == Row.Empresa && x.Articulo == Row.Componente).FirstOrDefault();
        //    if (Componente != null)
        //    {
        //        Result.Precio       = Componente.ArtPrecioPVP;
        //        Result.Valor        = Row.Cantidad * Row.Precio;
        //        Result.Descripcion  = Componente.ArtDes;
        //    }

        //    if (resultProcess)
        //    {
        //        return StatusCode(200, Result);
        //    }
        //    else
        //    {
        //        return StatusCode(400, null);
        //    }

        //}

        //[HttpPost]
        //public async Task<IActionResult> CreateEdit_MO(ArticulosMO Row)
        //{
        //    bool resultProcess = false;
        //    try
        //    {
        //        if (Row.Guid == Guid.Empty)
        //        {
        //            var Secuencia = 1;
        //            var FindSec = ctxDB.ArticulosMO.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Row.Articulo).OrderBy(x => x.MOSecuencia).LastOrDefault();
        //            if (FindSec != null)
        //            {
        //                Secuencia = FindSec.MOSecuencia + 1;
        //            }

        //            // Creacion
        //            Row.Empresa = GrupoClaims.SessionEmpresa;
        //            Row.MOSecuencia = Secuencia;
        //            Row.CodMODescripcion = Row.CodMO;
        //            Row.IsoUser = GrupoClaims.SessionUsuarioNombre;
        //            Row.IsoFecAlt = DateTime.Now;
        //            Row.IsoFecMod = DateTime.Now;

        //            try
        //            {
        //                ctxDB.ArticulosMO.Add(Row);
        //                await ctxDB.SaveChangesAsync();
        //                resultProcess = true;
        //            }
        //            catch (Exception e)
        //            {

        //                var error = e.InnerException;
        //            }
        //        }
        //        else
        //        {
        //            // Edicion
        //            Row.CodMODescripcion = Row.CodMO;

        //            Row.IsoUser = GrupoClaims.SessionUsuarioNombre;
        //            Row.IsoFecMod = DateTime.Now;

        //            try
        //            {
        //                ctxDB.ArticulosMO.Update(Row);
        //                await ctxDB.SaveChangesAsync();
        //                resultProcess = true;
        //            }
        //            catch (Exception e)
        //            {
        //                var error = e.InnerException;
        //            }


        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(400, e.Message);  //  Send "Error"
        //    }


        //    var Result = ctxDB.ArticulosMO.Where(x => x.Guid == Row.Guid).FirstOrDefault();
        //    if (Result.MOUniMed == "G")
        //    {
        //        var SumPes = ctxDB.ArticulosMET.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Row.Articulo).Sum(x => x.Peso);
        //        var PesoART = SumPes;
        //        Result.MOCantidad = SumPes;
        //        Result.Valor = PesoART * Result.MOPrecio;

        //        //var Articulo = ctxDB.Articulos.Where(x => x.Empresa == Row.Empresa && x.Articulo == Row.Articulo).OrderBy(x => x.Articulo).FirstOrDefault();
        //        //if (Articulo != null)
        //        //{
        //        //    var PesoART = (decimal)Articulo.ArtPeso;
        //        //    Result.Valor = PesoART * Result.MOPrecio;
        //        //}
        //    }
        //    else
        //    {
        //        Result.Valor = Result.MOCantidad * Result.MOPrecio;
        //    }



        //    if (resultProcess)
        //    {
        //        return StatusCode(200, Result);
        //    }
        //    else
        //    {
        //        return StatusCode(400, null);
        //    }

        //}

        //[HttpPost]
        //public async Task<IActionResult> Alternativa_Add(string Articulo, string Alternativa)
        //{


        //    var FindRow = ctxDB.ArticulosALT.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Articulo && x.Alternativa == Alternativa).FirstOrDefault();
        //    if (FindRow == null)
        //    {
        //        var Secuencia = 1;
        //        var FindSec = ctxDB.ArticulosALT.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Articulo).OrderBy(x => x.Secuencia).LastOrDefault();
        //        if (FindSec != null)
        //        {
        //            Secuencia = FindSec.Secuencia + 1;
        //        }

        //        var AltNew = new ArticulosALT();
        //        AltNew.Empresa      = GrupoClaims.SessionEmpresa;
        //        AltNew.Articulo     = Articulo;
        //        AltNew.Alternativa  = Alternativa;
        //        AltNew.Secuencia    = Secuencia;

        //        AltNew.IsoUser      = GrupoClaims.SessionUsuarioNombre;
        //        AltNew.IsoFecAlt    = DateTime.Now;
        //        AltNew.IsoFecMod    = DateTime.Now;

        //        try
        //        {
        //            ctxDB.ArticulosALT.Add(AltNew);
        //            await ctxDB.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            var errorImg = e.InnerException;
        //            return StatusCode(200, null);
        //        }

        //        AltNew.PrecioCoste = (decimal)ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Alternativa).FirstOrDefault().ArtPrecioCoste;
        //        AltNew.PrecioVenta = (decimal)ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Alternativa).FirstOrDefault().ArtPrecioPVP;
        //        AltNew.Descripcion = ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == Alternativa).FirstOrDefault().ArtDes;


        //        return StatusCode(200, AltNew);

        //    }


        //    return StatusCode(200, null);
        //}

        ///********************************************************************************************************************************/
        ///*                                                  DELETE : MET - COMP - MO                                                    */
        ///********************************************************************************************************************************/

       



        //[HttpPost]
        //public async Task<IActionResult> Delete_MET(Guid Guid)
        //{
        //    var FindRow = ctxDB.ArticulosMET.Where(x => x.Guid == Guid).FirstOrDefault();
        //    if (FindRow != null)
        //    {
        //        try
        //        {
        //            ctxDB.ArticulosMET.Remove(FindRow);
        //            await ctxDB.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode(400, e.Message);
        //        }

        //        var ListArticuloMO = ctxDB.ArticulosMO.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == FindRow.Articulo).OrderBy(x => x.MOSecuencia).ToList();
        //        foreach (var itemMO in ListArticuloMO)
        //        {
        //            if (itemMO.MOUniMed == "G")
        //            {
        //                // Sum Peso ArticulosMET
        //                decimal PesoART = 0;
        //                var SumPes = ctxDB.ArticulosMET.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == FindRow.Articulo).Sum(x => x.Peso);
        //                PesoART = SumPes;

        //                itemMO.MOCantidad = PesoART;
        //                itemMO.Valor = PesoART * itemMO.MOPrecio;
        //            }
        //            else
        //            {
        //                itemMO.Valor = itemMO.MOCantidad * itemMO.MOPrecio;
        //            }
        //        }


        //        return StatusCode(200, ListArticuloMO);
        //    }

        //    return StatusCode(200, null);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Delete_COMP(Guid Guid)
        //{
        //    var FindRow = ctxDB.ArticulosCOMP.Where(x => x.Guid == Guid).FirstOrDefault();
        //    if (FindRow != null)
        //    {
        //        try
        //        {
        //            ctxDB.ArticulosCOMP.Remove(FindRow);
        //            await ctxDB.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode(400, e.Message);
        //        }

        //        return StatusCode(200, "OK");
        //    }

        //    return StatusCode(200, null);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Delete_COMP_PIE(Guid Guid)
        //{
        //    var FindRow = ctxDB.ArticulosCOMP.Where(x => x.Guid == Guid).FirstOrDefault();
        //    if (FindRow != null)
        //    {
        //        try
        //        {
        //            ctxDB.ArticulosCOMP.Remove(FindRow);
        //            await ctxDB.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode(400, e.Message);
        //        }

        //        return StatusCode(200, "OK");
        //    }

        //    return StatusCode(200, null);
        //}


        //[HttpPost]
        //public async Task<IActionResult> Delete_MO(Guid Guid)
        //{
        //    var FindRow = ctxDB.ArticulosMO.Where(x => x.Guid == Guid).FirstOrDefault();
        //    if (FindRow != null)
        //    {
        //        try
        //        {
        //            ctxDB.ArticulosMO.Remove(FindRow);
        //            await ctxDB.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode(400, e.Message);
        //        }

        //        return StatusCode(200, "OK");
        //    }

        //    return StatusCode(200, null);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Delete_ALT(Guid Guid)
        //{
        //    var FindRow = ctxDB.ArticulosALT.Where(x => x.Guid == Guid).FirstOrDefault();
        //    if (FindRow != null)
        //    {
        //        try
        //        {
        //            ctxDB.ArticulosALT.Remove(FindRow);
        //            await ctxDB.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode(400, e.Message);
        //        }

        //        return StatusCode(200, "OK");
        //    }

        //    return StatusCode(200, null);
        //}

        //[HttpPost]
        //public async Task<IActionResult> ARTICULO_SAVE(Articulos S_Articulo)
        //{

        //    try
        //    {
        //        ctxDB.Articulos.Update(S_Articulo);
        //        await ctxDB.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException exUpdt)
        //    {
        //        var xxx = exUpdt.Message;
        //        var xxxx = exUpdt.InnerException;

        //        return StatusCode(200, null);
        //    }
        //    catch (Exception e)
        //    {
        //        var xxx = e.Message;
        //        var xxxx = e.InnerException;

        //        return StatusCode(200, null);
        //    }

        //    return StatusCode(200, "OK");

        //}

        /********************************************************************************************************************************/
        /*                                                  FUNCIONES GENERAL ARTICULOS                                                 */
        /********************************************************************************************************************************/


        [HttpPost]
        public async Task<ActionResult> GetArticuloLazing(string query, int max)
        {
            try
            {
                query = query ?? string.Empty;

                var FindQueryArt = await ctxDB.Articulos
                    .Where(x => x.Empresa == GrupoClaims.SessionEmpresa && (x.Articulo.StartsWith(query) || x.ArtDes.Contains(query)))
                    .Select(c => new ArtComponentes
                    {
                        Guid = c.Guid,
                        Articulo = c.Articulo,
                        ArtDes = c.ArtDes,
                        ArtTipoVenta = c.ArtTipoVenta,
                        CalcArticuloDesc = c.Articulo + "  :  " + c.ArtDes,
                        ArtPrecioCoste = c.ArtPrecioCoste,
                        ArtPrecioPVP = c.ArtPrecioPVP
                    })
                    .Take(max)
                    .ToListAsync();

                return Json(new { Success = true, Data = FindQueryArt });
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return Json(new { Success = false, Message = ex.Message });
            }
        }

        public IActionResult _DialogArtRENCOP(string Articulo)
        {
            ViewBag.ArticuloOld = Articulo;

            return PartialView("_DialogArtRENCOP");
        }

        public IActionResult _DialogArtNEW()
        {
            return PartialView("_DialogArtNEW");
        }


        [HttpPost]
        public async Task<IActionResult> Articulo_Renombrar(string ArticuloOld, string ArticuloNew)
        {

            var FindArticuloNew = ctxDB.Articulos.Where(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Articulo == ArticuloNew).FirstOrDefault();
            if (FindArticuloNew != null)
            {
                return Json(new { success = false, message = "Artículo ya existe." });
            }

            var FindArticulo = ctxDB.Articulos.Where(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Articulo == ArticuloOld).FirstOrDefault();

            if (FindArticulo != null)
            {
                try
                {
                    FindArticulo.Articulo = ArticuloNew;
                    ctxDB.Update(FindArticulo);


                    // MO
                    var ArtMO = ctxDB.ArticulosMO.Where(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Articulo == ArticuloOld);
                    foreach (var itemMO in ArtMO)
                    {
                        itemMO.Articulo = ArticuloNew;
                        ctxDB.Update(itemMO);
                    }

                    // COMPONENTES
                    var ArtCOMP = ctxDB.ArticulosCOMP.Where(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Articulo == ArticuloOld);
                    foreach (var itemCOMP in ArtCOMP)
                    {
                        itemCOMP.Articulo = ArticuloNew;
                        ctxDB.Update(itemCOMP);
                    }

                    // COMPONENTES
                    var ArtCOMP2 = ctxDB.ArticulosCOMP.Where(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Componente == ArticuloOld);
                    foreach (var itemCOMP2 in ArtCOMP2)
                    {
                        itemCOMP2.Componente = ArticuloNew;
                        ctxDB.Update(itemCOMP2);
                    }

                    // IMAGENES
                    var ArtIMG = ctxDB.ArticulosIMG.Where(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Articulo == ArticuloOld);
                    foreach (var itemIMG in ArtIMG)
                    {
                        itemIMG.Articulo = ArticuloNew;
                        ctxDB.Update(itemIMG);
                    }

                    await ctxDB.SaveChangesAsync();

                    return Json(new { success = true, data = FindArticulo });
                }
                catch (Exception e)
                {
                    return Json(new { success = false, message = "Error al renombrar el artículo", error = e.Message });
                }

            }

            return Json(new { success = false, message = "Artículo no encontrado." });

        }


        [HttpPost]

        public async Task<IActionResult> Articulo_Copiar(string ArticuloOld, string ArticuloNew)
        {
            try
            {
                var FindArticuloNew = await ctxDB.Articulos
                    .FirstOrDefaultAsync(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Articulo == ArticuloNew);
                if (FindArticuloNew != null)
                {
                    return Json(new { success = false, message = "El artículo nuevo ya existe." });
                }

                var FindArticulo = await ctxDB.Articulos
                    .FirstOrDefaultAsync(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Articulo == ArticuloOld);
                if (FindArticulo == null)
                {
                    return Json(new { success = false, message = "Artículo original no encontrado." });
                }

                // Crear una copia del artículo
                var nuevoArticulo = FindArticulo.CloneAndModify(v =>
                {
                    v.Articulo = ArticuloNew;
                    v.Guid = Guid.NewGuid();
                });
                ctxDB.Articulos.Add(nuevoArticulo);

                // Guardar cambios para obtener el ID del nuevo artículo
                await ctxDB.SaveChangesAsync();

                // Copiar las tablas relacionadas
                // MO
                var ArtMO = await ctxDB.ArticulosMO
                    .Where(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Articulo == ArticuloOld)
                    .ToListAsync();
                foreach (var itemMO in ArtMO)
                {
                    var nuevoItemMO = itemMO.CloneAndModify(v =>
                    {
                        v.Articulo = ArticuloNew;
                        v.Guid = Guid.NewGuid();
                    });
                    ctxDB.ArticulosMO.Add(nuevoItemMO);
                }

                // COMPONENTES
                var ArtCOMP = await ctxDB.ArticulosCOMP
                    .Where(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Articulo == ArticuloOld)
                    .ToListAsync();
                foreach (var itemCOMP in ArtCOMP)
                {
                    var nuevoItemCOMP = itemCOMP.CloneAndModify(v =>
                    {
                        v.Articulo = ArticuloNew;
                        v.Guid = Guid.NewGuid();
                    });
                    ctxDB.ArticulosCOMP.Add(nuevoItemCOMP);
                }

                var ArtCOMP2 = await ctxDB.ArticulosCOMP
                    .Where(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Componente == ArticuloOld)
                    .ToListAsync();
                foreach (var itemCOMP2 in ArtCOMP2)
                {
                    var nuevoItemCOMP2 = itemCOMP2.CloneAndModify(v =>
                    {
                        v.Componente = ArticuloNew;
                        v.Guid = Guid.NewGuid();
                    });
                    ctxDB.ArticulosCOMP.Add(nuevoItemCOMP2);
                }

                // IMAGENES
                var ArtIMG = await ctxDB.ArticulosIMG
                    .Where(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Articulo == ArticuloOld)
                    .ToListAsync();
                foreach (var itemIMG in ArtIMG)
                {
                    var nuevoItemIMG = itemIMG.CloneAndModify(v =>
                    {
                        v.Articulo = ArticuloNew;
                        v.Guid = Guid.NewGuid();
                    });
                    ctxDB.ArticulosIMG.Add(nuevoItemIMG);
                }

                await ctxDB.SaveChangesAsync();
                return Json(new { success = true, data = nuevoArticulo });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    message = "Ocurrió un error durante la copia.",
                    error = e.Message,
                    stackTrace = e.StackTrace,
                    innerException = e.InnerException?.Message
                });
            }
        }


        // [HttpPost]
        // public async Task<IActionResult> ManoObraReordenar(string MoveEmpresa, string MoveArticulo, Guid MoveGuid, int NuevaPos)
        //{
        //    try
        //    {
        //        var FindRowToMove = ctxDB.ArticulosMO.Where(x => x.Guid == MoveGuid).FirstOrDefault();
        //        if (FindRowToMove != null)
        //        {
        //            NuevaPos++;

        //            var ListaActual = ctxDB.ArticulosMO.Where(x => x.Empresa == MoveEmpresa && x.Articulo == MoveArticulo).OrderBy(x => x.MOSecuencia).ToList();

        //            var ListaNew = new List<ArticulosMO>();
        //            var NewSec = 1;
        //            var AgregadaNewPos = false;

        //            foreach (var row in ListaActual)
        //            {
        //                if (NewSec == NuevaPos)
        //                {
        //                    FindRowToMove.MOSecuencia = NuevaPos;
        //                    ListaNew.Add(FindRowToMove);
        //                    NewSec++;
        //                    AgregadaNewPos = true;

        //                    row.MOSecuencia = NewSec;
        //                    ListaNew.Add(row);
        //                    NewSec++;
        //                }
        //                else
        //                {
        //                    if (row.Guid != MoveGuid)
        //                    {
        //                        row.MOSecuencia = NewSec;
        //                        ListaNew.Add(row);
        //                        NewSec++;
        //                    }
        //                }
        //            }

        //            // Por si se ha movido a la ultima posicion
        //            if (AgregadaNewPos == false)
        //            {
        //                FindRowToMove.MOSecuencia = NuevaPos;
        //                ListaNew.Add(FindRowToMove);
        //            }

        //            // Eliminar los registros existentes
        //            ctxDB.ArticulosMO.RemoveRange(ListaActual);
        //            await ctxDB.SaveChangesAsync();

        //            // Agregar la nueva lista ordenada a la base de datos
        //            ctxDB.ArticulosMO.AddRange(ListaNew);
        //            await ctxDB.SaveChangesAsync();


        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        var error = ex.Message;
        //    }



        //    return Json("Ok");
        //}

        [HttpPost]
        public async Task<IActionResult> HistorificarArticulos([FromBody] List<Guid> guids)
        {
            try
            {
                var articulos = await ctxDB.Articulos.Where(a => guids.Contains(a.Guid)).ToListAsync();
                if (articulos.Count == 0)
                {
                    return NotFound(new { success = false, message = "No se encontraron artículos para historificar." });
                }

                foreach (var articulo in articulos)
                {
                    articulo.Activo = !articulo.Activo;
                }
                await ctxDB.SaveChangesAsync();

                return Ok(new { success = true, message = $"{articulos.Count} artículo(s) actualizado(s)." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error al historificar los artículos", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarMultiplesArticulos([FromBody] List<Guid> guids)
        {
            try
            {
                var articulos = await ctxDB.Articulos.Where(a => guids.Contains(a.Guid)).ToListAsync();

                if (articulos.Count == 0)
                {
                    return NotFound(new { success = false, message = "No se encontraron artículos para eliminar." });
                }

                // Eliminar artículos relacionados
                foreach (var articulo in articulos)
                {
                    var lineasCOMP = ctxDB.ArticulosCOMP.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == articulo.Articulo);
                    ctxDB.ArticulosCOMP.RemoveRange(lineasCOMP);

                    var lineasIMG = ctxDB.ArticulosIMG.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == articulo.Articulo);
                    ctxDB.ArticulosIMG.RemoveRange(lineasIMG);

                    var lineasMO = ctxDB.ArticulosMO.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Articulo == articulo.Articulo);
                    ctxDB.ArticulosMO.RemoveRange(lineasMO);
                }

                ctxDB.Articulos.RemoveRange(articulos);
                await ctxDB.SaveChangesAsync();

                string mensaje = articulos.Count == 1 ? "Artículo eliminado correctamente." : $"{articulos.Count} artículos eliminados correctamente.";
                return Ok(new { success = true, message = mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error al eliminar los artículos", error = ex.Message });
            }
        }

    }
   


}