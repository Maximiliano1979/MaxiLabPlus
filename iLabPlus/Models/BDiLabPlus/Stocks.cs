using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Stocks
    {
        public Guid     Guid                { get; set; }
        public string   Empresa             { get; set; }
		public string   StkAlmacen          { get; set; }
		public string   StkArticulo         { get; set; }
		public string   StkKilates          { get; set; }
		public string   StkColor            { get; set; }
		public string   StkAcabado          { get; set; }
		public string   StkBano             { get; set; }

        public string?		StkUbicacion    { get; set; }

		public decimal?		StkFisico		{ get; set; }
		public decimal?		StkMaximo		{ get; set; }
		public decimal?		StkMinimo		{ get; set; }
		
        public decimal?		StkFisicoPeso	{ get; set; }
        public decimal?		StkReservado	{ get; set; }
		public decimal?		StkIniFIS		{ get; set; }
		public decimal?		StkIniFISpes	{ get; set; }
		public decimal?		StkCiclico		{ get; set; }
		public DateTime?	StkCiclicoFecha { get; set; }
		public decimal?		StkCiclicoPeso	{ get; set; }

        public string?       IsoUser         { get; set; }
        public DateTime?    IsoFecAlt       { get; set; }
        public DateTime?    IsoFecMod       { get; set; }

		/* CAMPOS CALCULADOS */
		[NotMapped]
		public string		ArtDescrip		{ get; set; }

		[NotMapped]
		public int?			StkAlmOrden		{ get; set; }
	}
}
