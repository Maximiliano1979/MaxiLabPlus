using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class ArticulosMO
    {
        public Guid         Guid                { get; set; }
        public string       Empresa             { get; set; }
        public string       Articulo            { get; set; }
        public int          MOSecuencia         { get; set; }

        public string       CodMO               { get; set; }
        public string       CodMODescripcion    { get; set; }

        public string       MOExtInt            { get; set; }
        public decimal      MOCantidad          { get; set; }
        public decimal      MOPrecio            { get; set; }
        public decimal?     MOPrecioExt         { get; set; }
        public decimal?     MOTiempo            { get; set; }

        public string?       MOUniMed            { get; set; }
        
        public string?       IsoUser             { get; set; }
        public DateTime?    IsoFecAlt           { get; set; }
        public DateTime?    IsoFecMod           { get; set; }

        /* CAMPOS CALCULADOS */
        //[NotMapped]
        //public decimal Valor { get; set; }

        [NotMapped]
        public decimal? Valor { get; set; }

    }
}
