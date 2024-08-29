using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iLabPlus.Models.BDiLabPlus;
using System.Security.Claims;
using MailKit.Net.Smtp;
using MimeKit;

namespace iLabPlus.Helpers
{
    [Authorize]
    public class FunctionsMails : Controller
    {

        private readonly DbContextiLabPlus ctxDB;

        private readonly ClaimsIdentity ClaimsIdentity;
        private readonly string SessionEmpresa;
        private readonly string SessionEmpresaNombre;
        private readonly string SessionUsuario;
        private readonly string SessionUsuarioNombre;
        private readonly string SessionUsuarioTipo;


        public FunctionsMails(DbContextiLabPlus Context, IHttpContextAccessor ContextAccessor, ILogger<FunctionsMails> logger)
        {
            
            ctxDB = Context;

            ClaimsIdentity = ContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if (ClaimsIdentity.Claims.Count() > 0)
            {
                if (ClaimsIdentity.Claims.Where(x => x.Type == "Empresa").FirstOrDefault() != null)
                {
                    SessionEmpresa = ClaimsIdentity.FindFirst("Empresa").Value;
                }
                if (ClaimsIdentity.Claims.Where(x => x.Type == "EmpresaNombre").FirstOrDefault() != null)
                {
                    SessionEmpresaNombre = ClaimsIdentity.FindFirst("EmpresaNombre")?.Value;
                }
                if (ClaimsIdentity.Claims.Where(x => x.Type == "Usuario").FirstOrDefault() != null)
                {
                    SessionUsuario = ClaimsIdentity.FindFirst("Usuario").Value;
                }
                if (ClaimsIdentity.Claims.Where(x => x.Type == "UsuarioNombre").FirstOrDefault() != null)
                {
                    SessionUsuarioNombre = ClaimsIdentity.FindFirst("UsuarioNombre").Value;
                }
                if (ClaimsIdentity.Claims.Where(x => x.Type == "UsuarioTipo").FirstOrDefault() != null)
                {
                    SessionUsuarioTipo = ClaimsIdentity.FindFirst("UsuarioTipo")?.Value;
                }
            }

        }


        public virtual string MailSend(string remitente, string destinatario, string CCO, string asunto, string cuerpo, IList<IFormFile> adjuntos)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(remitente, remitente));
                emailMessage.To.Add(new MailboxAddress(destinatario, destinatario));
                if (!string.IsNullOrEmpty(CCO))
                {
                    emailMessage.Bcc.Add(new MailboxAddress(CCO, CCO));
                }
                emailMessage.Subject = asunto;

                // Crear una parte multipart para el mensaje
                var multipart = new Multipart("mixed");

                // Agregar cuerpo del correo
                var cuerpoMensaje = new TextPart("html") { Text = cuerpo };
                multipart.Add(cuerpoMensaje);

                //// Configurar el cuerpo del mensaje como HTML
                //emailMessage.Body = new TextPart("html") { Text = cuerpo };

                // Agregar archivos adjuntos
                foreach (var archivo in adjuntos)
                {
                    if (archivo.Length > 0)
                    {
                        var attachment = new MimePart()
                        {
                            Content = new MimeContent(archivo.OpenReadStream()),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = Path.GetFileName(archivo.FileName)
                        };
                        multipart.Add(attachment);
                    }
                }

                emailMessage.Body = multipart;

                // Configura tu servidor SMTP y credenciales aquí
                using (var client = new SmtpClient())
                {
                    client.Connect("lin164.loading.es", 465, true);
                    client.Authenticate("soporte@roistech.com", "rois12_qq*");

                    client.Send(emailMessage);
                    client.Disconnect(true);
                }

                return emailMessage.MessageId;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar correo: " + ex.Message);
                return null;
            }
        }

        //public bool ReenviarCorreo(Guid guid)
        //{
        //    try
        //    {
        //        var correoExistente = ctxDB.CorreosSalientes.Include(c => c.Adjuntos).FirstOrDefault(c => c.Guid == guid);
        //        if (correoExistente == null)
        //        {
        //            Console.WriteLine("Correo no encontrado.");
        //            return false;
        //        }

        //        var remitente = correoExistente.Remitente;
        //        var destinatario = correoExistente.Destinatario;
        //        var CCO = correoExistente.CCO;
        //        var asunto = "Re: " + correoExistente.Asunto;
        //        var cuerpo = correoExistente.Cuerpo;
        //        IList<IFormFile> adjuntos = new List<IFormFile>();

        //        _logger.LogError($"Preparando para reenviar correo: {correoExistente.Guid}");
        //        var messageId = MailSend(remitente, destinatario, CCO, asunto, cuerpo, adjuntos);
        //        _logger.LogError($"Correo reenviado. MessageId: {messageId}");
        //        return messageId != null;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Error al reenviar correo: " + ex.ToString()); 
        //        return false;
        //    }
        //}




    }
}
