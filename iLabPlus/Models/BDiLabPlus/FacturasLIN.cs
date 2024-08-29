using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class FacturasLIN
    {
        public Guid Guid { get; set; }

        [MaxLength(15)]
        public string Empresa { get; set; }

        [MaxLength(30)]
        public string Cliente { get; set; }

        [MaxLength(25)]
        public string Factura { get; set; }

        [MaxLength(25)]
        public string Pedido { get; set; }

        [MaxLength(25)]
        public string Albaran { get; set; }

        public int FacLinea { get; set; }

        [MaxLength(25)]
        public string FacArt { get; set; }

        [MaxLength(150)]
        public string FacDesc { get; set; }

        [MaxLength(15)]
        public string FacOro { get; set; }

        [MaxLength(25)]
        public string FacTarifa { get; set; }

        [MaxLength(15)]
        public string FacKIL { get; set; }

        [MaxLength(15)]
        public string FacCOL { get; set; }

        [MaxLength(15)]
        public string FacACB { get; set; }

        [MaxLength(15)]
        public string FacBAN { get; set; }

        [MaxLength(25)]
        public string FacArtTipoVenta { get; set; }

        public decimal? FacQty { get; set; }

        public decimal? FacPeso { get; set; }

        public decimal? FacMerma { get; set; }

        public decimal? FacPrecio { get; set; }

        public decimal? FacDtoLin { get; set; }

        public decimal? FacPesoTotal { get; set; }

        public decimal? FacPrecioTotal { get; set; }

        [MaxLength(150)]
        public string FacObserv { get; set; }

        [MaxLength(25)]
        public string FacAlmacen { get; set; }

        [MaxLength(15)]
        public string FacMovTra { get; set; }

        [MaxLength(15)]
        public string FacMovSTK { get; set; }

        public int? FacMovSTKlin { get; set; }

        [MaxLength(25)]
        public string FacEstado { get; set; }

        public decimal? FacQtyENT { get; set; }

        public DateTime? FacFecEntrega { get; set; }

        public decimal? FacLinComision { get; set; }

        [MaxLength(150)]
        public string FacMedidas { get; set; }

        public int? FacImageSelect { get; set; }

        [MaxLength(50)]
        public string IsoUser { get; set; }

        public DateTime? IsoFecAlt { get; set; }

        public DateTime? IsoFecMod { get; set; }
    }
}
