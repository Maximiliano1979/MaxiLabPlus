using iLabPlus.Models.BDiLabPlus;
using System;
using System.Collections.Generic;

namespace iLabPlus.Models.Clases
{
    public partial class EstDashClientesFact
    {
        public string   Cliente                     { get; set; }
        public decimal? ClienteQtyfact              { get; set; }
        public decimal? TotalQtyTodosClientes       { get; set; }

        public decimal? ClienteImporteFact          { get; set; }
        public decimal? TotalImporteTodosClientes   { get; set; }

        public string   Porcentaje                  { get; set; }
        public string   CliNombre                   { get; set; }

    }
}