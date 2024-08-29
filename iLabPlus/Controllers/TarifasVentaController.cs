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
using Newtonsoft.Json;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;

//using NLog;



namespace iLabPlus.Controllers
{
    [Authorize]
    public class TarifasVentaController : Controller
    {

        private readonly DbContextiLabPlus   ctxDB;

        private readonly FunctionsBBDD      FunctionsBBDD;
        private readonly GrupoClaims        GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;


        public TarifasVentaController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB               = Context;
            
            FunctionsBBDD       = _FunctionsBBDD;
            GrupoClaims         = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridTarifas");
        }


        public IActionResult Index()
        {
            ViewBag.MenuUserList        = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser   = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser   = GrupoColumnsLayout.ColumnsPinnedUser;


            var Tarifas = ctxDB.TarifasVenta.Where(x=>x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x=>x.Tarifa).ToList();

            return View("TarifasVenta", Tarifas);
        }

        public IActionResult DialogTarifa(Guid Guid)
        {
            var FindTar = ctxDB.TarifasVenta.Where(x => x.Guid == Guid).FirstOrDefault();
            if (FindTar == null)
            {
                FindTar = new TarifasVenta();
            }

            return PartialView("_DialogTarifasVenta", FindTar);
        }


        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> CreateEdit(TarifasVenta RowTar)
        {
            bool resultProcess = false;

            try
            {
                if (RowTar.Guid == Guid.Empty)
                {
                    var FindTar = ctxDB.TarifasVenta.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Tarifa == RowTar.Tarifa).FirstOrDefault();
                    if (FindTar == null)
                    {
                        // Creacion
                        RowTar.Empresa      = GrupoClaims.SessionEmpresa;
                        RowTar.IsoUser      = GrupoClaims.SessionUsuarioNombre;
                        RowTar.IsoFecAlt    = DateTime.Now;
                        RowTar.IsoFecMod    = DateTime.Now;

                        ctxDB.TarifasVenta.Add(RowTar);
                        await ctxDB.SaveChangesAsync();

                        //ctxDB.TarifasVenta.Add(RowTar);
                        //await ctxDB.SaveChangesAsync();
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
                    RowTar.IsoUser    = GrupoClaims.SessionUsuarioNombre;
                    RowTar.IsoFecMod  = DateTime.Now;

                    ctxDB.TarifasVenta.Update(RowTar);
                    await ctxDB.SaveChangesAsync();
                    resultProcess = true;
                }

            }
            catch (Exception e)
            {
                //Logger.Error("[M]: " + e.Message + "[StackT]: " + e.StackTrace + "[HLink]: " + e.HelpLink + "[HResult]: " + e.HResult + "[Source]: " + e.Source);
                //ModelState.AddModelError("", "Unable to save changes. " + "Try again, and if the problem persists " + "see your system administrator.");
                return StatusCode(400, e.Message);  //  Send "Error"
            }


            var Result = ctxDB.TarifasVenta.Where(x => x.Guid == RowTar.Guid).FirstOrDefault();

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
        public async Task<IActionResult> Delete_TARIFA(Guid Guid)
        {

            try
            {
                var FindTar = ctxDB.TarifasVenta.Where(x => x.Guid == Guid).FirstOrDefault();
                if (FindTar != null)
                {
                    ctxDB.TarifasVenta.Remove(FindTar);
                    await ctxDB.SaveChangesAsync();

                    return StatusCode(200, "OK");
                }

                return StatusCode(200, null);


            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);
            }


        }


        public IActionResult _DialogTarifaCopiar(string Tarifa)
        {
            ViewBag.TarifaOld = Tarifa;
            return PartialView("_DialogTarifaCopiar");
        }

        [HttpPost]
        public async Task<IActionResult> Tarifa_Copiar(string TarifaOld, string TarifaNew)
        {
            try
            {
                // Verifico si la nueva tarifa ya existe
                var tarifaExistente = await ctxDB.TarifasVenta
                    .FirstOrDefaultAsync(t => t.Empresa == GrupoClaims.SessionEmpresa && t.Tarifa == TarifaNew);

                if (tarifaExistente != null)
                {
                    return Json(new { success = false, message = "El código de la nueva tarifa ya existe." });
                }

                // Obtengo la tarifa original
                var tarifaOriginal = await ctxDB.TarifasVenta
                    .FirstOrDefaultAsync(t => t.Empresa == GrupoClaims.SessionEmpresa && t.Tarifa == TarifaOld);

                if (tarifaOriginal == null)
                {
                    return Json(new { success = false, message = "No se encontró la tarifa original." });
                }

                // Creo una nueva instancia de TarifasVenta con los datos de la tarifa original
                var nuevaTarifa = new TarifasVenta
                {
                    Guid = Guid.NewGuid(),
                    Empresa = tarifaOriginal.Empresa,
                    Tarifa = TarifaNew,
                    TarDescripcion = tarifaOriginal.TarDescripcion,
                    TarEtiqueta = tarifaOriginal.TarEtiqueta,
                    TarPeso = tarifaOriginal.TarPeso,
                    TarPesoHechura = tarifaOriginal.TarPesoHechura,
                    TarHechura = tarifaOriginal.TarHechura,
                    TarObserv = tarifaOriginal.TarObserv,
                    IsoUser = GrupoClaims.SessionUsuarioNombre,
                    IsoFecAlt = DateTime.Now,
                    IsoFecMod = DateTime.Now
                };

                // Agregar la nueva tarifa a la base de datos
                ctxDB.TarifasVenta.Add(nuevaTarifa);
                await ctxDB.SaveChangesAsync();

                return Json(new { success = true, data = nuevaTarifa });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al copiar la tarifa: {ex.Message}" });
            }
        }


    }

}