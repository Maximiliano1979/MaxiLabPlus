using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iLabPlus.Helpers
{
    [Authorize]
    public class FunctionsBBDD : Controller
    {

        private readonly DbContextiLabPlus ctxDB;
        private readonly IHttpContextAccessor _httpContextAccessor;


        private ClaimsIdentity ClaimsIdentity;
        private string SessionEmpresa;
        private string SessionEmpresaNombre;
        private string SessionUsuario;
        private string SessionUsuarioNombre;
        private string SessionUsuarioTipo;

        public FunctionsBBDD(DbContextiLabPlus Context, IHttpContextAccessor ContextAccessor)
        {
            ctxDB = Context;
            _httpContextAccessor = ContextAccessor;
            InitializeSessionProperties();
        }

        private void InitializeSessionProperties()
        {
            if (_httpContextAccessor.HttpContext?.User?.Identity is ClaimsIdentity claimsIdentity)
            {
                ClaimsIdentity = claimsIdentity;
                if (ClaimsIdentity.Claims.Any())
                {
                    SessionEmpresa = ClaimsIdentity.FindFirst("Empresa")?.Value;
                    SessionEmpresaNombre = ClaimsIdentity.FindFirst("EmpresaNombre")?.Value;
                    SessionUsuario = ClaimsIdentity.FindFirst("Usuario")?.Value;
                    SessionUsuarioNombre = ClaimsIdentity.FindFirst("UsuarioNombre")?.Value;
                    SessionUsuarioTipo = ClaimsIdentity.FindFirst("UsuarioTipo")?.Value;
                }
            }
            else
            {
                SessionEmpresa = "DefaultEmpresa";
                SessionUsuario = "DefaultUsuario";
            }
        }

        public virtual GrupoClaims GetClaims()
        {
            var ResultClaims = new GrupoClaims
            {
                SessionEmpresa = SessionEmpresa,
                SessionEmpresaNombre = SessionEmpresaNombre,
                SessionUsuario = SessionUsuario,
                SessionUsuarioNombre = SessionUsuarioNombre,
                SessionUsuarioTipo = SessionUsuarioTipo
            };

            return ResultClaims;
        }

        public virtual GrupoColumnsLayout GetColumnsLayout(string GridName)
        {
            var Result = new GrupoColumnsLayout();

            var UserConfigGrid = ctxDB.UsuariosGridsCfg.Where(x => x.Empresa == SessionEmpresa && x.Usuario == SessionUsuario && x.GridID == GridName).FirstOrDefault();
            if (UserConfigGrid != null)
            {
                Result.ColumnsLayoutUser = UserConfigGrid.ColumnsLayout;


                if (UserConfigGrid.ColumnsPinned == null)
                {
                    Result.ColumnsPinnedUser = 3;
                }
                else
                {
                    Result.ColumnsPinnedUser = UserConfigGrid.ColumnsPinned;
                }
            }
            else
            {
                Result.ColumnsPinnedUser = 3;
            }

            return Result;
        }


        public virtual List<MenuUser> GetMenuAccesos()
        {
            var MenuUser = new List<MenuUser>();

            var UserFind = ctxDB.Usuarios.Where(x => x.Empresa == SessionEmpresa && x.Usuario == SessionUsuario).FirstOrDefault();
            if (UserFind != null)
            {
                //MenuUser = JsonSerializer.Deserialize<List<MenuUser>>(UserFind.Menus);
                MenuUser = JsonConvert.DeserializeObject<List<MenuUser>>(UserFind.Menus);

            }

            return MenuUser;

        }


        public List<MenuUser> GetMenuAccesosStandar()
        {

            var MenuUser = new List<MenuUser>();

            var MenuUserAccesosList = new List<MenuUserAccesos>();
            MenuUserAccesosList.Add(new MenuUserAccesos("iLabPlus", "Cuadro de mandos", "Dashboard", "Index", "Cuadro de mandos", "fa-chart-line"));
            MenuUserAccesosList.Add(new MenuUserAccesos("iLabPlus", "Usuario y Roles", "Usuarios", "Index", "Usuario y Roles", "fa-users"));
            //MenuUserAccesosList.Add(new MenuUserAccesos("iLabPlus", "Interface", "Interfaces", "Index", "Interfaces", "fa-database"));
            //MenuUserAccesosList.Add(new MenuUserAccesos("iLabPlus", "Clientes", "Clientes", "Index", "Clientes", "fa-id-card"));
            //MenuUserAccesosList.Add(new MenuUserAccesos("iLabPlus", "Valores del sistema", "Valsys", "Index", "Valores del sistema", "fa-chart-line"));            
            MenuUser.Add(new MenuUser("iLabPlus", "Mis accesos", MenuUserAccesosList, "fa-link"));


            MenuUserAccesosList = new List<MenuUserAccesos>();
            MenuUserAccesosList.Add(new MenuUserAccesos("Maestros", "Articulos",            "Articulos",        "Index",    "Artículos",                "fa-barcode-read"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Maestros", "Clientes",             "Clientes",         "Index",    "Clientes",                 "fa-id-card"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Maestros", "Proveedores",          "Proveedores",      "Index",    "Proveedores",              "fa-person-dolly"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Maestros", "Tarifas de Venta",     "TarifasVenta",     "Index",    "Tarifas de Venta",         "fa-tags"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Maestros", "Divisas",              "Divisas",          "Index",    "Divisas",                  "fa-badge-dollar"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Maestros", "Operarios exteriores", "OperariosExt",     "Index",    "Operarios exteriores",     "fa-user-md"));

            MenuUserAccesosList.Add(new MenuUserAccesos("Maestros", "Separator",                "", "", "", ""));

            MenuUserAccesosList.Add(new MenuUserAccesos("Maestros", "Empleados",                "Empleados",        "Index",                "Empleados",                "fa-user-md"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Maestros", "Control Horario",          "ControlHorario",   "Index",                "Control Horario",          "fa-user-cog"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Maestros", "Control Horario Admin",    "ControlHorario",   "ControlHorarioAdmin",    "Control Horario Admin",  "fa-user-cog"));

            MenuUser.Add(new MenuUser("Maestros", "Maestros", MenuUserAccesosList, "fa-sitemap"));


            MenuUserAccesosList = new List<MenuUserAccesos>();
            MenuUserAccesosList.Add(new MenuUserAccesos("Comercial", "Simulación de precios",       "Articulos",        "DialogSimuladorPrecios", "", "fa-dollar-sign"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Comercial", "Pedidos Venta",               "Pedidos",          "Index",        "", "fa-clipboard-list"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Comercial", "Albaranes Venta",             "Albaranes",        "Index",        "", "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Comercial", "Facturas Venta",              "Facturas",         "Index",        "", "fa-file-invoice"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Comercial", "Ordenes de compra",           "OrdenesCompra",    "Index",        "", "fa-ballot"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Comercial", "Facturas proveedores",        "FacturasProveedor","Index",        "", "fa-file-invoice"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Comercial", "Metales - Fixing",            "Fixing",           "Index",        "", "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Comercial", "Vendedores",                  "Vendedores",       "Index",        "Vendedores", "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Comercial", "Liquidación Vend.",           "Vendedores",       "Index",        "", "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Comercial", "Mailing Clientes",            "CorreosSalientes", "Index",        "", "fa-inbox-out"));
            MenuUser.Add(new MenuUser("Comercial", "Comercial", MenuUserAccesosList, "fa-user-chart"));


            MenuUserAccesosList = new List<MenuUserAccesos>();
            MenuUserAccesosList.Add(new MenuUserAccesos("Fabricación", "Planes de fabricación (Mrp)",   "Mrp",              "Index",        "", "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Fabricación", "Libro de metal",                "LibroMetal",       "Index",        "", "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Fabricación", "Control Ope.Ext.",              "OperariosExt",     "Control",      "", "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Fabricación", "Liquidacion Ope.Ext.",          "OperariosExt",     "Liquidacion",  "", "fa-file-alt"));
            MenuUser.Add(new MenuUser("Fabricación", "Fabricación", MenuUserAccesosList, "fa-industry-alt"));


            MenuUserAccesosList = new List<MenuUserAccesos>();
            
            MenuUserAccesosList.Add(new MenuUserAccesos("Almacenes", "Balance de stocks",           "Stocks",           "Index",            "Balance de stocks", "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Almacenes", "Movimientos Stk.",            "Stocks",           "Index",            "Movimientos Stk.", "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Almacenes", "Ruptura Stk.",                "Stocks",           "RupturaDeStocks",  "Ruptura Stk.", "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Almacenes", "Recuentos Stk.",              "Stocks",           "Index",            "Recuentos Stk.", "fa-file-alt"));
            MenuUser.Add(new MenuUser("Almacenes", "Almacenes", MenuUserAccesosList, "fa-warehouse-alt"));


            MenuUserAccesosList = new List<MenuUserAccesos>();
            MenuUserAccesosList.Add(new MenuUserAccesos("Estadisticas", "Cuadro de mandos",             "Dashboard",    "Index", "", "fa-chart-line"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Estadisticas", "Estadísticas",                 "Estadisticas", "Index", "", "fa-file-alt"));
            //MenuUserAccesosList.Add(new MenuUserAccesos("Estadisticas", "Estadisticas Proveedores",     "Estadisticas", "Index", "", "fa-file-alt"));
            //MenuUserAccesosList.Add(new MenuUserAccesos("Estadisticas", "Estadisticas Fabricación",     "Estadisticas", "Index", "", "fa-file-alt"));
            MenuUser.Add(new MenuUser("Estadisticas", "Estadisticas", MenuUserAccesosList, "fa-chart-pie"));


            MenuUserAccesosList = new List<MenuUserAccesos>();
            MenuUserAccesosList.Add(new MenuUserAccesos("Contabilidad", "CashFlow",                 "Contabilidad", "CashFlow",         "CashFlow",                 "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Contabilidad", "Bancos",                   "Contabilidad", "Bancos",           "Bancos",                   "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Contabilidad", "Cuentas Contables",        "Contabilidad", "Cuentas",          "Cuentas Contables",        "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Contabilidad", "Cartera de Efectos",       "Contabilidad", "Efectos",          "Cartera de Efectos",       "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Contabilidad", "Remesas",                  "ContaRemesas", "Index",            "Remesas",                  "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Contabilidad", "Libro Diario",             "Contabilidad", "LibroDiario",      "Libro Diario",             "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Contabilidad", "Libro Mayor",              "Contabilidad", "LibroMayor",       "Libro Mayor",              "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Contabilidad", "Cierre de Ejercicio",      "Contabilidad", "CierreEjercicio",  "Cierre de Ejercicio",      "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Contabilidad", "Ley Antifraude 11/2021", "Contabilidad",   "LeyAntifraude",    "Ley Antifraude 11/2021",   "fa-file-alt"));
            MenuUser.Add(new MenuUser("Contabilidad", "Contabilidad", MenuUserAccesosList, "fa-abacus"));


            MenuUserAccesosList = new List<MenuUserAccesos>();

            MenuUserAccesosList.Add(new MenuUserAccesos("Administración", "Usuario y Roles",        "Usuarios",         "Index",    "Usuario y Roles",      "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Administración", "Configuraciones",        "Configuraciones",   "Index",   "Configuraciones",      "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Administración", "Valores del sistema",    "Valsys",           "Index",    "Valores del sistema",  "fa-chart-line"));
            //MenuUserAccesosList.Add(new MenuUserAccesos("Administración", "Perfil",                 "Administracion",   "Index",    "",                     "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Administración", "APIs Plataformas",       "APIsPlataformas",  "Index",    "Logs Versiones",       "fa-file-alt"));

            MenuUserAccesosList.Add(new MenuUserAccesos("Administración", "Ley AntiFraude",         "LeyAntiFraude",    "Index",    "Ley AntiFraude",       "fa-lock-alt"));

            MenuUserAccesosList.Add(new MenuUserAccesos("Administración", "Logs Versiones",         "LogsVersiones",    "Index",    "Logs Versiones",       "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Administración", "Logs Accesos",           "LogsAccesos",      "Index",    "Logs Accesos",         "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Administración", "Hanna Test",             "HannaIA",          "Index",    "Hanna Test",           "fa-file-alt"));
            MenuUserAccesosList.Add(new MenuUserAccesos("Administración", "Chat",                   "Chat",             "Index",    "Chat",                 "fa-file-alt"));

            MenuUserAccesosList.Add(new MenuUserAccesos("Administración", "InterfacesRAYDEN",       "InterfacesRAYDEN", "Index",    "Interfaces RAYDEN",    "fa-database"));

            MenuUser.Add(new MenuUser("Administración", "Administración", MenuUserAccesosList, "fa-user-cog"));




            //MenuUserAccesosList = new List<MenuUserAccesos>();
            //MenuUserAccesosList.Add(new MenuUserAccesos("Test", "Productos", "Test", "Productos", "Productos", "fa-barcode-read"));
            //MenuUserAccesosList.Add(new MenuUserAccesos("Test", "Roles Usuario", "Test", "NestableList", "NestableList", "fa-users"));
            //MenuUserAccesosList.Add(new MenuUserAccesos("Test", "Test Posits", "Test", "Posits", "Test Posits", "fa-clipboard"));
            //MenuUserAccesosList.Add(new MenuUserAccesos("Test", "Interfaces", "Interfaces", "Index", "Interfaces", "fa-database"));
            //MenuUser.Add(new MenuUser("Test", "Test", MenuUserAccesosList, "fa-text-height"));


            return MenuUser;
        }

        [HttpPost]
        public async Task<IActionResult> GridConfigSave(string GridIdToSave, string ColumnsToSave, int ColumnsPinned)
        {
            bool resultProcess = false;


            try
            {
                var FindUsuarioGrid = ctxDB.UsuariosGridsCfg.Where(x=>x.Empresa == SessionEmpresa && x.Usuario == SessionUsuario && x.GridID == GridIdToSave).FirstOrDefault();
                if (FindUsuarioGrid != null)
                {
                    // Edicion
                    FindUsuarioGrid.GridID          = GridIdToSave;
                    FindUsuarioGrid.ColumnsLayout   = ColumnsToSave;
                    FindUsuarioGrid.ColumnsPinned   = ColumnsPinned;

                    FindUsuarioGrid.Empresa    = SessionEmpresa;
                    FindUsuarioGrid.IsoUser    = SessionUsuarioNombre;
                    FindUsuarioGrid.IsoFecMod  = DateTime.Now;

                    ctxDB.UsuariosGridsCfg.Update(FindUsuarioGrid);
                    await ctxDB.SaveChangesAsync();
                    resultProcess = true;
                }
                else
                {
                    // Creacion
                    var NewCFG = new UsuariosGridsCfg();
                    NewCFG.Empresa = SessionEmpresa;
                    NewCFG.Usuario = SessionUsuario;

                    NewCFG.GridID           = GridIdToSave;
                    NewCFG.ColumnsLayout    = ColumnsToSave;
                    NewCFG.ColumnsPinned    = ColumnsPinned;

                    NewCFG.Empresa          = SessionEmpresa;
                    NewCFG.IsoUser          = SessionUsuarioNombre;
                    NewCFG.IsoFecAlt        = DateTime.Now;

                    ctxDB.UsuariosGridsCfg.Add(NewCFG);
                    await ctxDB.SaveChangesAsync();
                    resultProcess = true;
                }



            }
            catch (Exception e)
            {
                //Logger.Error("[M]: " + e.Message + "[StackT]: " + e.StackTrace + "[HLink]: " + e.HelpLink + "[HResult]: " + e.HResult + "[Source]: " + e.Source);
                //ModelState.AddModelError("", "Unable to save changes. " + "Try again, and if the problem persists " + "see your system administrator.");
                return StatusCode(400, e.InnerException);  //  Send "Error"
            }

            if (resultProcess)
            {
                return StatusCode(200, "OK");
            }
            else
            {
                return StatusCode(400, null);
            }

        }


        [HttpPost]
        public async Task<IActionResult> GridConfigReset(string GridIdToSave)
        {
            bool resultProcess = false;


            try
            {
                var FindUsuarioGrid = ctxDB.UsuariosGridsCfg.Where(x => x.Empresa == SessionEmpresa && x.Usuario == SessionUsuario && x.GridID == GridIdToSave).FirstOrDefault();
                if (FindUsuarioGrid != null)
                {
                    // Edicion
                    FindUsuarioGrid.ColumnsLayout   = null;
                    FindUsuarioGrid.ColumnsPinned   = null;

                    FindUsuarioGrid.IsoUser         = SessionUsuarioNombre;
                    FindUsuarioGrid.IsoFecMod       = DateTime.Now;

                    ctxDB.UsuariosGridsCfg.Update(FindUsuarioGrid);
                    await ctxDB.SaveChangesAsync();
                    resultProcess = true;
                }
                else
                {
                    // Creacion
                    var NewCFG              = new UsuariosGridsCfg();
                    NewCFG.Empresa          = SessionEmpresa;
                    NewCFG.Usuario          = SessionUsuario;

                    NewCFG.GridID           = GridIdToSave;
                    NewCFG.ColumnsLayout    = null;
                    NewCFG.ColumnsPinned    = null;

                    NewCFG.Empresa          = SessionEmpresa;
                    NewCFG.IsoUser          = SessionUsuarioNombre;
                    NewCFG.IsoFecAlt        = DateTime.Now;

                    ctxDB.UsuariosGridsCfg.Add(NewCFG);
                    await ctxDB.SaveChangesAsync();
                    resultProcess = true;
                }



            }
            catch (Exception e)
            {
                //Logger.Error("[M]: " + e.Message + "[StackT]: " + e.StackTrace + "[HLink]: " + e.HelpLink + "[HResult]: " + e.HResult + "[Source]: " + e.Source);
                //ModelState.AddModelError("", "Unable to save changes. " + "Try again, and if the problem persists " + "see your system administrator.");
                return StatusCode(400, e.InnerException);  //  Send "Error"
            }

            if (resultProcess)
            {
                return StatusCode(200, "OK");
            }
            else
            {
                return StatusCode(400, null);
            }

        }

    }
}
