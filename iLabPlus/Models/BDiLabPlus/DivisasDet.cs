using System;
using System.Collections.Generic;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class DivisasDet
    {
        public Guid         Guid			{ get; set; }
        public string       Empresa			{ get; set; }
        public string	    Divisa			{ get; set; }
		public DateTime     DivFecha        { get; set; }

		public decimal      DivCambio       { get; set; }



    }
}
