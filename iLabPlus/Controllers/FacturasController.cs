using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using iLabPlus.FacturaE;
using System.Text.RegularExpressions;
using System.Diagnostics.Metrics;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
// using static iLabPlus.Controllers.AlbaranesController;

namespace iLabPlus.Controllers
{
    [Authorize]
    public class FacturasController : Controller
    {
        private readonly IConfiguration         _configuration;
        private readonly DbContextiLabPlus      ctxDB;
        private readonly FunctionsBBDD          FunctionsBBDD;
        private readonly GrupoClaims            GrupoClaims;
        private readonly GrupoColumnsLayout     GrupoColumnsLayout;
        private readonly FunctionsLeyAntiFraude FunctionsLeyAntiFraude;

        public FacturasController(DbContextiLabPlus Context,  IConfiguration configuration, FunctionsBBDD _FunctionsBBDD, FunctionsLeyAntiFraude _FunctionsLeyAntiFraude)
        {
            ctxDB                   = Context;
            _configuration          = configuration;
            FunctionsLeyAntiFraude  = _FunctionsLeyAntiFraude;
            FunctionsBBDD           = _FunctionsBBDD;
            GrupoClaims             = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout      = FunctionsBBDD.GetColumnsLayout("gridFacturas");            
        }



        public IActionResult Index(Guid Guid)
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;

            if (Guid == Guid.Empty)
            {
                ViewBag.RepositionGrid = null;
            }
            else
            {
                ViewBag.RepositionGrid = Guid;
            }

            var ValsysVacio = new ValSys
            {
                Indice = "",
                Clave = "",
                Valor1 = ""
            };

            var Empresas = new List<string> { "Empresa" };
            Empresas.AddRange(ctxDB.ValSys
            .Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "Empresas")
            .OrderBy(x => x.Indice)
            .ThenBy(x => x.Clave)
            .Select(x => x.Clave)
            .ToList());
            ViewBag.Empresas = Empresas;

            var ValsysFacTipos = new List<ValSys>
                {
                new ValSys { Indice = "", Clave = "", Valor1 = "Tipo" },
                new ValSys { Indice = "FacTipo", Clave = "NAC", Valor1 = "Nacional" },
                new ValSys { Indice = "FacTipo", Clave = "EXP", Valor1 = "Exportacion" },
                new ValSys { Indice = "FacTipo", Clave = "PR", Valor1 = "Proforma" }
                };
            ViewBag.ValsysFacTipos = ValsysFacTipos;

            var ListClientes = new List<Clientes>();
            ViewBag.ListClientes = JsonConvert.SerializeObject(ListClientes);


            var Facturas = new List<FacturasCAB>();

            return View("Facturas", Facturas);
        }



        //public IActionResult Index(Guid Guid)
        //{
        //    ViewBag.MenuUserList        = FunctionsBBDD.GetMenuAccesos();
        //    ViewBag.ColumnsLayoutUser   = GrupoColumnsLayout.ColumnsLayoutUser;
        //    ViewBag.ColumnsPinnedUser   = GrupoColumnsLayout.ColumnsPinnedUser;


        //    if (Guid == Guid.Empty)
        //    {
        //        ViewBag.RepositionGrid = null;
        //    }
        //    else
        //    {
        //        ViewBag.RepositionGrid = Guid;
        //    }

        //    var ValsysVacio = new ValSys();
        //    ValsysVacio.Indice = "";
        //    ValsysVacio.Clave = "";
        //    ValsysVacio.Valor1 = "";


        //    var Empresas = new List<string>();
        //    Empresas.Add("Empresa");
        //    Empresas.AddRange(ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "Empresas").OrderBy(x => x.Indice).ThenBy(x => x.Clave).Select(x => x.Clave).ToList());
        //    ViewBag.Empresas = Empresas;

        //    var ValsysFacTipos = new List<ValSys>();
        //    ValsysVacio.Valor1 = "Tipo";
        //    ValsysFacTipos.Insert(0, ValsysVacio);

        //    var ValsysP = new ValSys();
        //    ValsysP.Indice = "FacTipo";
        //    ValsysP.Clave = "NAC";
        //    ValsysP.Valor1 = "Nacional";
        //    ValsysFacTipos.Add(ValsysP);
        //    ValsysP = new ValSys();
        //    ValsysP.Indice = "FacTipo";
        //    ValsysP.Clave = "EXP";
        //    ValsysP.Valor1 = "Exportacion";
        //    ValsysFacTipos.Add(ValsysP);
        //    ValsysP = new ValSys();
        //    ValsysP.Indice = "FacTipo";
        //    ValsysP.Clave = "PR";
        //    ValsysP.Valor1 = "Proforma";
        //    ValsysFacTipos.Add(ValsysP);

        //    ViewBag.ValsysFacTipos = ValsysFacTipos;

        //    var ListClientes = new List<Clientes>();
        //    ViewBag.ListClientes = JsonConvert.SerializeObject(ListClientes);

        //    var Facturas = new List<FacturasCAB>();
        //    return View("Facturas", Facturas);

        //}



        [HttpPost]
        public ActionResult GetDataFacturas(string DataRangeSelect, DateTime FechaINI, DateTime FechaFIN)
        {

            var ListaFacturas = new List<FacturasCAB>();

            if (DataRangeSelect == "Personalizado" || DataRangeSelect == "Personalizado")
            {
                ListaFacturas = ctxDB.FacturasCAB.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.FacFecha >= FechaINI && x.FacFecha <= FechaFIN)
                        .Select(c => new FacturasCAB
                        {
                            Guid = c.Guid,
                            Empresa = c.Empresa,
                            MultiEmpresa = c.MultiEmpresa,
                            Cliente = c.Cliente,
                            ClienteNombre = ctxDB.Clientes.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == c.Cliente).FirstOrDefault().CliNombre,
                            Factura = c.Factura,
                            FacFecha = c.FacFecha,

                            FacTipo = c.FacTipo,
                            FacTipoVenta = c.FacTipoVenta,
                            FacRefCliente = c.FacRefCliente,
                            FacOroFino = c.FacOroFino,
                            FacVendedor = c.FacVendedor,
                            VendedorNombre = ctxDB.Vendedores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Vendedor == c.FacVendedor).FirstOrDefault().VenNombre,
                            FacDivisa = c.FacDivisa,

                            FacEstado = c.FacEstado,

                            FacIVA = c.FacIVA,
                            FacDTOCial = c.FacDTOCial,
                            FacDTOPpago = c.FacDTOPpago,
                            FacDTORappel = c.FacDTORappel,

                            //TotalFacBI      = (decimal)ctxDB.FacturasLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == c.Cliente && x.Factura == c.Factura).Sum(X => X.FacPrecioTotal),
                            //TotalFacPendBI  = (decimal)ctxDB.FacturasLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == c.Cliente && x.Factura == c.Factura && x.FacEstado == "").Sum(X => X.FacPrecioTotal),

                            FacEnviada = c.FacEnviada,
                            FacFirmada = c.FacFirmada,

                            TotalFac = c.TotalFac,
                            TotalFacBI = c.TotalFacBI,
                            TotalFacIVA = c.TotalFacIVA,
                            TotalFacDTOs = c.TotalFacDTOs,
                            TotalDtoCial = c.TotalDtoCial,
                            TotalDtoPpago = c.TotalDtoPpago,
                            TotalDtoRappel = c.TotalDtoRappel,
                            TotalFacPend = c.TotalFacPend,
                            TotalFacPendBI = c.TotalFacPendBI

                            //Facturas         = string.Join(",", ctxDB.AlbaranesLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == c.Cliente && x.Albaran == c.Albaran).Select(a => a.Factura).Distinct().ToList()),

                        })
                    .OrderByDescending(x => x.FacFecha)
                    .ThenByDescending(x => x.Factura)
                    .ToList();
            }
            else
            {
                DateTime FechaInitDefault = new DateTime(DateTime.Now.Year - 1, 01, 01);
                DateTime FechaFintDefault = new DateTime(DateTime.Now.Year, 12, 31);


                ListaFacturas = ctxDB.FacturasCAB.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.FacFecha >= FechaInitDefault && x.FacFecha <= FechaFintDefault)
                        .Select(c => new FacturasCAB
                        {
                            Guid = c.Guid,
                            Empresa = c.Empresa,
                            MultiEmpresa = c.MultiEmpresa,
                            Cliente = c.Cliente,
                            ClienteNombre = ctxDB.Clientes.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == c.Cliente).FirstOrDefault().CliNombre,
                            Factura = c.Factura,
                            FacFecha = c.FacFecha,

                            FacTipo = c.FacTipo,
                            FacTipoVenta = c.FacTipoVenta,
                            FacRefCliente = c.FacRefCliente,
                            FacOroFino = c.FacOroFino,
                            FacVendedor = c.FacVendedor,
                            VendedorNombre = ctxDB.Vendedores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Vendedor == c.FacVendedor).FirstOrDefault().VenNombre,
                            FacDivisa = c.FacDivisa,

                            FacEstado = c.FacEstado,

                            FacIVA = c.FacIVA,
                            FacDTOCial = c.FacDTOCial,
                            FacDTOPpago = c.FacDTOPpago,
                            FacDTORappel = c.FacDTORappel,

                            //TotalFacBI      = (decimal)ctxDB.FacturasLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == c.Cliente && x.Factura == c.Factura).Sum(X => X.FacPrecioTotal),
                            //TotalFacPendBI  = (decimal)ctxDB.FacturasLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == c.Cliente && x.Factura == c.Factura && x.FacEstado == "").Sum(X => X.FacPrecioTotal),

                            FacEnviada = c.FacEnviada,
                            FacFirmada = c.FacFirmada,

                            TotalFac = c.TotalFac,
                            TotalFacBI = c.TotalFacBI,
                            TotalFacIVA = c.TotalFacIVA,
                            TotalFacDTOs = c.TotalFacDTOs,
                            TotalDtoCial = c.TotalDtoCial,
                            TotalDtoPpago = c.TotalDtoPpago,
                            TotalDtoRappel = c.TotalDtoRappel,
                            TotalFacPend = c.TotalFacPend,
                            TotalFacPendBI = c.TotalFacPendBI


                        })
                    .OrderByDescending(x => x.FacFecha)
                    .ThenByDescending(x => x.Factura)
                    .ToList();
            }


            return Json(ListaFacturas);


        }







        //[HttpPost]
        //public ActionResult GetDataFacturas(string DataRangeSelect, DateTime FechaINI, DateTime FechaFIN)
        //{

        //    var ListaFacturas = new List<FacturasCAB>();

        //    if (DataRangeSelect == "Personalizado")
        //    {
        //        ListaFacturas = ctxDB.FacturasCAB.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.FacFecha >= FechaINI && x.FacFecha <= FechaFIN)
        //                .Select(c => new FacturasCAB
        //                {
        //                    Guid            = c.Guid,
        //                    Empresa         = c.Empresa,
        //                    MultiEmpresa    = c.MultiEmpresa,
        //                    Cliente         = c.Cliente,
        //                    ClienteNombre   = ctxDB.Clientes.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == c.Cliente).FirstOrDefault().CliNombre,
        //                    Factura         = c.Factura,
        //                    FacFecha        = c.FacFecha,

        //                    FacTipo         = c.FacTipo,
        //                    FacTipoVenta    = c.FacTipoVenta,
        //                    FacRefCliente   = c.FacRefCliente,
        //                    FacOroFino      = c.FacOroFino,
        //                    FacVendedor     = c.FacVendedor,
        //                    VendedorNombre  = ctxDB.Vendedores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Vendedor == c.FacVendedor).FirstOrDefault().VenNombre,
        //                    FacDivisa       = c.FacDivisa,

        //                    FacEstado       = c.FacEstado,

        //                    FacIVA          = c.FacIVA,
        //                    FacDTOCial      = c.FacDTOCial,
        //                    FacDTOPpago     = c.FacDTOPpago,
        //                    FacDTORappel    = c.FacDTORappel,

        //                    FacEnviada      = c.FacEnviada,
        //                    FacFirmada      = c.FacFirmada,

        //                    TotalFac        = c.TotalFac,
        //                    TotalFacBI      = c.TotalFacBI,
        //                    TotalFacIVA     = c.TotalFacIVA,
        //                    TotalFacDTOs    = c.TotalFacDTOs,
        //                    TotalDtoCial    = c.TotalDtoCial,
        //                    TotalDtoPpago   = c.TotalDtoPpago,
        //                    TotalDtoRappel  = c.TotalDtoRappel,
        //                    TotalFacPend    = c.TotalFacPend,
        //                    TotalFacPendBI  = c.TotalFacPendBI

        //                })
        //            .OrderByDescending(x => x.FacFecha)
        //            .ThenByDescending(x => x.Factura)
        //            .ToList();
        //    }
        //    else
        //    {
        //        DateTime FechaInitDefault = new DateTime(DateTime.Now.Year - 1, 01, 01);
        //        DateTime FechaFintDefault = new DateTime(DateTime.Now.Year, 12, 31);


        //        ListaFacturas = ctxDB.FacturasCAB.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.FacFecha >= FechaInitDefault && x.FacFecha <= FechaFintDefault)
        //                .Select(c => new FacturasCAB
        //                {
        //                    Guid            = c.Guid,
        //                    Empresa         = c.Empresa,
        //                    MultiEmpresa    = c.MultiEmpresa,
        //                    Cliente         = c.Cliente,
        //                    ClienteNombre   = ctxDB.Clientes.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == c.Cliente).FirstOrDefault().CliNombre,
        //                    Factura         = c.Factura,
        //                    FacFecha        = c.FacFecha,

        //                    FacTipo         = c.FacTipo,
        //                    FacTipoVenta    = c.FacTipoVenta,
        //                    FacRefCliente   = c.FacRefCliente,
        //                    FacOroFino      = c.FacOroFino,
        //                    FacVendedor     = c.FacVendedor,
        //                    VendedorNombre  = ctxDB.Vendedores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Vendedor == c.FacVendedor).FirstOrDefault().VenNombre,
        //                    FacDivisa       = c.FacDivisa,

        //                    FacEstado       = c.FacEstado,

        //                    FacIVA          = c.FacIVA,
        //                    FacDTOCial      = c.FacDTOCial,
        //                    FacDTOPpago     = c.FacDTOPpago,
        //                    FacDTORappel    = c.FacDTORappel,

        //                    FacEnviada      = c.FacEnviada,
        //                    FacFirmada      = c.FacFirmada,

        //                    TotalFac        = c.TotalFac,
        //                    TotalFacBI      = c.TotalFacBI,
        //                    TotalFacIVA     = c.TotalFacIVA,
        //                    TotalFacDTOs    = c.TotalFacDTOs,
        //                    TotalDtoCial    = c.TotalDtoCial,
        //                    TotalDtoPpago   = c.TotalDtoPpago,
        //                    TotalDtoRappel  = c.TotalDtoRappel,
        //                    TotalFacPend    = c.TotalFacPend,
        //                    TotalFacPendBI  = c.TotalFacPendBI


        //                })
        //            .OrderByDescending(x => x.FacFecha)
        //            .ThenByDescending(x => x.Factura)
        //            .ToList();
        //    }


        //    return Json(ListaFacturas);


        //}



        public FacturasCAB Factura_Calculo_Totales(FacturasCAB item)
        {
            // Sumas seguras, manejando valores nulos
            item.TotalFacBI = ctxDB.FacturasLIN
                .Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == item.Cliente && x.Factura == item.Factura)
                .Sum(x => (decimal?)x.FacPrecioTotal) ?? 0;

            item.TotalFacPendBI = ctxDB.FacturasLIN
                .Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == item.Cliente && x.Factura == item.Factura && x.FacEstado == "")
                .Sum(x => (decimal?)x.FacPrecioTotal) ?? 0;

            item.TotalFacPend = item.TotalFacPendBI * (((decimal)item.FacIVA.GetValueOrDefault(0) / 100) + 1);

            if (item.FacDTOCial.HasValue)
            {
                item.TotalDtoCial = item.TotalFacBI * item.FacDTOCial.Value / 100;
            }
            else
            {
                item.TotalDtoCial = 0;
            }

            if (item.FacDTOPpago.HasValue)
            {
                item.TotalDtoPpago = item.TotalFacBI * item.FacDTOPpago.Value / 100;
            }
            else
            {
                item.TotalDtoPpago = 0;
            }

            if (item.FacDTORappel.HasValue)
            {
                item.TotalDtoRappel = item.TotalFacBI * item.FacDTORappel.Value / 100;
            }
            else
            {
                item.TotalDtoRappel = 0;
            }

            item.TotalFacDTOs = item.TotalDtoCial.GetValueOrDefault(0) + item.TotalDtoPpago.GetValueOrDefault(0) + item.TotalDtoRappel.GetValueOrDefault(0);

            decimal totalFacBI = item.TotalFacBI.GetValueOrDefault(0);
            decimal totalFacDTOs = item.TotalFacDTOs.GetValueOrDefault(0);
            decimal facIVA = item.FacIVA.GetValueOrDefault(0);

            item.TotalFacIVA = Math.Round((totalFacBI - totalFacDTOs) * facIVA / 100, 2);
            item.TotalFac = Math.Round((totalFacBI - totalFacDTOs) + item.TotalFacIVA.GetValueOrDefault(0), 2);

            return item;
        }

        public IActionResult _DialogFacNew(string Cliente)
        {

            var ListClientes = ctxDB.Clientes.Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
                .Select(c => new Clientes
                {
                    Guid = c.Guid,
                    Cliente = c.Cliente,
                    CliNombre = c.CliNombre,
                    CliTarifaVenta = c.CliTarifaVenta,
                    CalcClienteNombre = c.Cliente + "  :  " + c.CliNombre
                })
                .ToList();
            ViewBag.ListClientes = JsonConvert.SerializeObject(ListClientes);


            var ValsysFacTipos = new List<ValSys>();
            var ValsysP = new ValSys();
            ValsysP.Indice = "FacTipo";
            ValsysP.Clave = "NAC";
            ValsysP.Valor1 = "Nacional";
            ValsysFacTipos.Add(ValsysP);
            ValsysP = new ValSys();
            ValsysP.Indice = "FacTipo";
            ValsysP.Clave = "EXP";
            ValsysP.Valor1 = "Exportacion";
            ValsysFacTipos.Add(ValsysP);
            ValsysP = new ValSys();
            ValsysP.Indice = "FacTipo";
            ValsysP.Clave = "PR";
            ValsysP.Valor1 = "Proforma";
            ValsysFacTipos.Add(ValsysP);
            ViewBag.ValsysFacTipos = ValsysFacTipos;


            return PartialView("_DialogFacNew");
        }


        public FacturasLIN Factura_Lin_Calculo_Totales(FacturasLIN item)
        {
            decimal Bruto = 0;
            decimal PrecioTotal = 0;

            switch (item.FacArtTipoVenta)
            {
                case "Etiqueta":
                case "Hechura":
                    Bruto = (decimal)(item.FacQty * item.FacPrecio);
                    PrecioTotal = Bruto - (Bruto * (decimal)item.FacDtoLin / 100);
                    break;

                case "Peso":
                case "PesoHechura":
                    decimal PesoTotal = (decimal)(item.FacPeso + (item.FacPeso * item.FacMerma / 100));
                    PrecioTotal = PesoTotal * (decimal)item.FacPrecio;
                    PrecioTotal -= PrecioTotal * (decimal)item.FacDtoLin / 100;
                    break;

                default:
                    Bruto = (decimal)(item.FacQty * item.FacPrecio);
                    PrecioTotal = Bruto - (Bruto * (decimal)item.FacDtoLin / 100);
                    break;
            }

            item.FacPrecioTotal = Math.Round(PrecioTotal, 2);
            return item;
        }


        public IActionResult _DialogFacLinMedidas(string Medidas, decimal QtyLin)
        {

            ViewBag.LinQtyTotal = QtyLin;

            var ValMedidas = new List<string>();
            ValMedidas.Add(" ");
            ValMedidas.Add("06");
            ValMedidas.Add("07");
            ValMedidas.Add("08");
            ValMedidas.Add("09");
            ValMedidas.Add("10");
            ValMedidas.Add("11");
            ValMedidas.Add("12");
            ValMedidas.Add("13");
            ValMedidas.Add("14");
            ValMedidas.Add("15");
            ValMedidas.Add("16");
            ValMedidas.Add("17");
            ValMedidas.Add("18");
            ViewBag.SysValMedidas = ValMedidas;

            ViewBag.Medidas = Medidas;

            return PartialView("_DialogFacLinMedidas");
        }



        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> Create_Factura(string Cliente, string TipoFac, string TipoVenta)
        {
            try
            {
                string GuidNewFac = null;
                var Factura = new FacturasCAB();

                var FindCliente = ctxDB.Clientes.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == Cliente).FirstOrDefault();
                if (FindCliente != null)
                {
                    var Contador = "";
                    var LastFac = ctxDB.FacturasCAB.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.FacTipo == TipoFac).OrderBy(x => x.Factura).LastOrDefault();

                    if (LastFac != null)
                    {
                        if (TipoFac == "EXP" || TipoFac == "PR")
                        {
                            if (LastFac.Factura.Contains('/'))
                            {
                                string[] partes = LastFac.Factura.Split('/');
                                if (partes.Length == 2)
                                {
                                    int numero = int.Parse(partes[1]);
                                    numero++;
                                    Contador = partes[0] + "/" + numero.ToString("D6");
                                }
                                else
                                {
                                    return StatusCode(400, "El formato de la última factura es inválido.");
                                }
                            }
                            else
                            {
                                return StatusCode(400, "El formato de la última factura es inválido.");
                            }
                        }
                        else if (TipoFac == "NAC")
                        {
                            if (int.TryParse(LastFac.Factura, out int numero))
                            {
                                numero++;
                                Contador = numero.ToString("D6");
                            }
                            else
                            {
                                return StatusCode(400, "El formato de la última factura es inválido.");
                            }
                        }
                    }
                    else
                    {
                        if (TipoFac == "EXP" || TipoFac == "PR")
                        {
                            Contador = TipoFac + "/" + (1).ToString("D6");
                        }
                        else if (TipoFac == "NAC")
                        {
                            Contador = (1).ToString("D6");
                        }
                    }

                    if (TipoVenta == null)
                    {
                        switch (FindCliente.CliFacturaTipo)
                        {
                            case "Etiqueta":
                                TipoVenta = "E";
                                break;
                            case "Hechura":
                                TipoVenta = "H";
                                break;
                            case "Peso":
                                TipoVenta = "P";
                                break;
                            case "PesoHechura":
                                TipoVenta = "PH";
                                break;
                            default:
                                TipoVenta = "E";
                                break;
                        }
                    }

                    Factura.FacTipoVenta = TipoVenta;
                    Factura.Empresa = GrupoClaims.SessionEmpresa;
                    Factura.MultiEmpresa = null; // OJO poner dinámico
                    Factura.Cliente = Cliente;
                    Factura.Factura = Contador;
                    Factura.FacTipo = TipoFac;
                    Factura.FacRefCliente = "";
                    Factura.FacFecha = DateTime.Now;
                    Factura.FacEstado = "";
                    Factura.FacVendedor = FindCliente.CliVendedor;
                    Factura.FacIVA = FindCliente.CliIVA;
                    Factura.FacRE = FindCliente.CliRE;
                    Factura.FacIGIC = FindCliente.CliIGIC;
                    Factura.FacIRPF = FindCliente.CliIRPF;
                    Factura.FacTarifaVenta = FindCliente.CliTarifaVenta;
                    Factura.FacDivisa = FindCliente.CliDivisa;
                    Factura.FacIdioma = FindCliente.CliIdioma;
                    Factura.FacDTOCial = FindCliente.CliDTOCial;
                    Factura.FacDTOPpago = FindCliente.CliDTOPpago;
                    Factura.FacDTORappel = FindCliente.CliDTORappel;
                    Factura.FacObserv = "";
                    Factura.FacObserv2 = "";
                    Factura.IsoUser = GrupoClaims.SessionUsuario;
                    Factura.IsoFecAlt = DateTime.Now;
                    Factura.IsoFecMod = DateTime.Now;
                    Factura.FacDirMerDireccion = FindCliente.CliDirMerDireccion;
                    Factura.FacDirMerDP = FindCliente.CliDirMerDP;
                    Factura.FacDirMerPoblacion = FindCliente.CliDirMerPoblacion;
                    Factura.FacDirMerProvincia = FindCliente.CliDirMerProvincia;
                    Factura.FacDirMerPais = FindCliente.CliDirMerPais;
                    Factura.FacDirFacDireccion = FindCliente.CliDirFacDireccion;
                    Factura.FacDirFacDP = FindCliente.CliDirFacDP;
                    Factura.FacDirFacPoblacion = FindCliente.CliDirFacPoblacion;
                    Factura.FacDirFacProvincia = FindCliente.CliDirFacProvincia;
                    Factura.FacDirFacPais = FindCliente.CliDirFacPais;

                    try
                    {
                        ctxDB.FacturasCAB.Add(Factura);
                        await ctxDB.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        var error = e.InnerException;
                        return StatusCode(400, null);
                    }

                    Factura = Factura_Calculo_Totales(Factura);
                    var LA_RegNew = JsonConvert.SerializeObject(Factura);
                    var LA_Result = await FunctionsLeyAntiFraude.RegistrarAsync("FacEmi", "C", "", Factura.Factura, null, LA_RegNew);

                }

                GuidNewFac = Factura.Guid.ToString();

                if (GuidNewFac != null && GuidNewFac != "")
                {
                    return StatusCode(200, GuidNewFac);
                }
                else
                {
                    return StatusCode(400, null);
                }
            }
            catch (Exception e)
            {
                var error = e.InnerException;
                return StatusCode(400, null);
            }
        }



        [HttpGet("/Facturas/Ficha/{Guid}")]
        public IActionResult Ficha(Guid Guid)
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();

            var EstFichaFac = new EstFichaFac();
            var FacturaEst = new FacturasCAB();
            var FacturaLins = new List<FacturasLIN>();

            if (Guid == Guid.Empty)
            {
                // Factura Nuevo
                //FacturaEst.FacFecha = DateTime.Now;
            }
            else
            {
                FacturaEst = ctxDB.FacturasCAB.Where(x => x.Guid == Guid).OrderBy(x => x.Factura).FirstOrDefault();
                if (FacturaEst != null)
                {
                    var cliente = ctxDB.Clientes.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == FacturaEst.Cliente).FirstOrDefault();
                    if (cliente != null)
                    {
                        FacturaEst.ClienteNombre = cliente.CliNombre;
                    }

                    // Manejar posibles valores nulos
                    FacturaEst.TotalFacBI = ctxDB.FacturasLIN
                        .Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == FacturaEst.Cliente && x.Factura == FacturaEst.Factura)
                        .Sum(x => (decimal?)x.FacPrecioTotal) ?? 0;

                    FacturaEst.TotalFacPendBI = ctxDB.FacturasLIN
                        .Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == FacturaEst.Cliente && x.Factura == FacturaEst.Factura && x.FacEstado == "")
                        .Sum(x => (decimal?)x.FacPrecioTotal) ?? 0;

                    decimal facIVA = FacturaEst.FacIVA.GetValueOrDefault(0);
                    FacturaEst.TotalFacPend = FacturaEst.TotalFacPendBI * ((facIVA / 100) + 1);

                    var Totales = Factura_Calculo_Totales(FacturaEst);
                    FacturaEst.TotalDtoCial = Totales.FacDTOCial ?? 0;
                    FacturaEst.TotalDtoPpago = Totales.TotalDtoPpago ?? 0;
                    FacturaEst.TotalDtoRappel = Totales.TotalDtoRappel ?? 0;
                    FacturaEst.TotalFacDTOs = Totales.TotalFacDTOs ?? 0;
                    FacturaEst.TotalFacIVA = Totales.TotalFacIVA ?? 0;
                    FacturaEst.TotalFac = Totales.TotalFac ?? 0;
                    FacturaEst.TotalFacPend = Totales.TotalFacPend ?? 0;

                    FacturaLins = ctxDB.FacturasLIN
                        .Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == FacturaEst.Cliente && x.Factura == FacturaEst.Factura)
                        .ToList();
                }
            }

            EstFichaFac.FacturaEst = FacturaEst;
            EstFichaFac.ListFacturaLIN = FacturaLins;

            /********************************************************************************************/
            /* VALORES AUXILIARES */
            /********************************************************************************************/
            var ValsysFacTipos = new List<ValSys>
    {
        new ValSys { Indice = "FacTipo", Clave = "NAC", Valor1 = "Nacional" },
        new ValSys { Indice = "FacTipo", Clave = "EXP", Valor1 = "Exportacion" },
        new ValSys { Indice = "FacTipo", Clave = "PR", Valor1 = "Proforma" }
    };
            ViewBag.ValsysFacTipos = ValsysFacTipos;

            var VendedorVacio = new Vendedores { Vendedor = "", VenNombre = "" };

            var Vendedores = ctxDB.Vendedores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Vendedor).ToList();
            Vendedores.Insert(0, VendedorVacio);
            ViewBag.ListVendedores = Vendedores;

            var Tarifas = ctxDB.TarifasVenta.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Tarifa).ToList();
            ViewBag.ListTarifas = Tarifas;

            var TipoVenta = new List<string> { "", "Etiqueta", "Hechura", "Peso", "PesoHechura" };
            ViewBag.TipoVenta = TipoVenta;

            var Divisas = ctxDB.Divisas.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Divisa).ToList();
            ViewBag.ListDivisas = Divisas;

            var Idiomas = new List<string> { "Español", "Ingles" };
            ViewBag.ListIdiomas = Idiomas;

            var ListVaciaArticulos = new List<ArtComponentes> { new ArtComponentes() };
            ViewBag.ListVaciaArticulos = JsonConvert.SerializeObject(ListVaciaArticulos);

            var ValKilates = new List<string> { "", "24", "19", "18", "14", "10", "9", "8" };
            ViewBag.ValKilates = ValKilates;

            var ValTipoOro = new List<string> { "", "AM", "BL", "RS", "BC", "AM-BL", "BL-AM", "RS-BL", "BL-RS", "AM-RS" };
            ViewBag.ValTipoOro = ValTipoOro;

            var _DocumentsDirectory = _configuration.GetSection("URLStrings").GetSection("DocumentsDirectory").Value;
            var PathArtImg = _DocumentsDirectory + "/" + GrupoClaims.SessionEmpresa + "/" + "Articulos" + "/";
            ViewBag.RutaArtImg = PathArtImg;
            var PathArtImg3D = _DocumentsDirectory + "/" + GrupoClaims.SessionEmpresa + "/" + "Articulos3D" + "/";
            ViewBag.RutaArtImg3D = PathArtImg3D;

            /********************************************************************************************/
            /********************************************************************************************/

            return View("FacFicha", EstFichaFac);
        }


        [HttpPost]
        public async Task<IActionResult> Save_FACTURA(FacturasCAB FacturaEst)  // MODIFICADO EN ILABPLUS
        {
            var LA_RegOld = "";
            var FindFact = ctxDB.FacturasCAB.Where(x => x.Guid == FacturaEst.Guid).FirstOrDefault();
            if (FindFact != null)
            {
                LA_RegOld   = JsonConvert.SerializeObject(FindFact);
                ctxDB.Entry(FindFact).State = EntityState.Detached; // Desconectar la entidad
            }

            try
            {
                FacturaEst = Factura_Calculo_Totales(FacturaEst);

                FacturaEst.IsoUser = GrupoClaims.SessionUsuario;
                FacturaEst.IsoFecMod = DateTime.Now;

                ctxDB.FacturasCAB.Update(FacturaEst);
                await ctxDB.SaveChangesAsync();
            }
            catch (Exception e)
            {
                var xxx = e.Message;
                var xxxx = e.InnerException;

                return StatusCode(200, null);
            }
            
            var LA_RegNew = JsonConvert.SerializeObject(FacturaEst);
            var LA_Result = await FunctionsLeyAntiFraude.RegistrarAsync("FacEmi", "M", "", FacturaEst.Factura, LA_RegOld, LA_RegNew);

            return StatusCode(200, "OK");
        }


        [HttpPost]
        public async Task<IActionResult> Delete_FACTURA(Guid Guid) // Modificado en ILABPLUS
        {
            var FacFind = ctxDB.FacturasCAB.Where(x => x.Guid == Guid).FirstOrDefault();
            if (FacFind != null)
            {
                FacFind = Factura_Calculo_Totales(FacFind);
                var LA_RegOld = JsonConvert.SerializeObject(FacFind);

                try
                {

                    var Lineas = ctxDB.FacturasLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == FacFind.Cliente && x.Factura == FacFind.Factura).ToList();

                    ctxDB.FacturasLIN.RemoveRange(Lineas);

                    ctxDB.FacturasCAB.Remove(FacFind);
                    await ctxDB.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return StatusCode(400, e.Message);
                }

                var LA_Result = await FunctionsLeyAntiFraude.RegistrarAsync("FacEmi","D","", FacFind.Factura, LA_RegOld,null);


                return StatusCode(200, "OK");
            }

            return StatusCode(200, null);
        }



        [HttpPost]
        public async Task<IActionResult> Save_FACLIN([FromForm] FacturasLIN FacturaLIN)
        {
            try
            {
                if (FacturaLIN == null)
                {
                    System.Diagnostics.Debug.WriteLine("FacturaLIN es nulo");
                    return BadRequest("El modelo FacturaLIN es nulo");
                }

                // Verificación de tamaños de las propiedades
                var propiedades = typeof(FacturasLIN).GetProperties();
                foreach (var propiedad in propiedades)
                {
                    var valor = propiedad.GetValue(FacturaLIN)?.ToString();
                    if (valor != null)
                    {
                        var maxLength = GetMaxLength(propiedad);
                        if (maxLength.HasValue && valor.Length > maxLength.Value)
                        {
                            System.Diagnostics.Debug.WriteLine($"La propiedad {propiedad.Name} excede el tamaño máximo permitido de {maxLength.Value} caracteres. Valor: {valor}");
                        }
                    }
                }

                // Asegúrate de que el campo Albaran tenga un valor
                if (string.IsNullOrEmpty(FacturaLIN.Albaran))
                {
                    FacturaLIN.Albaran = "DEFAULT_ALBARAN"; // Cambia esto por un valor predeterminado apropiado
                }

                // Registro de datos recibidos para depuración
                //System.Diagnostics.Debug.WriteLine("Datos recibidos en Save_FACLIN:");
                //System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(FacturaLIN));

                var LA_RegOld = "";
                var FindFacLin = ctxDB.FacturasLIN.Where(x => x.Guid == FacturaLIN.Guid).FirstOrDefault();
                if (FindFacLin != null)
                {
                    LA_RegOld = JsonConvert.SerializeObject(FindFacLin);
                    ctxDB.Entry(FindFacLin).State = EntityState.Detached; // Desconectar la entidad
                }

                FacturaLIN.IsoUser = GrupoClaims.SessionUsuario;
                FacturaLIN.IsoFecMod = DateTime.Now;

                if (FindFacLin != null)
                {
                    ctxDB.FacturasLIN.Update(FacturaLIN);
                }
                else
                {
                    FacturaLIN.Guid = Guid.NewGuid();
                    FacturaLIN.IsoFecAlt = DateTime.Now;
                    ctxDB.FacturasLIN.Add(FacturaLIN);
                }

                await ctxDB.SaveChangesAsync();

                FacturaLIN = Factura_Lin_Calculo_Totales(FacturaLIN);

                var LA_RegNew = JsonConvert.SerializeObject(FacturaLIN);
                var LA_Result = await FunctionsLeyAntiFraude.RegistrarAsync("FacLin", "M", "", FacturaLIN.Factura, LA_RegOld, LA_RegNew);

                return Ok(FacturaLIN); // Devuelve la entidad actualizada
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine("Error al guardar FacLin:");
                //System.Diagnostics.Debug.WriteLine(e.ToString());
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // Método auxiliar para obtener la longitud máxima de una propiedad
        private int? GetMaxLength(PropertyInfo propiedad)
        {
            var atributoMaxLength = propiedad.GetCustomAttribute<MaxLengthAttribute>();
            if (atributoMaxLength != null)
            {
                return atributoMaxLength.Length;
            }

            // Verificar si la propiedad tiene un atributo StringLength en su lugar
            var atributoStringLength = propiedad.GetCustomAttribute<StringLengthAttribute>();
            if (atributoStringLength != null)
            {
                return atributoStringLength.MaximumLength;
            }

            return null;
        }


        //public async Task<IActionResult> Save_FACLIN(FacturasLIN FacturaLIN)
        //{
        //    try
        //    {
        //        var LA_RegOld = "";
        //        var FindFacLin = ctxDB.FacturasLIN.Where(x => x.Guid == FacturaLIN.Guid).FirstOrDefault();
        //        if (FindFacLin != null)
        //        {
        //            LA_RegOld = JsonConvert.SerializeObject(FindFacLin);
        //            ctxDB.Entry(FindFacLin).State = EntityState.Detached; // Desconectar la entidad
        //        }

        //        FacturaLIN.IsoUser = GrupoClaims.SessionUsuario;
        //        FacturaLIN.IsoFecMod = DateTime.Now;

        //        if (FindFacLin != null)
        //        {
        //            ctxDB.FacturasLIN.Update(FacturaLIN);
        //        }
        //        else
        //        {
        //            FacturaLIN.Guid = Guid.NewGuid();
        //            FacturaLIN.IsoFecAlt = DateTime.Now;
        //            ctxDB.FacturasLIN.Add(FacturaLIN);
        //        }

        //        await ctxDB.SaveChangesAsync();

        //        FacturaLIN = Factura_Lin_Calculo_Totales(FacturaLIN);

        //        var LA_RegNew = JsonConvert.SerializeObject(FacturaLIN);
        //        var LA_Result = await FunctionsLeyAntiFraude.RegistrarAsync("FacLin", "M", "", FacturaLIN.Factura, LA_RegOld, LA_RegNew);
        //    }
        //    catch (Exception e)
        //    {
        //        var xxx = e.Message;
        //        var xxxx = e.InnerException;

        //        return StatusCode(400, null);
        //    }
        //    return StatusCode(200, "OK");
        //}




        [HttpPost]

        //public async Task<IActionResult> Delete_FACTURA_LINEA(string FactLinGuid)
        //{
        //    var FindFacturaLin = ctxDB.FacturasLIN.Where(x => x.Guid == new Guid(FactLinGuid)).FirstOrDefault();
        //    if (FindFacturaLin != null)
        //    {
        //        ctxDB.FacturasLIN.Remove(FindFacturaLin);
        //        await ctxDB.SaveChangesAsync();
        //    }
        //    return StatusCode(200, "OK");
        //}


        public async Task<IActionResult> Delete_FACTURA_LINEA(Guid Guid)
        {
            var FacLinFind = ctxDB.FacturasLIN.Where(x => x.Guid == Guid).FirstOrDefault();
            if (FacLinFind != null)
            {
                var LA_RegOld = "";
                var FindFactura = ctxDB.FacturasCAB.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == FacLinFind.Cliente && x.Factura == FacLinFind.Factura).FirstOrDefault();
                if (FindFactura != null)
                {
                    FindFactura = Factura_Calculo_Totales(FindFactura);
                    LA_RegOld = JsonConvert.SerializeObject(FindFactura);

                    //var FacLinCOMPFind = ctxDB.FacturasLINCOMP.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == FacLinFind.Cliente && x.Factura == FacLinFind.Factura && x.FacLinea == FacLinFind.FacLinea).ToList();
                    //if (FacLinCOMPFind.Count() > 0)
                    //{
                    //    try
                    //    {
                    //        ctxDB.RemoveRange(FacLinCOMPFind);
                    //        await ctxDB.SaveChangesAsync();
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        return StatusCode(400, e.Message);
                    //    }
                    //}

                    try
                    {
                        ctxDB.FacturasLIN.Remove(FacLinFind);
                        await ctxDB.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        return StatusCode(400, e.Message);
                    }

                    FindFactura = Factura_Calculo_Totales(FindFactura);
                    var LA_RegNew = JsonConvert.SerializeObject(FindFactura);
                    var LA_Result = await FunctionsLeyAntiFraude.RegistrarAsync("FacEmi", "DL", "", FindFactura.Factura, LA_RegOld, LA_RegNew);
                }



                return StatusCode(200, "OK");
            }

            return StatusCode(200, null);
        }



        public static decimal Calcular_FacLin_PrecioTotal(FacturasLIN FacLin)
        {
            var Bruto = 0m;
            var PrecioTotal = 0m;

            switch (FacLin.FacArtTipoVenta)
            {
                case "Etiqueta":
                    Bruto = (decimal)(FacLin.FacQty * FacLin.FacPrecio);
                    PrecioTotal = Bruto - (Bruto * (decimal)FacLin.FacDtoLin / 100);
                    break;

                case "Hechura":
                    Bruto = (decimal)(FacLin.FacQty * FacLin.FacPrecio);
                    PrecioTotal = Bruto - (Bruto * (decimal)FacLin.FacDtoLin / 100);
                    break;

                case "Peso":

                    break;

                case "PesoHechura":

                    break;

                default:
                    // Por defecto como si fuera Etiqueta
                    Bruto = (decimal)(FacLin.FacQty * FacLin.FacPrecio);
                    PrecioTotal = Bruto - (Bruto * (decimal)FacLin.FacDtoLin / 100);
                    break;
            }


            return PrecioTotal;
        }


        public IActionResult DialogSelFacLin(string ListSelectFacRowsGuidParam, bool FiltroAlbaran)
        {
            var ListSelectFacRows = ListSelectFacRowsGuidParam.Split("**");

            var CadFacturas = "";
            var ListaFacLineas = new List<FacturasLIN>();

            foreach (var itemFac in ListSelectFacRows)
            {
                var GuidFac = new Guid(itemFac);
                var FindFac = ctxDB.FacturasCAB.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Guid == GuidFac).FirstOrDefault();
                if (FindFac != null)
                {
                    CadFacturas = CadFacturas + FindFac.Factura + "   ";

                    if (FiltroAlbaran == true)
                    {
                        var Lineas = ctxDB.FacturasLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Factura == FindFac.Factura && x.FacEstado == "").ToList();
                        ListaFacLineas.AddRange(Lineas);
                    }
                    else
                    {
                        var Lineas = ctxDB.FacturasLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Factura == FindFac.Factura).ToList();
                        ListaFacLineas.AddRange(Lineas);
                    }
                }
            }

            ViewBag.CadFacturas = CadFacturas;

            return PartialView("_DialogFacSelectLineas", ListaFacLineas);
        }

        public class ListFactCliTipos
        {
            public string Cliente       { get; set; }
            public string TipoVenta     { get; set; }
            public string Factura       { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> PedLinFacturaCrear(string ListSelectPedLineas)
        {

            var ArrPedLineas    = ListSelectPedLineas.Split(',');
            var KeyFact         = "";
            var Year            = DateTime.Now.ToString("yy");

            var ListFactCliTiposNew = new List<ListFactCliTipos>();
            var ListaPedidosAct     = new List<PedidosCAB>();

            try
            {
                // BUCLE DE LAS LINEAS DE PEDIDO A FACTURAR
                // Se busca a que clientes y tipoventa (etq,hechura, etc...)
                foreach (var GuidLinPedStr in ArrPedLineas)
                {
                    var GuidLinPed = new Guid(GuidLinPedStr);
                    var RowLinPed = ctxDB.PedidosLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Guid == GuidLinPed).FirstOrDefault();
                    if (RowLinPed != null)
                    {
                        // SE COMPRUEBA SI YA EXISTE ALGUNA FACTURA DE ESE TIPOVENTA EN EL TEMPORAL ListFactCliTiposNew, SI YA EXISTE LO AGREGARA EN ESA FACTURA SINO LO CREARA
                        var FindFactTipo = ListFactCliTiposNew.Where(x => x.Cliente == RowLinPed.Cliente && x.TipoVenta == RowLinPed.PedArtTipoVenta).FirstOrDefault();
                        if (FindFactTipo != null)
                        {
                            KeyFact = FindFactTipo.Factura;
                        }
                        else
                        {
                            var LastFact = ctxDB.FacturasCAB.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Factura).LastOrDefault();
                            if (LastFact != null)
                            {
                                var LastFactStrIni = LastFact.Factura.Substring(0, 3);
                                var LastFactStrNum = LastFact.Factura.Substring(4);
                                var LastFactNum = Convert.ToInt32(LastFactStrNum);

                                KeyFact = "XX" + Year + "/" + (LastFactNum + 1).ToString("D6");
                            }
                            else
                            {
                                KeyFact = "XX" + Year + "/" + "000001";
                            }

                            var KeyFactTmp       = new ListFactCliTipos();
                            KeyFactTmp.Cliente   = RowLinPed.Cliente;
                            KeyFactTmp.TipoVenta = RowLinPed.PedArtTipoVenta;
                            KeyFactTmp.Factura   = KeyFact;
                            ListFactCliTiposNew.Add(KeyFactTmp);

                            // CREAR FACTURA CAB
                            var NewFactCab          = new FacturasCAB();
                            NewFactCab.Empresa      = GrupoClaims.SessionEmpresa;
                            NewFactCab.IsoUser      = GrupoClaims.SessionUsuario;
                            NewFactCab.IsoFecAlt    = DateTime.Now;
                            NewFactCab.IsoFecMod    = DateTime.Now;

                            NewFactCab.Cliente      = RowLinPed.Cliente;
                            NewFactCab.Factura      = KeyFact;
                            NewFactCab.FacFecha     = DateTime.Now;
                            NewFactCab.FacTipoVenta = KeyFactTmp.TipoVenta;


                            var RowPedCab = ctxDB.PedidosCAB.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == RowLinPed.Cliente && x.Pedido == RowLinPed.Pedido).FirstOrDefault();
                            if (RowPedCab != null)
                            {
                                NewFactCab.FacRefCliente = RowPedCab.PedRefCliente;
                                NewFactCab.MultiEmpresa = RowPedCab.MultiEmpresa;
                                NewFactCab.FacTipo = RowPedCab.PedTipo;
                                NewFactCab.FacOroFino = RowPedCab.PedOroFino;
                                NewFactCab.FacVendedor = RowPedCab.PedVendedor;
                                NewFactCab.FacPrioridad = RowPedCab.PedPrioridad;

                                NewFactCab.FacIVA = RowPedCab.PedIVA;
                                NewFactCab.FacRE = RowPedCab.PedRE;
                                NewFactCab.FacIGIC = RowPedCab.PedIGIC;
                                NewFactCab.FacIRPF = RowPedCab.PedIRPF;

                                NewFactCab.FacTarifaVenta = RowPedCab.PedTarifaVenta;

                                NewFactCab.FacDatFPago = RowPedCab.PedDatFPago;
                                NewFactCab.FacDatFPint = RowPedCab.PedDatFPint;
                                NewFactCab.FacDatFPlazo = RowPedCab.PedDatFPlazo;
                                NewFactCab.FacDatFPvto = RowPedCab.PedDatFPvto;
                                NewFactCab.FacDatFPdia1 = RowPedCab.PedDatFPdia1;
                                NewFactCab.FacDatFPdia2 = RowPedCab.PedDatFPdia2;
                                NewFactCab.FacDatFPdia3 = RowPedCab.PedDatFPdia3;

                                NewFactCab.FacKIL = RowPedCab.PedKIL;
                                NewFactCab.FacCOL = RowPedCab.PedCOL;
                                NewFactCab.FacACB = RowPedCab.PedACB;
                                NewFactCab.FacBAN = RowPedCab.PedBAN;
                                NewFactCab.FacDivisa = RowPedCab.PedDivisa;

                                NewFactCab.FacIdioma = RowPedCab.PedIdioma;
                                NewFactCab.FacTipoImp = RowPedCab.PedTipoImp;

                                NewFactCab.FacDTOCial = RowPedCab.PedDTOCial;
                                NewFactCab.FacDTOPpago = RowPedCab.PedDTOPpago;
                                NewFactCab.FacDTORappel = RowPedCab.PedDTORappel;

                                NewFactCab.FacDirMerDireccion = RowPedCab.PedDirMerDireccion;
                                NewFactCab.FacDirMerDP = RowPedCab.PedDirMerDP;
                                NewFactCab.FacDirMerPoblacion = RowPedCab.PedDirMerPoblacion;
                                NewFactCab.FacDirMerPoblacion = RowPedCab.PedDirMerProvincia;
                                NewFactCab.FacDirMerPais = RowPedCab.PedDirMerPais;

                                NewFactCab.FacDirFacDireccion = RowPedCab.PedDirFacDireccion;
                                NewFactCab.FacDirFacDP = RowPedCab.PedDirFacDP;
                                NewFactCab.FacDirFacPoblacion = RowPedCab.PedDirFacPoblacion;
                                NewFactCab.FacDirFacPoblacion = RowPedCab.PedDirFacProvincia;
                                NewFactCab.FacDirFacPais = RowPedCab.PedDirFacPais;

                                NewFactCab.CliBcoBanco = RowPedCab.CliBcoBanco;
                                NewFactCab.CliBcoCtaCte1 = RowPedCab.CliBcoCtaCte1;
                                NewFactCab.CliBcoCtaCte2 = RowPedCab.CliBcoCtaCte2;
                                NewFactCab.CliBcoCtaCte3 = RowPedCab.CliBcoCtaCte3;
                                NewFactCab.CliBcoCtaCte4 = RowPedCab.CliBcoCtaCte4;
                                NewFactCab.CliBcoSwift = RowPedCab.CliBcoSwift;
                                NewFactCab.CliBcoIBAN = RowPedCab.CliBcoIBAN;

                                //NewAlbCab.AlbObserv     = RowPedCab.PedObserv;
                                //NewAlbCab.AlbObserv2    = RowPedCab.PedObserv2;
                            }


                            ctxDB.FacturasCAB.Add(NewFactCab);
                            await ctxDB.SaveChangesAsync();
                        }


                        var NewFacLin       = new FacturasLIN();
                        NewFacLin.Empresa   = GrupoClaims.SessionEmpresa;
                        NewFacLin.IsoUser   = GrupoClaims.SessionUsuario;
                        NewFacLin.IsoFecAlt = DateTime.Now;
                        NewFacLin.IsoFecMod = DateTime.Now;

                        NewFacLin.Cliente   = RowLinPed.Cliente;
                        NewFacLin.Factura   = KeyFact;

                        NewFacLin.Pedido    = RowLinPed.Pedido;
                        NewFacLin.Albaran   = "";
                        NewFacLin.FacLinea  = RowLinPed.PedLinea;
                        NewFacLin.FacArt    = RowLinPed.PedArt;
                        NewFacLin.FacDesc   = RowLinPed.PedDesc;
                        NewFacLin.FacArtTipoVenta = RowLinPed.PedArtTipoVenta;

                        NewFacLin.FacOro    = RowLinPed.PedOro;
                        NewFacLin.FacTarifa = RowLinPed.PedTarifa;
                        NewFacLin.FacKIL    = RowLinPed.PedKIL;
                        NewFacLin.FacCOL    = RowLinPed.PedCOL;
                        NewFacLin.FacACB    = RowLinPed.PedACB;
                        NewFacLin.FacBAN    = RowLinPed.PedBAN;

                        NewFacLin.FacQty            = RowLinPed.PedQty;
                        NewFacLin.FacPeso           = RowLinPed.PedPeso;
                        NewFacLin.FacMerma          = RowLinPed.PedMerma;
                        NewFacLin.FacPrecio         = RowLinPed.PedPrecio;
                        NewFacLin.FacDtoLin         = RowLinPed.PedDtoLin;
                        NewFacLin.FacPesoTotal      = RowLinPed.PedPesoTotal;
                        NewFacLin.FacPrecioTotal    = RowLinPed.PedPrecioTotal;

                        NewFacLin.FacObserv         = RowLinPed.PedObserv;
                        NewFacLin.FacAlmacen        = RowLinPed.PedAlmacen;
                        NewFacLin.FacMovTra         = RowLinPed.PedMovTra;
                        NewFacLin.FacMovSTK         = RowLinPed.PedMovSTK;
                        NewFacLin.FacMovSTKlin      = RowLinPed.PedMovSTKlin;

                        NewFacLin.FacEstado         = RowLinPed.PedEstado;
                        NewFacLin.FacQtyENT         = RowLinPed.PedQtyENT;
                        NewFacLin.FacFecEntrega     = RowLinPed.PedFecEntrega;
                        NewFacLin.FacLinComision    = RowLinPed.PedLinComision;
                        NewFacLin.FacMedidas        = RowLinPed.PedMedidas;
                        NewFacLin.FacImageSelect    = RowLinPed.PedImageSelect;

                        ctxDB.FacturasLIN.Add(NewFacLin);
                        await ctxDB.SaveChangesAsync();

                        // SE MODIFICA PEDESTADO A "E" : Entregado
                        RowLinPed.PedEstado = "E";
                        ctxDB.PedidosLIN.Update(RowLinPed);
                        await ctxDB.SaveChangesAsync();

                        // AGREGAMOS EL PEDIDOCAB A ListaPedidosAct
                        var FindPedCab = ctxDB.PedidosCAB.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == RowLinPed.Cliente && x.Pedido == RowLinPed.Pedido).FirstOrDefault();
                        if (FindPedCab != null)
                        {
                            var Find = ListaPedidosAct.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == RowLinPed.Cliente && x.Pedido == RowLinPed.Pedido).FirstOrDefault();
                            if (Find == null)
                            {
                                ListaPedidosAct.Add(FindPedCab);
                            }

                        }

                    }

                }

                // TOTALES Y REGISTROS DE LEY ANTIFRAUDE
                foreach (var itemFac in ListFactCliTiposNew)
                {
                    var FindFactura = ctxDB.FacturasCAB.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == itemFac.Cliente && x.Factura == itemFac.Factura).FirstOrDefault();
                    if (FindFactura != null)
                    {
                        FindFactura = Factura_Calculo_Totales(FindFactura);

                        ctxDB.FacturasCAB.Update(FindFactura);
                        await ctxDB.SaveChangesAsync();

                        var LA_RegNew = JsonConvert.SerializeObject(FindFactura);
                        var LA_Result = await FunctionsLeyAntiFraude.RegistrarAsync("FacEmi", "C", "", FindFactura.Factura, null, LA_RegNew);
                    }

                }

                // se deberia generar alguna funcion para automatizar este trozo....
                foreach (var itemPed in ListaPedidosAct)
                {
                    itemPed.ClienteNombre = ctxDB.Clientes.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == itemPed.Cliente).FirstOrDefault().CliNombre;
                    itemPed.VendedorNombre = ctxDB.Vendedores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Vendedor == itemPed.PedVendedor).FirstOrDefault().VenNombre;

                    itemPed.TotalPedBI = (decimal)ctxDB.PedidosLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == itemPed.Cliente && x.Pedido == itemPed.Pedido).Sum(X => X.PedPrecioTotal);
                    itemPed.TotalPedPendBI = (decimal)ctxDB.PedidosLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == itemPed.Cliente && x.Pedido == itemPed.Pedido && x.PedEstado == "").Sum(X => X.PedPrecioTotal);

                    itemPed.PedMRPQuery = ctxDB.MrpPlanesLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Pedido == itemPed.Pedido).GroupBy(x => x.PlanMrp).Select(s => s.Key).ToList();

                    itemPed.PedMRP = String.Join("-", itemPed.PedMRPQuery);

                    //var Totales = Pedido_Calculo_Totales(itemPed);
                    //itemPed.TotalDtoCial = (decimal)Totales.PedDTOCial;
                    //itemPed.TotalDtoPpago = (decimal)Totales.TotalDtoPpago;
                    //itemPed.TotalDtoRappel = (decimal)Totales.TotalDtoRappel;
                    //itemPed.TotalPedDTOs = (decimal)Totales.TotalPedDTOs;
                    //itemPed.TotalPedIVA = (decimal)Totales.TotalPedIVA;
                    //itemPed.TotalPed = (decimal)Totales.TotalPed;
                    //itemPed.TotalPedPend = (decimal)Totales.TotalPedPend;

                    // itemPed.Albaranes = string.Join(",", ctxDB.AlbaranesLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == itemPed.Cliente && x.Pedido == itemPed.Pedido).Select(a => a.Albaran).Distinct().ToList());

                    if (itemPed.Albaranes != "")
                    {
                        var xxx = "";
                    }

                    // SE COMPRUEBA SI ESTA TODO ENTREGADO PARA ACTUALIZAR PEDESTADO DE LA CABCERA
                    if (itemPed.TotalPedPendBI == 0)
                    {
                        itemPed.PedEstado = "F";
                        ctxDB.PedidosCAB.Update(itemPed);
                        await ctxDB.SaveChangesAsync();
                    }
                }

                return StatusCode(200, ListaPedidosAct);


            }
            catch (Exception e)
            {
                var errorMes = e.Message;
                var errorInner = e.InnerException;

                return StatusCode(200, null);
            }

        }



        /********************************************************************************************************************************/
        /* PROCESOS FACTURAE - VISUALIZAR PDF -  ENVIAR FACTURA                                                                         */
        /********************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> EnviarFactura(Guid Guid, int Tipo)
        {
            JsonResult resultado = Json(new { status = "", result = "" });

            resultado = await FuncEnviarFactura(Guid, Tipo);

            return resultado;
        }


        [HttpPost]
        public async Task<IActionResult> FirmarFacturaE(Guid Guid, int Tipo)
        {
            JsonResult resultado = Json(new { status = "", result = "" });

            // FIRMAR
            if (Tipo == 1)
            {
                resultado = await FuncFirmarFacturaE(Guid);
            }

            // FIRMAR Y ENVIAR
            if (Tipo == 2)
            {
                resultado = await FuncFirmarFacturaE(Guid);
                resultado = await FuncEnviarFactura(Guid, Tipo);
            }

             return resultado;

        }


        private async Task<JsonResult> FuncEnviarFactura(Guid Guid, int Tipo)
        {
            var Factura = ctxDB.FacturasCAB.Where(x => x.Guid == Guid).FirstOrDefault();
            if (Factura != null)
            {
                try
                {
                    // ENVIAR
                    if (Tipo == 1)
                    {
                        Factura.FacEnviada = DateTime.Now;

                        ctxDB.FacturasCAB.Update(Factura);
                        await ctxDB.SaveChangesAsync();
                    }

                    // FIRMAR Y ENVIAR
                    if (Tipo == 2)
                    {
                        var Procesar = true;

                        var resultado = await FuncFirmarFacturaE(Guid);

                        var jsonResult = resultado as JsonResult;
                        if (jsonResult != null)
                        {
                            var resultValue = jsonResult.Value;

                            var status = resultValue.GetType().GetProperty("status")?.GetValue(resultValue)?.ToString();
                            var mensaje = resultValue.GetType().GetProperty("result")?.GetValue(resultValue)?.ToString();

                            if (status == "error")
                            {
                                Procesar = false;

                                return Json(new { status = "error", result = mensaje });
                            }
                        }

                        if (Procesar == true)
                        {
                            Factura.FacEnviada = DateTime.Now;
                            Factura.FacFirmada = DateTime.Now;

                            ctxDB.FacturasCAB.Update(Factura);
                            await ctxDB.SaveChangesAsync();
                        }


                    }




                }
                catch (Exception e)
                {
                    return Json(new { status = "error", result = e.Message });
                }

                Factura.ClienteNombre = ctxDB.Clientes.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == Factura.Cliente).FirstOrDefault()?.CliNombre;
                Factura.VendedorNombre = ctxDB.Vendedores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Vendedor == Factura.FacVendedor).FirstOrDefault()?.VenNombre;
                Factura.TotalFacBI = (decimal)ctxDB.FacturasLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == Factura.Cliente && x.Factura == Factura.Factura).Sum(X => X.FacPrecioTotal);
                Factura.TotalFacPendBI = (decimal)ctxDB.FacturasLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == Factura.Cliente && x.Factura == Factura.Factura && x.FacEstado == "").Sum(X => X.FacPrecioTotal);
                var Totales = Factura_Calculo_Totales(Factura);
                Factura.TotalFac = (decimal)Totales.TotalFac;

                return Json(new { status = "ok", result = Factura });

            }

            return Json(new { status = "error", result = "Factura no existe: " + Guid.ToString() });
        }

        private async Task<JsonResult> FuncFirmarFacturaE(Guid Guid)
        {
            var Procesar    = true;
            var Error       = "";

            var Factura = ctxDB.FacturasCAB.Where(x => x.Guid == Guid).FirstOrDefault();
            if (Factura != null)
            {

                try
                {
                    var ListFacLin = ctxDB.FacturasLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == Factura.Cliente && x.Factura == Factura.Factura).ToList();
                    if (ListFacLin.Count() == 0)
                    {
                        Procesar = false;
                        Error = "OBLIGATORIO, La factura no dispone de lineas.";
                    }

                    // SE COMPRUEBA SI SE TIENE CERTIFICADO PARA FIRMAR LA FACTURA
                    var FactEmisor          = new Empresas();
                    var FactReceptor        = new Clientes();
                    var CertificateObjX509  = new X509Certificate2();

                    var ParentPathFic   = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();
                    var dirCertificados = Path.Combine(ParentPathFic, "Nemesis365Docs", GrupoClaims.SessionEmpresa, "Certificados");

                    var FindCertificado = ctxDB.EmpresasCertificados.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).FirstOrDefault();
                    if (FindCertificado != null)
                    {

                        var pathCertificado = Path.Combine(dirCertificados, FindCertificado.Fichero);

                        // DESENCRIPTAMOS LA PASS DE LA BBDD QUE ESTA EN AES (BYTES)
                        var PasswordDecrypt = FunctionsCrypto.DecryptAES(FindCertificado.Password);

                        CertificateObjX509 = new X509Certificate2(pathCertificado, PasswordDecrypt);


                        FactEmisor = ctxDB.Empresas.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).FirstOrDefault();
                        if (FactEmisor == null)
                        {
                            Procesar = false;
                            Error = "No se localiza datos de la empresa emisora.";
                        }

                        FactReceptor = ctxDB.Clientes.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == Factura.Cliente).FirstOrDefault();
                        if (FactEmisor == null)
                        {
                            Procesar = false;
                            Error = "No se localiza datos del receptor (cliente).";
                        }

                    }
                    else
                    {
                        Procesar = false;
                        Error = "Certificado no existe, debe subir un certificado valido en configuraciones.";                        
                    }




                    if (Procesar == true)
                    {
                        var eInvoice = new Facturae();

                        var isValid = eInvoice

                            .Seller()
                                //.SetIdentification("00001")
                                .AsResidentInSpain()
                                .SetIdentificationNumber(FactEmisor.Nif)
                                .AsLegalEntity()
                                    .SetCorporateName(FactEmisor.RazonSocial)
                                    .SetAddress(FactEmisor.Direccion)
                                    .SetProvince(FactEmisor.Provincia)
                                    .SetTown(FactEmisor.Poblacion)
                                    .SetPostCode(FactEmisor.CodigoPostal.Trim())
                                    .SetCountryCode(CountryType.ESP)
                                    .Party()
                                .Invoice()

                            .Buyer()
                                //.SetIdentification("00002")
                                .AsResidentInSpain()
                                .SetIdentificationNumber(FactReceptor.CliNIF)
                                .AsLegalEntity()
                                    .SetCorporateName(FactReceptor.CliRazon)
                                    .SetAddress(FactReceptor.CliDirFacDireccion)
                                    .SetProvince(FactReceptor.CliDirFacProvincia)
                                    .SetTown(FactReceptor.CliDirFacPoblacion)
                                    .SetPostCode(FactReceptor.CliDirFacDP.Trim())
                                    .SetCountryCode(CountryType.ESP)
                                .Party()
                            .Invoice()

                            .CreateInvoice()
                                .SetCurrency(CurrencyCodeType.EUR)
                                //.SetExchangeRate(1, DateTime.Now)
                                .SetTaxCurrency(CurrencyCodeType.EUR)
                                .SetLanguage(LanguageCodeType.es)
                                //.SetPlaceOfIssue(string.Empty, "00000")

                                .IsOriginal()
                                .IsComplete()
                                .SetInvoiceSeries("R24")
                                .SetInvoiceNumber("00002")
                                .SetIssueDate((DateTime)Factura.FacFecha)

                                //necesario tener 1 al menos
                                .AddInvoiceItem("INI", "INI")
                                    .GiveQuantity(1.0)
                                    .GiveUnitPriceWithoutTax(0.00)
                                    .GiveVATRate(21.00)
                                    .GiveTaxRate(0.00, TaxTypeCodeType.PersonalIncomeTax)
                                    //.GiveDiscount(10.01, "Line Discount")
                                    //.GiveTaxRate(0.00, TaxTypeCodeType.PersonalIncomeTax)
                                    .CalculateTotals()

                                .CalculateTotals()

                            .CalculateTotals()

                            .Validate()
                            ;



                        eInvoice.Invoices[0].Items.Clear();

                        foreach (var FacLin in ListFacLin)
                        {
                            var qty = FacLin.FacQty ?? 0;
                            decimal precio = FacLin.FacPrecio ?? 0;
                            decimal totalRedondeado = Math.Round(qty * precio, 2, MidpointRounding.AwayFromZero);

                            eInvoice.Invoices[0].AddInvoiceItem(FacLin.FacArt, FacLin.FacDesc)
                                                    .GiveQuantity((double)qty)
                                                    .GiveUnitPriceWithoutTax((double)precio)
                                                    .GiveVATRate((double)Factura.FacIVA)
                                                    .GiveTaxRate(0.00, TaxTypeCodeType.PersonalIncomeTax)
                                                    .CalculateTotals()
                                                    ;
                        }

                        eInvoice.Invoices[0].CalculateTotals();
                        eInvoice.CalculateTotals();


                        var Sign = eInvoice.Signature;


                        var PathDocEmp = Path.Combine(ParentPathFic, "Nemesis365Docs", GrupoClaims.SessionEmpresa);
                        if (!Directory.Exists(PathDocEmp))
                        {
                            Directory.CreateDirectory(PathDocEmp);
                        }

                        var dirFacturaE = Path.Combine(ParentPathFic, "Nemesis365Docs", GrupoClaims.SessionEmpresa, "FacturaE");
                        if (!Directory.Exists(dirFacturaE))
                        {
                            Directory.CreateDirectory(dirFacturaE);
                        }

                        string FacFechaFormat = "";

                        if (Factura.FacFecha.HasValue)
                        {
                            FacFechaFormat = Factura.FacFecha.Value.ToString("yyyyMMdd");
                        }

                        string invalidCharsPattern = "[" + Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars())) + "]";

                        string FactReg = Regex.Replace(Factura.Factura, invalidCharsPattern, "");

                        var pathFacturaE = Path.Combine(dirFacturaE, FactReg + "-FacturaE-" + FacFechaFormat + ".xsig");


                        eInvoice.Sign(CertificateObjX509).WriteToFile(pathFacturaE).CheckSignature();


                        Factura.FacFirmada          = DateTime.Now;
                        Factura.FacFicheroFirmado   = FactReg + "-FacturaE-" + FacFechaFormat;

                        ctxDB.FacturasCAB.Update(Factura);
                        await ctxDB.SaveChangesAsync();
                        
                    }
                    else
                    {
                        return Json(new { status = "error", result = Error });
                    }

                }
                catch (Exception e)
                {
                    return Json(new { status = "error", result = e.Message });
                }

                Factura.ClienteNombre   = ctxDB.Clientes.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == Factura.Cliente).FirstOrDefault()?.CliNombre;
                Factura.VendedorNombre  = ctxDB.Vendedores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Vendedor == Factura.FacVendedor).FirstOrDefault()?.VenNombre;
                Factura.TotalFacBI      = (decimal)ctxDB.FacturasLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == Factura.Cliente && x.Factura == Factura.Factura).Sum(X => X.FacPrecioTotal);
                Factura.TotalFacPendBI  = (decimal)ctxDB.FacturasLIN.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == Factura.Cliente && x.Factura == Factura.Factura && x.FacEstado == "").Sum(X => X.FacPrecioTotal);
                var Totales             = Factura_Calculo_Totales(Factura);
                Factura.TotalFac        = (decimal)Totales.TotalFac;

                return Json(new { status = "ok", result = Factura });

            }

            return Json(new { status = "error", result = "Factura no existe: " + Guid.ToString() });
        }


        public async Task<IActionResult> DescargarFacturaE(Guid Guid)
        {
            var pathFacturaE = "";

            var Factura = ctxDB.FacturasCAB.Where(x => x.Guid == Guid).FirstOrDefault();
            if (Factura != null)
            {
                var ParentPathFic = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();

                var PathDocEmp = Path.Combine(ParentPathFic, "Nemesis365Docs", GrupoClaims.SessionEmpresa);
                if (!Directory.Exists(PathDocEmp))
                {
                    Directory.CreateDirectory(PathDocEmp);
                }
                var dirFacturaE = Path.Combine(ParentPathFic, "Nemesis365Docs", GrupoClaims.SessionEmpresa, "FacturaE");
                if (!Directory.Exists(dirFacturaE))
                {
                    Directory.CreateDirectory(dirFacturaE);
                }

                string FacFechaFormat = "";

                if (Factura.FacFecha.HasValue)
                {
                    FacFechaFormat = Factura.FacFecha.Value.ToString("yyyyMMdd");
                }

                string invalidCharsPattern = "[" + Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars())) + "]";

                string FactReg = Regex.Replace(Factura.Factura, invalidCharsPattern, "");

                pathFacturaE = Path.Combine(dirFacturaE, FactReg + "-FacturaE-" + FacFechaFormat + ".xsig");

                if (!System.IO.File.Exists(pathFacturaE))
                {
                    return NotFound(); // Manejar el caso donde el archivo no existe
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(pathFacturaE);

                var response = new
                {
                    XsigfileBlob = Convert.ToBase64String(fileBytes),
                    XsigfileName = FactReg + "-FacturaE-" + FacFechaFormat + ".xsig"
                };

                return Json(response);

                //return File(fileBytes, "application/octet-stream", pathFacturaE);

            }

            return NotFound();

        }




    }

}