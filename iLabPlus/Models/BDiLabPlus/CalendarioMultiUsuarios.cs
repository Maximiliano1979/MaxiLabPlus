using System;

namespace iLabPlus.Models.BDiLabPlus
{
    public class CalendarioMultiUsuarios
    {
        public Guid Guid { get; set; }

        public Guid GuidEvento { get; set; }
        public string Usuario { get; set; }
        public string Empresa { get; set; }

       // public bool AgregadoAutomaticamente { get; set; }
    }
}
