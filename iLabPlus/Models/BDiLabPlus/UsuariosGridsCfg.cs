using System;
using System.Collections.Generic;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class UsuariosGridsCfg
    {
        public Guid         Guid            { get; set; }
        public string       Empresa         { get; set; }
		public string       Usuario         { get; set; }
		public string       GridID          { get; set; }
		public string?       ColumnsLayout   { get; set; }
		public int?         ColumnsPinned   { get; set; }

        public string?       IsoUser         { get; set; }
        public DateTime?    IsoFecAlt       { get; set; }
        public DateTime?    IsoFecMod       { get; set; }



    }
}
