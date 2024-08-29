using System;

namespace iLabPlus.Models.BDiLabPlus
{

    public partial class Empleados
    {
        public Guid Guid { get; set; }
        public string Empresa { get; set; }
        public string Empleado { get; set; }

        public string? EmpTipo { get; set; }
        public string? EmpNombre { get; set; }
        public string? EmpNIF { get; set; }

        public string? EmpImagen { get; set; }

        public string? EmpDireccion { get; set; }
        public string? EmpDirDP { get; set; }
        public string? EmpDirPoblacion { get; set; }
        public string? EmpDirProvincia { get; set; }
        public string? EmpDirPais { get; set; }

        public string? EmpTelefono1 { get; set; }
        public string? EmpTelefono1Desc { get; set; }
        public string? EmpTelefono2 { get; set; }
        public string? EmpTelefono2Desc { get; set; }
        public string? EmpTelefono3 { get; set; }
        public string? EmpTelefono3Desc { get; set; }

        public string? EmpMail { get; set; }

        public string? EmpBcoBanco { get; set; }
        public string? EmpBcoCtaCte1 { get; set; }
        public string? EmpBcoCtaCte2 { get; set; }
        public string? EmpBcoCtaCte3 { get; set; }
        public string? EmpBcoCtaCte4 { get; set; }
        public string? EmpBcoSwift { get; set; }
        public string? EmpBcoIBAN { get; set; }

        public string? EmpObserv { get; set; }



        public string? EmpFPago { get; set; }
        public decimal? EmpFPvto { get; set; }
        public decimal? EmpFPint { get; set; }
        public decimal? EmpFPlazo { get; set; }
        public decimal? EmpFPdia1 { get; set; }
        public decimal? EmpFPdia2 { get; set; }
        public decimal? EmpFPdia3 { get; set; }

        public string? EmpCuentaContable { get; set; }

        public string? EmpPassword { get; set; }

        public string? IsoUser { get; set; }
        public DateTime? IsoFecAlt { get; set; }
        public DateTime? IsoFecMod { get; set; }

    }
}
