using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class Contactos
    {
        [Required]
        [Key]
        public Guid Guid                    { get; set; }

        [Required]
        [StringLength(15)]
        public string Empresa               { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre                { get; set; }


        [StringLength(100)]
        public string? Organizacion          { get; set; }



        [StringLength(50)]
        public string? Cargo                 { get; set; }


        [Required]
        [StringLength(250)]
        public string Email                 { get; set; }

        [Required]
        [StringLength(250)]
        public string Direccion             { get; set; }

        [Required]
        [StringLength(250)]
        public string CP                    { get; set; }

        [Required]
        [StringLength(250)]
        public string Poblacion             { get; set; }

        [Required]
        [StringLength(250)]
        public string Pais                  { get; set; }


        [StringLength(50)]
        public string? TelefonoFijoEmpresa   { get; set; }

        [StringLength(50)]
        public string? TelefonoMovilEmpresa  { get; set; }

        [StringLength(50)]
        public string? TelefonoMovilPersonal { get; set; }

        [StringLength(50)]
        public string? TelefonoFijoPersonal  { get; set; }

        public DateTime? FechaNacimiento    { get; set; }

        [StringLength(500)]
        public string? Notas                 { get; set; }

        [StringLength(250)]
        public string? Web                   { get; set; }

        [StringLength(50)]

        public string? Tipo                  { get; set; }
        public string? IsoUser               { get; set; }
        public DateTime? IsoFecAlt          { get; set; }
        public DateTime? IsoFecMod          { get; set; }
        public Boolean? Activo               { get; set; }
    }
}
