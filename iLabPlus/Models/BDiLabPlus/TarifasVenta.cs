using System;
using System.Collections.Generic;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class TarifasVenta
	{
        public Guid     Guid			{ get; set; }
        public string   Empresa			{ get; set; }
        public string	Tarifa			{ get; set; }
		public string	TarDescripcion	{ get; set; }
		

		public decimal? TarEtiqueta		{ get; set; }
		public decimal? TarPeso			{ get; set; }
		public decimal? TarPesoHechura	{ get; set; }
		public decimal? TarHechura		{ get; set; }

		public string?	 TarObserv		{ get; set; }

		public string?       IsoUser		{ get; set; }
        public DateTime?    IsoFecAlt	{ get; set; }
        public DateTime?    IsoFecMod	{ get; set; }



    }
}
