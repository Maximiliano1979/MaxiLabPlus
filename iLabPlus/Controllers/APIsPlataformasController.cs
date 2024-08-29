using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iLabPlus.Controllers
{
    [Authorize]
    public class APIsPlataformasController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly DbContextiLabPlus ctxDB;

        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;

        public APIsPlataformasController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD, IConfiguration configuration)
        {
            ctxDB               = Context;
            _configuration      = configuration;

            FunctionsBBDD       = _FunctionsBBDD;
            GrupoClaims         = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout  = FunctionsBBDD.GetColumnsLayout("gridAPIsPlataformas");
        }

        public IActionResult Index()
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();

            return View("APIsPlataformas");
        }
    }
}
