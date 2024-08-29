using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class ContaRemesasLIN
    {
        public Guid         Guid				{ get; set; }
        public string       Empresa				{ get; set; }
        public string	    Remesa              { get; set; }
        public int			Secuencia			{ get; set; }

        public string       Factura             { get; set; }
        public string?      Concepto            { get; set; }
        public string?       CtaCte              { get; set; }

        public DateTime     FechaVto            { get; set; }
        public DateTime?    FechaCobro          { get; set; }
        public bool?        Devuelto            { get; set; }

        public decimal      Importe             { get; set; }

        public string?      CtaCteBanco         { get; set; }
        public string?       BcoCtaCte1          { get; set; }
        public string?      BcoCtaCte2          { get; set; }
        public string? BcoCtaCte3          { get; set; }
        public string? BcoCtaCte4          { get; set; }
        public string? BcoSwift            { get; set; }
        public string? BcoIBAN             { get; set; }

        public string? IsoUser             { get; set; }
        public DateTime?    IsoFecAlt           { get; set; }
        public DateTime?    IsoFecMod           { get; set; }


    }
}
