using System;
using System.Collections.Generic;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Vendedores
    {
        public Guid     Guid			{ get; set; }
        public string   Empresa			{ get; set; }
        public string   Vendedor		{ get; set; }
		public string?	VenNombre		{ get; set; }
		public string?	VenNIF			{ get; set; }
		public string? VenDireccion		{ get; set; }
		public string? VenDirDP			{ get; set; }
		public string? VenDirPoblacion	{ get; set; }
		public string? VenDirProvincia	{ get; set; }
		public string? VenDirPais		{ get; set; }
		public string? VenTelefono1		{ get; set; }
		public string? VenTelefono1Desc	{ get; set; }
		public string? VenTelefono2		{ get; set; }
		public string? VenTelefono2Desc	{ get; set; }
		public string? VenTelefono3		{ get; set; }
		public string? VenTelefono3Desc	{ get; set; }
		public string? VenMail			{ get; set; }
		public string? VenBcoBanco		{ get; set; }
        public string? VenBcoCtaCte1		{ get; set; }
        public string? VenBcoCtaCte2		{ get; set; }
        public string? VenBcoCtaCte3		{ get; set; }
        public string? VenBcoCtaCte4		{ get; set; }
        public string? VenBcoSwift		{ get; set; }
        public string? VenBcoIBAN		{ get; set; }
		public string? VenObserv			{ get; set; }
		public decimal? VenIVA					{ get; set; }
		public decimal? VenIRPF					{ get; set; }
		public decimal? VenComisionEtiqueta		{ get; set; }
		public decimal? VenComisionPeso			{ get; set; }
		public decimal? VenComisionPesoHechura	{ get; set; }
		public decimal? VenComisionHechura		{ get; set; }
		public string? VenFPago				{ get; set; }
		public decimal? VenFPvto				{ get; set; }
		public decimal? VenFPint				{ get; set; }
		public decimal? VenFPlazo				{ get; set; }
		public decimal? VenFPdia1				{ get; set; }
		public decimal? VenFPdia2				{ get; set; }
		public decimal? VenFPdia3				{ get; set; }
		public string? IsoUser { get; set; }
        public DateTime?    IsoFecAlt { get; set; }
        public DateTime?    IsoFecMod { get; set; }

    }
}
