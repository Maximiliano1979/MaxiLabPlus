using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

namespace iLabPlus
{
    [Authorize]
    public class Notifications
    {
        //private readonly static Logger Logger = LogManager.GetCurrentClassLogger();
        public virtual Task<string> EmailNotification(string[] ConfigMail, string ParamTo, string ParamBcc, string ParamSubject, string ParamBody)
        {
            string _Cuenta = ConfigMail[0];
            string _Password = ConfigMail[1];
            string _Host = ConfigMail[2];
            int _Port = 0;
            if (ConfigMail[3] != "0")
            {
                _Port = Convert.ToInt32(ConfigMail[3]);
            }


            SmtpClient smtpClient = null;
            MailMessage message = null;
            try
            {
                smtpClient = new SmtpClient
                {

                    Host = _Host, // SMTP server
                    Port = _Port, // Port 25 587 ...
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_Cuenta, _Password)


                };

                if (ParamTo == null || ParamTo == "")
                {
                    ParamTo = "xavi@roistech.com";
                    ParamSubject = "Notificación sin mail: " + ParamSubject;
                }

                if (ParamTo != null && ParamTo != "")
                {
                    // To: first contact
                    List<string> ToList = ParamTo.Split(";").ToList();

                    message = new MailMessage()
                    {
                        From = new MailAddress(_Cuenta, "iLabPlus"),
                    };



                    message.To.Add(ToList[0]);
                    // DEBUG
                    //message.To.Add("xavi@roistech.com");

                    // To: others contacts
                    for (int i = 1; i < ToList.Count; i++)
                    {
                        if (ToList[i] != null && ToList[i] != "") { message.To.Add(ToList[i]); }
                    }

                    // Bcc:
                    if (ParamBcc != null)
                    {
                        List<string> BccList = ParamBcc.Split(";").ToList();
                        for (int i = 0; i < BccList.Count; i++)
                        {
                            if (BccList[i] != null && BccList[i] != "") { message.Bcc.Add(BccList[i]); }
                        }
                    }

                    message.Subject = ParamSubject;
                    message.Body = ParamBody;
                    message.IsBodyHtml = true;
                }
                else
                {
                    // return "";  //  Send "Bad request"
                }
                smtpClient.SendCompleted += (s, e) =>
                {
                    SmtpClient callbackClient = s as SmtpClient;
                    MailMessage callbackMailMessage = e.UserState as MailMessage;
                    smtpClient.Dispose();
                    message.Dispose();
                };


                smtpClient.SendMailAsync(message);

            }
            catch (Exception e)
            {


                var error = e.Message;

                var ParentPathFic = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();
                var PathDocLog = Path.Combine(ParentPathFic, "Documentos", "LogDebug.log");

                //var logPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot/images/", "LogDebug.log");
                var logFile = System.IO.File.Create(PathDocLog);
                var logWriter = new System.IO.StreamWriter(logFile);
                logWriter.WriteLine(e.InnerException.Message);
                logWriter.WriteLine("  ");
                logWriter.WriteLine(e.Message);
                logWriter.Dispose();

                //Logger.Error(e.InnerException.Message);


            }
            finally { smtpClient.Dispose(); message.Dispose(); }


            return Task.FromResult("200");
        }



    }
}



