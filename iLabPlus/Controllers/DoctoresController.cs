using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iLabPlus.Controllers
{
    // [Authorize]
    public class DoctoresController : Controller
    {
        private readonly DbContextiLabPlus ctxDB;
        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;


        public DoctoresController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB = Context;
            FunctionsBBDD = _FunctionsBBDD;
            GrupoClaims = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridDoctores"); // ATENCION AQUÍ

        }

        public IActionResult Index()
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;


            var Doctores = ctxDB.Doctores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Doctor).ToList();

            return View("Doctores", Doctores);
        }

        public IActionResult DialogDoctor(Guid? guid)
        {
            var Doctor = ctxDB.Doctores.Where(x => x.Guid == guid).FirstOrDefault() ?? new Doctores();

            ViewBag.ListaClinicas = ctxDB.Clientes.Select(c => new SelectListItem 
            { 
                Text = $"({c.Cliente}) - {c.CliNombre}", 
                Value = c.Guid.ToString() 
            }).ToList();

            ViewBag.ListActivoTipo = new List<SelectListItem>
            {
                new SelectListItem { Text = "Sí", Value = "true" },
                new SelectListItem { Text = "No", Value = "false" }
            };

            return PartialView("_DialogDoctor", Doctor);
        }




        [HttpPost]
        public async Task<IActionResult> CreateEdit(Doctores RowDoctor)
        {

            bool resultProcess = false;

            try
            {
                if (RowDoctor.Guid == Guid.Empty)
                {
                    // Si el valor es "Automático", calcula el correcto número de empleado
                    if (RowDoctor.Doctor == "Automático" || string.IsNullOrEmpty(RowDoctor.Doctor))
                    {
                        var maxDoctor = ctxDB.Doctores
                            .Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
                            .OrderByDescending(x => x.Doctor)
                            .Select(x => x.Doctor)
                            .FirstOrDefault();

                        var maxNumber = 0;
                        if (maxDoctor != null && int.TryParse(maxDoctor, out maxNumber))
                        {
                            RowDoctor.Doctor = (maxNumber + 1).ToString("D3");
                        }
                        else
                        {
                            RowDoctor.Doctor = "001"; // Primer empleado si no hay otros
                        }
                    }

                    var FindDoc = ctxDB.Doctores
                      .Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Doctor == RowDoctor.Doctor)
                      .FirstOrDefault();


                    if (FindDoc == null)
                    {


                        // Configuración de datos iniciales para un nuevo doctor
                        RowDoctor.Empresa = GrupoClaims.SessionEmpresa;
                        RowDoctor.IsoUser = GrupoClaims.SessionUsuarioNombre;
                        RowDoctor.IsoFecAlt = DateTime.Now;

                        // Agregar nuevo doctor
                        ctxDB.Doctores.Add(RowDoctor);
                        await ctxDB.SaveChangesAsync();
                        resultProcess = true;
                        // resultProcess = true;
                    }
                    else
                    {
                        return StatusCode(200, "EXIST");
                    }

                }
                else
                {
                    // Buscar doctor existente para actualizar
                    var existingDoctor = await ctxDB.Doctores.FindAsync(RowDoctor.Guid);
                    if (existingDoctor == null)
                    {
                        return NotFound();
                    }

                    // Actualizar campos del doctor existente
                    existingDoctor.Nombre = RowDoctor.Nombre;
                    existingDoctor.NumColegiado = RowDoctor.NumColegiado;
                    existingDoctor.NIF = RowDoctor.NIF;
                    existingDoctor.Mail = RowDoctor.Mail;
                    existingDoctor.DirDireccion = RowDoctor.DirDireccion;
                    existingDoctor.DirDP = RowDoctor.DirDP;
                    existingDoctor.DirPoblacion = RowDoctor.DirPoblacion;
                    existingDoctor.DirProvincia = RowDoctor.DirProvincia;
                    existingDoctor.Telefono1 = RowDoctor.Telefono1;
                    existingDoctor.Observ = RowDoctor.Observ;
                    existingDoctor.Activo = RowDoctor.Activo;
                    existingDoctor.IsoUser = GrupoClaims.SessionUsuarioNombre;
                    existingDoctor.IsoFecMod = DateTime.Now;

                    ctxDB.Doctores.Update(existingDoctor);
                    await ctxDB.SaveChangesAsync();
                    resultProcess = true;
                }

                var updatedDoctor = await ctxDB.Doctores.AsNoTracking().FirstOrDefaultAsync(d => d.Guid == RowDoctor.Guid);


                return Ok(new
                {
                    success = true,
                    Guid = updatedDoctor.Guid,
                    Doctor = updatedDoctor.Doctor,
                    Nombre = updatedDoctor.Nombre,
                    NumColegiado = updatedDoctor.NumColegiado,
                    NIF = updatedDoctor.NIF,
                    Mail = updatedDoctor.Mail,
                    DirDireccion = updatedDoctor.DirDireccion,
                    DirDP = updatedDoctor.DirDP,
                    DirPoblacion = updatedDoctor.DirPoblacion,
                    DirProvincia = updatedDoctor.DirProvincia,
                    Telefono1 = updatedDoctor.Telefono1,
                    Observ = updatedDoctor.Observ,
                    Activo = updatedDoctor.Activo,
                    IsoUser = updatedDoctor.IsoUser,
                    IsoFecAlt = updatedDoctor.IsoFecAlt,
                    IsoFecMod = updatedDoctor.IsoFecMod
                });
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Algo fue como no debía: {ex}");

                // Devolver un error HTTP 500 y loguear la excepción
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        public async Task<IActionResult> GetClinicasAsociadas(string doctorCode)
        {
            try {

            var asociaciones = await ctxDB.DoctoresClinicas
                                          .Where(dc => dc.Doctor == doctorCode) // Asumiendo que Doctor es el número de colegiado
                                          .Join(ctxDB.Clientes,
                                                dc => dc.Clinica,    // Nombre de la clínica en DoctoresClinicas
                                                c => c.CliNombre,    // Nombre de la clínica en Clientes
                                                (dc, c) => new       // Selección de campos a incluir en el resultado final
                                                {
                                                    Guid = dc.Guid,
                                                    NombreClinica = c.CliNombre,
                                                    DoctorColegiado = dc.Doctor,
                                                    RazonSocial = c.CliRazon  // Campo Razón Social de la tabla Clientes
                                                })
                                          .ToListAsync();

            return Json(new { success = true, clinicas = asociaciones });

            }

            catch (Exception ex)

            {

                return Json(new { success = false, message = "Error al obtener las clínicas asociadas. " + ex.Message

            });

            }
        }


        [HttpPost]
        public async Task<IActionResult> AsociarDoctorClinica(string doctorCode, Guid clinicaId)
        {

            try
            {

                // Obtengo los datos que necesitaré
                var doctor = await ctxDB.Doctores.FirstOrDefaultAsync(d => d.Doctor == doctorCode);
                var clinica = await ctxDB.Clientes.FindAsync(clinicaId);


                if (doctor == null || clinica == null)
            {
                return Json(new { success = false, message = "Doctor o clínica no encontrados." });
            }


            var existe = await ctxDB.DoctoresClinicas.AnyAsync(dc => dc.Doctor == doctor.Doctor && dc.Clinica == clinica.CliNombre);
            if (!existe)
            {
                var asociacion = new DoctoresClinicas
                {
                    // Guid = Guid.NewGuid(),
                    Empresa = GrupoClaims.SessionEmpresa, // Asegúrate de ajustar esto según tu lógica de negocio
                    Doctor = doctor.Doctor,
                    Clinica = clinica.CliNombre,
                    IsoUser = User.Identity.Name,
                    IsoFecAlt = DateTime.Now,
                    IsoFecMod = DateTime.Now
                };

                ctxDB.DoctoresClinicas.Add(asociacion);
                await ctxDB.SaveChangesAsync();
                    return Json(new { success = true, 
                        clinica = new { cliente = doctor.Doctor, 
                            cliNombre = clinica.CliNombre, 
                            cliRazon = clinica.CliRazon, 
                            guid = asociacion.Guid 
                        },
                        doctor = new { doctor = doctor.Doctor,
                            nombre = doctor.Nombre,
                            numColegiado = doctor.NumColegiado,
                            nif = doctor.NIF,
                            mail = doctor.Mail,
                            telefono1 = doctor.Telefono1,
                            activo = doctor.Activo,
                            guid = asociacion.Guid
                        }
                    });

                }

                return Json(new { success = false, message = "La asociación ya existe." });

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Error al asociar doctor con clinica: {ex.Message}");

                // Aquí capturas cualquier error que ocurra durante la transacción
                return Json(new { success = false, message = ex.Message });

            }
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarAsociacion(Guid asociacionId) 
        {
            try
            {
                var asociacion = await ctxDB.DoctoresClinicas.FindAsync(asociacionId);
                if (asociacion == null)
                {
                    return Json(new { success = false, message = "Asociación no encontrada." });
                }

                ctxDB.DoctoresClinicas.Remove(asociacion);
                await ctxDB.SaveChangesAsync();
                return Json(new { success = true, message = "Asociación eliminada correctamente." });

            }

            catch (Exception ex)

            {
                // Log the error
                Console.WriteLine($"Algo no fue como debía: {ex}");

                // Devolver un error HTTP 500 y loguear la excepción
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }



         
        [HttpPost]
        public async Task<IActionResult> Delete_Doctor(Guid guid)
        {
            try
            {
                var doctor = await ctxDB.Doctores.Where(x => x.Guid == guid).FirstOrDefaultAsync() ?? null;
                if (doctor == null)
                {
                    return NotFound();
                }

                ctxDB.Doctores.Remove(doctor);
                await ctxDB.SaveChangesAsync();

                return Ok(new { success = true, message = "Doctor eliminado correctamente." });
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Something went wrong: {ex}");

                // Devolver un error HTTP 500 y loguear la excepción
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }





    }
}