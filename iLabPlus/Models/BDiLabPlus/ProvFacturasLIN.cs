using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace iLabPlus.Models.BDiLabPlus
{
    public partial class ProvFacturasLIN
    {
        public Guid         Guid			{ get; set; }
        public string       Empresa			{ get; set; }
		public string		Proveedor		{ get; set; }
		public string		Factura			{ get; set; }

        public string		Pedido			{ get; set; }
        public string		Albaran			{ get; set; }
        public int			FacLinea		{ get; set; }

        public string		FacArt			{ get; set; }
		public string?		FacDesc			{ get; set; }

		public string?		FacOro			{ get; set; }
		public string?		FacTarifa		{ get; set; }
		public string?		FacKIL			{ get; set; }
		public string?		FacCOL			{ get; set; }
		public string?		FacACB			{ get; set; }
		public string?		FacBAN			{ get; set; }

		public string?		FacArtTipoVenta { get; set; }

		public decimal?		FacQty			{ get; set; }
		public decimal?		FacPeso			{ get; set; }
		public decimal?		FacMerma		{ get; set; }
		public decimal?		FacPrecio		{ get; set; }
		public decimal?		FacDtoLin		{ get; set; }
		public decimal?		FacPesoTotal	{ get; set; }
		public decimal?		FacPrecioTotal	{ get; set; }

		public string?		FacObserv		{ get; set; }
		public string?		FacAlmacen		{ get; set; }
		public string?		FacMovTra		{ get; set; }
		public string?		FacMovSTK		{ get; set; }
		public int?			FacMovSTKlin	{ get; set; }

		public string?		FacEstado		{ get; set; }
		public decimal?		FacQtyENT		{ get; set; }
		public DateTime?	FacFecEntrega	{ get; set; }
		public decimal?		FacLinComision	{ get; set; }

		public string?		FacMedidas		{ get; set; }
		public int?			FacImageSelect	{ get; set; }

        public string?       IsoUser			{ get; set; }
        public DateTime?    IsoFecAlt		{ get; set; }
        public DateTime?    IsoFecMod		{ get; set; }


        /* CAMPOS CALCULADOS */

	}
}
