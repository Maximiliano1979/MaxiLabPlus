using iLabPlus.Helpers;
using iLabPlus.Models;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace iLabPlus.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly DbContextiLabPlus ctxDB;

        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;

        public HomeController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB               = Context;

            FunctionsBBDD       = _FunctionsBBDD;
            GrupoClaims         = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout  = FunctionsBBDD.GetColumnsLayout("gridHome");

        }

        public IActionResult Index()
        {

            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();

            var UserFind = ctxDB.Usuarios.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Usuario == GrupoClaims.SessionUsuario).OrderBy(x => x.UsuarioNombre).FirstOrDefault();
            if (UserFind != null)
            {
                if (UserFind.ControllerInit != null && UserFind.ControllerInit != "")
                {
                    return RedirectToAction("Index", UserFind.ControllerInit);
                }
                else
                {
                    return RedirectToAction("Index", "Clientes");
                }

            }
            else
            {
                return RedirectToAction("Index", "Clientes");
            }

        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
