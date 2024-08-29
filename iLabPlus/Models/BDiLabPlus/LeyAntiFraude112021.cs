using System;


namespace iLabPlus.Models.BDiLabPlus
{
    public class LeyAntiFraude112021
    {
    
        public Guid     Guid            { get; set; }
        public string   Empresa         { get; set; }
        public string   Usuario         { get; set; }
        public DateTime FechaReg        { get; set; }
        public string   Tipo            { get; set; }
        public string   Accion          { get; set; }
        public string?   Motivo          { get; set; }
        public string   Documento       { get; set; }
        public string?   RegistroOld     { get; set; }
        public string?   RegistroNew     { get; set; }


    }
}
