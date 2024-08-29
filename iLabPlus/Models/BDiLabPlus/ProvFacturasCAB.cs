using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace iLabPlus.Models.BDiLabPlus
{
    public partial class ProvFacturasCAB
    {
        public Guid         Guid			{ get; set; }
        public string       Empresa			{ get; set; }
		public string?		MultiEmpresa	{ get; set; }
		public string       Proveedor		{ get; set; }

		public string		Factura			{ get; set; }
		public string?		FacRefProveedor { get; set; }
		public string?		FacTipo			{ get; set; }
		public string?		FacTipoVenta	{ get; set; }
		public DateTime?	FacFecha		{ get; set; }

        public string?		FacEstado		{ get; set; }
		public decimal?		FacOroFino		{ get; set; }
		public string?		FacPrioridad	{ get; set; }
		public string?		FacVendedor		{ get; set; }

		public decimal?		FacIVA			{ get; set; }
		public decimal?		FacRE			{ get; set; }
		public decimal?		FacIGIC			{ get; set; }
		public decimal?		FacIRPF			{ get; set; }

		public string?		FacTarifa		{ get; set; }

		public string?		FacDatFPago		{ get; set; }
		public decimal?		FacDatFPint		{ get; set; }
		public decimal?		FacDatFPlazo	{ get; set; }
		public decimal?		FacDatFPvto		{ get; set; }
		public decimal?		FacDatFPdia1	{ get; set; }
		public decimal?		FacDatFPdia2	{ get; set; }
		public decimal?		FacDatFPdia3	{ get; set; }

		public string?		FacKIL			{ get; set; }
		public string?		FacCOL			{ get; set; }
		public string?		FacACB			{ get; set; }
		public string?		FacBAN			{ get; set; }

		public string?		FacDivisa		{ get; set; }
		public string?		FacIdioma		{ get; set; }
		public string?		FacTipoImp		{ get; set; }

		public decimal?		FacDTOCial		{ get; set; }
		public decimal?		FacDTOPpago		{ get; set; }
		public decimal?		FacDTORappel	{ get; set; }

		public string?		FacDirMerDireccion	{ get; set; }
		public string?		FacDirMerDP			{ get; set; }
		public string?		FacDirMerPoblacion	{ get; set; }
		public string?		FacDirMerProvincia	{ get; set; }
		public string?		FacDirMerPais		{ get; set; }
		public string?		FacDirFacDireccion	{ get; set; }
		public string?		FacDirFacDP			{ get; set; }
		public string?		FacDirFacPoblacion	{ get; set; }
		public string?		FacDirFacProvincia	{ get; set; }
		public string?		FacDirFacPais		{ get; set; }

		public string?		CliBcoBanco			{ get; set; }
		public string?		CliBcoCtaCte1		{ get; set; }
		public string?		CliBcoCtaCte2		{ get; set; }
		public string?		CliBcoCtaCte3		{ get; set; }
		public string?		CliBcoCtaCte4		{ get; set; }
		public string?		CliBcoSwift			{ get; set; }
		public string?		CliBcoIBAN			{ get; set; }

		
		
		public string?		FacObserv			{ get; set; }
		public string?		FacObserv2			{ get; set; }

		public string?       IsoUser     { get; set; }
        public DateTime?    IsoFecAlt   { get; set; }
        public DateTime?    IsoFecMod   { get; set; }




        /* CAMPOS CALCULADOS */

        [NotMapped]
        public string Pedidos { get; set; }

        [NotMapped]
        public string Albaranes { get; set; }

        [NotMapped]
        public string ProveedorNombre { get; set; }

		[NotMapped]
		public string VendedorNombre { get; set; }

		[NotMapped]
		public decimal TotalFacBI { get; set; }

		[NotMapped]
		public decimal TotalDtoCial { get; set; }
		[NotMapped]
		public decimal TotalDtoPpago { get; set; }
		[NotMapped]
		public decimal TotalDtoRappel { get; set; }

		[NotMapped]
		public decimal TotalFacDTOs { get; set; }

		[NotMapped]
		public decimal TotalFacIVA { get; set; }

		[NotMapped]
		public decimal TotalFacPendBI { get; set; }

		[NotMapped]
		public decimal TotalFacPend { get; set; }

		[NotMapped]
		public decimal TotalFac { get; set; }

	}
}
