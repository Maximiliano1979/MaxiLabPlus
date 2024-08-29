using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace iLabPlus.Models.BDiLabPlus
{
    public partial class FacturasCAB
    {
        public Guid         Guid			{ get; set; }
        public string       Empresa			{ get; set; }
		public string?		MultiEmpresa	{ get; set; }

		public string       Cliente			{ get; set; }
		public string		Factura			{ get; set; }
		public string? FacRefCliente	{ get; set; }
		public string? FacTipo			{ get; set; }
		public string? FacTipoVenta	{ get; set; }
		public DateTime?	FacFecha		{ get; set; }

        public string? FacEstado		{ get; set; }
		public decimal?		FacOroFino		{ get; set; }
		public string? FacPrioridad	{ get; set; }
		public string? FacVendedor		{ get; set; }


		public decimal?		FacIVA			{ get; set; }
		public decimal?		FacRE			{ get; set; }
		public decimal?		FacIGIC			{ get; set; }
		public decimal?		FacIRPF			{ get; set; }

		public string?		FacTarifaVenta	{ get; set; }

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

        public DateTime?	FacEnviada			{ get; set; }
        public DateTime?	FacFirmada			{ get; set; }
        public string?		FacFicheroFirmado	{ get; set; }

        public string?       IsoUser				{ get; set; }
        public DateTime?    IsoFecAlt			{ get; set; }
        public DateTime?    IsoFecMod			{ get; set; }

        public decimal?		TotalFac			{ get; set; }
        public decimal?		TotalFacBI			{ get; set; }
        public decimal?		TotalFacIVA			{ get; set; }

		public decimal?		TotalFacDTOs		{ get; set; }
        public decimal?		TotalDtoCial		{ get; set; }
        public decimal?		TotalDtoPpago		{ get; set; }
        public decimal?		TotalDtoRappel		{ get; set; }
        
        public decimal?		TotalFacPend		{ get; set; }
        public decimal?		TotalFacPendBI		{ get; set; }

        /* CAMPOS CALCULADOS */
        [NotMapped]
        public string? Acciones { get; set; }

        [NotMapped]
        public string? Pedidos { get; set; }

        [NotMapped]
        public string? Albaranes { get; set; }

        [NotMapped]
        public string? ClienteNombre { get; set; }

		[NotMapped]
		public string? VendedorNombre { get; set; }


		

	}
}
