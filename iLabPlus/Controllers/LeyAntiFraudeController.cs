
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iLabPlus.Controllers
{
    [Authorize]
    public class LeyAntiFraudeController : Controller
    {

        private readonly DbContextiLabPlus ctxDB;
        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;

        public LeyAntiFraudeController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB = Context;
            FunctionsBBDD = _FunctionsBBDD;
            GrupoClaims = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout = FunctionsBBDD.GetColumnsLayout("gridEmpleados");
        }


        public IActionResult Index()
        {
            ViewBag.MenuUserList        = FunctionsBBDD.GetMenuAccesos();
            ViewBag.ColumnsLayoutUser   = GrupoColumnsLayout.ColumnsLayoutUser;
            ViewBag.ColumnsPinnedUser   = GrupoColumnsLayout.ColumnsPinnedUser;


            var ListLeyAntiFraude = ctxDB.LeyAntiFraude112021.Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
                                    .Select(c => new LeyAntiFraude112021
                                    {
                                        Guid = c.Guid,
                                        Empresa = c.Empresa,
                                        Usuario = c.Usuario,
                                        FechaReg = c.FechaReg,
                                        Tipo = c.Tipo,
                                        Accion = c.Accion,
                                        Motivo = c.Motivo,
                                        Documento = c.Documento,
                                        RegistroNew = ConvertStrJsonFac(c.RegistroNew) ,
                                        RegistroOld = ConvertStrJsonFac(c.RegistroOld)
                                    })
                                    .OrderByDescending(x => x.FechaReg).ToList();

            return View("LeyAntiFraude", ListLeyAntiFraude);
        }

        public IActionResult DialogLeyAntiFraude(Guid Guid)
        {
            try
            {
                var LA = ctxDB.LeyAntiFraude112021.Where(x => x.Guid == Guid ).FirstOrDefault();
                if (LA != null)
                {


                    if (LA.Tipo.Contains("Fac"))
                    {
                        // SI SON REGISTROS DE FACTURAS, PONEMOS LOS TOTALES DEBAJO DE CLIENTE,FACTURA
                        var newJson = ConvertStrJsonFac(LA.RegistroNew);
                        ViewBag.RegistroNew = newJson;

                        var oldJson = ConvertStrJsonFac(LA.RegistroOld);
                        ViewBag.RegistroOld = oldJson;

                    }
                    else
                    {
                        ViewBag.RegistroNew = LA.RegistroNew;
                        ViewBag.RegistroOld = LA.RegistroOld;
                    }



                    return PartialView("_DialogLeyAntiFraude", LA);
                }
                return null;


                
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Retorna un error 500 con el mensaje de la excepción
            }
        }

        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/

        public static string ConvertStrJsonFac(string CadJson)
        {

            if (CadJson == null)
            {
                return "{}";
            }
            else
            {
                dynamic jsonNew = JsonConvert.DeserializeObject(CadJson);

                // Para que sino viene de un registro de facturas no los ponga al inicio estos campos
                if (jsonNew.ContainsKey("TotalFac"))
                {
                    var dictAct = new Dictionary<string, object>();

                    dictAct["Cliente"] = jsonNew.Cliente;
                    dictAct["Factura"] = jsonNew.Factura;
                    dictAct["TotalFac"] = jsonNew.TotalFac;
                    dictAct["TotalFacBI"] = jsonNew.TotalFacBI;
                    dictAct["TotalFacIVA"] = jsonNew.TotalFacIVA;
                    dictAct["TotalFacDTOs"] = jsonNew.TotalFacDTOs;

                    foreach (var prop in jsonNew)
                    {
                        if (!dictAct.ContainsKey(prop.Name))
                        {
                            dictAct[prop.Name] = prop.Value;
                        }
                    }
                    var newJson = JsonConvert.SerializeObject(dictAct);

                    return newJson;
                }
                else
                {
                    return CadJson;
                }
            }
            
        }



    }

}