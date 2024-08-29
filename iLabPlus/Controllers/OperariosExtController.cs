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

//using NLog;



namespace Nemesis365.Controllers
{
    [Authorize]
    public class OperariosExtController : Controller
    {

        private readonly DbContextiLabPlus ctxDB;

        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;


        public OperariosExtController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB = Context;

            FunctionsBBDD = _FunctionsBBDD;
            GrupoClaims = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridOperariosEXT");
        }


        public IActionResult Index()
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;


            var OperariosEXT = ctxDB.OperariosEXT.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Operario).ToList();

            return View("OperariosEXT", OperariosEXT);
        }

        public IActionResult DialogOperario(Guid Guid)
        {
            var Operario = ctxDB.OperariosEXT.Where(x => x.Guid == Guid).FirstOrDefault();
            if (Operario == null)
            {
                Operario = new OperariosEXT();
            }


            var OperarioTipo = new List<string>();
            OperarioTipo.Add("");
            OperarioTipo.Add("Engastador");
            OperarioTipo.Add("Sacador de fuego");
            ViewBag.ListOperarioTipo = OperarioTipo;

            return PartialView("_DialogOperarioEXT", Operario);
        }




        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> CreateEdit(OperariosEXT RowOperario)
        {
            bool resultProcess = false;

            try
            {
                if (RowOperario.Guid == Guid.Empty)
                {

                    // Calcular el siguiente número de operario con 6 dígitos, sólo para códigos numéricos
                    var maxOperario = ctxDB.OperariosEXT
                        .Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
                        .ToList() // Cargar los operarios en memoria
                        .Where(x => IsNumeric(x.Operario)) // Filtrar por códigos numéricos
                        .OrderByDescending(x => int.Parse(x.Operario))
                        .Select(x => x.Operario)
                        .FirstOrDefault();

                    int nextNumber = 1; // Número inicial si no hay operarios
                    if (!string.IsNullOrEmpty(maxOperario))
                    {
                        nextNumber = int.Parse(maxOperario) + 1; // Incrementar el número
                    }

                    RowOperario.Operario = nextNumber.ToString("D6"); // Formatear con 6 dígitos (por ejemplo, "000001")


                    var FindOpe = ctxDB.OperariosEXT.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Operario == RowOperario.Operario).FirstOrDefault();
                    if (FindOpe == null)
                    {
                        // Creacion
                        RowOperario.Empresa = GrupoClaims.SessionEmpresa;
                        RowOperario.IsoUser = GrupoClaims.SessionUsuarioNombre;
                        RowOperario.IsoFecAlt = DateTime.Now;

                        ctxDB.OperariosEXT.Add(RowOperario);
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
                    RowOperario.IsoUser = GrupoClaims.SessionUsuarioNombre;
                    RowOperario.IsoFecMod = DateTime.Now;

                    ctxDB.OperariosEXT.Update(RowOperario);
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


            var Result = ctxDB.OperariosEXT.Where(x => x.Guid == RowOperario.Guid).FirstOrDefault();

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


        [HttpPost]
        public async Task<IActionResult> Delete_Operario(Guid Guid)
        {
            var OpeFind = ctxDB.OperariosEXT.Where(x => x.Guid == Guid).FirstOrDefault();
            if (OpeFind != null)
            {
                try
                {
                    ctxDB.OperariosEXT.Remove(OpeFind);
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