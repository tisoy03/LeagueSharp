using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace ThreshHunter.Utility
{
    class MenuGenerator
    {
        public static void Init(Menu RootMenu)
        {
            var ComboMenu = new Menu("Combo", "dz191.thresh.combo");
            {
                ComboMenu.AddItem(new MenuItem("dz191.thresh.combo.useQ", "Use Q").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.thresh.combo.useW", "Use W").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.thresh.combo.useE", "Use E").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.thresh.combo.useR", "Use R").SetValue(true));

                var ComboOptionsMenu = new Menu("Combo - Options","dz191.thresh.combo.settings");
                {
                    ComboOptionsMenu.AddItem(new MenuItem("dz191.thresh.combo.settings.minr", "Min Enemies for R").SetValue(new Slider(2, 1, 5)));
                    ComboOptionsMenu.AddItem(new MenuItem("dz191.thresh.combo.settings.pullebox", "Use E to pull inside box").SetValue(true));
                    ComboOptionsMenu.AddItem(new MenuItem("dz191.thresh.combo.settings.q2", "Use Q2 (Q to go in)").SetValue(false));

                    ComboMenu.AddSubMenu(ComboOptionsMenu);
                }
                RootMenu.AddSubMenu(ComboMenu);
            }

            var HarassMenu = new Menu("Harass", "dz191.thresh.harass");
            {
                HarassMenu.AddItem(new MenuItem("dz191.thresh.harass.useQ", "Use Q").SetValue(true));
                HarassMenu.AddItem(new MenuItem("dz191.thresh.harass.useE", "Use E").SetValue(true));
                HarassMenu.AddItem(new MenuItem("dz191.thresh.harass.mm", "Min Mana % for Harass").SetValue(new Slider(50)));

                RootMenu.AddSubMenu(HarassMenu);
            }


        }
    }
}
