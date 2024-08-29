using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;



namespace iLabPlus.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {

        private readonly DbContextiLabPlus ctxDB;
        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;
        private readonly ILogger<UsuariosController> _logger;


        public UsuariosController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD, ILogger<UsuariosController> logger)
        {
            ctxDB = Context;
            FunctionsBBDD = _FunctionsBBDD;
            GrupoClaims = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridUsuarios");
            _logger = logger;
        }


        public IActionResult Index()
        {

            var MenuUserList = FunctionsBBDD.GetMenuAccesos();
            ViewBag.MenuUserList = MenuUserList;

            ViewBag.SessionUsuario = GrupoClaims.SessionUsuario;

            var Usuarios = new List<Usuarios>();

            var UsuarioPrincipal = ctxDB.Usuarios.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Usuario == GrupoClaims.SessionUsuario).FirstOrDefault();
            if (UsuarioPrincipal != null)
            {
                Usuarios.Add(UsuarioPrincipal);
            }
            var UsuarioResto = ctxDB.Usuarios.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Usuario != GrupoClaims.SessionUsuario).OrderBy(x => x.UsuarioTipo).ThenBy(x => x.UsuarioNombre);
            if (UsuarioResto != null)
            {
                Usuarios.AddRange(UsuarioResto);
            }

            var Roles = new List<string>();
            Roles.Add("");
            Roles.Add("ADMIN");
            Roles.Add("USUARIO");
            ViewBag.UsuariosTipos = Roles;

            var ControllersInit = new List<string>();
            ControllersInit.Add("");
            foreach (var item in MenuUserList)
            {
                foreach (var itemc in item.Accesos)
                {
                    if (!ControllersInit.Contains(itemc.Menu_ItemCrontoller))
                    {
                        ControllersInit.Add(itemc.Menu_ItemCrontoller);
                    }

                }
            }

            ViewBag.ControllersInit = ControllersInit;

            return View("Usuarios", Usuarios);
        }

        public IActionResult GetMenuStandarView()
        {

            var MenuUser = FunctionsBBDD.GetMenuAccesosStandar();

            return PartialView("_MenuUserStandar", MenuUser);
        }

        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/

        [HttpGet]

        public IActionResult ObtenerTipoUsuario()
        {
            var tipoUsuario = GrupoClaims.SessionUsuarioTipo;

            if (string.IsNullOrEmpty(tipoUsuario))
            {
                return NotFound("Tipo de ususario no encontrado");
            }

            return Ok(new { UsuarioTipo = tipoUsuario });
        }


        [HttpPost]
        public IActionResult UserGetInfo(string Usuario)
        {
            var UserFind = ctxDB.Usuarios.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Usuario == Usuario).FirstOrDefault();
            if (UserFind != null)
            {
                return StatusCode(200, UserFind);
            }

            return StatusCode(200, null);
        }

        public class MenuItem
        {
            public string id { get; set; }
            public List<MenuItem> children { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> UserSave(string Tipo, string Usuario, string UsuarioNombre, string Password, string FiltroAccesoMac, string UsuarioTipo, string ControllerInit,
                                                    string HoraInicial, string HoraFinal, string MenuUserJson)
        {
            _logger.LogInformation($"Iniciando UserSave. Tipo: {Tipo}, Usuario: {Usuario}");
            var MenuUserListStandar = FunctionsBBDD.GetMenuAccesosStandar();
            var MenuUserToSave = new List<MenuUser>();

            try
            {
                List<MenuItem> dynJson = JsonConvert.DeserializeObject<List<MenuItem>>(MenuUserJson);

                foreach (var item in dynJson)
                {

                    var Obj = item.id;

                    var ObjChildren = item.children;

                    if (ObjChildren != null)
                    {
                        var FindItemStandar = MenuUserListStandar.Where(x => x.Menu == Obj).FirstOrDefault();
                        if (FindItemStandar != null)
                        {
                            var MenuUserAccesosList = new List<MenuUserAccesos>();
                            foreach (var itemCH in ObjChildren)
                            {
                                var FindItemStandarCH = MenuUserListStandar.Where(x => x.Accesos.Any(c => c.Menu_ItemName == itemCH.id)).FirstOrDefault();
                                if (FindItemStandarCH != null)
                                {
                                    var TmpAccesos = FindItemStandarCH.Accesos.ToList();
                                    var Acceso = TmpAccesos.Where(x => x.Menu_ItemName == itemCH.id).FirstOrDefault();
                                    if (Acceso != null)
                                    {
                                        MenuUserAccesosList.Add(new MenuUserAccesos(FindItemStandar.Menu, Acceso.Menu_ItemName, Acceso.Menu_ItemCrontoller, Acceso.Menu_ItemAction, Acceso.Menu_ItemTooltip, Acceso.Menu_ItemIcono));
                                    }
                                }
                            }

                            MenuUserToSave.Add(new MenuUser(FindItemStandar.Menu, FindItemStandar.Menu_Tooltip, MenuUserAccesosList, FindItemStandar.Menu_Icono));
                        }
                        else
                        {
                        }
                    }
                }


                var MenuUserToSaveString = JsonConvert.SerializeObject(MenuUserToSave);

                if (Tipo == "New")
                {
                    _logger.LogInformation($"Creando nuevo usuario: {Usuario}");
                    var NewUser = new Usuarios();

                    NewUser.Empresa = GrupoClaims.SessionEmpresa;
                    NewUser.Usuario = Usuario;
                    NewUser.UsuarioNombre = UsuarioNombre;

                    _logger.LogInformation("Aplicando hash a la contraseña del nuevo usuario");
                    NewUser.Password = FunctionsCrypto.HashPassword(Password);
                    NewUser.UsuarioTipo = UsuarioTipo;
                    NewUser.ControllerInit = ControllerInit;
                    NewUser.FiltroAccesoMac = FiltroAccesoMac;

                    NewUser.Menus = MenuUserToSaveString;

                    NewUser.IsoUser = GrupoClaims.SessionUsuarioNombre;
                    NewUser.IsoFecAlt = DateTime.Now;

                    try
                    {
                        _logger.LogInformation("Añadiendo nuevo usuario a la base de datos");
                        ctxDB.Usuarios.Add(NewUser);
                        await ctxDB.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        return StatusCode(400, e.InnerException);
                    }

                    return StatusCode(200, NewUser);
                }
                else
                {
                    _logger.LogInformation($"Actualizando usuario existente: {Usuario}");
                    var UserFind = ctxDB.Usuarios.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Usuario == Usuario).FirstOrDefault();
                    if (UserFind != null)
                    {
                        UserFind.UsuarioNombre = UsuarioNombre;

                        if (!string.IsNullOrEmpty(Password)) 
                        {
                            _logger.LogInformation("Actualizando y aplicando hash a la nueva contraseña");
                            UserFind.Password = FunctionsCrypto.HashPassword(Password); 
                        }
                        else
                        {
                            _logger.LogInformation("No se proporcionó nueva contraseña, manteniendo la existente");
                        }

                        UserFind.UsuarioTipo = UsuarioTipo;
                        UserFind.ControllerInit = ControllerInit;
                        UserFind.FiltroAccesoMac = FiltroAccesoMac;

                        UserFind.Menus = MenuUserToSaveString;

                        UserFind.IsoUser = GrupoClaims.SessionUsuarioNombre;
                        UserFind.IsoFecMod = DateTime.Now;

                        try
                        {
                            _logger.LogInformation("Actualizando usuario en la base de datos");
                            ctxDB.Usuarios.Update(UserFind);
                            _logger.LogInformation("Guardando cambios en la base de datos");
                            await ctxDB.SaveChangesAsync();
                            _logger.LogInformation("Cambios guardados exitosamente");
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Error al guardar/actualizar usuario");
                            return StatusCode(400, e.InnerException);
                        }

                        return StatusCode(200, UserFind);
                    }
                }
            }
            catch (Exception g)
            {
                return StatusCode(400, g.InnerException);

            }

            return StatusCode(200, null);
        }


        [HttpPost]
        public async Task<IActionResult> UserDelete(string Usuario)
        {
            var UserFind = ctxDB.Usuarios.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Usuario == Usuario).FirstOrDefault();
            if (UserFind != null)
            {
                try
                {
                    ctxDB.Usuarios.Remove(UserFind);
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

        //public IActionResult ConvertMenuUserToNestable(string Usuario)
        //{



        //    var UserFind = ctxDB.Usuarios.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Usuario == Usuario).FirstOrDefault();
        //    if (UserFind != null)
        //    {
        //        List<MenuUser> MenuUserList = new List<MenuUser>();

        //        if (UserFind.Menus != "" && UserFind.Menus != null)
        //        {
        //            MenuUserList = JsonConvert.DeserializeObject<List<MenuUser>>(UserFind.Menus);
        //        }
        //        else
        //        {
        //            MenuUserList = FunctionsBBDD.GetMenuAccesosStandar();
        //        }


        //        return PartialView("_MenuUser", MenuUserList);


        //    }
        //    else
        //    {
        //        //var MenuUser = FunctionsBBDD.GetMenuAccesos();

        //        return PartialView("_MenuUser", null);
        //    }


        //}


        public IActionResult ConvertMenuUserToNestable(string Usuario)
        {
            var UserFind = ctxDB.Usuarios.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Usuario == Usuario).FirstOrDefault();
            if (UserFind == null)
            {
                return PartialView("_MenuUser", null);
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(UserFind.Menus))
                {
                    var MenuUserList = JsonConvert.DeserializeObject<List<MenuUser>>(UserFind.Menus);
                    return PartialView("_MenuUser", MenuUserList);
                }
                else
                {
                    var MenuUserList = FunctionsBBDD.GetMenuAccesosStandar();
                    return PartialView("_MenuUser", MenuUserList);
                }
            }
            catch (JsonException ex) // Catch any JSON related exception
            {
                // Log error or take action
                return StatusCode(500, "Error al procesar los datos del menú: " + ex.Message);
            }
            catch (Exception ex) // Catch any other exception
            {
                // Log error or take action
                return StatusCode(500, "Error general al procesar los datos: " + ex.Message);
            }
        }


    }

}