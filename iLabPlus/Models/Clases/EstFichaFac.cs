using iLabPlus.Models.BDiLabPlus;
using System;
using System.Collections.Generic;

namespace iLabPlus.Models.Clases
{
    public partial class EstFichaFac
    {
        public FacturasCAB FacturaEst { get; set; }
        public List<FacturasLIN> ListFacturaLIN { get; set; }


    }
}