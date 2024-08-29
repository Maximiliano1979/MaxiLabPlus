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
using iLabPlus.Models.Clases;
using Newtonsoft.Json;

//using NLog;



namespace iLabPlus.Controllers
{
    [Authorize]
    public class LogsVersionesController : Controller
    {

        private readonly DbContextiLabPlus  ctxDB;

        private readonly FunctionsBBDD      FunctionsBBDD;
        private readonly GrupoClaims        GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;


        public LogsVersionesController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB               = Context;
            
            FunctionsBBDD       = _FunctionsBBDD;
            GrupoClaims         = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridLogsVersiones");
        }


        public IActionResult Index()
        {
            ViewBag.MenuUserList        = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser   = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser   = GrupoColumnsLayout.ColumnsPinnedUser;


            var LogsVersiones = ctxDB.Logs_Versiones.Where(x=>x.Empresa == GrupoClaims.SessionEmpresa).OrderByDescending(x=>x.Fecha).ToList();

            return View("LogsVersiones", LogsVersiones);
        }

        //public IActionResult DialogDivisa(Guid Guid)
        //{
        //    var FindDivisa = ctxDB.Divisas.Where(x => x.Guid == Guid).FirstOrDefault();
        //    if (FindDivisa == null)
        //    {
        //        FindDivisa = new Divisas();
        //    }
        //    else
        //    {
        //        var DivisaDet = ctxDB.DivisasDet.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Divisa == FindDivisa.Divisa).OrderByDescending(x=>x.DivFecha);

        //        FindDivisa.ListCotizaciones = DivisaDet;
        //    }

        //    var UserConfigGrid = ctxDB.UsuariosGridsCfg.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Usuario == SessionUsuario && x.GridID == "gridDivisasDet").FirstOrDefault();
        //    if (UserConfigGrid != null)
        //    {
        //        ViewBag.ColumnsLayoutUser = UserConfigGrid.ColumnsLayout;

        //        if (UserConfigGrid.ColumnsPinned == null)
        //        {
        //            ViewBag.ColumnsPinnedUser = 3;
        //        }
        //        else
        //        {
        //            ViewBag.ColumnsPinnedUser = UserConfigGrid.ColumnsPinned;
        //        }
        //    }
        //    else
        //    {
        //        ViewBag.ColumnsPinnedUser = 3;
        //    }

        //    return PartialView("_DialogDivisa", FindDivisa);
        //}


        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/

        //[HttpPost]
        //public async Task<IActionResult> CreateEdit(Divisas RowDivisa)
        //{
        //    bool resultProcess = false;

        //    try
        //    {
        //        if (RowDivisa.Guid == Guid.Empty)
        //        {
        //            var FindDiv = ctxDB.Divisas.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Divisa == RowDivisa.Divisa).FirstOrDefault();                    
        //            if (FindDiv == null)
        //            {
        //                // Creacion
        //                RowDivisa.Empresa       = GrupoClaims.SessionEmpresa;
        //                RowDivisa.IsoUser       = SessionUsuarioNombre;
        //                RowDivisa.IsoFecAlt     = DateTime.Now;
        //                RowDivisa.IsoFecMod     = DateTime.Now;

        //                ctxDB.Divisas.Add(RowDivisa);
        //                await ctxDB.SaveChangesAsync();
                        

        //                var NewDivisaDet        = new DivisasDet();
        //                NewDivisaDet.Empresa    = RowDivisa.Empresa;
        //                NewDivisaDet.Divisa     = RowDivisa.Divisa;
        //                NewDivisaDet.DivFecha   = RowDivisa.DivFecha;
        //                NewDivisaDet.DivCambio  = RowDivisa.DivCambio;

        //                ctxDB.DivisasDet.Add(NewDivisaDet);
        //                await ctxDB.SaveChangesAsync();

        //                resultProcess = true;
        //            }
        //            else
        //            {
        //                return StatusCode(200, "EXIST");
        //            }

        //        }
        //        else
        //        {
        //            // Edicion
        //            RowDivisa.IsoUser    = SessionUsuarioNombre;
        //            RowDivisa.IsoFecMod  = DateTime.Now;

        //            ctxDB.Divisas.Update(RowDivisa);
        //            await ctxDB.SaveChangesAsync();

        //            var NewDivisaDet        = new DivisasDet();
        //            NewDivisaDet.Empresa    = RowDivisa.Empresa;
        //            NewDivisaDet.Divisa     = RowDivisa.Divisa;
        //            NewDivisaDet.DivFecha   = RowDivisa.DivFecha;
        //            NewDivisaDet.DivCambio  = RowDivisa.DivCambio;

        //            ctxDB.DivisasDet.Add(NewDivisaDet);
        //            await ctxDB.SaveChangesAsync();

        //            resultProcess = true;
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        //Logger.Error("[M]: " + e.Message + "[StackT]: " + e.StackTrace + "[HLink]: " + e.HelpLink + "[HResult]: " + e.HResult + "[Source]: " + e.Source);
        //        //ModelState.AddModelError("", "Unable to save changes. " + "Try again, and if the problem persists " + "see your system administrator.");
        //        return StatusCode(400, e.Message);  //  Send "Error"
        //    }


        //    var Result = ctxDB.Divisas.Where(x => x.Guid == RowDivisa.Guid).FirstOrDefault();

        //    if (resultProcess)
        //    {
        //        return StatusCode(200, Result);
        //    }
        //    else
        //    {
        //        return StatusCode(400, null);
        //    }

        //}


        //[HttpPost]
        //public async Task<IActionResult> Delete_DIVISA(Guid Guid)
        //{
        //    var FindDiv = ctxDB.Divisas.Where(x => x.Guid == Guid).FirstOrDefault();
        //    if (FindDiv != null)
        //    {
        //        try
        //        {
        //            ctxDB.Divisas.Remove(FindDiv);
        //            await ctxDB.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            return StatusCode(400, e.Message);
        //        }

        //        return StatusCode(200, "OK");
        //    }

        //    return StatusCode(200, null);
        //}


    }

}