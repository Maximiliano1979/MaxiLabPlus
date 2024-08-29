using System;
using System.Collections.Generic;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class PlantillasEnvios
    {
        public Guid     Guid                    { get; set; }
        public string   Empresa                 { get; set; }
        public string   Tipo                    { get; set; }
        public string?  Texto                   { get; set; }
        
        public string       IsoUser             { get; set; }
        public DateTime?    IsoFecAlt           { get; set; }
        public DateTime?    IsoFecMod           { get; set; }
    }
}
