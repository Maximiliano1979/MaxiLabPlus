using System;
using System.Collections.Generic;

namespace iLabPlus.Models.Clases
{
    public partial class MenuUserAccesos
    {
        public string Menu                  { get; set; }
        public string Menu_ItemName         { get; set; }
        public string Menu_ItemCrontoller   { get; set; }
        public string Menu_ItemAction       { get; set; }
        public string Menu_ItemTooltip      { get; set; }
        public string Menu_ItemIcono { get; set; }


        public MenuUserAccesos(string Menu, string Menu_ItemName, string Menu_ItemCrontoller, string Menu_ItemAction, string Menu_ItemTooltip, string Menu_ItemIcono)
        {
            this.Menu                   = Menu;
            this.Menu_ItemName          = Menu_ItemName;
            this.Menu_ItemCrontoller    = Menu_ItemCrontoller;
            this.Menu_ItemAction        = Menu_ItemAction;
            this.Menu_ItemTooltip       = Menu_ItemTooltip;
            this.Menu_ItemIcono         = Menu_ItemIcono;

        }

    }
}