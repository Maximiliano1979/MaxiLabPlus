using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iLabPlus.Models.BDiLabPlus
{
    public class DoctoresClinicas
    {
        public Guid Guid { get; set; }

        public string Empresa { get; set; }

        public string Doctor { get; set; }

        public string Clinica { get; set; }

        public string? IsoUser { get; set; }

        public DateTime? IsoFecAlt { get; set; }

        public DateTime? IsoFecMod { get; set; }
    }
}
