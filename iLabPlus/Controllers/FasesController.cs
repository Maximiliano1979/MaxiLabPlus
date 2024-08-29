
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
    public class FasesController : Controller
    {

        private readonly DbContextiLabPlus ctxDB;
        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;


        public FasesController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB = Context;
            FunctionsBBDD = _FunctionsBBDD;
            GrupoClaims = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridFases"); // ATENCION AQUÍ
        }


        public IActionResult Index()
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;


            var Fases = ctxDB.Fases.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Fase).ToList();

            return View("Fases", Fases);
        }

        public IActionResult DialogFase(Guid Guid)
        {

            var Fase = ctxDB.Fases.Where(x => x.Guid == Guid).FirstOrDefault() ?? new Fases();


            ViewBag.ListIntExtTipo = new List<SelectListItem>
            {
                new SelectListItem { Text = "Interior", Value = "true" },
                new SelectListItem { Text = "Exterior", Value = "false" }
            };

            return PartialView("_DialogFase", Fase);
        }


        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> CreateEdit(Fases RowFase)
        {
            bool resultProcess = false;

            try
            {

                // Si la Fase es nueva, GUID vacío
                if (RowFase.Guid == Guid.Empty)
                {
                        // Creación del nuevo empleado, asegurándose de que la contraseña se incluya
                        RowFase.Empresa = GrupoClaims.SessionEmpresa;
                        RowFase.IsoUser = GrupoClaims.SessionUsuarioNombre;
                        RowFase.IsoFecAlt = DateTime.Now;

                        ctxDB.Fases.Add(RowFase);
                        await ctxDB.SaveChangesAsync();
                        resultProcess = true;
                  
                }
                else
                {
                    // Actualización de una fase existente
                    var faseToUpdate = await ctxDB.Fases.FindAsync(RowFase.Guid);
                    if (faseToUpdate != null)
                    {
                        // Actualizo los campos necesarios aquí
                        faseToUpdate.Fase = RowFase.Fase;
                        faseToUpdate.IntExt = RowFase.IntExt; 
                        faseToUpdate.Color = RowFase.Color;
                        faseToUpdate.Descripcion = RowFase.Descripcion;
                        faseToUpdate.IsoUser = GrupoClaims.SessionUsuarioNombre;
                        faseToUpdate.IsoFecMod = DateTime.Now;

                        ctxDB.Fases.Update(faseToUpdate);
                        await ctxDB.SaveChangesAsync();
                        resultProcess = true;
                    }
                }
            }
            catch (Exception e)
            {
                // Manejo de la excepción
                return StatusCode(400, e.Message);
            }

            if (resultProcess)
            {
                return Ok(new { success = true, Guid = RowFase.Guid });
            }
            else
            {
                return StatusCode(400, "No se pudo procesar la solicitud.");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Delete_Fase(Guid Guid)
        {
            var FasFind = ctxDB.Fases.Where(x => x.Guid == Guid).FirstOrDefault();
            if (FasFind != null)
            {
                try
                {
                    ctxDB.Fases.Remove(FasFind);
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


    }

}