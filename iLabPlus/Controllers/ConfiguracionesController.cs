
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Xunit.Abstractions;
using System.IO.Abstractions;

namespace iLabPlus.Controllers
{
    [Authorize]
    public class ConfiguracionesController : Controller
    {

        private readonly DbContextiLabPlus ctxDB;
        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFunctionsCrypto _functionsCrypto;
        private readonly IFileSystem _fileSystem;


        public ConfiguracionesController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD, ITestOutputHelper testOutputHelper = null, IFunctionsCrypto functionsCrypto = null, IFileSystem fileSystem = null)
        {
            ctxDB = Context;
            FunctionsBBDD = _FunctionsBBDD;
            GrupoClaims = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridConfiguraciones");
            _testOutputHelper = testOutputHelper;
            _functionsCrypto = functionsCrypto ?? new FunctionsCryptoWrapper();
            _fileSystem = fileSystem ?? new FileSystem();

        }


        private void Log(string message)
        {
            _testOutputHelper?.WriteLine(message);
        }


        public IActionResult Index()
        {
            Log("Iniciando método Index");

            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;

            Log($"ViewBag configurado: MenuUserList={ViewBag.MenuUserList != null}, ColumnsLayoutUser={ViewBag.ColumnsLayoutUser}, ColumnsPinnedUser={ViewBag.ColumnsPinnedUser}");

            var EstConfiguraciones = new EstConfiguraciones();

            var FindEmpresa = ctxDB.Empresas.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).FirstOrDefault();
            if (FindEmpresa != null)
            {
                Log($"Empresa encontrada: {FindEmpresa.Empresa}");
                EstConfiguraciones.EmpresasEst = FindEmpresa;

                var FindEmpresaConfig = ctxDB.EmpresasConfig.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).FirstOrDefault();
                if (FindEmpresaConfig != null)
                {
                    Log("Configuración de empresa encontrada");
                    EstConfiguraciones.EmpresasConfigEst = FindEmpresaConfig;
                }
                else
                {
                    Log("No se encontró configuración de empresa");
                }
            }
            else
            {
                Log("No se encontró la empresa");
            }

            Log("Retornando vista Configuraciones");
            return View("Configuraciones", EstConfiguraciones);
        }



        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/


        [HttpPost]
        public async Task<IActionResult> Save_CONFIG_Mail(EmpresasConfig EmpresasConfigEst)
        {
            Log($"Iniciando Save_CONFIG_Mail para EmpresasConfig con Guid: {EmpresasConfigEst.Guid}");
            try
            {
                await CrearEmpresasConfigSinoExiste(EmpresasConfigEst);
                Log("CrearEmpresasConfigSinoExiste completado");

                var FindEmpresasConfigEst = ctxDB.EmpresasConfig.Where(x => x.Guid == EmpresasConfigEst.Guid).FirstOrDefault();
                if (FindEmpresasConfigEst != null)
                {
                    Log($"EmpresasConfig encontrado: {FindEmpresasConfigEst.Guid}");

                    FindEmpresasConfigEst.MailsEnvILabPlus = EmpresasConfigEst.MailsEnvILabPlus ?? false;
                    Log($"MailsEnvILabPlus actualizado: {FindEmpresasConfigEst.MailsEnvILabPlus}");

                    if (EmpresasConfigEst.MailsEnvNombre != null)
                    {
                        FindEmpresasConfigEst.MailsEnvNombre = EmpresasConfigEst.MailsEnvNombre;
                        Log($"MailsEnvNombre actualizado: {FindEmpresasConfigEst.MailsEnvNombre}");
                    }
                    if (EmpresasConfigEst.MailsEnvCuenta != null)
                    {
                        FindEmpresasConfigEst.MailsEnvCuenta = EmpresasConfigEst.MailsEnvCuenta;
                        Log($"MailsEnvCuenta actualizado: {FindEmpresasConfigEst.MailsEnvCuenta}");
                    }
                    if (EmpresasConfigEst.MailsEnvServidor != null)
                    {
                        FindEmpresasConfigEst.MailsEnvServidor = EmpresasConfigEst.MailsEnvServidor;
                        Log($"MailsEnvServidor actualizado: {FindEmpresasConfigEst.MailsEnvServidor}");
                    }
                    if (EmpresasConfigEst.MailsEnvPuerto != null)
                    {
                        FindEmpresasConfigEst.MailsEnvPuerto = EmpresasConfigEst.MailsEnvPuerto;
                        Log($"MailsEnvPuerto actualizado: {FindEmpresasConfigEst.MailsEnvPuerto}");
                    }
                    if (EmpresasConfigEst.MailsEnvDirRespuesta != null)
                    {
                        FindEmpresasConfigEst.MailsEnvDirRespuesta = EmpresasConfigEst.MailsEnvDirRespuesta;
                        Log($"MailsEnvDirRespuesta actualizado: {FindEmpresasConfigEst.MailsEnvDirRespuesta}");
                    }

                    FindEmpresasConfigEst.TextosFirmaMail = EmpresasConfigEst.TextosFirmaMail;
                    Log($"TextosFirmaMail actualizado: {FindEmpresasConfigEst.TextosFirmaMail}");

                    FindEmpresasConfigEst.IsoUser = GrupoClaims.SessionUsuarioNombre;
                    FindEmpresasConfigEst.IsoFecMod = DateTime.Now;
                    Log($"IsoUser actualizado: {FindEmpresasConfigEst.IsoUser}, IsoFecMod: {FindEmpresasConfigEst.IsoFecMod}");

                    ctxDB.EmpresasConfig.Update(FindEmpresasConfigEst);
                    await ctxDB.SaveChangesAsync();
                    Log("Cambios guardados en la base de datos");

                    Log("Redirigiendo a Index");
                    return RedirectToAction("Index");
                }
                else
                {
                    Log("No se encontró EmpresasConfig");
                }
            }
            catch (Exception e)
            {
                Log($"Error en Save_CONFIG_Mail: {e.Message}");
                Log($"StackTrace: {e.StackTrace}");
                return StatusCode(400, e.Message);
            }

            Log("Retornando StatusCode 400");
            return StatusCode(400, null);
        }



        [HttpPost]
        public async Task<IActionResult> Save_CONFIG_Empresa(Empresas EmpresasEst, EmpresasConfig EmpresasConfigEst)
        {
            Log($"Iniciando Save_CONFIG_Empresa para Empresa con Guid: {EmpresasEst.Guid} y EmpresasConfig con Guid: {EmpresasConfigEst.Guid}");
            try
            {
                await CrearEmpresasConfigSinoExiste(EmpresasConfigEst);
                Log("CrearEmpresasConfigSinoExiste completado");

                var FindEmpresa = ctxDB.Empresas.Where(x => x.Guid == EmpresasEst.Guid).FirstOrDefault();
                if (FindEmpresa != null)
                {
                    Log($"Empresa encontrada: {FindEmpresa.Guid}");
                    UpdateEmpresaProperties(FindEmpresa, EmpresasEst);

                    ctxDB.Empresas.Update(FindEmpresa);
                    await ctxDB.SaveChangesAsync();
                    Log("Empresa actualizada y guardada");
                }
                else
                {
                    Log("No se encontró la Empresa");
                }

                var FindEmpresasConfigEst = ctxDB.EmpresasConfig.Where(x => x.Guid == EmpresasConfigEst.Guid).FirstOrDefault();
                if (FindEmpresasConfigEst != null)
                {
                    Log($"EmpresasConfig encontrado: {FindEmpresasConfigEst.Guid}");
                    UpdateEmpresasConfigProperties(FindEmpresasConfigEst, EmpresasConfigEst);

                    ctxDB.EmpresasConfig.Update(FindEmpresasConfigEst);
                    await ctxDB.SaveChangesAsync();
                    Log("EmpresasConfig actualizado y guardado");
                }
                else
                {
                    Log("No se encontró EmpresasConfig");
                }

                Log("Redirigiendo a Index");
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Log($"Error en Save_CONFIG_Empresa: {e.Message}");
                Log($"StackTrace: {e.StackTrace}");
                return StatusCode(400, e.Message);
            }
        }

        private void UpdateEmpresaProperties(Empresas FindEmpresa, Empresas EmpresasEst)
        {
            if (EmpresasEst.Nombre != null) { FindEmpresa.Nombre = EmpresasEst.Nombre; Log($"Nombre actualizado: {FindEmpresa.Nombre}"); }
            if (EmpresasEst.RazonSocial != null) { FindEmpresa.RazonSocial = EmpresasEst.RazonSocial; Log($"RazonSocial actualizado: {FindEmpresa.RazonSocial}"); }
            if (EmpresasEst.Nif != null) { FindEmpresa.Nif = EmpresasEst.Nif; Log($"Nif actualizado: {FindEmpresa.Nif}"); }
            if (EmpresasEst.Mail != null) { FindEmpresa.Mail = EmpresasEst.Mail; Log($"Mail actualizado: {FindEmpresa.Mail}"); }
            if (EmpresasEst.Direccion != null) { FindEmpresa.Direccion = EmpresasEst.Direccion; Log($"Direccion actualizado: {FindEmpresa.Direccion}"); }
            if (EmpresasEst.CodigoPostal != null) { FindEmpresa.CodigoPostal = EmpresasEst.CodigoPostal; Log($"CodigoPostal actualizado: {FindEmpresa.CodigoPostal}"); }
            if (EmpresasEst.Poblacion != null) { FindEmpresa.Poblacion = EmpresasEst.Poblacion; Log($"Poblacion actualizado: {FindEmpresa.Poblacion}"); }
            if (EmpresasEst.Provincia != null) { FindEmpresa.Provincia = EmpresasEst.Provincia; Log($"Provincia actualizado: {FindEmpresa.Provincia}"); }
            if (EmpresasEst.Pais != null) { FindEmpresa.Pais = EmpresasEst.Pais; Log($"Pais actualizado: {FindEmpresa.Pais}"); }
            if (EmpresasEst.Telefono != null) { FindEmpresa.Telefono = EmpresasEst.Telefono; Log($"Telefono actualizado: {FindEmpresa.Telefono}"); }
            if (EmpresasEst.Web != null) { FindEmpresa.Web = EmpresasEst.Web; Log($"Web actualizado: {FindEmpresa.Web}"); }
            if (EmpresasEst.Persona != null) { FindEmpresa.Persona = EmpresasEst.Persona; Log($"Persona actualizado: {FindEmpresa.Persona}"); }
        }

        private void UpdateEmpresasConfigProperties(EmpresasConfig FindEmpresasConfigEst, EmpresasConfig EmpresasConfigEst)
        {
            FindEmpresasConfigEst.FormaCobroClientes = EmpresasConfigEst.FormaCobroClientes;
            FindEmpresasConfigEst.CuentaBancaria = EmpresasConfigEst.CuentaBancaria;
            FindEmpresasConfigEst.IsoUser = GrupoClaims.SessionUsuarioNombre;
            FindEmpresasConfigEst.IsoFecMod = DateTime.Now;
            Log($"FormaCobroClientes actualizado: {FindEmpresasConfigEst.FormaCobroClientes}");
            Log($"CuentaBancaria actualizado: {FindEmpresasConfigEst.CuentaBancaria}");
            Log($"IsoUser actualizado: {FindEmpresasConfigEst.IsoUser}");
            Log($"IsoFecMod actualizado: {FindEmpresasConfigEst.IsoFecMod}");
        }


        [HttpPost]
        public async Task<IActionResult> Save_CONFIG_TextosPlantillas(EmpresasConfig EmpresasConfigEst)
        {
            Log("Entrando en Save_CONFIG_TextosPlantillas");
            try
            {
                bool configuracionCreada = await CrearEmpresasConfigSinoExiste(EmpresasConfigEst);
                Log($"Configuración creada: {configuracionCreada}");
                var FindEmpresasConfigEst = ctxDB.EmpresasConfig.Where(x => x.Guid == EmpresasConfigEst.Guid).FirstOrDefault();
                if (FindEmpresasConfigEst != null)
                {
                    Log("Configuración encontrada, actualizando...");
                    FindEmpresasConfigEst.TextosAlbPiePag = EmpresasConfigEst.TextosAlbPiePag;
                    FindEmpresasConfigEst.TextosAlbRPGD = EmpresasConfigEst.TextosAlbRPGD;
                    FindEmpresasConfigEst.TextosFacPiePag = EmpresasConfigEst.TextosFacPiePag;
                    FindEmpresasConfigEst.TextosFacRPGD = EmpresasConfigEst.TextosFacRPGD;
                    FindEmpresasConfigEst.RegistroMercantil = EmpresasConfigEst.RegistroMercantil;
                    ctxDB.EmpresasConfig.Update(FindEmpresasConfigEst);
                    await ctxDB.SaveChangesAsync();
                }
                if (!configuracionCreada && FindEmpresasConfigEst == null)
                {
                    Log("No se encontró la configuración, devolviendo StatusCode 400");
                    return StatusCode(400, "No se encontró la configuración de la empresa.");
                }
                Log("Redirigiendo a Index");
                var result = RedirectToAction("Index");
                Log($"Resultado de Save_CONFIG_TextosPlantillas: {result.GetType()}");
                return result;
            }
            catch (InvalidOperationException e)
            {
                Log($"Error de operación inválida en Save_CONFIG_TextosPlantillas: {e.Message}");
                Log($"StackTrace: {e.StackTrace}");
                return StatusCode(400, e.Message);
            }
            catch (Exception e)
            {
                Log($"Error en Save_CONFIG_TextosPlantillas: {e.Message}");
                Log($"StackTrace: {e.StackTrace}");
                return StatusCode(500, "Error interno del servidor");
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetCertificadosExistentes()
        {
            try
            {
                var nombresCertificados = await ctxDB.EmpresasCertificados
                                               .Where(c => c.Empresa == GrupoClaims.SessionEmpresa)
                                               .Select(c => c.Fichero)
                                               .ToListAsync();

                return Json(nombresCertificados);
            }
            catch (Exception ex)
            {
                Log($"Error en GetCertificadosExistentes: {ex.Message}");  // <-- Log adicional
                return Json(new { error = "Error interno del servidor" }); // <-- Cambiamos StatusCode por Json
            }
        }



        [HttpPost]
        public async Task<IActionResult> SubirCertificado(IFormFile certificado, string password)
        {

            Log("Iniciando SubirCertificado");

            if (certificado == null || (!certificado.FileName.EndsWith(".pfx") && !certificado.FileName.EndsWith(".p12")) || string.IsNullOrEmpty(password) || password.Length < 4)
            {
                Log($"Validación fallida: certificado es null: {certificado == null}, nombre de archivo: {certificado?.FileName}, longitud de contraseña: {password?.Length}");
                // Puedes usar ViewBag o ViewData para enviar un mensaje de error a la vista
                ViewBag.ErrorMessage = "Validación fallida: Asegúrate de que el archivo sea .pfx y la contraseña tenga al menos 8 caracteres.";
                return RedirectToAction("Index");
            }

            try
            {
                // Continuar con la lógica para guardar el certificado si no existe
                var ParentPathFic = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();

                var PathDoc = Path.Combine(ParentPathFic, "iLabPlusDocs");
                if (!Directory.Exists(PathDoc))
                {
                    Directory.CreateDirectory(PathDoc);
                }

                var PathDocEmp = Path.Combine(ParentPathFic, "iLabPlusDocs", GrupoClaims.SessionEmpresa);
                if (!Directory.Exists(PathDocEmp))
                {
                    Directory.CreateDirectory(PathDocEmp);
                }

                var directorioEmpresa = Path.Combine(ParentPathFic, "iLabPlusDocs", GrupoClaims.SessionEmpresa, "Certificados");
                if (!Directory.Exists(directorioEmpresa))
                {
                    Directory.CreateDirectory(directorioEmpresa);
                }

                var nombreArchivo = certificado.FileName;
                Log($"Nombre del archivo: {nombreArchivo}");


                var pathCertificado = Path.Combine(directorioEmpresa, nombreArchivo);

                using (var stream = new FileStream(pathCertificado, FileMode.Create))
                {
                    await certificado.CopyToAsync(stream);
                }

                var passwordCrypt = _functionsCrypto.EncryptAES(password);

                // Verificar si el certificado ya existe en la base de datos
                var certificadoExistente = ctxDB.EmpresasCertificados.FirstOrDefault(c => c.Fichero == nombreArchivo);
                if (certificadoExistente != null)
                {

                    Log("Certificado existente encontrado, actualizando...");
                    certificadoExistente.RutaArchivo = pathCertificado;
                    certificadoExistente.Password = passwordCrypt;
                    certificadoExistente.Fichero = nombreArchivo;
                    ctxDB.EmpresasCertificados.Update(certificadoExistente);

                }
                else
                {
                    Log("Nuevo certificado, agregando a la base de datos...");
                    var nuevoCertificado = new EmpresasCertificados
                    {
                        Guid = Guid.NewGuid(),
                        Empresa = GrupoClaims.SessionEmpresa,
                        Fichero = nombreArchivo,
                        RutaArchivo = pathCertificado,
                        Password = passwordCrypt // Guardar la contraseña tal cual 
                    };

                    ctxDB.EmpresasCertificados.Add(nuevoCertificado);
                    Log($"Nuevo certificado creado con Guid: {nuevoCertificado.Guid}");

                }

                await ctxDB.SaveChangesAsync();

                Log("Certificado guardado con éxito en la base de datos");
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Log($"Error al subir certificado: {e.Message}");
                return RedirectToAction("Index");
            }
        }

        public IActionResult GetDirectoriosEmpresa()
        {
            Log("Iniciando GetDirectoriosEmpresa");
            try
            {
                if (_fileSystem == null)
                {
                    Log("Error: _fileSystem es null");
                    return Json(new { error = "Sistema de archivos no inicializado." });
                }
                Log("_fileSystem está inicializado");

                var currentDirectory = _fileSystem.Directory.GetCurrentDirectory();
                Log($"Directorio actual: {currentDirectory}");

                string PathDocEmp;
                if (currentDirectory == "C:\\")
                {
                    PathDocEmp = _fileSystem.Path.Combine(currentDirectory, "iLabPlusDocs", GrupoClaims.SessionEmpresa);
                    Log($"Directorio raíz, usando ruta: {PathDocEmp}");
                }
                else
                {
                    PathDocEmp = _fileSystem.Path.Combine(currentDirectory, "iLabPlusDocs", GrupoClaims.SessionEmpresa);
                    Log($"Directorio no raíz, usando ruta: {PathDocEmp}");
                }

                if (!_fileSystem.Directory.Exists(PathDocEmp))
                {
                    Log($"El directorio no existe: {PathDocEmp}");
                    return Json(new { error = $"El directorio no existe: {PathDocEmp}" });
                }

                var directorios = _fileSystem.Directory.GetDirectories(PathDocEmp)
                                            .Select(dir => {
                                                Log($"Procesando directorio: {dir}");
                                                return new
                                                {
                                                    Nombre = _fileSystem.DirectoryInfo.New(dir).Name,
                                                    Tamaño = GetDirectorySize(dir)
                                                };
                                            }).ToList();

                Log($"Número de directorios encontrados: {directorios.Count}");
                return Json(directorios);
            }
            catch (Exception ex)
            {
                Log($"Error en GetDirectoriosEmpresa: {ex.Message}");
                Log($"StackTrace: {ex.StackTrace}");
                return Json(new { error = $"Error interno del servidor: {ex.Message}" });
            }
        }


        //public IActionResult GetDirectoriosEmpresa()
        //{
        //    Log("Iniciando GetDirectoriosEmpresa");
        //    try
        //    {
        //        if (_fileSystem == null)
        //        {
        //            Log("Error: _fileSystem es null");
        //            return Json(new { error = "Sistema de archivos no inicializado." });
        //        }
        //        Log("_fileSystem está inicializado");

        //        var currentDirectory = _fileSystem.Directory.GetCurrentDirectory();
        //        Log($"Directorio actual: {currentDirectory}");

        //        string ParentPathFic;
        //        if (currentDirectory == "C:\\")
        //        {
        //            ParentPathFic = currentDirectory;
        //            Log("El directorio actual es la raíz, usando como ParentPathFic");
        //        }
        //        else
        //        {
        //            var parentDirectory = _fileSystem.Directory.GetParent(currentDirectory);
        //            if (parentDirectory == null)
        //            {
        //                Log("Error: No se pudo obtener el directorio padre");
        //                return Json(new { error = "No se pudo obtener el directorio padre." });
        //            }
        //            ParentPathFic = parentDirectory.FullName;
        //        }
        //        Log($"ParentPathFic: {ParentPathFic}");

        //        if (GrupoClaims == null)
        //        {
        //            Log("Error: GrupoClaims es null");
        //            return Json(new { error = "Información de grupo de claims no disponible." });
        //        }
        //        if (string.IsNullOrEmpty(GrupoClaims.SessionEmpresa))
        //        {
        //            Log("Error: SessionEmpresa es null o vacío");
        //            return Json(new { error = "Información de empresa no disponible." });
        //        }
        //        Log($"SessionEmpresa: {GrupoClaims.SessionEmpresa}");

        //        var PathDocEmp = _fileSystem.Path.Combine(ParentPathFic, "iLabPlusDocs", GrupoClaims.SessionEmpresa);
        //        Log($"PathDocEmp: {PathDocEmp}");

        //        if (!_fileSystem.Directory.Exists(PathDocEmp))
        //        {
        //            Log($"El directorio no existe: {PathDocEmp}");
        //            return Json(new { error = $"El directorio no existe: {PathDocEmp}" });
        //        }

        //        var directorios = _fileSystem.Directory.GetDirectories(PathDocEmp)
        //                                    .Select(dir => {
        //                                        Log($"Procesando directorio: {dir}");
        //                                        return new
        //                                        {
        //                                            Nombre = _fileSystem.DirectoryInfo.New(dir).Name,
        //                                            Tamaño = GetDirectorySize(dir)
        //                                        };
        //                                    }).ToList();

        //        Log($"Número de directorios encontrados: {directorios.Count}");
        //        return Json(directorios);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log($"Error en GetDirectoriosEmpresa: {ex.Message}");
        //        Log($"StackTrace: {ex.StackTrace}");
        //        return Json(new { error = $"Error interno del servidor: {ex.Message}" });
        //    }
        //}




        private long GetDirectorySize(string path)
        {
            Log($"Calculando tamaño para: {path}");
            var size = _fileSystem.DirectoryInfo.New(path).EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            Log($"Tamaño calculado: {size}");
            return size;
        }

      


        private async Task<bool> CrearEmpresasConfigSinoExiste(EmpresasConfig EmpresasConfigEst)
        {
            var FindEmpresasConfigEst = ctxDB.EmpresasConfig.Where(x => x.Guid == EmpresasConfigEst.Guid).FirstOrDefault();
            if (FindEmpresasConfigEst == null)
            {
                EmpresasConfigEst.Empresa = GrupoClaims.SessionEmpresa;
                ctxDB.EmpresasConfig.Add(EmpresasConfigEst);
                await ctxDB.SaveChangesAsync();
                return true; // La configuración fue creada
            }
            return false; // La configuración ya existía
        }


    }

}