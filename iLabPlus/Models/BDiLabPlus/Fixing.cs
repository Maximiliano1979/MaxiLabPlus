using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Fixing
    {
        public Guid         Guid                { get; set; }
        public string       Empresa             { get; set; }

        [Required(ErrorMessage = "El campo Metal es obligatorio.")]
        public string       Metal               { get; set; }
        public DateTime     Fecha               { get; set; }

        [Required(ErrorMessage = "El campo Oro Fino Venta es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El campo Oro Fino Venta debe ser mayor que 0.")]
        public decimal      OroFinoVenta        { get; set; }
        public decimal      OroFinoCompra       { get; set; }

        public decimal      K24Milesimas        { get; set; }
        public decimal      K24Ley              { get; set; }
        public decimal      K19Milesimas        { get; set; }
        public decimal      K19Ley              { get; set; }
        public decimal      K18Milesimas        { get; set; }
        public decimal      K18Ley              { get; set; }
        public decimal      K14Milesimas        { get; set; }
        public decimal      K14Ley              { get; set; }
        public decimal      K10Milesimas        { get; set; }
        public decimal      K10Ley              { get; set; }
        public decimal      K9Milesimas         { get; set; }
        public decimal      K9Ley               { get; set; }
        public decimal      K8Milesimas         { get; set; }
        public decimal      K8Ley               { get; set; }

        public string       Observaciones       { get; set; }

        public string?       Tipo                { get; set; }
        public decimal?     Precio              { get; set; }


        public string?       IsoUser             { get; set; }
        public DateTime?    IsoFecAlt           { get; set; }
        public DateTime?    IsoFecMod           { get; set; }

        /* CAMPOS CALCULADOS */
        [NotMapped]
        public string       CalcMetalCombo      { get; set; }
    }
}
