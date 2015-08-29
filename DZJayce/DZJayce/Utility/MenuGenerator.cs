using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace DZJayce.Utility
{
    class MenuGenerator
    {
        public static void OnLoad(Menu RootMenu)
        {
            var ComboMenu = new Menu("Combo","dz191.jayce.combo");
            {
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.qranged", "Use Q Ranged").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.wranged", "Use W Ranged").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.eranged", "Use E Ranged").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.qmelee", "Use Q Melee").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.wmelee", "Use W Melee").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.emelee", "Use E Melee").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.r", "Use R").SetValue(true));
                RootMenu.AddSubMenu(ComboMenu);
            }

            RootMenu.AddToMainMenu();
        }
    }
}
