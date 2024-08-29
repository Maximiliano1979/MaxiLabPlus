using System;
using System.Collections.Generic;

namespace iLabPlus.Models.Clases
{
    public partial class MenuUser
    {
        public string Menu                  { get; set; }
        public string Menu_Tooltip { get; set; }
        public string Menu_Icono { get; set; }
        public List<MenuUserAccesos> Accesos     { get; set; }



        public MenuUser(string Menu, string Menu_Tooltip, List<MenuUserAccesos> Accesos, string Menu_Icono)
        {
            this.Menu       = Menu;
            this.Menu_Tooltip = Menu_Tooltip;
            this.Accesos    = Accesos;
            this.Menu_Icono = Menu_Icono;

        }

    }
}