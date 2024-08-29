using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class FasesEmpleados
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Guid { get; set; }

        [Required]
        [StringLength(15)]
        public string Empresa { get; set; }

        [Required]
        [StringLength(50)]
        public string Fase { get; set; }

        [Required]
        [StringLength(30)]
        public string Empleado { get; set; }

        [StringLength(50)]
        public string? IsoUser { get; set; }

        public DateTime? IsoFecAlt { get; set; }

        public DateTime? IsoFecMod { get; set; }
    }
}
