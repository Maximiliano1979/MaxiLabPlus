using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace iLabPlus.Models.BDiLabPlus
{
    public partial class MrpPlanesLINMO
    {
        public Guid         Guid			{ get; set; }
        public string       Empresa			{ get; set; }
        public int          PlanMrp         { get; set; }
        public int          PlanSobre       { get; set; }

        public string       Cliente			{ get; set; }
		public string		Pedido			{ get; set; }
		public int			PedLinea		{ get; set; }
        public int?         Secuencia       { get; set; }


        public string       Articulo            { get; set; }
        public int          MOSecuencia         { get; set; }

        public string       CodMO               { get; set; }
        public string       CodMODescripcion    { get; set; }

        public string       MOExtInt            { get; set; }
        public decimal      MOCantidad          { get; set; }
        public decimal      MOPrecio            { get; set; }

        
        public string?       MOUniMed            { get; set; }
        
        public string?       PedOro              { get; set; }
        public string?       PedKil              { get; set; }

        public string?       IsoUser             { get; set; }
        public DateTime?    IsoFecAlt           { get; set; }
        public DateTime?    IsoFecMod           { get; set; }

        [NotMapped]
        public decimal      Valor               { get; set; }

    }
}
