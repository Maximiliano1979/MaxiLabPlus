using iLabPlus.Models.BDiLabPlus;
using System;
using System.Collections.Generic;

namespace iLabPlus.Models.Clases
{
    public partial class EstFichaArt
    {
        public Articulos                Articulo            { get; set; }
        public List<ArticulosCOMP>      ListArticuloPIE     { get; set; }
        public List<ArticulosCOMP>      ListArticuloCOMP    { get; set; }
        public List<ArticulosMO>        ListArticuloMO      { get; set; }
        public List<ArticulosIMG>       ListArticuloIMG     { get; set; }

        public PedidosLIN               PedLin              { get; set; }

        public FacturasLIN              FacLin              { get; set; }

    }
}