using iLabPlus.Models.BDiLabPlus;
using System;
using System.Collections.Generic;

namespace iLabPlus.Models.Clases
{
    public partial class EstFichaPed
    {
        public PedidosCAB               PedidoEst           { get; set; }
        public List<PedidosLIN>         ListPedidoLIN       { get; set; }


    }
}