using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iLabPlus.Helpers;
using iLabPlus.Models;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iLabPlus.Controllers
{
    [Authorize]

    public class ControlHorarioController : Controller
    {

        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly DbContextiLabPlus ctxDB;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;

        public ControlHorarioController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {

            ctxDB = Context;
            FunctionsBBDD = _FunctionsBBDD;
            GrupoClaims = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridControlHorarioAdmin");


        }

        public IActionResult Index()
        {

            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;

            return View("ControlHorario");

        }

        public IActionResult GetEmpleados()
        {
            var empleados = ctxDB.Empleados
                .Where(e => e.Empresa == GrupoClaims.SessionEmpresa)
                .Select(e => new {e.Empleado, e.EmpNombre})
                .ToList();

            return new JsonResult(empleados);
        }

        public IActionResult ControlHorarioAdmin()
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;

            var RegistrosHorarios = ctxDB.ControlHorario
                .Where(r => r.Empresa == GrupoClaims.SessionEmpresa)
                .Select(r => new ControlHorario
                {
                    Guid = r.Guid,
                    Empresa = r.Empresa,
                    Empleado = r.Empleado,
                    Fecha = r.Fecha,
                    HoraEntrada = r.HoraEntrada,
                    HoraSalida = r.HoraSalida,
                    HorasTrabajadas = r.HorasTrabajadas,
                    Cierre = r.Cierre,
                    Observaciones = r.Observaciones,

                    // Rellena el nombre del empleado
                    EmpleadoNombre = ctxDB.Empleados
                .Where(e => e.Empleado == r.Empleado && e.Empresa == r.Empresa)
                .Select(e => e.EmpNombre) 
                .FirstOrDefault()
                })
                .OrderByDescending(e => e.Fecha)
                .ThenByDescending(e => e.HoraEntrada)
                .ToList();

            return View("ControlHorarioAdmin", RegistrosHorarios);

        }


        public IActionResult DialogControlHorarioAdmin(Guid Guid)
        {
            var RegistroHorario = ctxDB.ControlHorario
                .Where(x => x.Guid == Guid)
                .Select(r => new ControlHorario
                {
                    // Propiedades existentes...
                    Guid = r.Guid,
                    Empresa = r.Empresa,
                    Empleado = r.Empleado,
                    Fecha = r.Fecha,
                    HoraEntrada = r.HoraEntrada,
                    HoraSalida = r.HoraSalida,
                    HorasTrabajadas = r.HorasTrabajadas,
                    Cierre = r.Cierre,
                    Observaciones = r.Observaciones,

                    // Relleno el nombre del empleado
                    EmpleadoNombre = ctxDB.Empleados
                .Where(e => e.Empleado == r.Empleado && e.Empresa == r.Empresa)
                .Select(e => e.EmpNombre)
                .FirstOrDefault()
                })
                .FirstOrDefault();

            if (RegistroHorario == null)
            {
                RegistroHorario = new ControlHorario();
            }

            return PartialView("_DialogControlHorarioAdmin", RegistroHorario);
        }



        [HttpPost]
        public async Task<IActionResult> RegistroControlHorario(string mensajeQR)
        {
            // Descompongo el mensaje QR y validadr si es un empleado
            var partesQR = mensajeQR.Split('%', StringSplitOptions.RemoveEmptyEntries);
            if (partesQR.Length < 2 || partesQR[0] != "25")
            {
                return Json(new { success = false, message = "No es un empleado registrado por la empresa" });
            }

            string codigoEmpleado = partesQR[1];
            // Verifico si el empleado existe
            var empleado = ctxDB.Empleados.Where(e => e.Empleado == codigoEmpleado && e.Empresa == GrupoClaims.SessionEmpresa).FirstOrDefault();
            if (empleado == null)
            {
                return Json(new { success = false, message = "El empleado no existe" });
            }

            // Verificar si ya existe un registro abierto para hoy
            var fechaHoy = DateTime.Today;
            var registroHoy = ctxDB.ControlHorario.Where(r => r.Empleado == codigoEmpleado && r.Empresa == GrupoClaims.SessionEmpresa && r.Fecha == fechaHoy && r.HoraSalida == null).FirstOrDefault();

            if (registroHoy == null)
            {
                // Creo un nuevo registro de entrada
                var nuevoRegistro = new ControlHorario
                {
                    Guid = Guid.NewGuid(),
                    Empresa = empleado.Empresa,
                    Empleado = empleado.Empleado,
                    Fecha = fechaHoy,
                    HoraEntrada = DateTime.Now.TimeOfDay,
                    HoraSalida = null
                };

                ctxDB.ControlHorario.Add(nuevoRegistro);

                // Cerrar registros abiertos de otros días
                var registrosAbiertos = ctxDB.ControlHorario.Where(r => r.Empleado == codigoEmpleado && r.Empresa == GrupoClaims.SessionEmpresa && r.Fecha < fechaHoy && r.HoraSalida == null);
                foreach (var reg in registrosAbiertos)
                {
                    reg.HoraSalida = new TimeSpan(20, 0, 0);
                    reg.HorasTrabajadas = reg.HoraSalida.Value - reg.HoraEntrada;
                    reg.Cierre = "AUTO";
                    ctxDB.ControlHorario.Update(reg);
                }

                await ctxDB.SaveChangesAsync();
                return Json(new { success = true, message = empleado.EmpNombre + " - Hora de Entrada", hora = DateTime.Now.ToString("HH:mm") });

            }

            else

            {
                // Actualizar el registro existente con la hora de salida
                registroHoy.HoraSalida = DateTime.Now.TimeOfDay;
                registroHoy.HorasTrabajadas = registroHoy.HoraSalida.Value - registroHoy.HoraEntrada;
                ctxDB.ControlHorario.Update(registroHoy);

                await ctxDB.SaveChangesAsync();
                return Json(new { success = true, message = empleado.EmpNombre + " - Hora de Salida", hora = DateTime.Now.ToString("HH:mm") });

            }

        }

        [HttpPost]

        public async Task<IActionResult> UpdateRegistroHorario(ControlHorario registroHorario)
        {
            if (registroHorario == null || registroHorario.Guid == Guid.Empty)
            {
                return Json(new { success = false, message = "Registro no válido" });
            }

            var registroExistente = ctxDB.ControlHorario.Where(x => x.Guid == registroHorario.Guid).FirstOrDefault();
            if (registroExistente != null) 
            {
                registroExistente.Empleado = registroHorario.Empleado;
                registroExistente.Fecha = registroHorario.Fecha.Date;
                registroExistente.HoraEntrada = registroHorario.HoraEntrada;
                registroExistente.HoraSalida = registroHorario.HoraSalida;
                registroExistente.Empresa = registroHorario.Empresa;
                registroExistente.HorasTrabajadas = registroHorario.HoraSalida - registroHorario.HoraEntrada;
                registroExistente.Cierre = registroHorario.Cierre;
                registroExistente.Observaciones = registroHorario.Observaciones;

                ctxDB.Update(registroExistente);
                await ctxDB.SaveChangesAsync();
               
                return Json(new
                { 
                    success = true, 
                    message = "Registro actualizado correctamente",
                    data = new
                    {
                        //Guid = registroExistente.Guid,
                        Empleado = registroExistente.Empleado,
                        Fecha = registroExistente.Fecha.ToString("dd/MM/yyyy"),
                        HoraEntrada = registroExistente.HoraEntrada,
                        HoraSalida = registroExistente.HoraSalida,
                        Empresa = registroExistente.Empresa,
                        HorasTrabajadas = registroExistente.HorasTrabajadas, 
                        Cierre = registroExistente.Cierre,
                        Observaciones = registroExistente.Observaciones
                    }
                });

            }
            else
            {
                return Json(new {success = false, message = "Registro no encontrado"});
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRegistroHorario(Guid Guid)
        {
            var registroHorario = ctxDB.ControlHorario.Where(x => x.Guid == Guid).FirstOrDefault();
            if (registroHorario != null)
            {
                try
                {
                    ctxDB.ControlHorario.Remove(registroHorario);
                    await ctxDB.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return StatusCode(400, e.Message);
                }

                return StatusCode(200, "Registro eliminado correctamente");
            }

            return StatusCode(200, "Registro no encontrado");
        }

    }

}
