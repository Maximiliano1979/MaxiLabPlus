using System;
using System.Collections.Generic;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class EmpresasConfig
    {
        public Guid     Guid                    { get; set; }
        public string   Empresa                 { get; set; }
        public string? TextosAlbPiePag         { get; set; }
        public string? TextosAlbRPGD           { get; set; }
        public string? TextosFacPiePag         { get; set; }
        public string? TextosFacRPGD           { get; set; }

        public string? TextosFirmaMail         { get; set; }


        public string? MailsEnvNombre          { get; set; }
        public string? MailsEnvCuenta          { get; set; }
        public string? MailsEnvServidor        { get; set; }
        public int?     MailsEnvPuerto          { get; set; }        
        public string? MailsEnvDirRespuesta    { get; set; }
        public bool?    MailsEnvILabPlus        { get; set; }

        public string? FormaCobroClientes      { get; set; }
        public string? CuentaBancaria          { get; set; }
        public string? RegistroMercantil       { get; set; }

        
        public string? IsoUser                 { get; set; }
        public DateTime?    IsoFecAlt           { get; set; }
        public DateTime?    IsoFecMod           { get; set; }
    }
}
