using System;
using System.Collections.Generic;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class StocksALM
	{
        public Guid         Guid                { get; set; }
        public string       Empresa             { get; set; }
		public string       StkAlmacen          { get; set; }
		public string?	    StkNombre           { get; set; }
		public string?      StkDescripcion      { get; set; }
        public int?         StkOrden            { get; set; }

        
    }
}
