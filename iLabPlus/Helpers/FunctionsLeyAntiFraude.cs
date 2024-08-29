using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;


namespace iLabPlus.Models.Clases
{
    [Authorize]
    public class FunctionsLeyAntiFraude 
    {
        private readonly DbContextiLabPlus ctxDB;

        private readonly FunctionsBBDD  FunctionsBBDD;
        private readonly GrupoClaims    GrupoClaims;
        

        public FunctionsLeyAntiFraude(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {

            ctxDB           = Context;

            FunctionsBBDD   = _FunctionsBBDD;
            GrupoClaims     = FunctionsBBDD.GetClaims();

        }


        public async Task<bool> RegistrarAsync(string Tipo, string Accion, string Motivo, string Documento, string RegistroJsonOld, string RegistroJsonNew)
        {
            try
            {
                var NewReg          = new LeyAntiFraude112021();
                NewReg.Empresa      = GrupoClaims.SessionEmpresa;
                NewReg.Usuario      = GrupoClaims.SessionUsuario;
                NewReg.FechaReg     = DateTime.Now;
                NewReg.Tipo         = Tipo;
                NewReg.Accion       = Accion;
                NewReg.Motivo       = Motivo;
                NewReg.Documento    = Documento;
                //NewReg.RegistroOld  = RegistroJsonOld;
                //NewReg.RegistroNew  = RegistroJsonNew;

                if (RegistroJsonOld != null)
                {
                    JObject oldJson = JObject.Parse(RegistroJsonOld);
                    FormatDateFields(oldJson);
                    NewReg.RegistroOld = oldJson.ToString(Newtonsoft.Json.Formatting.None);
                }

                if (RegistroJsonNew != null)
                {
                    JObject newJson = JObject.Parse(RegistroJsonNew);
                    FormatDateFields(newJson);
                    NewReg.RegistroNew = newJson.ToString(Newtonsoft.Json.Formatting.None);
                }


                ctxDB.LeyAntiFraude112021.Add(NewReg);
                await ctxDB.SaveChangesAsync();

            }
            catch (Exception e)
            {
                var error = e.InnerException;
                return false;
            }


            return true;
        }

        private void FormatDateFields(JObject json)
        {
            foreach (var property in json.Properties())
            {
                //if (property.Value.Type == JTokenType.Date && property.Name.ToLower().Contains("fecha"))
                if (property.Value.Type == JTokenType.Date )
                {
                    if (DateTime.TryParse(property.Value.ToString(), out DateTime date))
                    {
                        property.Value = date.ToString("yyyy-MM-ddTHH:mm:ss");
                    }
                }
            }
        }



    }
}
