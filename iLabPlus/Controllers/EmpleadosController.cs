
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iLabPlus.Controllers
{
    [Authorize]
    public class EmpleadosController : Controller
    {

        private readonly DbContextiLabPlus ctxDB;
        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;


        public EmpleadosController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB = Context;
            FunctionsBBDD = _FunctionsBBDD;
            GrupoClaims = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridEmpleados");
        }


        public IActionResult Index()
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;


            var Empleados = ctxDB.Empleados.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Empleado).ToList();

            return View("Empleados", Empleados);
        }

        public IActionResult DialogEmpleado(Guid Guid)
        {
            var Empleado = ctxDB.Empleados.Where(x => x.Guid == Guid).FirstOrDefault();
            if (Empleado == null)
            {
                Empleado = new Empleados();
            } 

            var EmpleadoTipo = new List<string>();
            EmpleadoTipo.Add("");
            EmpleadoTipo.Add("Dirección");
            EmpleadoTipo.Add("Administración");
            EmpleadoTipo.Add("Jefe de Sección");
            EmpleadoTipo.Add("Operario");
            EmpleadoTipo.Add("Finanzas");
            EmpleadoTipo.Add("Marketing");
            EmpleadoTipo.Add("Almacén");
            ViewBag.ListEmpleadoTipo = EmpleadoTipo;

            return PartialView("_DialogEmpleado", Empleado);
        }


        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> CreateEdit(Empleados RowEmpleado, IFormFile EmpImagenRecortada)
        {
            bool resultProcess = false;

            try
            {
                if (RowEmpleado.Guid == Guid.Empty)
                {
                    // Calcular el siguiente número de empleado con 3 dígitos, sólo para códigos numéricos
                    var maxEmpleado = ctxDB.Empleados
                        .Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
                        .ToList() // Cargar los empleados en memoria
                        .Where(x => IsNumeric(x.Empleado)) // Filtrar por códigos numéricos
                        .OrderByDescending(x => int.Parse(x.Empleado))
                        .Select(x => x.Empleado)
                        .FirstOrDefault();

                    int nextNumber = 1; // Número inicial si no hay empleados
                    if (!string.IsNullOrEmpty(maxEmpleado))
                    {
                        nextNumber = int.Parse(maxEmpleado) + 1; // Incrementar el número
                    }

                    RowEmpleado.Empleado = nextNumber.ToString("D3"); // Formatear con 3 dígitos (por ejemplo, "001")

                    var FindEmp = ctxDB.Empleados
                        .Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Empleado == RowEmpleado.Empleado)
                        .FirstOrDefault();

                    if (FindEmp == null)
                    {
                        // Creación del nuevo empleado
                        RowEmpleado.Empresa = GrupoClaims.SessionEmpresa;
                        RowEmpleado.IsoUser = GrupoClaims.SessionUsuarioNombre;
                        RowEmpleado.IsoFecAlt = DateTime.Now;
                        RowEmpleado.IsoFecMod = DateTime.Now;

                        // Procesar la imagen si está presente
                        if (EmpImagenRecortada != null)
                        {
                            var imagenRuta = GuardarArchivoImagen(EmpImagenRecortada);
                            RowEmpleado.EmpImagen = imagenRuta;
                        }

                        ctxDB.Empleados.Add(RowEmpleado);
                        await ctxDB.SaveChangesAsync();
                        resultProcess = true;
                    }
                    else
                    {
                        return StatusCode(200, "EXIST");
                    }
                }
                else
                {
                    // Actualización de un empleado existente
                    var empleadoToUpdate = await ctxDB.Empleados.FindAsync(RowEmpleado.Guid);
                    if (empleadoToUpdate != null)
                    {
                        // Actualizar los campos necesarios
                        empleadoToUpdate.EmpTipo = RowEmpleado.EmpTipo;
                        empleadoToUpdate.EmpNombre = RowEmpleado.EmpNombre;
                        empleadoToUpdate.EmpNIF = RowEmpleado.EmpNIF;
                        empleadoToUpdate.EmpDireccion = RowEmpleado.EmpDireccion;
                        empleadoToUpdate.EmpDirDP = RowEmpleado.EmpDirDP;
                        empleadoToUpdate.EmpPassword = RowEmpleado.EmpPassword;

                        // Procesar la imagen si se subió una nueva
                        if (EmpImagenRecortada != null)
                        {
                            var imagenRuta = GuardarArchivoImagen(EmpImagenRecortada);
                            empleadoToUpdate.EmpImagen = imagenRuta;
                        }

                        empleadoToUpdate.IsoUser = GrupoClaims.SessionUsuarioNombre;
                        empleadoToUpdate.IsoFecMod = DateTime.Now;

                        ctxDB.Empleados.Update(empleadoToUpdate);
                        await ctxDB.SaveChangesAsync();
                        resultProcess = true;
                    }
                }
            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);
            }

            var Result = ctxDB.Empleados.Where(x => x.Guid == RowEmpleado.Guid).FirstOrDefault();

            if (resultProcess)
            {
                return StatusCode(200, Result);
            }
            else
            {
                return StatusCode(400, null);
            }
        }

        // Método auxiliar para verificar si una cadena es numérica
        private bool IsNumeric(string str)
        {
            return str.All(char.IsDigit);
        }


        private string GuardarArchivoImagen(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return null;

            // Obtener la ruta del directorio padre del directorio actual del proyecto.
            var ParentPathFic = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();

            // Construir las rutas necesarias.
            var PathDoc = Path.Combine(ParentPathFic, "iLabPlusDocs");
            var PathDocEmp = Path.Combine(PathDoc, GrupoClaims.SessionEmpresa);
            var PathDirFic = Path.Combine(PathDocEmp, "ImagenEmpleados");

            // Asegurarse de que los directorios existan.
            if (!Directory.Exists(PathDoc))
            {
                Directory.CreateDirectory(PathDoc);
            }
            if (!Directory.Exists(PathDocEmp))
            {
                Directory.CreateDirectory(PathDocEmp);
            }
            if (!Directory.Exists(PathDirFic))
            {
                Directory.CreateDirectory(PathDirFic);
            }

            // Crear una ruta única para cada archivo para evitar sobrescribir archivos con el mismo nombre.
            // Importante: usar PathDirFic como base para la ruta del archivo.
            var rutaArchivo = Path.Combine(PathDirFic, Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName));

            // Guardar el archivo.
            using (var fileStream = new FileStream(rutaArchivo, FileMode.Create))
            {
                archivo.CopyTo(fileStream);
            }

            // Devolver la ruta relativa del archivo (o la ruta absoluta si es necesario).
            return rutaArchivo;
        }



        [HttpPost]
        public async Task<IActionResult> Delete_Empleado(Guid Guid)
        {
            var EmpFind = ctxDB.Empleados.Where(x => x.Guid == Guid).FirstOrDefault();
            if (EmpFind != null)
            {
                try
                {
                    ctxDB.Empleados.Remove(EmpFind);
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
        public async Task<IActionResult> Empleado_Copiar(string EmpleadoOld, string EmpleadoNew)
        {
            // Verificar si el nuevo empleado ya existe
            var findEmpleadoNew = ctxDB.Empleados.FirstOrDefault(e => e.Empresa == GrupoClaims.SessionEmpresa && e.Empleado == EmpleadoNew);
            if (findEmpleadoNew != null)
            {
                return Json(new { success = false, message = "El empleado nuevo ya existe." });
            }

            // Buscar el empleado original
            var empleadoOriginal = ctxDB.Empleados.FirstOrDefault(e => e.Empresa == GrupoClaims.SessionEmpresa && e.Empleado == EmpleadoOld);
            if (empleadoOriginal == null)
            {
                return Json(new { success = false, message = "Empleado original no encontrado." });
            }

            try
            {

                // Crear una copia del cliente original
                var nuevoEmpleado = empleadoOriginal.CloneAndModify(v =>
                {
                    v.Guid = Guid.NewGuid();
                    v.Empleado = EmpleadoNew;
                    v.IsoUser = GrupoClaims.SessionUsuarioNombre;
                    v.IsoFecAlt = DateTime.Now;
                    v.IsoFecMod = DateTime.Now;
                });

                ctxDB.Empleados.Add(nuevoEmpleado);
                await ctxDB.SaveChangesAsync();

                return Json(new { success = true, data = nuevoEmpleado });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Ocurrió un error durante la copia del empleado." });
            }
        }

        public IActionResult _DialogEmpleadoCopiar(string Empleado)
        {
            ViewBag.EmpleadoOld = Empleado;
            return PartialView("_DialogEmpleadoCopiar");
        }

        [HttpPost]
        public async Task<IActionResult> Empleado_Renombrar(string EmpleadoOld, string EmpleadoNew)
        {
            // Verificar si el nuevo nombre de empleado ya existe
            var findEmpleadoNew = ctxDB.Empleados.FirstOrDefault(e => e.Empresa == GrupoClaims.SessionEmpresa && e.Empleado == EmpleadoNew);
            if (findEmpleadoNew != null)
            {
                return Json(new { success = false, message = "El nuevo nombre de empleado ya existe." });
            }

            // Buscar el empleado original
            var empleadoOriginal = ctxDB.Empleados.FirstOrDefault(e => e.Empresa == GrupoClaims.SessionEmpresa && e.Empleado == EmpleadoOld);
            if (empleadoOriginal == null)
            {
                return Json(new { success = false, message = "Empleado original no encontrado." });
            }

            try
            {
                // Actualizar el nombre del empleado
                empleadoOriginal.Empleado = EmpleadoNew;
                empleadoOriginal.IsoUser = GrupoClaims.SessionUsuarioNombre;
                empleadoOriginal.IsoFecMod = DateTime.Now;

                ctxDB.Empleados.Update(empleadoOriginal);
                await ctxDB.SaveChangesAsync();

                return Json(new { success = true, data = empleadoOriginal });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Ocurrió un error durante el cambio de nombre del empleado." });
            }
        }

        public IActionResult _DialogEmpleadoRenombrar(string Empleado)
        {
            ViewBag.EmpleadoOld = Empleado;
            return PartialView("_DialogEmpleadoRenombrar");
        }

    }

}