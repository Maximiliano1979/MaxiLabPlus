using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class ArticulosIMG
    {
        public Guid         Guid                { get; set; }
        public string       Empresa             { get; set; }
        public string       Articulo            { get; set; }
        public int          Secuencia           { get; set; }

        public string       Tipo                { get; set; }
        public string       Imagen              { get; set; }

        public string?       IsoUser             { get; set; }
        public DateTime?    IsoFecAlt           { get; set; }
        public DateTime?    IsoFecMod           { get; set; }


    }
}
