using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using System.Diagnostics;

namespace iLabPlus.Controllers
{

    // [Authorize]
    public class ContactosController : Controller
    {
        private readonly DbContextiLabPlus ctxDB;
        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<ContactosController> _logger;

        public ContactosController(
             DbContextiLabPlus Context,
             FunctionsBBDD _FunctionsBBDD,
             ITestOutputHelper testOutputHelper = null,
             ILogger<ContactosController> logger = null)
        {
            ctxDB = Context;
            FunctionsBBDD = _FunctionsBBDD;
            GrupoClaims = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridConfiguraciones");
            _testOutputHelper = testOutputHelper;
            _logger = logger;

            Log($"Constructor: GrupoClaims is null: {GrupoClaims == null}");
            Log($"Constructor: GrupoColumnsLayout is null: {GrupoColumnsLayout == null}");
        }

        private void Log(string message)
        {
            _testOutputHelper?.WriteLine(message);
            _logger?.LogInformation(message);
            Debug.WriteLine(message);
        }



        public async Task<IActionResult> Index()
        {
            Log("Iniciando método Index");

            try
            {
                ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
                Log("MenuUserList obtenido");

                if (GrupoColumnsLayout != null)
                {
                    ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
                    ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;
                    Log("ColumnsLayout y ColumnsPinned configurados");
                }
                else
                {
                    Log("GrupoColumnsLayout es null, no se pudieron configurar ColumnsLayout y ColumnsPinned");
                }

                if (GrupoClaims == null || string.IsNullOrEmpty(GrupoClaims.SessionEmpresa))
                {
                    Log("GrupoClaims o SessionEmpresa es null o vacío");
                    return BadRequest("No se pudo obtener la información de la empresa");
                }

                if (ctxDB.Contactos == null)
                {
                    Log("ctxDB.Contactos es null");
                    return BadRequest("No se pudo acceder a la base de datos de contactos");
                }

                var contactos = await ctxDB.Contactos
                                   .Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Activo == true)
                                   .OrderBy(x => x.Nombre)
                                   .ToListAsync();
                Log($"Contactos obtenidos. Cantidad: {contactos.Count}");

                var letrasIniciales = contactos
                                        .Select(c => c.Nombre.Substring(0, 1).ToUpper())
                                        .Distinct()
                                        .OrderBy(x => x)
                                        .ToList();
                Log($"Letras iniciales obtenidas. Cantidad: {letrasIniciales.Count}");

                ViewBag.LetrasIniciales = letrasIniciales;

                Log("Retornando vista con contactos");
                return View("Contactos", contactos);
            }
            catch (Exception ex)
            {
                Log($"Error en método Index: {ex.Message}");
                Log($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }



        [HttpGet]
        public async Task<IActionResult> DetallesContacto(Guid id)
        {
            var contacto = await ctxDB.Contactos.FindAsync(id);

            if (contacto == null || contacto.Empresa != GrupoClaims.SessionEmpresa)
            {
                return NotFound();
            }

            return Json(contacto);
        }


        public async Task<IActionResult> DialogContacto(Guid? id = null)
        {
            Contactos contacto;
            if (id.HasValue)
            {
                contacto = await ctxDB.Contactos.FindAsync(id.Value);
                if (contacto == null)
                {
                    contacto = new Contactos();
                }
            }
            else
            {
                contacto = new Contactos();
            }

            return PartialView("_DialogContacto", contacto);
        }


        [HttpPost]
        public async Task<IActionResult> CrearEditarContacto(Contactos contacto)
        {
            try
            {
                Log($"Iniciando CrearEditarContacto para contacto con Guid: {contacto?.Guid}");

                if (contacto == null)
                {
                    Log("Error: contacto es null");
                    return Json(new { success = false, message = "Error: El contacto proporcionado es nulo." });
                }

                if (ctxDB == null || ctxDB.Contactos == null)
                {
                    Log("Error: ctxDB o ctxDB.Contactos es null");
                    return Json(new { success = false, message = "Error interno del servidor: Base de datos no disponible." });
                }

                if (contacto.Guid == Guid.Empty)
                {
                    Log("Creando nuevo contacto");
                    try
                    {
                        Log("Buscando contactos existentes");
                        var contactoExistente = await ctxDB.Contactos
                            .FirstOrDefaultAsync(c => c.Email == contacto.Email && c.Empresa == GrupoClaims.SessionEmpresa);

                        if (contactoExistente == null)
                        {
                            Log("Contacto no existe, procediendo a crear");
                            contacto.Empresa = GrupoClaims.SessionEmpresa;
                            contacto.IsoUser = User?.Identity?.Name ?? "Unknown";
                            contacto.IsoFecAlt = DateTime.Now;
                            contacto.IsoFecMod = DateTime.Now;
                            contacto.Activo = true;

                            ctxDB.Contactos.Add(contacto);
                            Log("Llamando a SaveChangesAsync");
                            await ctxDB.SaveChangesAsync();
                            Log("Contacto creado exitosamente");
                            return Json(new { success = true, message = "Contacto creado exitosamente.", contacto });
                        }
                        else
                        {
                            Log("Contacto ya existe");
                            return Json(new { success = false, message = "El contacto ya existe." });
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Error al buscar o crear contacto: {ex.Message}");
                        Log($"StackTrace: {ex.StackTrace}");
                        return Json(new { success = false, message = $"Error al buscar o crear contacto: {ex.Message}" });
                    }
                }
                else
                {
                    Log("Editando contacto existente");
                    try
                    {
                        var contactoExistente = await ctxDB.Contactos
                            .FirstOrDefaultAsync(c => c.Guid == contacto.Guid && c.Empresa == GrupoClaims.SessionEmpresa);

                        if (contactoExistente == null)
                        {
                            Log("Contacto no encontrado");
                            return Json(new { success = false, message = "Contacto no encontrado." });
                        }

                        // Actualizar propiedades
                        contactoExistente.Nombre = contacto.Nombre;
                        contactoExistente.Email = contacto.Email;
                        contactoExistente.Organizacion = contacto.Organizacion;
                        contactoExistente.Cargo = contacto.Cargo;
                        contactoExistente.Direccion = contacto.Direccion;
                        contactoExistente.Pais = contacto.Pais;
                        contactoExistente.Poblacion = contacto.Poblacion;
                        contactoExistente.CP = contacto.CP;
                        contactoExistente.TelefonoFijoEmpresa = contacto.TelefonoFijoEmpresa;
                        contactoExistente.TelefonoMovilEmpresa = contacto.TelefonoMovilEmpresa;
                        contactoExistente.TelefonoMovilPersonal = contacto.TelefonoMovilPersonal;
                        contactoExistente.FechaNacimiento = contacto.FechaNacimiento;
                        contactoExistente.Notas = contacto.Notas;
                        contactoExistente.Web = contacto.Web;

                        contactoExistente.IsoUser = User?.Identity?.Name ?? "Unknown";
                        contactoExistente.IsoFecMod = DateTime.Now;

                        ctxDB.Contactos.Update(contactoExistente);
                        await ctxDB.SaveChangesAsync();


                        // Devolver la lista actualizada de contactos tras la edición
                        var updatedContacts = await ctxDB.Contactos
                            .Where(c => c.Empresa == GrupoClaims.SessionEmpresa && c.Activo == true)
                            .OrderBy(c => c.Nombre)
                            .ToListAsync();

                        return Json(new { success = true, message = "Contacto actualizado exitosamente.", contactos = updatedContacts });
                    }
                    catch (Exception ex)
                    {
                        Log($"Error al actualizar contacto: {ex.Message}");
                        Log($"StackTrace: {ex.StackTrace}");
                        return Json(new { success = false, message = $"Error al actualizar contacto: {ex.Message}" });
                    }
                }
            }
            catch (Exception e)
            {
                Log($"Error en CrearEditarContacto: {e.Message}");
                Log($"StackTrace: {e.StackTrace}");
                return Json(new { success = false, message = $"Error: {e.Message}" });
            }
        }


        [HttpPost]

        public async Task<IActionResult> EliminarContacto(Guid id)
        {
            var contacto = await ctxDB.Contactos.FindAsync(id);
            if (contacto == null)
            {
                return Json(new { success = false, message = "Contacto no encontrado" });
            }

            ctxDB.Contactos.Remove(contacto);
            await ctxDB.SaveChangesAsync();

            return Json(new { success = true, message = "Contacto eliminado Correctamente" });
        }

    }
}

