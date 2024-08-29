using iLabPlus.Models.BDiLabPlus;
using System;
using System.Collections.Generic;

namespace iLabPlus.Models.Clases
{
    public partial class PrecioEstructura
    {
        public string Id { get; set; }
        public string Header { get; set; }
        public List<PrecioEstructura> Items { get; set; }
        public decimal Valor { get; set; }
    }
}