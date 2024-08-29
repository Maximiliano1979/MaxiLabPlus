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
    public class LogsAccesosController : Controller
    {
        private readonly DbContextiLabPlus   ctxDB;

        private readonly FunctionsBBDD      FunctionsBBDD;
        private readonly GrupoClaims        GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;


        public LogsAccesosController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB               = Context;
            
            FunctionsBBDD       = _FunctionsBBDD;
            GrupoClaims         = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridLogsAccesos");
        }


        public IActionResult Index()
        {
            ViewBag.MenuUserList        = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser   = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser   = GrupoColumnsLayout.ColumnsPinnedUser;


            var LogsAccesos = ctxDB.Logs_Accesos.Where(x=>x.Empresa == GrupoClaims.SessionEmpresa).OrderByDescending(x=>x.IsoFecAlt).ToList();

            return View("LogsAccesos", LogsAccesos);
        }



        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/


    }

}