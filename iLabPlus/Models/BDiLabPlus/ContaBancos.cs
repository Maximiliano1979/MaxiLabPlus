using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class ContaBancos
    {
        public Guid     Guid				{ get; set; }
        public string   Empresa				{ get; set; }
        public string	Banco				{ get; set; }
		public string?	BcoNombre			{ get; set; }
        public string?	BcoCuentaContable	{ get; set; }
        public string?	BcoMail				{ get; set; }
		public string? BcoPerContacto		{ get; set; }

		public string? BcoTelefono1		{ get; set; }
		public string? BcoTelefono2		{ get; set; }
		public string? BcoTelefono1Desc	{ get; set; }
		public string? BcoTelefono2Desc	{ get; set; }

		public string? BcoDireccion		{ get; set; }
		public string? BcoDirDP			{ get; set; }
		public string? BcoDirPoblacion		{ get; set; }
		public string? BcoDirProvincia		{ get; set; }
		public string? BcoDirPais			{ get; set; }

		public string? BcoCtaCte1			{ get; set; }
		public string? BcoCtaCte2			{ get; set; }
		public string? BcoCtaCte3			{ get; set; }
		public string? BcoCtaCte4			{ get; set; }
		public string? BcoSwift			{ get; set; }
		public string? BcoIBAN				{ get; set; }

		public string? BcoObserv			{ get; set; }

		public string? IsoUser         { get; set; }
        public DateTime?    IsoFecAlt       { get; set; }
        public DateTime?    IsoFecMod       { get; set; }



    }
}
