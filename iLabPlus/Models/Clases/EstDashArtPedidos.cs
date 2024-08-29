using iLabPlus.Models.BDiLabPlus;
using System;
using System.Collections.Generic;

namespace iLabPlus.Models.Clases
{
    public partial class EstDashArtPedidos
    {
        public string   Articulo                    { get; set; }
        public decimal? ArticuloQtyPedido           { get; set; }
        public decimal? TotalQtyTodosArticulos      { get; set; }

        public decimal? ArticuloPrecioPedido        { get; set; }
        public decimal? TotalPrecioTodosArticulos   { get; set; }

        public string   Porcentaje                  { get; set; }
        public string   ArtDesc                     { get; set; }

    }
}