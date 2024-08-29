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
using Newtonsoft.Json.Converters;

namespace iLabPlus.Controllers
{
    [Authorize]
    public class ValsysController : Controller
    {

        private readonly DbContextiLabPlus ctxDB;

        private readonly FunctionsBBDD      FunctionsBBDD;
        private readonly GrupoClaims        GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;

        public ValsysController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB               = Context;
            
            FunctionsBBDD       = _FunctionsBBDD;
            GrupoClaims         = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridValsys");

        }


        public IActionResult Index()
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;


            var Valsys = ctxDB.ValSys.Where(x=>x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x=>x.Indice).ThenBy(x => x.Clave).ToList();

            return View("Valsys", Valsys);
        }

        public IActionResult DialogValsys(Guid Guid)
        {

            var ValsysRow = ctxDB.ValSys.Where(x => x.Guid == Guid).FirstOrDefault();
            if (ValsysRow == null)
            {
                ValsysRow = new ValSys();
            }

            var Indices = new List<string>();
            Indices.Add("");
            var ListIndices = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).GroupBy(x=>x.Indice).Select(x => x.Key).ToList();
            foreach (var item in ListIndices)
            {
                Indices.Add(item);
            }

            ViewBag.ListIndices = Indices;

            return PartialView("_DialogValsys", ValsysRow);
        }

        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> CreateEdit(ValSys ValSysRow)
        {
            bool resultProcess = false;

            try
            {
                if (ValSysRow.Guid == Guid.Empty)
                {
                    var FindClient = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == ValSysRow.Indice && x.Clave == ValSysRow.Clave).FirstOrDefault();
                    if (FindClient == null)
                    {
                        // Creacion
                        ValSysRow.Empresa   = GrupoClaims.SessionEmpresa;
                        ValSysRow.IsoUser   = GrupoClaims.SessionUsuarioNombre;
                        ValSysRow.IsoFecAlt = DateTime.Now;

                        ctxDB.ValSys.Add(ValSysRow);
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
                    ValSysRow.IsoUser   = GrupoClaims.SessionUsuarioNombre;
                    ValSysRow.IsoFecMod = DateTime.Now;

                    ctxDB.ValSys.Update(ValSysRow);
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


            var Result = ctxDB.ValSys.Where(x => x.Guid == ValSysRow.Guid).FirstOrDefault();

            var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd-MM-yyyy" };
            var Result2 =  Content(JsonConvert.SerializeObject(Result, dateTimeConverter).ToString(), "application/json");

            if (resultProcess)
            {
                return StatusCode(200, Result);
            }
            else
            {
                return StatusCode(400, null);
            }

        }




    }

}