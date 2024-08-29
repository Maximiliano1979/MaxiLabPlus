using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace iLabPlus.Models.BDiLabPlus
{
    public class CorreosSalientes
    {
    
        public Guid     Guid            { get; set; }
        public string   Empresa         { get; set; }
        public string?   MessageId       { get; set; }
        public DateTime FechaEnv        { get; set; }
        public string   Tipo            { get; set; }
        public string   Estado          { get; set; }
        public string   Remitente       { get; set; }
        public string   Destinatario   { get; set; }
        public string?   CCO             { get; set; }
        public string?   Asunto          { get; set; }
        public string?   Cuerpo          { get; set; }

        public string?       IsoUser     { get; set; }
        public DateTime?    IsoFecAlt   { get; set; }
        public DateTime?    IsoFecMod   { get; set; }

       [NotMapped]
        public virtual List<CorreosSalientesAdj> Adjuntos { get; set; }

    }
}
