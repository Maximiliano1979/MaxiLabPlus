using System;

namespace iLabPlus.Models.BDiLabPlus
{
    public class CorreosSalientesAdj
    {
        public Guid     Guid            { get; set; }
        public string   Empresa         { get; set; }
        public string?   MessageId       { get; set; }

        public string?   NombreArchivo   { get; set; }
        public string?   RutaArchivo     { get; set; }


    }
}
