using System;
using System.Collections.Generic;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class ValSys
    {
        public Guid         Guid                    { get; set; }
        public string       Empresa                 { get; set; }
        public string       Indice                  { get; set; }
        public string       Clave                   { get; set; }
        public string?       Valor1                  { get; set; }
        public string?       Valor2                  { get; set; }
        public string?       Valor3                  { get; set; }
        public decimal?     Valor4                  { get; set; }

        public string?       IsoUser                 { get; set; }
        public DateTime?    IsoFecAlt               { get; set; }
        public DateTime?    IsoFecMod               { get; set; }

    }
}
