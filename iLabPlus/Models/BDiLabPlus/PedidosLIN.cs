using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace iLabPlus.Models.BDiLabPlus
{
    public partial class PedidosLIN
	{
        public Guid         Guid			{ get; set; }
        public string       Empresa			{ get; set; }
		public string       Cliente			{ get; set; }
		public string		Pedido			{ get; set; }
		public int			PedLinea		{ get; set; }

		public string		PedArt			{ get; set; }
		public string?		PedDesc			{ get; set; }

		public string?		PedOro			{ get; set; }
		public string?		PedTarifa		{ get; set; }
		public string?		PedKIL			{ get; set; }
		public string?		PedCOL			{ get; set; }
		public string?		PedACB			{ get; set; }
		public string?		PedBAN			{ get; set; }

		public string?		PedArtTipoVenta { get; set; }

		public decimal?		PedQty			{ get; set; }
		public decimal?		PedPeso			{ get; set; }
		public decimal?		PedMerma		{ get; set; }
		public decimal?		PedPrecio		{ get; set; }
		public decimal?		PedDtoLin		{ get; set; }
		public decimal?		PedPesoTotal	{ get; set; }
		public decimal?		PedPrecioTotal	{ get; set; }

		public string?		PedObserv		{ get; set; }
		public string?		PedAlmacen		{ get; set; }
		public string?		PedMovTra		{ get; set; }
		public string?		PedMovSTK		{ get; set; }
		public int?			PedMovSTKlin	{ get; set; }

		public string?		PedEstado		{ get; set; }
		public decimal?		PedQtyENT		{ get; set; }
		public DateTime?	PedFecEntrega	{ get; set; }
		public decimal?		PedLinComision	{ get; set; }

		public string?		PedMedidas		{ get; set; }
		public int?			PedImageSelect	{ get; set; }

        public int?			PedMRP			{ get; set; }

        public string?       IsoUser			{ get; set; }
        public DateTime?    IsoFecAlt		{ get; set; }
        public DateTime?    IsoFecMod		{ get; set; }


        /* CAMPOS CALCULADOS */
  //      [NotMapped]
  //      public string? ClienteNombre { get; set; }

		//[NotMapped]
		//public string? VendedorNombre { get; set; }

		//[NotMapped]
		//public decimal TotalPed { get; set; }

		//[NotMapped]
		//public decimal TotalPedPend { get; set; }


	}
}
