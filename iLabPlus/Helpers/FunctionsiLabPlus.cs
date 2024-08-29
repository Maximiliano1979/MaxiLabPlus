using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iLabPlus.Helpers
{
    [Authorize]
    public class FunctionsiLabPlus : Controller
    {

        private readonly DbContextiLabPlus ctxDB;

        private readonly ClaimsIdentity ClaimsIdentity;
        private readonly string SessionEmpresa;
        private readonly string SessionEmpresaNombre;
        private readonly string SessionUsuario;
        private readonly string SessionUsuarioNombre;

        public FunctionsiLabPlus(DbContextiLabPlus Context, IHttpContextAccessor ContextAccessor)
        {
            ctxDB = Context;

            ClaimsIdentity = ContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if (ClaimsIdentity.Claims.Count() > 0)
            {
                if (ClaimsIdentity.Claims.Where(x => x.Type == "Empresa").FirstOrDefault() != null)
                {
                    SessionEmpresa = ClaimsIdentity.FindFirst("Empresa").Value;
                }
                if (ClaimsIdentity.Claims.Where(x => x.Type == "EmpresaNombre").FirstOrDefault() != null)
                {
                    SessionEmpresaNombre = ClaimsIdentity.FindFirst("EmpresaNombre")?.Value;
                }
                if (ClaimsIdentity.Claims.Where(x => x.Type == "Usuario").FirstOrDefault() != null)
                {
                    SessionUsuario = ClaimsIdentity.FindFirst("Usuario").Value;
                }
                if (ClaimsIdentity.Claims.Where(x => x.Type == "UsuarioNombre").FirstOrDefault() != null)
                {
                    SessionUsuarioNombre = ClaimsIdentity.FindFirst("UsuarioNombre").Value;
                }
            }

        }


        public static  List<PrecioEstructura> GetPreciosEstructura()
        {
            var CosteTotalItems = new List<PrecioEstructura>();
            CosteTotalItems.Add(new PrecioEstructura
            {
                Id      = "Metales",
                Header  = "Metales <span style='position: absolute;right: 45px;font-weight:bold '>0,000€</span>",
                Items   = new List<PrecioEstructura>{
                    new PrecioEstructura{Id="Oro",              Header = "Oro <span style='position: absolute;right: 95px;'>0,000€</span>"},
                    new PrecioEstructura{Id="Oro Componentes",  Header = "Oro Componentes <span style='position: absolute;right: 95px;'>0,000€</span>"},
                }
            });

            CosteTotalItems.Add(new PrecioEstructura
            {
                Id = "Materiales",
                Header = "Materiales <span style='position: absolute;right: 45px;font-weight:bold '>0,000€</span>",
                Items = new List<PrecioEstructura>{
                    new PrecioEstructura{Id="Piedras",          Header = "Piedras <span style='position: absolute;right: 95px;'>0,000€</span>"},
                    new PrecioEstructura{Id="Componentes",      Header = "Componentes <span style='position: absolute;right: 95px;'>0,000€</span>"}
                }
            });

            CosteTotalItems.Add(new PrecioEstructura
            {
                Id      = "Mano de obra",
                Header  = "Mano de obra <span style='position: absolute;right: 45px;font-weight:bold '>0,000€</span>",
                Items   = new List<PrecioEstructura>{
                    new PrecioEstructura{Id="Interna",          Header = "Interna <span style='position: absolute;right: 95px;'>0,000€</span>"},
                    new PrecioEstructura{Id="Externa",          Header = "Externa <span style='position: absolute;right: 95px;'>0,000€</span>"},
                    new PrecioEstructura{Id="MO Componentes",   Header = "MO Componentes <span style='position: absolute;right: 95px;'>0,000€</span>"}
                }
            });
            CosteTotalItems.Add(new PrecioEstructura
            {
                Id      = "Fabricacion",
                Header  = "Fabricación <span style='position: absolute;right: 45px;font-weight:bold '>0,000€</span>",
                Items   = new List<PrecioEstructura>{
                    new PrecioEstructura{Id="Liga de oro",      Header = "Liga de oro <span style='position: absolute;right: 95px;'>0,000€</span>"},
                    new PrecioEstructura{Id="Rodio",            Header = "Rodio <span style='position: absolute;right: 95px;'>0,000€</span>"},
                    //new PrecioEstructura{Id="Etiquetado",       Header = "Etiquetado <span style='position: absolute;right: 95px;'>0,000€</span>"},
                    //new PrecioEstructura{Id="Garantia",         Header = "Garantia <span style='position: absolute;right: 95px;'>0,000€</span>"}
                }
            });


            var PrecioEstructura = new List<PrecioEstructura>();
            PrecioEstructura.Add(new PrecioEstructura
            {
                Id = "Beneficio",
                Header = "Beneficio <span style='position: absolute;right: 25px;font-weight:bold;font-size: 14px !important; '>0,000€</span>",
                Items = null
            });
            PrecioEstructura.Add(new PrecioEstructura
            {
                Id      = "Coste Total",
                Header  = "Coste Total <span style='position: absolute;right: 25px;font-weight:bold;font-size: 14px !important; '>0,000€</span>",
                Items   = CosteTotalItems
            });



            return PrecioEstructura;
        }



        public string ExtractDecimalFromText(string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                throw new ArgumentException("La cadena de texto es nula o vacía");
            }

            // Define el patrón de expresión regular para encontrar el primer decimal
            string pattern = @"(\d+\.\d+|\d+,\d+)";

            // Busca coincidencias en la cadena de texto
            Match match = Regex.Match(inputText, pattern);

            // Verifica si se encontró una coincidencia
            if (match.Success)
            {
                // Obtiene el valor del primer decimal encontrado
                return match.Value;
            }
            else
            {
                return "";
                //throw new ArgumentException("No se encontró ningún decimal en la cadena de texto");
            }
        }



        public ArticuloEstructuraPrecio GetArticuloEstructuraPrecios(
                                                        string Articulo, int SelectKilates, string SelectTipoVenta, string SelectTarifaVenta, decimal SelectOroFino, string SelectTipoOro,
                                                        List<ArticulosCOMP> EstructuraCOMP_Mod, List<ArticulosCOMP> EstructuraPIE_Mod
                                                        )
        {
            string SessionEmpresa       ="";
            string SessionUsuario       = "";
            string SessionUsuarioNombre = "";

            if (ClaimsIdentity.Claims.Count() > 0)
            {
                if (ClaimsIdentity.Claims.Where(x => x.Type == "Empresa").FirstOrDefault() != null)
                {
                    SessionEmpresa = ClaimsIdentity.FindFirst("Empresa").Value;
                }
                if (ClaimsIdentity.Claims.Where(x => x.Type == "Usuario").FirstOrDefault() != null)
                {
                    SessionUsuario = ClaimsIdentity.FindFirst("Usuario").Value;
                }
                if (ClaimsIdentity.Claims.Where(x => x.Type == "UsuarioNombre").FirstOrDefault() != null)
                {
                    SessionUsuarioNombre = ClaimsIdentity.FindFirst("UsuarioNombre").Value;
                }
            }

            var CalcDecimales           = 3;

            var TreeEstructuraPrecio    = FunctionsiLabPlus.GetPreciosEstructura();

            var ArticuloPrecio          = new ArticuloEstructuraPrecio();
            var EstFichaArtCostes       = new EstFichaArtCostes();
            ArticuloPrecio.EstCOSTES    = EstFichaArtCostes;

            ArticuloPrecio.Articulo             = Articulo;
            ArticuloPrecio.EstCOSTES.TipoVenta  = SelectTipoVenta;
            ArticuloPrecio.EstCOSTES.Kilates    = SelectKilates;
                        
            
            decimal _Beneficio              = 0M;
            decimal _CosteTotal             = 0M;
            decimal _CosteTotalEtiqueta     = 0M;
            decimal _CosteTotalHechura      = 0M;

            decimal _CosteMetalesTot        = 0M;
            decimal _CosteMatOro            = 0M;
            decimal _CosteMatOroComp        = 0M;

            decimal _CosteMaterialesTot     = 0M;
            decimal _CosteMatPie            = 0M;
            decimal _CosteMatComp           = 0M;

            decimal _CosteMOTot             = 0M;
            decimal _CosteMOInt             = 0M;
            decimal _CosteMOExt             = 0M;
            decimal _CosteMOComp            = 0M;

            decimal _CosteFAB           = 0M;
            decimal _CosteFABLiga       = 0M;
            decimal _LigaPeso           = 0M;
            decimal _CosteFABRodio      = 0M;
            decimal _CosteFABEtq        = 0M;
            decimal _CosteFABGar        = 0M;

            decimal _PesoTotal          = 0M;
            decimal _PesoTotalSinMerma  = 0M;

            decimal _PesoMetTotal           = 0M;
            decimal _PesoMetTotalSinMerma   = 0M;
            decimal _PesoCompTotal          = 0M;
            decimal _PesoCompTotalSinMerma  = 0M;


            List<ArticulosCOMP>         _EstPIE;
            List<ArticulosCOMP>         _EstCOMP;
            List<ArticulosMO>           _EstMO;

            /****************************************************************************************************/
            /* CALCULO COSTES ORO                                                                           */
            /****************************************************************************************************/
            var Coeficiente     = 0M;
            var OroLey          = 0M;
            var OroFino         = 0M;
            var CoeChgPesoKil   = 1m;

            var FindFixing = ctxDB.Fixing.Where(x => x.Empresa == SessionEmpresa && x.Metal == "ORO").OrderBy(x => x.Fecha).LastOrDefault();
            if (FindFixing != null)
            {
                OroFino = FindFixing.OroFinoVenta;

                switch (SelectKilates)
                {
                    case 24:
                        CoeChgPesoKil = 1.5m; // revisar
                        Coeficiente = Math.Round(1000 / FindFixing.K24Milesimas, 3);
                        //OroLey = Math.Round(SelectOroFino / Coeficiente, 3);
                        OroLey = FindFixing.K24Ley;
                        
                        break;
                    case 19:
                        CoeChgPesoKil = 1.1m;
                        Coeficiente = Math.Round(1000 / FindFixing.K19Milesimas, 3);
                        //OroLey = Math.Round(SelectOroFino / Coeficiente, 3);
                        OroLey = FindFixing.K19Ley;
                        break;
                    case 18:
                        CoeChgPesoKil = 1m;
                        Coeficiente = Math.Round(1000 / FindFixing.K18Milesimas, 3);
                        //OroLey = Math.Round(SelectOroFino / Coeficiente, 3);
                        OroLey = FindFixing.K18Ley;
                        break;
                    case 14:
                        CoeChgPesoKil = 0.85m;
                        Coeficiente = Math.Round(1000 / FindFixing.K14Milesimas, 3);
                        //OroLey = Math.Round(SelectOroFino / Coeficiente, 3);
                        OroLey = FindFixing.K14Ley;
                        break;
                    case 10:
                        CoeChgPesoKil = 0.78m;
                        Coeficiente = Math.Round(1000 / FindFixing.K10Milesimas, 3);
                        //OroLey = Math.Round(SelectOroFino / Coeficiente, 3);
                        OroLey = FindFixing.K10Ley;
                        break;
                    case 9:
                        CoeChgPesoKil = 0.75m;
                        Coeficiente = Math.Round(1000 / FindFixing.K9Milesimas, 3);
                        //OroLey = Math.Round(SelectOroFino / Coeficiente, 3);
                        OroLey = FindFixing.K9Ley;
                        break;
                    case 8:
                        CoeChgPesoKil = 0.72m;
                        Coeficiente = Math.Round(1000 / FindFixing.K8Milesimas, 3);
                        //OroLey = Math.Round(SelectOroFino / Coeficiente, 3);
                        OroLey = FindFixing.K8Ley;
                        break;
                }
            }



            /****************************************************************************************************/
            /* CALCULO COSTES ORO COMP                                                                      */
            /****************************************************************************************************/
            var ListArtCOMP = new List<ArticulosCOMP>();
            if (EstructuraCOMP_Mod == null)
            {
                ListArtCOMP = ctxDB.ArticulosCOMP.Where(x => x.Empresa == SessionEmpresa && x.Articulo == Articulo && x.Tipo == "C").ToList();

            }
            else
            {
                ListArtCOMP = EstructuraCOMP_Mod;
            }
            
            var PesoComponentes         = 0m;
            var PesoComponentesSinMerma = 0m;
            var PesoComponentesPrecio   = 0m;
            foreach (var ArtComp in ListArtCOMP)
            {
                var PesoComp            = 0m;
                var PesoCompSinMerma    = 0m;
                if (PesoComp != 0)
                {
                    PesoComponentes         = PesoComponentes           + (PesoComp         * ArtComp.Cantidad);
                    PesoComponentesSinMerma = PesoComponentesSinMerma   + (PesoCompSinMerma * ArtComp.Cantidad);
                }

                _PesoTotal          = _PesoTotal            + ((PesoComponentes         ));
                _PesoTotalSinMerma  = _PesoTotalSinMerma    + ((PesoComponentesSinMerma ));

                _PesoCompTotal          = _PesoCompTotal + ((PesoComponentes));
                _PesoCompTotalSinMerma  = _PesoCompTotalSinMerma + ((PesoComponentesSinMerma));
            }
            PesoComponentesPrecio   = PesoComponentes * OroLey;
            _CosteMatOroComp        = PesoComponentesPrecio;

            /****************************************************************************************************/
            /* CALCULO COSTES MAT COMPONENTES, Y ALTERNATIVAS                                                   */
            /****************************************************************************************************/
            var ListArticuloCOMP                = new List<ArticulosCOMP>();

            if (EstructuraCOMP_Mod == null )
            {
                var Componentes = ctxDB.ArticulosCOMP.Where(x => x.Empresa == SessionEmpresa && x.Articulo == Articulo && x.Tipo == "C")
                    .Select(c => new ArticulosCOMP
                    {
                        Guid        = c.Guid,
                        Empresa     = c.Empresa,
                        Articulo    = c.Articulo,
                        Componente  = c.Componente,
                        Secuencia   = c.Secuencia,
                        TipoPrecio  = c.TipoPrecio,
                        Cantidad    = c.Cantidad,
                        Descripcion = ctxDB.Articulos.Where(x => x.Empresa == SessionEmpresa && x.Articulo == c.Componente).FirstOrDefault().ArtDes.ToUpper(),
                        IsoUser     = c.IsoUser,
                        IsoFecAlt   = c.IsoFecAlt,
                        IsoFecMod   = c.IsoFecMod,
                    }).OrderBy(x => x.Secuencia).ToList();

                ListArticuloCOMP.AddRange(Componentes);
            }
            else
            {
                ListArticuloCOMP = EstructuraCOMP_Mod;
            }

            foreach (var itemCOMP in ListArticuloCOMP)
            {
                if (itemCOMP.TipoPrecio == "C")
                {
                    itemCOMP.Precio = (decimal)ctxDB.Articulos.Where(x => x.Empresa == SessionEmpresa && x.Articulo == itemCOMP.Componente).FirstOrDefault().ArtPrecioCoste;
                }
                else
                {
                    itemCOMP.Precio = (decimal)ctxDB.Articulos.Where(x => x.Empresa == SessionEmpresa && x.Articulo == itemCOMP.Componente).FirstOrDefault().ArtPrecioPVP;
                }

                itemCOMP.Valor = itemCOMP.Cantidad * itemCOMP.Precio;
                _CosteMatComp = _CosteMatComp + (decimal)itemCOMP.Valor;


                // Peso componentes
                var PesoComp            = 0m;
                var PesoCompSinMerma    = 0m;
                if (PesoComp != 0)
                {
                    itemCOMP.Peso = PesoComp * itemCOMP.Cantidad;
                    itemCOMP.PesoSinMerma = PesoCompSinMerma * itemCOMP.Cantidad;
                }

                // BUSQUEDA DE ALTERNATIVAS DEL COMPONENTE
            }

            _EstCOMP                        = ListArticuloCOMP;

            /****************************************************************************************************/
            /* CALCULO COSTES MAT PIEDRAS                                */
            /****************************************************************************************************/
            var ListArticuloPIE = new List<ArticulosCOMP>();
            ////var ProcesarPieStd = true;

            if (EstructuraPIE_Mod == null)
            {
                var Piedras = ctxDB.ArticulosCOMP.Where(x => x.Empresa == SessionEmpresa && x.Articulo == Articulo && x.Tipo == "P")
                    .Select(c => new ArticulosCOMP
                    {
                        Guid = c.Guid,
                        Empresa = c.Empresa,
                        Articulo = c.Articulo,
                        Componente = c.Componente,
                        Secuencia = c.Secuencia,
                        TipoPrecio = c.TipoPrecio,
                        Cantidad = c.Cantidad,
                        Descripcion = ctxDB.Articulos.Where(x => x.Empresa == SessionEmpresa && x.Articulo == c.Componente).FirstOrDefault().ArtDes.ToUpper(),
                        IsoUser = c.IsoUser,
                        IsoFecAlt = c.IsoFecAlt,
                        IsoFecMod = c.IsoFecMod,
                    }).OrderBy(x => x.Secuencia).ToList();

                ListArticuloPIE.AddRange(Piedras);
            }
            else
            {
                ListArticuloPIE = EstructuraPIE_Mod;
            }

            foreach (var itemPIE in ListArticuloPIE)
            {
                if (itemPIE.TipoPrecio == "C")
                {
                    itemPIE.Precio = (decimal)ctxDB.Articulos.Where(x => x.Empresa == SessionEmpresa && x.Articulo == itemPIE.Componente).FirstOrDefault().ArtPrecioCoste;
                }
                else
                {
                    itemPIE.Precio = (decimal)ctxDB.Articulos.Where(x => x.Empresa == SessionEmpresa && x.Articulo == itemPIE.Componente).FirstOrDefault().ArtPrecioPVP;
                }

                itemPIE.Valor = itemPIE.Cantidad * itemPIE.Precio;
                _CosteMatPie = _CosteMatPie + (decimal)itemPIE.Valor;



                var CompoDecimalStr = ExtractDecimalFromText(itemPIE.Componente);
                if (CompoDecimalStr != "")
                {
                    var FindArtPieMedidas = ctxDB.Articulos.Where(x => x.Empresa == SessionEmpresa && x.Articulo.Contains(CompoDecimalStr)).ToList();
                
                }


            }

            _EstPIE                         = ListArticuloPIE;



            /****************************************************************************************************/
            /* CALCULO COSTES MO INT/EXT                                                                        */
            /****************************************************************************************************/
            var ListArticuloMO = ctxDB.ArticulosMO.Where(x => x.Empresa == SessionEmpresa && x.Articulo == Articulo).OrderBy(x => x.MOSecuencia).ToList();
            foreach (var itemMO in ListArticuloMO)
            {
                if (itemMO.MOUniMed == "G")
                {
                    decimal PesoART = 0;
                    //var SumPes = ctxDB.ArticulosMET.Where(x => x.Empresa == SessionEmpresa && x.Articulo == Articulo).Sum(x => x.Peso);
                    //PesoART = SumPes;

                    PesoART             = _PesoTotalSinMerma;

                    itemMO.MOCantidad   = PesoART;
                    itemMO.Valor        = PesoART * itemMO.MOPrecio;
                }
                else
                {
                    itemMO.Valor = itemMO.MOCantidad * itemMO.MOPrecio;
                }

                if (itemMO.MOExtInt == "I" || itemMO.MOExtInt == "" || itemMO.MOExtInt == null)
                {
                    _CosteMOInt = _CosteMOInt + (decimal)itemMO.Valor;
                }
                if (itemMO.MOExtInt == "E")
                {
                    _CosteMOExt = _CosteMOExt + (decimal)itemMO.Valor;
                }
            }

            _EstMO = ListArticuloMO;

            /****************************************************************************************************/
            /* CALCULO COSTES MO COMPONENTES                                                                    */
            /****************************************************************************************************/


            /****************************************************************************************************/
            /* CALCULO FAB LIGA                                                                                 */
            /****************************************************************************************************/ 
            //var LigaPeso    = _PesoTotalSinMerma * 0.33M;
            var LigaPeso = _PesoMetTotalSinMerma * 0.33M;
            var LigaPrecio  = 0M;

            var FindArticulo = ctxDB.Articulos.Where(x => x.Empresa == SessionEmpresa && x.Articulo == Articulo).FirstOrDefault();
            if (FindArticulo != null)
            {
                ArticuloPrecio.FichaArticulo = FindArticulo;

                //////if (FindArticulo.ArtLigaPrecio != 0 && FindArticulo.ArtLigaPrecio != null)
                //////{
                //////    LigaPrecio = (decimal)FindArticulo.ArtLigaPrecio;
                //////}
                //////else
                //////{
                //////    // Buscar precio de liga estandar
                //////    var MetalVal = "";
                //////    if (SelectTipoOro == "AM")
                //////    {
                //////        if (FindArticulo.ArtTipoLigaAM != null)
                //////        {
                //////            MetalVal = FindArticulo.ArtTipoLigaAM;
                //////        }                        
                //////    }
                //////    if (SelectTipoOro == "BL")
                //////    {
                //////        MetalVal = FindArticulo.ArtTipoLigaBL;
                //////    }
                //////    if (SelectTipoOro == "RS")
                //////    {
                //////        MetalVal = FindArticulo.ArtTipoLigaROSA;
                //////    }
                //////    if (SelectTipoOro != "AM" && SelectTipoOro != "BL" && SelectTipoOro != "RS")
                //////    {
                //////        MetalVal = FindArticulo.ArtTipoLigaBICOLOR;
                //////    }

                //////    //Sino tiene valor asignado en la ficha de articulo buscamos uno por defecto de AM LIGA
                //////    if (MetalVal == "" || MetalVal == null)
                //////    {
                //////        if (SelectTipoOro != "AM" && SelectTipoOro != "BL" && SelectTipoOro != "RS")
                //////        {
                //////            // SI NO ES UNO DE ESTOS TIPOS = BICOLOR = COGER LIGA AM
                //////            var FindFixingAM = ctxDB.Fixing.Where(x => x.Empresa == SessionEmpresa && x.Metal.Contains("AM")).OrderBy(x => x.Fecha).LastOrDefault();
                //////            if (FindFixingAM != null)
                //////            {
                //////                MetalVal = FindFixingAM.Metal;
                //////            }
                //////        }
                //////        else
                //////        {
                //////            var FindFixingAM = ctxDB.Fixing.Where(x => x.Empresa == SessionEmpresa && x.Metal.Contains(SelectTipoOro)).OrderBy(x => x.Fecha).LastOrDefault();
                //////            if (FindFixingAM != null)
                //////            {
                //////                MetalVal = FindFixingAM.Metal;
                //////            }
                //////        }

                //////    }

                //////    var FindLiga = ctxDB.Fixing.Where(x => x.Empresa == SessionEmpresa && x.Metal == MetalVal).FirstOrDefault();
                //////    if (FindLiga != null)
                //////    {
                //////        LigaPrecio = FindLiga.Precio ?? 0m;
                //////    }

                //////}
            }
            _CosteFABLiga   = LigaPeso * LigaPrecio;
            _LigaPeso       = LigaPeso;

            /****************************************************************************************************/
            /* CALCULO FAB RODIO                                                                                */
            /****************************************************************************************************/
            var RodioPeso   = _PesoTotalSinMerma;
            var RodioPrecio = 0m;
            FindArticulo = ctxDB.Articulos.Where(x => x.Empresa == SessionEmpresa && x.Articulo == Articulo).FirstOrDefault();
            if (FindArticulo != null)
            {
                //////if (SelectTipoOro == "AM")
                //////{
                //////    var FindRodio = ctxDB.Fixing.Where(x => x.Empresa == SessionEmpresa && x.Metal == FindArticulo.ArtTipoRodioAM).FirstOrDefault();
                //////    if (FindRodio != null)
                //////    {
                //////        RodioPrecio = FindRodio.Precio ?? 0m;
                //////    }
                //////}

                //////if (SelectTipoOro == "BL")
                //////{
                //////    var FindRodio = ctxDB.Fixing.Where(x => x.Empresa == SessionEmpresa && x.Metal == FindArticulo.ArtTipoRodioBL).FirstOrDefault();
                //////    if (FindRodio != null)
                //////    {
                //////        RodioPrecio = FindRodio.Precio ?? 0m;
                //////    }
                //////}

                //////if (SelectTipoOro != "AM" && SelectTipoOro != "BL")
                //////{
                //////    var FindRodio = ctxDB.Fixing.Where(x => x.Empresa == SessionEmpresa && x.Metal == FindArticulo.ArtTipoRodioBICOLOR).FirstOrDefault();
                //////    if (FindRodio != null)
                //////    {
                //////        RodioPrecio = FindRodio.Precio ?? 0m;
                //////    }
                //////}

            }

            _CosteFABRodio = RodioPeso * RodioPrecio;


            /****************************************************************************************************/
            /* CALCULO FAB ETIQUETADO                                                                           */
            /****************************************************************************************************/

            /****************************************************************************************************/
            /* CALCULO FAB GARANTIA                                                                             */
            /****************************************************************************************************/


            /****************************************************************************************************/
            /* CALCULO TOTALES                                                                                  */
            /****************************************************************************************************/
            var CosteOro            = _CosteMatOro      + _CosteMatOroComp;
            _CosteMetalesTot        = _CosteMatOro      + _CosteMatOroComp;
            _CosteMaterialesTot     = _CosteMatPie      + _CosteMatComp;
            _CosteMOTot             = _CosteMOInt       + _CosteMOExt           + _CosteMOComp;
            _CosteFAB               = _CosteFABLiga     + _CosteFABRodio        + _CosteFABEtq      + _CosteFABGar;
            _CosteTotal             = _CosteMetalesTot  + _CosteMaterialesTot   + _CosteMOTot       + _CosteFAB;

            _CosteTotalEtiqueta     = _CosteMetalesTot      + _CosteMaterialesTot + _CosteMOTot + _CosteFAB;
            _CosteTotalHechura      = _CosteMaterialesTot   + _CosteMOTot   + _CosteFAB;

            switch (SelectTipoVenta)
            {
                case "Etiqueta":
                    _CosteTotal = _CosteTotalEtiqueta;
                    break;
                case "Hechura":
                    _CosteTotal = _CosteTotalHechura;
                    break;
            }

            /****************************************************************************************************/
            /* CALCULO BENEFICIO                                                                                */
            /****************************************************************************************************/
            var FindTar = ctxDB.TarifasVenta.Where(x => x.Empresa == SessionEmpresa && x.Tarifa == SelectTarifaVenta).FirstOrDefault();
            if (FindTar != null)
            {
                switch (SelectTipoVenta)
                {
                    case "Etiqueta":
                        _Beneficio = (decimal)(_CosteTotalEtiqueta * FindTar.TarEtiqueta / 100);
                        break;
                    case "Hechura":
                        _Beneficio = (decimal)(_CosteTotalHechura * FindTar.TarEtiqueta / 100);
                        break;
                }
            }

            /****************************************************************************************************/
            /* FILL VAR ArticuloPrecio                                                                          */
            /****************************************************************************************************/
            ArticuloPrecio.EstCOSTES.Beneficio              = _Beneficio;
            ArticuloPrecio.EstCOSTES.CosteTotal             = _CosteTotal;
            ArticuloPrecio.EstCOSTES.CosteTotalEtiqueta     = _CosteTotalEtiqueta;
            ArticuloPrecio.EstCOSTES.CosteTotalHechura      = _CosteTotalHechura;

            ArticuloPrecio.EstCOSTES.CosteMetales         = _CosteMetalesTot;
            ArticuloPrecio.EstCOSTES.CosteMatOro          = _CosteMatOro;
            ArticuloPrecio.EstCOSTES.CosteMatOroComp      = _CosteMatOroComp;

            ArticuloPrecio.EstCOSTES.CosteMateriales      = _CosteMaterialesTot;
            ArticuloPrecio.EstCOSTES.CosteMatPie          = _CosteMatPie;
            ArticuloPrecio.EstCOSTES.CosteMatComp         = _CosteMatComp;

            ArticuloPrecio.EstCOSTES.CosteMO              = _CosteMOTot;
            ArticuloPrecio.EstCOSTES.CosteMOInt           = _CosteMOInt;
            ArticuloPrecio.EstCOSTES.CosteMOExt           = _CosteMOExt;
            ArticuloPrecio.EstCOSTES.CosteMOComp          = _CosteMOComp;

            ArticuloPrecio.EstCOSTES.CosteFAB             = _CosteFAB;
            ArticuloPrecio.EstCOSTES.CosteFABLiga         = _CosteFABLiga;
            ArticuloPrecio.EstCOSTES.LigaPeso             = _LigaPeso;
            ArticuloPrecio.EstCOSTES.CosteFABRodio        = _CosteFABRodio;
            ArticuloPrecio.EstCOSTES.CosteFABEtq          = _CosteFABEtq;
            ArticuloPrecio.EstCOSTES.CosteFABGar          = _CosteFABGar;

            ArticuloPrecio.EstCOSTES.PesoTotal            = _PesoTotal;
            ArticuloPrecio.EstCOSTES.PesoTotalSinMerma    = _PesoTotalSinMerma;

            ArticuloPrecio.EstCOSTES.PesoMetTotal           = _PesoMetTotal;
            ArticuloPrecio.EstCOSTES.PesoMetTotalSinMerma   = _PesoMetTotalSinMerma;
            ArticuloPrecio.EstCOSTES.PesoCompTotal          = _PesoCompTotal;
            ArticuloPrecio.EstCOSTES.PesoCompTotalSinMerma  = _PesoCompTotalSinMerma;


            ArticuloPrecio.EstCOSTES.HechuraPieza         = _CosteMatPie + _CosteMatComp + _CosteMOTot + _CosteFABLiga + _CosteFABRodio;
            if (_PesoTotalSinMerma != 0)
            {
                ArticuloPrecio.EstCOSTES.HechuraGramo = ArticuloPrecio.EstCOSTES.HechuraPieza / _PesoTotalSinMerma;
            }
            else
            {
                ArticuloPrecio.EstCOSTES.HechuraGramo = 0M;
            }
            
            ArticuloPrecio.EstCOSTES.CosteGramo           = 0M;

            switch (SelectTipoVenta)
            {
                case "Etiqueta":
                    ArticuloPrecio.EstCOSTES.PrecioPVP = _CosteTotalEtiqueta    + _Beneficio;
                    break;
                case "Hechura":
                    ArticuloPrecio.EstCOSTES.PrecioPVP = _CosteTotalHechura     + _Beneficio;
                    break;
            }

            


            ArticuloPrecio.EstPIE                       = _EstPIE;
            ArticuloPrecio.EstCOMP                      = _EstCOMP;
            ArticuloPrecio.EstMO                        = _EstMO;

            /****************************************************************************************************/
            /* CALCULO EST MET-COMP-MO                                                                          */
            /****************************************************************************************************/



            /****************************************************************************************************/
            /* PREPARACION ARRAY ESTRUCTURA TREE                                                                */
            /****************************************************************************************************/

            _CosteTotal         = Math.Round(_CosteTotal,       CalcDecimales);
            _CosteTotalEtiqueta = Math.Round(_CosteTotalEtiqueta, CalcDecimales);
            _CosteTotalHechura  = Math.Round(_CosteTotalHechura, CalcDecimales);
            _Beneficio          = Math.Round(_Beneficio,        CalcDecimales);

            _CosteMetalesTot    = Math.Round(_CosteMetalesTot,  CalcDecimales);
            _CosteMatOro        = Math.Round(_CosteMatOro,      CalcDecimales);
            _CosteMatOroComp    = Math.Round(_CosteMatOroComp,  CalcDecimales);


            _CosteMaterialesTot = Math.Round(_CosteMaterialesTot,   CalcDecimales);
            _CosteMatPie        = Math.Round(_CosteMatPie,          CalcDecimales);
            _CosteMatComp       = Math.Round(_CosteMatComp,         CalcDecimales);

            _CosteMOTot         = Math.Round(_CosteMOTot,          CalcDecimales);
            _CosteMOInt         = Math.Round(_CosteMOInt,       CalcDecimales);
            _CosteMOExt         = Math.Round(_CosteMOExt,       CalcDecimales);
            _CosteMOComp        = Math.Round(_CosteMOComp,      CalcDecimales);

            _CosteFAB           = Math.Round(_CosteFAB,         CalcDecimales);
            _CosteFABLiga       = Math.Round(_CosteFABLiga,     CalcDecimales);
            _CosteFABRodio      = Math.Round(_CosteFABRodio,    CalcDecimales);
            _CosteFABEtq        = Math.Round(_CosteFABEtq,      CalcDecimales);
            _CosteFABGar        = Math.Round(_CosteFABGar,      CalcDecimales);


            var NodeCosteTotal      = TreeEstructuraPrecio.Where(x => x.Id == "Coste Total").FirstOrDefault();
            var NodeBeneficio       = TreeEstructuraPrecio.Where(x => x.Id == "Beneficio").FirstOrDefault();

            var NodeMetales         = NodeCosteTotal.Items.Where(x => x.Id == "Metales").FirstOrDefault();
            var NodeMatOro          = NodeMetales.Items.Where(x => x.Id == "Oro").FirstOrDefault();
            var NodeMatOroComp      = NodeMetales.Items.Where(x => x.Id == "Oro Componentes").FirstOrDefault();

            var NodeMateriales      = NodeCosteTotal.Items.Where(x => x.Id == "Materiales").FirstOrDefault();
            var NodeMatPie          = NodeMateriales.Items.Where(x => x.Id == "Piedras").FirstOrDefault();
            var NodeMatComp         = NodeMateriales.Items.Where(x => x.Id == "Componentes").FirstOrDefault();

            var NodeManoDeObra      = NodeCosteTotal.Items.Where(x => x.Id == "Mano de obra").FirstOrDefault();
            var NodeMoInterna       = NodeManoDeObra.Items.Where(x => x.Id == "Interna").FirstOrDefault();
            var NodeMoExterna       = NodeManoDeObra.Items.Where(x => x.Id == "Externa").FirstOrDefault();
            var NodeMoCompo         = NodeManoDeObra.Items.Where(x => x.Id == "MO Componentes").FirstOrDefault();

            var NodeFabricacion     = NodeCosteTotal.Items.Where(x => x.Id == "Fabricacion").FirstOrDefault();
            var NodeFabLiga         = NodeFabricacion.Items.Where(x => x.Id == "Liga de oro").FirstOrDefault();
            var NodeFabRodio        = NodeFabricacion.Items.Where(x => x.Id == "Rodio").FirstOrDefault();
            var NodeFabEtiquetado   = NodeFabricacion.Items.Where(x => x.Id == "Etiquetado").FirstOrDefault();
            var NodeFabGarantia     = NodeFabricacion.Items.Where(x => x.Id == "Garantia").FirstOrDefault();

            

            NodeCosteTotal.Header       = NodeCosteTotal.Header.Replace("0,000", _CosteTotalEtiqueta.ToString().Replace(".", ","));
            NodeBeneficio.Header        = NodeBeneficio.Header.Replace("0,000", _Beneficio.ToString().Replace(".", ","));
            NodeCosteTotal.Valor        = _CosteTotalEtiqueta;            
            NodeBeneficio.Valor         = _Beneficio;

            NodeMetales.Header          = NodeMetales.Header.Replace("0,000",    _CosteMetalesTot.ToString().Replace(".", ","));
            NodeMatOro.Header           = NodeMatOro.Header.Replace("0,000",     _CosteMatOro.ToString().Replace(".", ","));
            NodeMatOroComp.Header       = NodeMatOroComp.Header.Replace("0,000", _CosteMatOroComp.ToString().Replace(".", ","));

            NodeMateriales.Header       = NodeMateriales.Header.Replace("0,000", _CosteMaterialesTot.ToString().Replace(".", ","));
            NodeMatPie.Header           = NodeMatPie.Header.Replace("0,000",     _CosteMatPie.ToString().Replace(".", ","));
            NodeMatComp.Header          = NodeMatComp.Header.Replace("0,000",    _CosteMatComp.ToString().Replace(".", ","));

            NodeMetales.Valor           = _CosteMetalesTot;
            NodeMatOro.Valor            = _CosteMatOro;
            NodeMatOroComp.Valor        = _CosteMatOroComp;

            NodeMateriales.Valor        = _CosteMaterialesTot;
            NodeMatPie.Valor            = _CosteMatPie;
            NodeMatComp.Valor           = _CosteMatComp;

            NodeManoDeObra.Header       = NodeManoDeObra.Header.Replace("0,000", _CosteMOTot.ToString().Replace(".", ","));
            NodeMoInterna.Header        = NodeMoInterna.Header.Replace("0,000",  _CosteMOInt.ToString().Replace(".", ","));
            NodeMoExterna.Header        = NodeMoExterna.Header.Replace("0,000",  _CosteMOExt.ToString().Replace(".", ","));
            NodeMoCompo.Header          = NodeMoCompo.Header.Replace("0,000",    _CosteMOComp.ToString().Replace(".", ","));
            NodeManoDeObra.Valor        = _CosteMOTot;
            NodeMoInterna.Valor         = _CosteMOInt;
            NodeMoExterna.Valor         = _CosteMOExt;
            NodeMoCompo.Valor           = _CosteMOComp;

            NodeFabricacion.Header      = NodeFabricacion.Header.Replace("0,000",    _CosteFAB.ToString().Replace(".", ","));
            NodeFabLiga.Header          = NodeFabLiga.Header.Replace("0,000",        _CosteFABLiga.ToString().Replace(".", ","));
            NodeFabRodio.Header         = NodeFabRodio.Header.Replace("0,000",       _CosteFABRodio.ToString().Replace(".", ","));
            //NodeFabEtiquetado.Header    = NodeFabEtiquetado.Header.Replace("0,000",  _CosteFABEtq.ToString().Replace(".", ","));
            //NodeFabGarantia.Header      = NodeFabGarantia.Header.Replace("0,000",    _CosteFABGar.ToString().Replace(".", ","));
            NodeFabricacion.Valor       = _CosteFAB;
            //NodeFabEtiquetado.Valor     = _CosteFABEtq;
            //NodeFabGarantia.Valor       = _CosteFABGar;

            ArticuloPrecio.TreeEstructuraPrecio = TreeEstructuraPrecio;

            var ListArticuloIMG = ctxDB.ArticulosIMG.Where(x => x.Empresa == SessionEmpresa && x.Articulo == Articulo).OrderBy(x => x.Tipo).ThenBy(x => x.Secuencia).ToList();
            ArticuloPrecio.ListArticuloIMG = ListArticuloIMG;

            return ArticuloPrecio;

        }



    }
}
