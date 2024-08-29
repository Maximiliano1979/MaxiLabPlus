using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace iLabPlus.Models.BDiLabPlus
{
    public class EmpresasCertificados
    {
        public Guid Guid { get; set; }
        public string Empresa { get; set; }
        public string Fichero { get; set; }
        
        public string? RutaArchivo { get; set; }

        [Column(TypeName = "varbinary(MAX)")]
        public byte[] Password { get; set; }

    }
}
