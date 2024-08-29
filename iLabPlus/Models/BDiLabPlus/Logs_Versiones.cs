using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Logs_Versiones
    {
        public Guid         Guid                { get; set; }
        public string       Empresa             { get; set; }
        public decimal      Version             { get; set; }
        public decimal      VersionSub          { get; set; }
        public DateTime     Fecha               { get; set; }

        public string?       Descripcion         { get; set; }

        public string?       IsoUser             { get; set; }
        public DateTime?    IsoFecAlt           { get; set; }
        public DateTime?    IsoFecMod           { get; set; }

    }
}
