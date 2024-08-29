using iLabPlus.Models.BDiLabPlus;
using System;
using System.Collections.Generic;

namespace iLabPlus.Models.Clases
{


    public class ArticuloEstructuraPrecio
    {
        public string Articulo { get; set; }

        public Articulos                FichaArticulo               { get; set; }

        public EstFichaArtCostes        EstCOSTES                   { get; set; }
        
        public List<ArticulosCOMP>      EstPIE                      { get; set; }
        public List<ArticulosCOMP>      EstCOMP                     { get; set; }
        public List<ArticulosMO>        EstMO                       { get; set; }

        public List<ArticulosIMG>       ListArticuloIMG             { get; set; }

        public List<PrecioEstructura>   TreeEstructuraPrecio        { get; set; }

    }
}