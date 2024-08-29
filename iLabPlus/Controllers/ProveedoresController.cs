using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using C1.Web.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Newtonsoft.Json;


namespace iLabPlus.Controllers
{
    [Authorize]
    public class ProveedoresController : Controller
    {

        private readonly DbContextiLabPlus   ctxDB;

        private readonly FunctionsBBDD      FunctionsBBDD;
        private readonly GrupoClaims        GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;

        public ProveedoresController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB               = Context;
            
            FunctionsBBDD       = _FunctionsBBDD;
            GrupoClaims         = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridProveedores");

        }


        public IActionResult DialogProveedor(Guid Guid)
        {
            var Proveedor = ctxDB.Proveedores.Where(x => x.Guid == Guid).FirstOrDefault();
            if (Proveedor == null)
            {
                Proveedor = new Proveedores();
            }

            var Idiomas = new List<string>();
            Idiomas.Add("Español");
            Idiomas.Add("Ingles");
            ViewBag.ListIdiomas = Idiomas;

            var Divisas = ctxDB.Divisas.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Divisa).ToList();
            ViewBag.ListDivisas = Divisas;

            var ProvTipo = new List<string>();
            ProvTipo.Add("");
            ProvTipo.Add("Materias Primas");
            ProvTipo.Add("Servicios");
            ProvTipo.Add("Insumos");
            ViewBag.ListProvTipo = ProvTipo;


            //var xxxx = PartialView("_DialogProveedor", Proveedor);

            return PartialView("_DialogProveedor", Proveedor);
        }

        public IActionResult Index()
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;


            var Proveedores = ctxDB.Proveedores.Where(x=>x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x=>x.Proveedor).ToList();

            return View("Proveedores", Proveedores);
        }

        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> CreateEdit(Proveedores RowProveedor)
        {

            bool resultProcess = false;

            try
            {
                if (RowProveedor.Guid == Guid.Empty)
                {

                    // Calcular el siguiente número de cliente con 6 dígitos, sólo para códigos numéricos
                    var maxProveedor = ctxDB.Proveedores
                       .Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
                       .ToList() // Cargar los proveedores en memoria
                       .Where(x => IsNumeric(x.Proveedor)) // Filtrar por códigos numéricos usando un método auxiliar
                       .OrderByDescending(x => int.Parse(x.Proveedor))
                       .Select(x => x.Proveedor)
                       .FirstOrDefault();

                    int nextNumber = 1; // Número inicial si no hay clientes
                    if (!string.IsNullOrEmpty(maxProveedor))
                    {
                            nextNumber = int.Parse(maxProveedor) + 1; // Incrementar el número
                    }

                    RowProveedor.Proveedor = nextNumber.ToString("D6"); // Formatear con 6 dígitos (por ejemplo, "000001")


                    var FindClient = ctxDB.Proveedores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Proveedor == RowProveedor.Proveedor).FirstOrDefault();
                    if (FindClient == null)
                    {
                        // Creacion
                        RowProveedor.Empresa   = GrupoClaims.SessionEmpresa;
                        RowProveedor.IsoUser   = GrupoClaims.SessionUsuarioNombre;
                        RowProveedor.IsoFecAlt = DateTime.Now;
                        RowProveedor.IsoFecMod = DateTime.Now;

                        ctxDB.Proveedores.Add(RowProveedor);
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
                    // Edicion
                    RowProveedor.IsoUser    = GrupoClaims.SessionUsuarioNombre;
                    RowProveedor.IsoFecMod  = DateTime.Now;

                    ctxDB.Proveedores.Update(RowProveedor);
                    await ctxDB.SaveChangesAsync();
                    resultProcess = true;
                }
                

            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);  //  Send "Error"
            }

            var Result = ctxDB.Proveedores.Where(x => x.Guid == RowProveedor.Guid).FirstOrDefault();

            if (resultProcess)
            { 
                return StatusCode(200, Result); 
            }else
            { 
                return StatusCode(400, null); 
            }

        }

        // Método auxiliar para verificar si una cadena es numérica
        private bool IsNumeric(string str)
        {
            return str.All(char.IsDigit);
        }

        [HttpPost]
        public async Task<IActionResult> Delete_PROVEEDOR(Guid Guid)
        {
            var ProFind = ctxDB.Proveedores.Where(x => x.Guid == Guid).FirstOrDefault();
            if (ProFind != null)
            {
                try
                {
                    ctxDB.Proveedores.Remove(ProFind);
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


        [HttpGet]
        public IActionResult _DialogProveedorCopiar(string Proveedor)
        {

            ViewBag.ProveedorOld = Proveedor;

            return PartialView("_DialogProveedorCopiar");
        }

        [HttpGet]
        public IActionResult _DialogProveedorRenombrar(string Proveedor)
        {
            ViewBag.ProveedorOld = Proveedor;

            return PartialView("_DialogProveedorRenombrar");
        }

        [HttpPost]
        public async Task<IActionResult> Proveedor_Copiar(string ProveedorOld, string ProveedorNew)
        {
            try
            {
                // Verificar si el nuevo proveedor ya existe
                var proveedorExistente = await ctxDB.Proveedores
                    .FirstOrDefaultAsync(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Proveedor == ProveedorNew);

                if (proveedorExistente != null)
                {
                    return Json(new { success = false, message = "El código del nuevo proveedor ya existe." });
                }

                // Obtener el proveedor original
                var proveedorOriginal = await ctxDB.Proveedores
                    .FirstOrDefaultAsync(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Proveedor == ProveedorOld);

                if (proveedorOriginal == null)
                {
                    return Json(new { success = false, message = "No se encontró el proveedor original." });
                }

                // Crear una nueva instancia de Proveedores con los datos del proveedor original
                var nuevoProveedor = new Proveedores
                {
                    Guid = Guid.NewGuid(),
                    Empresa = proveedorOriginal.Empresa,
                    Proveedor = ProveedorNew,
                    ProNombre = proveedorOriginal.ProNombre,
                    ProRazon = proveedorOriginal.ProRazon,

                    IsoUser = GrupoClaims.SessionUsuarioNombre,
                    IsoFecAlt = DateTime.Now,
                    IsoFecMod = DateTime.Now
                };

                // Agregar el nuevo proveedor a la base de datos
                ctxDB.Proveedores.Add(nuevoProveedor);
                await ctxDB.SaveChangesAsync();

                return Json(new { success = true, data = nuevoProveedor });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al copiar el proveedor: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Proveedor_Renombrar(string ProveedorOld, string ProveedorNew)
        {
            try
            {
                // Verificar si el nuevo nombre de proveedor ya existe
                var proveedorExistente = await ctxDB.Proveedores
                    .FirstOrDefaultAsync(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Proveedor == ProveedorNew);

                if (proveedorExistente != null)
                {
                    return Json(new { success = false, message = "El nuevo código de proveedor ya existe." });
                }

                // Obtener el proveedor a renombrar
                var proveedor = await ctxDB.Proveedores
                    .FirstOrDefaultAsync(p => p.Empresa == GrupoClaims.SessionEmpresa && p.Proveedor == ProveedorOld);

                if (proveedor == null)
                {
                    return Json(new { success = false, message = "No se encontró el proveedor." });
                }

                // Actualizar el código del proveedor
                proveedor.Proveedor = ProveedorNew;
                proveedor.IsoUser = GrupoClaims.SessionUsuarioNombre;
                proveedor.IsoFecMod = DateTime.Now;

                await ctxDB.SaveChangesAsync();

                return Json(new { success = true, data = proveedor });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al renombrar el proveedor: {ex.Message}" });
            }
        }

    }

}