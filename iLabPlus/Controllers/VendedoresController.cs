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


namespace Nemesis365.Controllers
{
    [Authorize]
    public class VendedoresController : Controller
    {

        private readonly DbContextiLabPlus   ctxDB;

        private readonly FunctionsBBDD      FunctionsBBDD;
        private readonly GrupoClaims        GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;


        public VendedoresController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB               = Context;
            
            FunctionsBBDD       = _FunctionsBBDD;
            GrupoClaims         = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout  = FunctionsBBDD.GetColumnsLayout("gridVendedores");
        }


        public IActionResult Index()
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;


            var Vendedores = ctxDB.Vendedores.Where(x=>x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x=>x.Vendedor).ToList();

            return View("Vendedores", Vendedores);
        }

        public IActionResult DialogVendedor(Guid Guid)
        {
            var Vendedor = ctxDB.Vendedores.Where(x => x.Guid == Guid).FirstOrDefault();
            if (Vendedor == null)
            {
                Vendedor = new Vendedores();
            }

            return PartialView("_DialogVendedor", Vendedor);
        }


        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/


        [HttpPost]
        public async Task<IActionResult> CreateEdit(Vendedores RowVendedor)
        {
            bool resultProcess = false;

            try
            {
                if (RowVendedor.Guid == Guid.Empty)
                {
                    var FindVen = ctxDB.Vendedores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Vendedor == RowVendedor.Vendedor).FirstOrDefault();                    
                    if (FindVen == null)
                    {
                        // Creacion
                        RowVendedor.Empresa    = GrupoClaims.SessionEmpresa;
                        RowVendedor.IsoUser    = GrupoClaims.SessionUsuarioNombre;
                        RowVendedor.IsoFecAlt  = DateTime.Now;

                        // Generar código de vendedor automático
                        var maxVendedor = ctxDB.Vendedores
                            .Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
                            .OrderByDescending(x => x.Vendedor)
                            .Select(x => x.Vendedor)
                            .FirstOrDefault();

                        int newVendedorNumber = 1;
                        if (!string.IsNullOrEmpty(maxVendedor))
                        {
                            if (int.TryParse(maxVendedor.Substring(1), out int lastVendedorNumber))
                            {
                                newVendedorNumber = lastVendedorNumber + 1;
                            }
                        }
                        RowVendedor.Vendedor = $"V{newVendedorNumber:D3}";

                        ctxDB.Vendedores.Add(RowVendedor);
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
                    RowVendedor.IsoUser    = GrupoClaims.SessionUsuarioNombre;
                    RowVendedor.IsoFecMod  = DateTime.Now;

                    ctxDB.Vendedores.Update(RowVendedor);
                    await ctxDB.SaveChangesAsync();
                    resultProcess = true;
                }

            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);  //  Send "Error"
            }


            var Result = ctxDB.Vendedores.Where(x => x.Guid == RowVendedor.Guid).FirstOrDefault();

            if (resultProcess)
            {
                return StatusCode(200, Result);
            }
            else
            {
                return StatusCode(400, null);
            }

        }


        [HttpPost]
        public async Task<IActionResult> Delete_VENDEDOR(Guid Guid)
        {
            var VenFind = ctxDB.Vendedores.Where(x => x.Guid == Guid).FirstOrDefault();
            if (VenFind != null)
            {
                try
                {
                    ctxDB.Vendedores.Remove(VenFind);
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
        public IActionResult _DialogVendedorCopiar(string Vendedor)
        {
            ViewBag.VendedorOld = Vendedor;
            return PartialView("_DialogVendedorCopiar");
        }


        [HttpPost]
        public async Task<IActionResult> Vendedor_Copiar(string VendedorOld, string VendedorNew)
        {
            try
            {
                // Verificar si el nuevo vendedor ya existe
                var vendedorExistente = await ctxDB.Vendedores
                    .FirstOrDefaultAsync(v => v.Empresa == GrupoClaims.SessionEmpresa && v.Vendedor == VendedorNew);

                if (vendedorExistente != null)
                {
                    return Json(new { success = false, message = "El código del nuevo vendedor ya existe." });
                }

                // Obtener el vendedor original
                var vendedorOriginal = await ctxDB.Vendedores
                    .FirstOrDefaultAsync(v => v.Empresa == GrupoClaims.SessionEmpresa && v.Vendedor == VendedorOld);

                if (vendedorOriginal == null)
                {
                    return Json(new { success = false, message = "No se encontró el vendedor original." });
                }

                // Crear una nueva instancia de Vendedores con los datos del vendedor original
                var nuevoVendedor = vendedorOriginal.CloneAndModify(v =>
                {
                    v.Guid = Guid.NewGuid();
                    v.Vendedor = VendedorNew;
                    v.IsoUser = GrupoClaims.SessionUsuarioNombre;
                    v.IsoFecAlt = DateTime.Now;
                    v.IsoFecMod = DateTime.Now;
                });

                // Agregar el nuevo vendedor a la base de datos
                ctxDB.Vendedores.Add(nuevoVendedor);
                await ctxDB.SaveChangesAsync();

                return Json(new { success = true, data = nuevoVendedor });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al copiar el vendedor: {ex.Message}" });
            }
        }

    }

}