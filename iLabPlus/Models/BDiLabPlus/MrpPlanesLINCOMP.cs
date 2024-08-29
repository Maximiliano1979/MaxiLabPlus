using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class MrpPlanesLINCOMP
    {
        public Guid         Guid			{ get; set; }
        public string       Empresa			{ get; set; }
        public int          PlanMrp         { get; set; }
        public int          PlanSobre       { get; set; }

        public string       Cliente			{ get; set; }
		public string		Pedido			{ get; set; }
		public int			PedLinea		{ get; set; }
        public int?         Secuencia       { get; set; }

        public string		Tipo			{ get; set; }
		public string		Articulo		{ get; set; }
		public string		Componente		{ get; set; }

		public decimal		Cantidad		{ get; set; }


		public string? TipoPrecio		{ get; set; }
		
        public string? PedOro          { get; set; }
        public string? PedKil          { get; set; }

		public string? IsoUser			{ get; set; }
        public DateTime?    IsoFecAlt		{ get; set; }
        public DateTime?    IsoFecMod		{ get; set; }


        /* CAMPOS CALCULADOS */
        [NotMapped]
        public decimal? Precio { get; set; }

        [NotMapped]
        public decimal? Valor { get; set; }

        [NotMapped]
        public string Descripcion { get; set; }

        [NotMapped]
        public string DescripcionArt { get; set; }

        [NotMapped]
        public decimal? Peso { get; set; }

        [NotMapped]
        public decimal? PesoSinMerma { get; set; }

        [NotMapped]
        public int SecVisTipo { get; set; }





    }
}
