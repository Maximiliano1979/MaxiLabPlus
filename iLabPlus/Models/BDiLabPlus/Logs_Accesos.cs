using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Logs_Accesos
    {
        public Guid         Guid                { get; set; }
        public string       Empresa             { get; set; }

        public string?       IpLocal             { get; set; }
        public string?       IpPublica           { get; set; }


        public string?       Recurso             { get; set; }
        public string?       Sistema             { get; set; }
        public string?       Dispositivo         { get; set; }


        public string?       UserNombre          { get; set; }

        public string?       IsoUser             { get; set; }
        public DateTime?    IsoFecAlt           { get; set; }
        public DateTime?    IsoFecMod           { get; set; }

    }
}
