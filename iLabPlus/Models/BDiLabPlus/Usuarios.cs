using System;
using System.Collections.Generic;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Usuarios
    {
        public Guid          Guid                    { get; set; }
        public string        Empresa                 { get; set; }
        public string        Usuario                 { get; set; }
        public string        UsuarioNombre           { get; set; }
        public string        UsuarioTipo             { get; set; }
        public string?       Password                { get; set; }
        public string?       ControllerInit          { get; set; }

        public string?       webpasswordchanged      { get; set; }
        public DateTime?     webpasswordlastchanged  { get; set; }
        public string?       tokenid                 { get; set; }
        public DateTime?     tokenissuedutc          { get; set; }
        public DateTime?     tokenexpiredutc         { get; set; }

        public string?       FiltroAccesoMac         { get; set; }
        public string?       FiltroAccesoHoraInicial { get; set; }
        public string?       FiltroAccesoHoraFinal   { get; set; }

        public string?       Menus                   { get; set; }

        public string?       IsoUser                 { get; set; }
        public DateTime?     IsoFecAlt               { get; set; }
        public DateTime?     IsoFecMod               { get; set; }

    }
}
