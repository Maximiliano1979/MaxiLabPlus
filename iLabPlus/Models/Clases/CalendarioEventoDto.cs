using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace iLabPlus.Models.Clases
{
    public class CalendarioEventoDto
    {
        public string Guid { get; set; }
        public string Empresa { get; set; }
        public string Titulo { get; set; }
        public string Fecha { get; set; }
        public string Detalle { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
        public string Alcance { get; set; }
        public string UsuarioEspecifico { get; set; }
        public List<string> Usuarios { get; set; }
        public bool RecibirMail { get; set; }
    }
}
