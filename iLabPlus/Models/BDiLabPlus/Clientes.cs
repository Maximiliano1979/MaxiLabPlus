using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Clientes
    {
        public Guid Guid { get; set; }
        public string Empresa { get; set; }
        public string Cliente { get; set; }
        public string? CliNombre { get; set; }
        public string? CliRazon { get; set; }
        public string? MultiEmpresa { get; set; }
        public string? CliNIF { get; set; }
        public string? CliCuentaContable { get; set; }
        public string? CliGrupo { get; set; }
        public string? CliTelefono1 { get; set; }
        public string? CliTelefono2 { get; set; }
        public string? CliTelefono3 { get; set; }
        public string? CliTelefono4 { get; set; }
        public string? CliTelefono1Desc { get; set; }
        public string? CliTelefono2Desc { get; set; }
        public string? CliTelefono3Desc { get; set; }
        public string? CliTelefono4Desc { get; set; }
        public string? CliMail { get; set; }
        public string? CliMailAlbaranes { get; set; }
        public string? CliMailFacturacion { get; set; }
        public string? CliWeb { get; set; }
        public string? CliPerContacto { get; set; }
        public string? CliVendedor { get; set; }
        public decimal? CliIVA { get; set; }
        public decimal? CliRE { get; set; }
        public decimal? CliIGIC { get; set; }
        public decimal? CliIRPF { get; set; }
        public string? CliTarifaVenta { get; set; }
        public string? CliDatFPago { get; set; }
        public decimal? CliDatFPint { get; set; }
        public decimal? CliDatFPlazo { get; set; }
        public decimal? CliDatFPvto { get; set; }
        public decimal? CliDatFPdia1 { get; set; }
        public decimal? CliDatFPdia2 { get; set; }
        public decimal? CliDatFPdia3 { get; set; }
        public string? CliObserv { get; set; }
        public string? CliKIL { get; set; }
        public string? CliCOL { get; set; }
        public string? CliACB { get; set; }
        public string? CliBAN { get; set; }
        public string? CliExpNac { get; set; }
        public string? CliDivisa { get; set; }
        public string? CliIdioma { get; set; }
        public string? CliFacturaTipo { get; set; }
        public string? CliFacturaTipoImp { get; set; }
        public decimal? CliDTOCial { get; set; }
        public decimal? CliDTOPpago { get; set; }
        public decimal? CliDTORappel { get; set; }
        public string? CliDirMerDireccion { get; set; }
        public string? CliDirMerDP { get; set; }
        public string? CliDirMerPoblacion { get; set; }
        public string? CliDirMerProvincia { get; set; }        
        public string? CliDirMerPais { get; set; }
        public string? CliDirFacDireccion { get; set; }
        public string? CliDirFacDP { get; set; }
        public string? CliDirFacPoblacion { get; set; }
        public string? CliDirFacProvincia { get; set; }
        public string? CliDirFacPais { get; set; }
        public decimal? CliDirLatitud { get; set; }
        public decimal? CliDirLongitud { get; set; }
        public string? CliBcoBanco { get; set; }
        public string? CliBcoCtaCte1 { get; set; }
        public string? CliBcoCtaCte2 { get; set; }
        public string? CliBcoCtaCte3 { get; set; }
        public string? CliBcoCtaCte4 { get; set; }
        public string? CliBcoSwift { get; set; }
        public string? CliBcoIBAN { get; set; }
        public string? IsoUser { get; set; }
        public DateTime? IsoFecAlt { get; set; }
        public DateTime? IsoFecMod { get; set; }

        /* CAMPOS CALCULADOS */
        [NotMapped]
        public string? CalcClienteNombre { get; set; }

    }
}
