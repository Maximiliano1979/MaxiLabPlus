using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Articulos
    {
        public Guid Guid { get; set; }
        public string Empresa { get; set; }
        public string? MultiEmpresa { get; set; }
        public string? Articulo { get; set; }
        public string? ArtDes { get; set; }
        public string? ArtTipo { get; set; }
        public string? ArtTipoMat { get; set; }
        public string? ArtFamilia { get; set; }
        public string? ArtObserv { get; set; }
        public string? ArtCalidades { get; set; }
        public decimal? ArtPrecioCoste { get; set; }
        public decimal? ArtPrecioCompra { get; set; }
        public decimal? ArtPrecioPVP { get; set; }
        public decimal? ArtPeso { get; set; }
        public string? ArtInterfaceGroup { get; set; }
        public string? CtaVentas { get; set; }
        public string? CtaCompras { get; set; }
        public string? ArtTipoVenta { get; set; }
        public string? ArtTipoCompra { get; set; }
        public string? ArtProveedor { get; set; }
        public string? ArtProveedorRef { get; set; }
        public string? CompraMoneda { get; set; }
        public decimal? CompraMonedaCoti { get; set; }
        public string? IsoUser { get; set; }
        public DateTime? IsoFecAlt { get; set; }
        public DateTime? IsoFecMod { get; set; }
        public bool? Activo { get; set; }

        /* CAMPOS CALCULADOS */
        [NotMapped]
        public decimal? HechuraPieza { get; set; }

        [NotMapped]
        public decimal? HechuraGramo { get; set; }

        [NotMapped]
        public decimal? CosteGramo { get; set; }

        [NotMapped]
        public string? CalcArticuloDesc { get; set; }
    }
}
