using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class ControlHorario
    {
        public Guid Guid { get; set; }
       
        public string Empresa { get; set; }

        public string Empleado { get; set; }

        public DateTime Fecha { get; set; }

        public TimeSpan HoraEntrada { get; set; }

        public TimeSpan? HoraSalida { get; set; }

        public TimeSpan? HorasTrabajadas { get; set; }

        public string? Cierre { get; set; }

        public string? Observaciones { get; set; }

        [NotMapped]
        public string? EmpleadoNombre { get; set; }
    }
}
