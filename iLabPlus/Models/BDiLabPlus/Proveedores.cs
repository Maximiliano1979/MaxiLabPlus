using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Proveedores
    {
        public Guid     Guid                { get; set; }
        public string   Empresa             { get; set; }
        public string   Proveedor           { get; set; }

        //[Required(ErrorMessage = "Este campo es obligatorio")]
        public string?   ProNombre           { get; set; }

        //[Required(ErrorMessage = "Este campo es obligatorio")]
        public string?   ProRazon            { get; set; }
        public string?   ProTipo             { get; set; }
        public string?   ProNIF              { get; set; }
        public string?   ProCuentaContable   { get; set; }


        public string?   ProTelefono1           { get; set; }
        public string?   ProTelefono2           { get; set; }
        public string?   ProTelefono3           { get; set; }
        public string?   ProTelefono4           { get; set; }
        public string?   ProTelefono1Desc       { get; set; }
        public string?   ProTelefono2Desc       { get; set; }
        public string?   ProTelefono3Desc       { get; set; }
        public string?   ProTelefono4Desc       { get; set; }
        public string?   ProMail                { get; set; }
        public string?   ProMailAlbaranes       { get; set; }
        public string?   ProMailFacturacion     { get; set; }
        public string?   ProWeb                 { get; set; }
        public string?   ProPerContacto         { get; set; }

        public decimal? ProIVA                 { get; set; }
        public decimal? ProRE                  { get; set; }
        public decimal? ProIGIC                { get; set; }
        public decimal? ProIRPF                { get; set; }
        
        public string   ProDatFPago            { get; set; }
        public decimal? ProDatFPint            { get; set; }
        public decimal? ProDatFPlazo           { get; set; }
        public decimal? ProDatFPvto            { get; set; }
        public decimal? ProDatFPdia1           { get; set; }
        public decimal? ProDatFPdia2           { get; set; }
        public decimal? ProDatFPdia3           { get; set; }
        public string?   ProObserv              { get; set; }

        public string?   ProDivisa              { get; set; }
        public string?   ProIdioma              { get; set; }
        public string?   ProFacturaTipo         { get; set; }
        public string?   ProFacturaTipoImp      { get; set; }

        public decimal? ProDTOCial             { get; set; }
        public decimal? ProDTOPpago            { get; set; }
        public decimal? ProDTORappel           { get; set; }
        public decimal? ProMerma               { get; set; }

        public string?   ProDirMerDireccion     { get; set; }
        public string?   ProDirMerDP            { get; set; }
        public string?   ProDirMerPoblacion     { get; set; }
        public string?   ProDirMerProvincia     { get; set; }        
        public string?   ProDirMerPais          { get; set; }

        public string?   ProDirFacDireccion     { get; set; }
        public string?   ProDirFacDP            { get; set; }
        public string?   ProDirFacPoblacion     { get; set; }
        public string?   ProDirFacProvincia     { get; set; }
        public string?   ProDirFacPais          { get; set; }

        public decimal? ProDirLatitud      { get; set; }
        public decimal? ProDirLongitud     { get; set; }

        public string?   ProBcoBanco        { get; set; }
        public string?   ProBcoCtaCte1      { get; set; }
        public string?   ProBcoCtaCte2      { get; set; }
        public string?   ProBcoCtaCte3      { get; set; }
        public string?   ProBcoCtaCte4      { get; set; }
        public string?   ProBcoSwift        { get; set; }
        public string?   ProBcoIBAN         { get; set; }


        public string?       IsoUser         { get; set; }
        public DateTime?    IsoFecAlt       { get; set; }
        public DateTime?    IsoFecMod       { get; set; }


    }
}
