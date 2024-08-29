using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace iLabPlus.Models.BDiLabPlus
{
    public partial class MrpPlanesLIN
    {
        public Guid         Guid			{ get; set; }
        public string       Empresa			{ get; set; }
        public int			PlanMrp			{ get; set; }
        public int          PlanSobre       { get; set; }
        
        public string       Cliente         { get; set; }
        public string       Pedido          { get; set; }

        //public string       Factura          { get; set; }
        public int          PedLinea        { get; set; }

        public string       PedArt          { get; set; }

        public string?       PedOro          { get; set; }
        public string?      PedKil          { get; set; }

        public string?       IsoUser         { get; set; }
        public DateTime?    IsoFecAlt       { get; set; }
        public DateTime?    IsoFecMod       { get; set; }

	}
}
