using System;
using System.Collections.Generic;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class OperariosEXT
    {
        public Guid Guid { get; set; }
        public string Empresa { get; set; }
        public string Operario { get; set; }

        public string? OpeTipo { get; set; }
        public string? OpeNombre { get; set; }
        public string? OpeNIF { get; set; }

        public string? OpeDireccion { get; set; }
        public string? OpeDirDP { get; set; }
        public string? OpeDirPoblacion { get; set; }
        public string? OpeDirProvincia { get; set; }
        public string? OpeDirPais { get; set; }

        public string? OpeTelefono1 { get; set; }
        public string? OpeTelefono1Desc { get; set; }
        public string? OpeTelefono2 { get; set; }
        public string? OpeTelefono2Desc { get; set; }
        public string? OpeTelefono3 { get; set; }
        public string? OpeTelefono3Desc { get; set; }

        public string? OpeMail { get; set; }

        public string? OpeBcoBanco { get; set; }
        public string? OpeBcoCtaCte1 { get; set; }
        public string? OpeBcoCtaCte2 { get; set; }
        public string? OpeBcoCtaCte3 { get; set; }
        public string? OpeBcoCtaCte4 { get; set; }
        public string? OpeBcoSwift { get; set; }
        public string? OpeBcoIBAN { get; set; }

        public string? OpeObserv { get; set; }

        public decimal? OpeIVA { get; set; }
        public decimal? OpeIRPF { get; set; }

        public decimal? OpeComisionEtiqueta { get; set; }
        public decimal? OpeComisionPeso { get; set; }
        public decimal? OpeComisionPesoHechura { get; set; }
        public decimal? OpeComisionHechura { get; set; }

        public string? OpeFPago { get; set; }
        public decimal? OpeFPvto { get; set; }
        public decimal? OpeFPint { get; set; }
        public decimal? OpeFPlazo { get; set; }
        public decimal? OpeFPdia1 { get; set; }
        public decimal? OpeFPdia2 { get; set; }
        public decimal? OpeFPdia3 { get; set; }

        public string? OpeCuentaContable { get; set; }

        public string? IsoUser { get; set; }
        public DateTime? IsoFecAlt { get; set; }
        public DateTime? IsoFecMod { get; set; }



    }
}
