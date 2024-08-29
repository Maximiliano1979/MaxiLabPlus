using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace iLabPlus.Models.BDiLabPlus
{
    public partial class PedidosCAB
    {
        public Guid         Guid			{ get; set; }
        public string       Empresa			{ get; set; }
		public string?		MultiEmpresa	{ get; set; }

		public string       Cliente			{ get; set; }
		public string		Pedido			{ get; set; }
		public string?		PedTipo			{ get; set; }

		public string?		PedTipoVenta	{ get; set; }
		public DateTime?	PedFecha		{ get; set; }
		public DateTime?	PedFechaEnt		{ get; set; }

		public string?		PedEstado		{ get; set; }
		public decimal?		PedOroFino		{ get; set; }
		public string?		PedPrioridad	{ get; set; }
		public string?		PedVendedor		{ get; set; }

		public decimal?		PedIVA			{ get; set; }
		public decimal?		PedRE			{ get; set; }
		public decimal?		PedIGIC			{ get; set; }
		public decimal?		PedIRPF			{ get; set; }

		public string?		PedTarifaVenta	{ get; set; }

		public string?		PedDatFPago		{ get; set; }
		public decimal?		PedDatFPint		{ get; set; }
		public decimal?		PedDatFPlazo	{ get; set; }
		public decimal?		PedDatFPvto		{ get; set; }
		public decimal?		PedDatFPdia1	{ get; set; }
		public decimal?		PedDatFPdia2	{ get; set; }
		public decimal?		PedDatFPdia3	{ get; set; }

		public string?		PedKIL			{ get; set; }
		public string?		PedCOL			{ get; set; }
		public string?		PedACB			{ get; set; }
		public string?		PedBAN			{ get; set; }

		public string?		PedDivisa		{ get; set; }
		public string?		PedIdioma		{ get; set; }
		public string?		PedTipoImp		{ get; set; }

		public decimal?		PedDTOCial		{ get; set; }
		public decimal?		PedDTOPpago		{ get; set; }
		public decimal?		PedDTORappel	{ get; set; }

		public string?		PedDirMerDireccion	{ get; set; }
		public string?		PedDirMerDP			{ get; set; }
		public string?		PedDirMerPoblacion	{ get; set; }
		public string?		PedDirMerProvincia	{ get; set; }
		public string?		PedDirMerPais		{ get; set; }
		public string?		PedDirFacDireccion	{ get; set; }
		public string?		PedDirFacDP			{ get; set; }
		public string?		PedDirFacPoblacion	{ get; set; }
		public string?		PedDirFacProvincia	{ get; set; }
		public string?		PedDirFacPais		{ get; set; }

		public string?		CliBcoBanco			{ get; set; }
		public string?		CliBcoCtaCte1		{ get; set; }
		public string?		CliBcoCtaCte2		{ get; set; }
		public string?		CliBcoCtaCte3		{ get; set; }
		public string?		CliBcoCtaCte4		{ get; set; }
		public string?		CliBcoSwift			{ get; set; }
		public string?		CliBcoIBAN			{ get; set; }

		
		public string?		PedRefCliente		{ get; set; }
		public string?		PedObserv			{ get; set; }
		public string?		PedObserv2			{ get; set; }

        public bool?       PedAlbaran			{ get; set; }

        public DateTime?	 PedAlbaranFecha	{ get; set; }


        public string?       IsoUser     { get; set; }
        public DateTime?    IsoFecAlt   { get; set; }
        public DateTime?    IsoFecMod   { get; set; }




        /* CAMPOS CALCULADOS */
        [NotMapped]
        public string? PedMRP { get; set; }

        //[NotMapped]
        //public IQueryable<string?> PedMRPQuery { get; set; }

        [NotMapped]
        public string? Albaranes { get; set; }

        [NotMapped]
        public List<int> PedMRPQuery { get; set; }

        [NotMapped]
        public string? ClienteNombre { get; set; }

		[NotMapped]
		public string? VendedorNombre { get; set; }

		[NotMapped]
		public decimal TotalPedBI { get; set; }

		[NotMapped]
		public decimal TotalDtoCial { get; set; }
		[NotMapped]
		public decimal TotalDtoPpago { get; set; }
		[NotMapped]
		public decimal TotalDtoRappel { get; set; }

		[NotMapped]
		public decimal TotalPedDTOs { get; set; }

		[NotMapped]
		public decimal TotalPedIVA { get; set; }

		[NotMapped]
		public decimal TotalPedPendBI { get; set; }

		[NotMapped]
		public decimal TotalPedPend { get; set; }

		[NotMapped]
		public decimal TotalPed { get; set; }

	}
}
