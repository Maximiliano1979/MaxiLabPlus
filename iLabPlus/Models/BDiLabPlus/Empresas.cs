using System;
using System.Collections.Generic;


namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Empresas
    {
        public Guid Guid { get; set; }
        public string Empresa { get; set; }
        public string Mail { get; set; }
        public string? Nombre { get; set; }
        public string? RazonSocial { get; set; }
        public string? Web { get; set; }
        public string? Telefono { get; set; }
        public string? Persona { get; set; }
        public string? PerfilImagen { get; set; }
        public string? Perfil { get; set; }
        public string? Language { get; set; }
        public string? Locale { get; set; }
        
        public bool? Activo { get; set; }
        public string? IsoUser { get; set; }
        public DateTime? IsoFecAlt { get; set; }
        public DateTime? IsoFecMod { get; set; }

        public string? Direccion { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Poblacion { get; set; }
        public string? Provincia { get; set; }
        public string? Pais { get; set; }

        public string? MailFact { get; set;}
        public string? Nif { get; set;}

        // Campos de control
        public string?       TokenPassword           { get; set; }
        public string?       TokenId                 { get; set; }
        public DateTime?    TokenIssuedUtc          { get; set; }
        public DateTime?    TokenExpiredUtc         { get; set; }
        public string?       TokenPasswordChg        { get; set; }
        public DateTime?    TokenPasswordLastChg    { get; set; }

        public string?       Subscripcion            { get; set; }
        public DateTime?    Renovacion              { get; set; }
    }
}
