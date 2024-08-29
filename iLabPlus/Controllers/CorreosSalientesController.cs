using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using iLabPlus.Helpers;
using iLabPlus.Models;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

namespace Nemesis365.Controllers
{
    [Authorize]
    public class CorreosSalientesController : Controller
    {
        private readonly DbContextiLabPlus ctxDB;
        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly FunctionsMails FunctionsMails;
        private readonly GrupoClaims GrupoClaims;
        private readonly ILogger<CorreosSalientesController> _logger;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFileSystem _fileSystem;


        public CorreosSalientesController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD, FunctionsMails _FunctionsMails, ILogger<CorreosSalientesController> logger, IFileSystem fileSystem, ITestOutputHelper testOutputHelper = null)
        {
            ctxDB = Context ?? throw new ArgumentNullException(nameof(Context));
            FunctionsBBDD = _FunctionsBBDD ?? throw new ArgumentNullException(nameof(_FunctionsBBDD));
            //FunctionsMails = _FunctionsMails ?? throw new ArgumentNullException(nameof(_FunctionsMails));
            this.FunctionsMails = _FunctionsMails;  // Puede ser nulo en las pruebas
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _testOutputHelper = testOutputHelper;
            GrupoClaims = FunctionsBBDD.GetClaims();
            _fileSystem = fileSystem ?? new FileSystem();

        }


        private void Log(string message)
        {
            _logger.LogInformation(message);
            _testOutputHelper?.WriteLine(message);
        }

        public string PublicGuardarArchivo(IFormFile archivo)
        {
            return GuardarArchivo(archivo);
        }


        public async Task<IActionResult> Index()
        {
            try
            {
                Log("Iniciando método Index");
                Log($"FunctionsBBDD: {(FunctionsBBDD != null ? "Inicializado" : "Null")}");
                Log($"GrupoClaims: {(GrupoClaims != null ? "Inicializado" : "Null")}");
                Log($"SessionEmpresa: {GrupoClaims?.SessionEmpresa ?? "Null"}");
                Log($"ctxDB: {(ctxDB != null ? "Inicializado" : "Null")}");
                Log($"CorreosSalientes: {(ctxDB?.CorreosSalientes != null ? "Inicializado" : "Null")}");

                if (FunctionsBBDD == null)
                {
                    Log("FunctionsBBDD es null");
                    return View("Error", "FunctionsBBDD no está inicializado");
                }

                List<MenuUser> menuUserList;
                try
                {
                    menuUserList = FunctionsBBDD.GetMenuAccesos();
                }
                catch (NullReferenceException)
                {
                    Log("Error al obtener MenuUserList. FunctionsBBDD puede ser null.");
                    return View("Error", "FunctionsBBDD no está inicializado");
                }

                if (menuUserList == null)
                {
                    Log("MenuUserList es null");
                    return View("Error", "No se pudo obtener la lista de menús de usuario");
                }

                ViewBag.MenuUserList = menuUserList;

                if (GrupoClaims == null || string.IsNullOrEmpty(GrupoClaims.SessionEmpresa))
                {
                    Log("GrupoClaims o SessionEmpresa es null o vacío");
                    return View("Error", "No se pudo obtener la información de la empresa de la sesión");
                }

                if (ctxDB == null || ctxDB.CorreosSalientes == null)
                {
                    Log("ctxDB o CorreosSalientes es null");
                    return View("Error", "El contexto de la base de datos no está inicializado correctamente");
                }

                Log($"Antes de la consulta LINQ");
                var query = ctxDB.CorreosSalientes
                    .Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
                    .OrderByDescending(x => x.FechaEnv)
                    .Select(c => new CorreosSalientes
                    {
                        Guid = c.Guid,
                        Empresa = c.Empresa,
                        MessageId = c.MessageId,
                        FechaEnv = c.FechaEnv,
                        Tipo = c.Tipo,
                        Estado = c.Estado,
                        Remitente = c.Remitente,
                        Destinatario = c.Destinatario,
                        CCO = c.CCO,
                        Asunto = c.Asunto,
                        Cuerpo = c.Cuerpo,
                        IsoUser = c.IsoUser,
                        IsoFecAlt = c.IsoFecAlt,
                        IsoFecMod = c.IsoFecMod,
                        Adjuntos = ctxDB.CorreosSalientesAdj
                                        .Where(adj => adj.Empresa == c.Empresa && adj.MessageId == c.MessageId)
                                        .ToList()
                    });

                Log($"Después de la consulta LINQ");

                Log($"Tipo de query: {query.GetType().FullName}");


                var ListCorreos = await Task.FromResult(query.ToList());

                Log($"Obtenidos {ListCorreos.Count} correos");
                return View("CorreosSalientes", ListCorreos);
            }
            catch (Exception ex)
            {
                Log($"Error en el método Index: {ex.Message}");
                Log($"Tipo de excepción: {ex.GetType().FullName}");
                Log($"StackTrace: {ex.StackTrace}");
                return View("Error", ex.Message);
            }
        }



        public IActionResult DialogCorreo(Guid guid)
        {
            try
            {
                var correoSaliente = ctxDB.CorreosSalientes.Where(x => x.Guid == guid).FirstOrDefault();                
                if (correoSaliente == null)
                {
                    // ES UN CORREO NUEVO
                    correoSaliente = new CorreosSalientes();

                    var FindEmpresaCfg = ctxDB.EmpresasConfig.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).FirstOrDefault();
                    if (FindEmpresaCfg != null)
                    {
                        if (FindEmpresaCfg.TextosFirmaMail != "")
                        {
                            correoSaliente.Cuerpo = "<br><br>" + FindEmpresaCfg.TextosFirmaMail;
                        }

                    }
                }
                else
                {
                    // SE ESTA REENVIANDO UN MAIL
                    correoSaliente.Cuerpo = "<br><br>" + "-----Mensaje original-----" + "<br><br>" + correoSaliente.Cuerpo;
                    correoSaliente.Asunto = "RV: " + correoSaliente.Asunto;

                }

                return PartialView("_DialogCorreosSalientes", correoSaliente);
            }
            catch (Exception ex)
            {
                return PartialView("Error", ex.Message );
            }
        }



        [HttpPost]
        public IActionResult CreateMail(string remitente, string destinatario, string CCO, string asunto, string cuerpo, IList<IFormFile> adjuntos)
        {
            try
            {
                Log("Iniciando CreateMail");
                var correoSaliente = new CorreosSalientes();
                correoSaliente.Empresa = GrupoClaims.SessionEmpresa;
                correoSaliente.Remitente = remitente;
                correoSaliente.Destinatario = destinatario;
                correoSaliente.CCO = CCO;
                correoSaliente.Asunto = asunto;
                correoSaliente.Cuerpo = cuerpo;
                correoSaliente.FechaEnv = DateTime.Now;
                correoSaliente.Guid = Guid.NewGuid();
                correoSaliente.IsoUser = GrupoClaims.SessionUsuario;
                correoSaliente.IsoFecAlt = DateTime.Now;
                correoSaliente.IsoFecMod = DateTime.Now;
                correoSaliente.Tipo = "M";
                correoSaliente.Estado = "Pendiente";
                correoSaliente.MessageId = null;

                Log("Añadiendo correoSaliente a la base de datos");
                ctxDB.CorreosSalientes.Add(correoSaliente);
                ctxDB.SaveChanges();
                Log("Correo guardado en la base de datos");

                Log("Enviando correo con FunctionsMails.MailSend");
                var MessageID = FunctionsMails.MailSend(remitente, destinatario, CCO, asunto, cuerpo, adjuntos);
                Log($"MessageID recibido: {MessageID}");

                if (MessageID != null)
                {
                    Log("Actualizando correo con MessageID");
                    correoSaliente.MessageId = MessageID;
                    correoSaliente.Estado = "Env.";
                    ctxDB.CorreosSalientes.Update(correoSaliente);

                    Log("Procesando adjuntos");
                    foreach (var adjunto in adjuntos)
                    {
                        Log($"Guardando adjunto: {adjunto.FileName}");
                        var rutaArchivo = GuardarArchivo(adjunto);
                        var adjuntoDB = new CorreosSalientesAdj
                        {
                            Guid = Guid.NewGuid(),
                            Empresa = GrupoClaims.SessionEmpresa,
                            MessageId = MessageID,
                            NombreArchivo = adjunto.FileName,
                            RutaArchivo = rutaArchivo
                        };
                        ctxDB.CorreosSalientesAdj.Add(adjuntoDB);
                    }

                    Log("Guardando cambios en la base de datos");
                    ctxDB.SaveChanges();
                }

                Log("Obteniendo adjuntos del correo");
                correoSaliente.Adjuntos = ctxDB.CorreosSalientesAdj
                                            .Where(adj => adj.Empresa == correoSaliente.Empresa && adj.MessageId == correoSaliente.MessageId)
                                            .ToList();

                Log("Finalizando CreateMail con éxito");
                return StatusCode(200, correoSaliente);
            }
            catch (Exception ex)
            {
                Log($"Error en CreateMail: {ex.Message}");
                Log($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, "Ocurrió un error al intentar crear el correo: " + ex.Message);
            }
        }


    
        protected virtual string GuardarArchivo(IFormFile archivo)
        {
            Log("Entrando en GuardarArchivo");
            Log($"_fileSystem es null: {_fileSystem == null}");

            if (archivo == null || archivo.Length == 0)
                return null;

            try
            {
                // Ruta base donde se crearán los directorios y el archivo
                var basePath = @"C:\Nemesis365Docs";

                var PathDocEmp = _fileSystem.Path.Combine(basePath, GrupoClaims.SessionEmpresa);
                Log($"PathDocEmp: {PathDocEmp}");

                var PathDirFic = _fileSystem.Path.Combine(PathDocEmp, "MailsFicAdjuntos");
                Log($"PathDirFic: {PathDirFic}");

                if (!_fileSystem.Directory.Exists(basePath))
                    _fileSystem.Directory.CreateDirectory(basePath);
                if (!_fileSystem.Directory.Exists(PathDocEmp))
                    _fileSystem.Directory.CreateDirectory(PathDocEmp);
                if (!_fileSystem.Directory.Exists(PathDirFic))
                    _fileSystem.Directory.CreateDirectory(PathDirFic);

                var rutaArchivo = _fileSystem.Path.Combine(PathDirFic, Guid.NewGuid().ToString() + _fileSystem.Path.GetExtension(archivo.FileName));
                Log($"rutaArchivo: {rutaArchivo}");

                using (var stream = _fileSystem.File.Create(rutaArchivo))
                {
                    archivo.CopyTo(stream);
                }

                Log("Saliendo de GuardarArchivo");
                return rutaArchivo;
            }
            catch (Exception ex)
            {
                Log($"Error en GuardarArchivo: {ex.Message}");
                throw;
            }
        }



        //protected virtual string GuardarArchivo(IFormFile archivo)
        //{
        //    Log("Entrando en GuardarArchivo"); // Nuevo log al inicio del método
        //    Log($"_fileSystem es null: {_fileSystem == null}"); // Verificar si _fileSystem es nulo

        //    if (archivo == null || archivo.Length == 0)
        //        return null;

        //    try
        //    {
        //        var ParentPathFic = _fileSystem.Directory.GetParent(_fileSystem.Directory.GetCurrentDirectory()).ToString();
        //        Log($"ParentPathFic: {ParentPathFic}"); // Registrar el valor de ParentPathFic

        //        var PathDoc = _fileSystem.Path.Combine(ParentPathFic, "Nemesis365Docs");
        //        Log($"PathDoc: {PathDoc}");

        //        var PathDocEmp = _fileSystem.Path.Combine(PathDoc, GrupoClaims.SessionEmpresa);
        //        Log($"PathDocEmp: {PathDocEmp}");

        //        var PathDirFic = _fileSystem.Path.Combine(PathDocEmp, "MailsFicAdjuntos");
        //        Log($"PathDirFic: {PathDirFic}");

        //        if (!_fileSystem.Directory.Exists(PathDoc))
        //            _fileSystem.Directory.CreateDirectory(PathDoc);
        //        if (!_fileSystem.Directory.Exists(PathDocEmp))
        //            _fileSystem.Directory.CreateDirectory(PathDocEmp);
        //        if (!_fileSystem.Directory.Exists(PathDirFic))
        //            _fileSystem.Directory.CreateDirectory(PathDirFic);

        //        var rutaArchivo = _fileSystem.Path.Combine(PathDirFic, Guid.NewGuid().ToString() + _fileSystem.Path.GetExtension(archivo.FileName));
        //        Log($"rutaArchivo: {rutaArchivo}");

        //        using (var stream = _fileSystem.File.Create(rutaArchivo))
        //        {
        //            archivo.CopyTo(stream);
        //        }

        //        Log("Saliendo de GuardarArchivo"); // Nuevo log al final del método
        //        return rutaArchivo;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log($"Error en GuardarArchivo: {ex.Message}"); // Registrar cualquier excepción
        //        throw; // Re-lanzar la excepción para que la prueba pueda capturarla
        //    }
        //}





        // [ExcludeFromCodeCoverage] // Es Excluido de aqui ya que se realiza el test con una simulacion directa y expresa desde el mismo test.
        //protected virtual string GuardarArchivo(IFormFile archivo)
        //{
        //    if (archivo == null || archivo.Length == 0)
        //        return null;

        //    // Obtener la ruta del directorio padre del directorio actual del proyecto.
        //    var ParentPathFic = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();

        //    // Construir las rutas necesarias.
        //    var PathDoc = Path.Combine(ParentPathFic, "Nemesis365Docs");
        //    var PathDocEmp = Path.Combine(PathDoc, GrupoClaims.SessionEmpresa);
        //    var PathDirFic = Path.Combine(PathDocEmp, "MailsFicAdjuntos");

        //    // Asegurarse de que los directorios existan.
        //    if (!Directory.Exists(PathDoc))
        //    {
        //        Directory.CreateDirectory(PathDoc);
        //    }
        //    if (!Directory.Exists(PathDocEmp))
        //    {
        //        Directory.CreateDirectory(PathDocEmp);
        //    }
        //    if (!Directory.Exists(PathDirFic))
        //    {
        //        Directory.CreateDirectory(PathDirFic);
        //    }

        //    // Crear una ruta única para cada archivo para evitar sobrescribir archivos con el mismo nombre.
        //    // Importante: usar PathDirFic como base para la ruta del archivo.
        //    var rutaArchivo = Path.Combine(PathDirFic, Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName));

        //    // Guardar el archivo.
        //    using (var fileStream = new FileStream(rutaArchivo, FileMode.Create))
        //    {
        //        archivo.CopyTo(fileStream);
        //    }

        //    // Devolver la ruta relativa del archivo (o la ruta absoluta si es necesario).
        //    return rutaArchivo;
        //}


        [HttpPost]
        public IActionResult BorrarCorreo(Guid guid)
        {
            try
            {
                var correoParaBorrar = ctxDB.CorreosSalientes.FirstOrDefault(c => c.Guid == guid);
                if (correoParaBorrar != null)
                {
                    ctxDB.CorreosSalientes.Remove(correoParaBorrar);
                    ctxDB.SaveChanges();
                    return Ok();
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error al intentar borrar el correo: " + ex.Message);
            }
        }

        public IActionResult DetalleCorreo(Guid guid)
        {
            try
            {
                var correo = ctxDB.CorreosSalientes.Where(c => c.Guid == guid).FirstOrDefault();
                if (correo != null)
                {
                    return Json(new 
                    { 
                        success = true, 
                        remitente = correo.Remitente,
                        destinatarios = correo.Destinatario,
                        CCO = correo.CCO,
                        asunto = correo.Asunto,
                        fechaEnvio = correo.FechaEnv.ToString("dd/MM/yyyy HH:mm"),
                        contenido = correo.Cuerpo,
                    });
                }
                return Json(new { success = false, message = "Correo no encontrado" });
            } 
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ocurrió un error al intentar obtener el correo: " + ex.Message });
            }
        }





    }
}

