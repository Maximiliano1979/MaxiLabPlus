using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Fases
    {
        public Guid Guid { get; set; }

        public string Empresa { get; set; }

        public string Fase { get; set; }

        public bool IntExt { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(150)]
        public string? Descripcion { get; set; }

        [StringLength(50)]
        public string? IsoUser { get; set; }

        public DateTime? IsoFecAlt { get; set; }

        public DateTime? IsoFecMod { get; set; }
    }
}

