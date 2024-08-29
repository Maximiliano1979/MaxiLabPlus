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
    public class DivisasController : Controller
    {

        private readonly DbContextiLabPlus   ctxDB;

        private readonly FunctionsBBDD      FunctionsBBDD;
        private readonly GrupoClaims        GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;


        public DivisasController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB               = Context;
            
            FunctionsBBDD       = _FunctionsBBDD;
            GrupoClaims         = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout  = FunctionsBBDD.GetColumnsLayout("gridDivisas");
        }


        public IActionResult Index()
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();

            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;


            var Divisas = ctxDB.Divisas.Where(x=>x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x=>x.Divisa).ToList();

            return View("Divisas", Divisas);
        }

        public IActionResult DialogDivisa(Guid Guid)
        {
            var FindDivisa = ctxDB.Divisas.Where(x => x.Guid == Guid).FirstOrDefault();
            if (FindDivisa == null)
            {
                FindDivisa = new Divisas();
            }
            else
            {
                var DivisaDet = ctxDB.DivisasDet.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Divisa == FindDivisa.Divisa).OrderByDescending(x=>x.DivFecha);

                FindDivisa.ListCotizaciones = DivisaDet;
            }

            ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;

            return PartialView("_DialogDivisa", FindDivisa);
        }


        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/

        [HttpPost]


        public async Task<IActionResult> CreateEdit(Divisas RowDivisa)
        {
            bool resultProcess = false;

            try
            {
                if (RowDivisa.Guid == Guid.Empty)
                {
                    var FindDiv = ctxDB.Divisas.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Divisa == RowDivisa.Divisa).FirstOrDefault();
                    if (FindDiv == null)
                    {
                        // Creacion
                        RowDivisa.Empresa = GrupoClaims.SessionEmpresa;
                        RowDivisa.IsoUser = GrupoClaims.SessionUsuarioNombre;
                        RowDivisa.IsoFecAlt = DateTime.Now;
                        RowDivisa.IsoFecMod = DateTime.Now;

                        ctxDB.Divisas.Add(RowDivisa);
                        await ctxDB.SaveChangesAsync();


                        var NewDivisaDet = new DivisasDet();
                        NewDivisaDet.Empresa = RowDivisa.Empresa;
                        NewDivisaDet.Divisa = RowDivisa.Divisa;
                        NewDivisaDet.DivFecha = RowDivisa.DivFecha;
                        NewDivisaDet.DivCambio = RowDivisa.DivCambio;

                        ctxDB.DivisasDet.Add(NewDivisaDet);
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
                    RowDivisa.IsoUser = GrupoClaims.SessionUsuarioNombre;
                    RowDivisa.IsoFecMod = DateTime.Now;

                    ctxDB.Divisas.Update(RowDivisa);

                    // Busca un DivisasDet existente para la Divisa y Empresa específicas
                    var existingDivisaDet = ctxDB.DivisasDet
                        .FirstOrDefault(dd => dd.Divisa == RowDivisa.Divisa && dd.Empresa == RowDivisa.Empresa);

                    if (existingDivisaDet != null)
                    {
                        // Si existe, actualiza los campos necesarios
                        existingDivisaDet.DivFecha = RowDivisa.DivFecha;
                        existingDivisaDet.DivCambio = RowDivisa.DivCambio;
                        ctxDB.DivisasDet.Update(existingDivisaDet);
                    }
                    else
                    {
                        // Si no existe, maneja la situación (crear uno nuevo, mostrar un error, etc.)
                        // Ejemplo para crear uno nuevo:
                        var newDivisaDet = new DivisasDet
                        {
                            Guid = Guid.NewGuid(),
                            Empresa = RowDivisa.Empresa,
                            Divisa = RowDivisa.Divisa,
                            DivFecha = RowDivisa.DivFecha,
                            DivCambio = RowDivisa.DivCambio
                        };
                        ctxDB.DivisasDet.Add(newDivisaDet);
                    }

                    await ctxDB.SaveChangesAsync();
                    resultProcess = true;
                }

            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);  //  Send "Error"
            }


            var Result = ctxDB.Divisas.Where(x => x.Guid == RowDivisa.Guid).FirstOrDefault();

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
        public async Task<IActionResult> Delete_DIVISA(Guid Guid)
        {
            var FindDiv = ctxDB.Divisas.Where(x => x.Guid == Guid).FirstOrDefault();
            if (FindDiv != null)
            {
                try
                {
                    ctxDB.Divisas.Remove(FindDiv);
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