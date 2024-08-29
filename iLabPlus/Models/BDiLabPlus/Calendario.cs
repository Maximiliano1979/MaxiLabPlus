using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace iLabPlus.Models.BDiLabPlus
{
    public class Calendario
    {
    
        public Guid Guid { get; set; }

        public string Usuario { get; set; }

        public string? Titulo { get; set; }

        public DateTime Fecha { get; set; }

        public string? Detalle { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        public string? Alcance { get; set; }

        public string? UsuarioEspecifico { get; set; }

        public string Empresa { get; set; }

        public bool RecibirMail { get; set; }
    }
}
