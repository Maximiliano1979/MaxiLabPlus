using iLabPlus.Models.BDiLabPlus;
using System;
using System.Collections.Generic;

namespace iLabPlus.Models.Clases
{
    public partial class EstFichaArtCostes
    {
        public string TipoVenta { get; set; }
        public int Kilates { get; set; }
        public decimal PrecioPVP { get; set; }
        public decimal HechuraPieza { get; set; }
        public decimal HechuraGramo { get; set; }
        public decimal CosteGramo { get; set; }

        public decimal PesoTotal { get; set; }
        public decimal PesoTotalSinMerma { get; set; }

        public decimal PesoMetTotal { get; set; }
        public decimal PesoMetTotalSinMerma { get; set; }

        public decimal PesoCompTotal { get; set; }
        public decimal PesoCompTotalSinMerma { get; set; }

        public decimal Beneficio            { get; set; }
        public decimal CosteTotal           { get; set; }
        public decimal CosteTotalEtiqueta   { get; set; }
        public decimal CosteTotalHechura    { get; set; }

        public decimal CosteMetales { get; set; }
        public decimal CosteMatOro { get; set; }
        public decimal CosteMatOroComp { get; set; }

        public decimal CosteMateriales { get; set; }
        public decimal CosteMatPie { get; set; }
        public decimal CosteMatComp { get; set; }

        public decimal CosteMO { get; set; }
        public decimal CosteMOInt { get; set; }
        public decimal CosteMOExt { get; set; }
        public decimal CosteMOComp { get; set; }

        public decimal CosteFAB { get; set; }
        public decimal CosteFABLiga { get; set; }
        public decimal CosteFABRodio { get; set; }
        public decimal CosteFABEtq { get; set; }
        public decimal CosteFABGar { get; set; }

        public decimal LigaPeso { get; set; }
    }
}