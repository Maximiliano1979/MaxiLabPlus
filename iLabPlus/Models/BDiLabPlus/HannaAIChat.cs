using System;
using System.Collections.Generic;

#nullable disable

namespace iLabPlus.Models.BDiLabPlus
{
    public partial class HannaAIChat
    {
        public Guid     Guid                { get; set; }
        public string   Empresa             { get; set; }
        public string   Usuario             { get; set; }        
        public DateTime Fecha               { get; set; }
        public string   Tipo                { get; set; }

        public string?   Prompt              { get; set; }        
        public string?   Answer              { get; set; }

        public int?     Tokens_Prompt       { get; set; }
        public int?     Tokens_Completion   { get; set; }
        public int?     Tokens_Total        { get; set; }

    }
}
