using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Doctores
    {
        public Guid Guid { get; set; }

        public string Empresa { get; set; }

        public string Doctor { get; set; }

        public string Nombre { get; set; }

        public string NumColegiado { get; set; }

        public string? NIF { get; set; }

        public DateTime? Aniversario { get; set; }

        public string? Mail { get; set; }

        public string? DirDireccion { get; set; }

        public string? DirDP { get; set; }

        public string? DirPoblacion { get; set; }

        public string? DirProvincia { get; set; }

        public string? DirPais { get; set; }

        public string? Telefono1 { get; set; }

        public string? Telefono1Desc { get; set; }

        public string? Telefono2 { get; set; }

        public string? Telefono2Desc { get; set; }

        public string? Observ { get; set; }

        public bool? Activo { get; set; }

        public string? IsoUser { get; set; }

        public DateTime? IsoFecAlt { get; set; }

        public DateTime? IsoFecMod { get; set; }
    }
}

