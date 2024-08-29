using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Divisas
	{
        public Guid         Guid			{ get; set; }
        public string       Empresa			{ get; set; }
        public string	    Divisa			{ get; set; }
		public string?      DivNombre       { get; set; }
        public DateTime     DivFecha        { get; set; }
        public string       DivSimbolo      { get; set; }

		public decimal      DivCambio       { get; set; }
        public Boolean?     DivBase         { get; set; }




        public string?      IsoUser         { get; set; }
        public DateTime?    IsoFecAlt       { get; set; }
        public DateTime?    IsoFecMod       { get; set; }

        /* CAMPOS CALCULADOS */
        [NotMapped]
        public IEnumerable<DivisasDet> ListCotizaciones { get; set; }
       
    }
}
