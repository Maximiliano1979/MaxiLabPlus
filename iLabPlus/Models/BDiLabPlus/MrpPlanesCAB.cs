using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class MrpPlanesCAB
    {
        public Guid         Guid			{ get; set; }
        public string       Empresa			{ get; set; }
        public int			PlanMrp			{ get; set; }

        public string?       PlanDen         { get; set; }
        public DateTime?     PlanFecLan      { get; set; }
        public DateTime?    PlanFecFin      { get; set; }
        public string?       PlanEstado      { get; set; }

        public string?       IsoUser         { get; set; }
        public DateTime?    IsoFecAlt       { get; set; }
        public DateTime?    IsoFecMod       { get; set; }


	}
}
