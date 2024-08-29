using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class ContaRemesas
    {
        public Guid         Guid				{ get; set; }
        public string       Empresa				{ get; set; }
        public int          Ejercicio           { get; set; }
        public string	    Remesa              { get; set; }
        public string       RemModelo           { get; set; }
        public DateTime     RemFecha            { get; set; }
        public DateTime?    RemFechaCargo       { get; set; }
        public string       RemEstado           { get; set; }

        public string?		RemObserv		    { get; set; }
        public string?       RemBancoCta         { get; set; }
        public string?       RemDocumento        { get; set; }

        public decimal      RemImporte          { get; set; }
        public decimal      RemImporteDev       { get; set; }
        public int          RemNumRecivos       { get; set; }
        public int          RemNumRecivosDev    { get; set; }

        public string?		IsoUser             { get; set; }
        public DateTime?    IsoFecAlt           { get; set; }
        public DateTime?    IsoFecMod           { get; set; }


    }
}
