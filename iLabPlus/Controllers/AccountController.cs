using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Controllers;
using iLabPlus;
using iLabPlus.Models;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.IO.Abstractions;
using iLabPlus.Models.Clases;

namespace AccountController.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {

        private readonly DbContextiLabPlus ctxDB;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IHttpContextAccessor httpContext;
        private readonly IWebHostEnvironment environment;
        private readonly IFileSystem fileSystem;
        private readonly Notifications _notifications; // Nuevo campo




        public AccountController(
            DbContextiLabPlus context,
            IWebHostEnvironment ienvironment,
            IHttpContextAccessor ihttpContext,
            IFileSystem fileSystem,
            Notifications notifications, // Nuevo parámetro
            ITestOutputHelper testOutputHelper = null)
        {
            ctxDB = context;
            environment = ienvironment;
            httpContext = ihttpContext;
            _testOutputHelper = testOutputHelper;
            this.fileSystem = fileSystem;
            _notifications = notifications; // Asignación del nuevo campo

        }

        private void Log(string message)
        {
            _testOutputHelper?.WriteLine(message);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            var LoginUser = new LoginModel { Usuario = "", Password = "" };

            return View("Login", LoginUser);
            
        }


        public virtual void CheckUser(LoginModel LoginUser, Usuarios LookupUser)
        {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            if (LookupUser != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, LookupUser.Usuario ?? ""));
                identity.AddClaim(new Claim("Empresa", LookupUser.Empresa ?? ""));
                identity.AddClaim(new Claim("Usuario", LookupUser.Usuario ?? ""));
                identity.AddClaim(new Claim("UsuarioNombre", LookupUser.UsuarioNombre ?? ""));
                identity.AddClaim(new Claim("UsuarioTipo", LookupUser.UsuarioTipo ?? ""));

                if (ctxDB.Empresas != null)
                {
                    var FindEmpresa = ctxDB.Empresas.FirstOrDefault(x => x.Empresa == LookupUser.Empresa);
                    if (FindEmpresa != null)
                    {
                        identity.AddClaim(new Claim("EmpresaNombre", FindEmpresa.Nombre ?? ""));
                    }
                }
            }

            if (HttpContext != null)
            {
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                        new ClaimsPrincipal(identity),
                                        new AuthenticationProperties { IsPersistent = LoginUser.Recordar });
            }
        }


        //public void CheckUser(LoginModel LoginUser, Usuarios LookupUser)
        //{
        //    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        //    if (LookupUser != null)
        //    {
        //        identity.AddClaim(new Claim(ClaimTypes.Name, LookupUser.Usuario ?? ""));
        //        identity.AddClaim(new Claim("Empresa", LookupUser.Empresa ?? ""));
        //        identity.AddClaim(new Claim("Usuario", LookupUser.Usuario ?? ""));
        //        identity.AddClaim(new Claim("UsuarioNombre", LookupUser.UsuarioNombre ?? ""));
        //        identity.AddClaim(new Claim("UsuarioTipo", LookupUser.UsuarioTipo ?? ""));

        //        if (ctxDB.Empresas != null)
        //        {
        //            var FindEmpresa = ctxDB.Empresas.FirstOrDefault(x => x.Empresa == LookupUser.Empresa);
        //            if (FindEmpresa != null)
        //            {
        //                identity.AddClaim(new Claim("EmpresaNombre", FindEmpresa.Nombre ?? ""));
        //            }
        //        }
        //    }

        //    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        //                            new ClaimsPrincipal(identity),
        //                            new AuthenticationProperties { IsPersistent = LoginUser.Recordar });
        //}




        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel LoginUser)
        {
            _testOutputHelper?.WriteLine("Iniciando método Login");
            _testOutputHelper?.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (ModelState.IsValid)
            {
                try
                {
                    _testOutputHelper?.WriteLine("Buscando usuario en la base de datos");
                    var LookupUser = await ctxDB.Usuarios.FirstOrDefaultAsync(x => x.Usuario.ToUpper() == LoginUser.Usuario.ToUpper());
                    _testOutputHelper?.WriteLine($"Usuario encontrado: {LookupUser != null}");

                    if (LookupUser != null)
                    {
                        bool isPasswordValid = false;
                        _testOutputHelper?.WriteLine($"Contraseña comienza con $2: {LookupUser.Password.StartsWith("$2")}");

                        if (LookupUser.Password.StartsWith("$2")) // Contraseña hasheada con BCrypt
                        {
                            isPasswordValid = FunctionsCrypto.VerifyPassword(LoginUser.Password, LookupUser.Password);
                        }

                        _testOutputHelper?.WriteLine($"Contraseña válida: {isPasswordValid}");

                        if (isPasswordValid)
                        {
                            _testOutputHelper?.WriteLine("Llamando a CheckUser");
                            CheckUser(LoginUser, LookupUser);
                            _testOutputHelper?.WriteLine($"ControllerInit: {LookupUser.ControllerInit}");

                            if (!string.IsNullOrEmpty(LookupUser.ControllerInit))
                            {
                                _testOutputHelper?.WriteLine($"Redirigiendo a {LookupUser.ControllerInit}");
                                return RedirectToAction("Index", LookupUser.ControllerInit);
                            }
                            else
                            {
                                _testOutputHelper?.WriteLine("Redirigiendo a Home");
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            _testOutputHelper?.WriteLine("Contraseña incorrecta");
                            ModelState.AddModelError(string.Empty, "Datos de acceso incorrectos. Vuelve a intentarlo o recupere su contraseña.");
                            return View("Login", LoginUser);
                        }
                    }
                    else
                    {
                        _testOutputHelper?.WriteLine("Usuario no encontrado");
                        ModelState.AddModelError(string.Empty, "Cuenta desactivada. Pongase en contacto con soporte técnico.");
                        return View("Login", LoginUser);
                    }
                }
                catch (Exception e)
                {
                    _testOutputHelper?.WriteLine($"Excepción: {e.Message}");
                    _testOutputHelper?.WriteLine($"StackTrace: {e.StackTrace}");
                    ModelState.AddModelError(string.Empty, "Error de conexión con el servidor iLabPlus [" + e.Message + "]");
                    return View("Login", LoginUser);
                }
            }
            else
            {
                _testOutputHelper?.WriteLine("ModelState no es válido");
                ModelState.AddModelError(string.Empty, "Datos de acceso incorrectos. Vuelve a intentarlo o utiliza la opción de recuperar contraseña.");
                return View("Login", LoginUser);
            }
        }


        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login(LoginModel LoginUser)
        //{

        //    if (ModelState.IsValid)
        //    {

        //        try
        //        {
        //            var LookupUser = await ctxDB.Usuarios.FirstOrDefaultAsync(x => x.Usuario.ToUpper() == LoginUser.Usuario.ToUpper());
        //            if (LookupUser != null)
        //            {

        //                bool isPasswordValid = false;

        //                if (LookupUser.Password.StartsWith("$2")) // Contraseña hasheada con BCrypt
        //                {
        //                    isPasswordValid = FunctionsCrypto.VerifyPassword(LoginUser.Password, LookupUser.Password);
        //                }


        //                if (isPasswordValid)
        //                {
        //                    CheckUser(LoginUser, LookupUser);
        //                    if (!string.IsNullOrEmpty(LookupUser.ControllerInit))
        //                    {
        //                        return RedirectToAction("Index", LookupUser.ControllerInit);
        //                    }
        //                    else
        //                    {
        //                        return RedirectToAction("Index", "Home");
        //                    }
        //                }
        //                else
        //                {
        //                    ModelState.AddModelError(string.Empty, "Datos de acceso incorrectos. Vuelve a intentarlo o recupere su contraseña.");
        //                    return View("Login", LoginUser);
        //                }
        //            }
        //            else
        //            {
        //                ModelState.AddModelError(string.Empty, "Cuenta desactivada. Pongase en contacto con soporte técnico.");
        //                return View("Login", LoginUser);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            ModelState.AddModelError(string.Empty, "Error de conexión con el servidor iLabPlus [" + e.Message + "]");
        //            return View("Login", LoginUser);
        //        }
        //    }
        //    else
        //    {
        //        ModelState.AddModelError(string.Empty, "Datos de acceso incorrectos. Vuelve a intentarlo o utiliza la opción de recuperar contraseña.");
        //        return View("Login", LoginUser);
        //    }
        //}



        [HttpGet]
        [AllowAnonymous]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }



        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SendPasswordResetLink(string username)
        {
            _testOutputHelper?.WriteLine($"Iniciando SendPasswordResetLink para usuario: {username}");
            if (string.IsNullOrEmpty(username))
            {
                _testOutputHelper?.WriteLine("El nombre de usuario está vacío o es nulo");
                return StatusCode(400, "El nombre de usuario es requerido");
            }

            var newToken = "";
            var newTokenURL = "";
            Usuarios authUser = null;
            try
            {
                _testOutputHelper?.WriteLine("Buscando usuario en la base de datos");
                authUser = await ctxDB.Usuarios.FirstOrDefaultAsync(x => x.Usuario.ToUpper() == username.ToUpper());
            }
            catch (Exception e)
            {
                _testOutputHelper?.WriteLine($"Error al buscar usuario: {e.Message}");
                return StatusCode(500, e.Message);
            }

            if (authUser != null)
            {
                try
                {
                    newToken = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");

                    var _Empresa_Mtab4_ConfigMail = await ctxDB.ValSys.FirstOrDefaultAsync(x => x.Empresa == "iLabPlus" && x.Clave == "Smtp");

                    if (_Empresa_Mtab4_ConfigMail == null)
                    {
                        _testOutputHelper?.WriteLine("Configuración de correo no encontrada");
                        return StatusCode(500, "Configuración de correo no encontrada");
                    }

                    if (httpContext?.HttpContext?.Request == null)
                    {
                        return StatusCode(500, "Error en la configuración del servidor");
                    }

                    var request = httpContext.HttpContext.Request;
                    var rawurl = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(request);
                    var uri = new Uri(rawurl);
                    var Urls = "https://iLabPlus.com/";
                    var HtmlTemplate_Recovery = "Mail_Recuperar_Password.html";
                    newTokenURL = Urls + "/Account/ResetPassword/" + WebUtility.UrlEncode(newToken);

                    authUser.tokenid = newToken;
                    authUser.tokenissuedutc = DateTime.UtcNow;
                    authUser.tokenexpiredutc = DateTime.UtcNow.AddMinutes(15);
                    ctxDB.Usuarios.Update(authUser);
                    await ctxDB.SaveChangesAsync();

                    if (environment?.WebRootPath == null)
                    {
                        _testOutputHelper?.WriteLine("WebRootPath es nulo");
                        return StatusCode(500, "Error en la configuración del servidor");
                    }

                    var pathToFile = Path.Combine(environment.WebRootPath, "EmailTemplates", HtmlTemplate_Recovery);
                    if (!fileSystem.File.Exists(pathToFile))
                    {
                        _testOutputHelper?.WriteLine($"Plantilla de correo no encontrada: {pathToFile}");
                        return StatusCode(500, "Plantilla de correo no encontrada");
                    }

                    var bodyHtml = new StringBuilder();
                    using (StreamReader sourceReader = fileSystem.File.OpenText(pathToFile))
                    {
                        bodyHtml.Append(sourceReader.ReadToEnd());
                    }

                    bodyHtml.Replace("{{Presentacion}}", username);
                    bodyHtml.Replace("{{Titulo_1}}", "Ahora puedes generar una nueva contraseña.");
                    bodyHtml.Replace("{{Titulo_2}}", "*El enlace permanencerá activo durante 15 minutos, una vez transcurrido este tiempo deberás solicitar un nuevo cambio de contraseña.");
                    bodyHtml.Replace("{{UrlObs}}", newTokenURL);

                    string _Cuenta = _Empresa_Mtab4_ConfigMail.Valor1;
                    string _Password = _Empresa_Mtab4_ConfigMail.Valor2;
                    string _Host = _Empresa_Mtab4_ConfigMail.Valor3;
                    int _Port = 0;
                    if (_Empresa_Mtab4_ConfigMail.Valor4 != null)
                    {
                        _Port = Convert.ToInt32(_Empresa_Mtab4_ConfigMail.Valor4);
                    }

                    string[] ConfigMail = new string[] { _Cuenta, _Password, _Host, _Port.ToString() };
                    _testOutputHelper?.WriteLine("Enviando correo electrónico");
                    var result = await _notifications.EmailNotification(ConfigMail, username, "", "Recuperar contraseña", bodyHtml.ToString());

                    if (result == "200")
                    {
                        _testOutputHelper?.WriteLine("Correo enviado exitosamente");
                        return StatusCode(200);
                    }
                    else
                    {
                        _testOutputHelper?.WriteLine($"Error al enviar el correo: {result}");
                        return StatusCode(500, "Error al enviar el correo electrónico");
                    }
                }
                catch (Exception e)
                {
                    _testOutputHelper?.WriteLine($"Error durante el proceso: {e.Message}");
                    return StatusCode(500, e.Message);
                }
            }
            else
            {
                _testOutputHelper?.WriteLine("Usuario no encontrado");
                return StatusCode(404, "Usuario no encontrado");
            }
        }




        [HttpGet("/Account/ResetPassword/{userToken}")]
        [AllowAnonymous]
        public IActionResult ResetPassword(string userToken)
        {
            var resultProcess = true;

            var authUser = ctxDB.Usuarios.Where(x => x.tokenid == WebUtility.UrlDecode(userToken)).FirstOrDefault();
            if (authUser != null)
            {
                if (DateTime.UtcNow <= authUser.tokenexpiredutc)
                {
                    // Valid Token
                    resultProcess = true;
                }
                else { resultProcess = false; }

                if (resultProcess == true)
                {
                    ViewBag.username = authUser.UsuarioNombre;
                    return View("NewPassword", null);
                }
                else
                {
                    return View("LinkError", null);
                }
            }
            else
            {
                return View("LinkError", null);
            }




        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ChangePassword()
        {
            return View("ChangePassword");
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RenewPassword(string _KeyMail, string _KeyPass)
        {
            if (string.IsNullOrEmpty(_KeyMail) || string.IsNullOrEmpty(_KeyPass))
            {
                return StatusCode(400);
            }

            var _User = await ctxDB.Usuarios.FirstOrDefaultAsync(x => x.Usuario.ToUpper() == _KeyMail.ToUpper());
            if (_User == null)
            {
                return StatusCode(400);
            }

            try
            {
                _User.Password = FunctionsCrypto.HashPassword(_KeyPass);
                _User.webpasswordlastchanged = DateTime.Now;
                _User.webpasswordchanged = "T";
                _User.IsoFecMod = DateTime.Now;
                _User.tokenid = null;
                _User.tokenissuedutc = null;
                _User.tokenexpiredutc = null;
                ctxDB.Update(_User);
                await ctxDB.SaveChangesAsync();
                return StatusCode(200);
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }


    }
}
