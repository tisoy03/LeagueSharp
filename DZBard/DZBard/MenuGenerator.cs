using LeagueSharp.Common;

namespace DZBard
{
    class MenuGenerator
    {
        internal static void OnLoad(Menu rootMenu)
        {
            var OrbwalkerMenu = new Menu("Orbwalker","dz191.bard.orbwalker");
            Bard.BardOrbwalker = new Orbwalking.Orbwalker(OrbwalkerMenu);
            rootMenu.AddSubMenu(OrbwalkerMenu);

            TargetSelector.AddToMenu(rootMenu);

            var comboMenu = new Menu("Combo","dz191.bard.combo");
            {
                comboMenu.AddItem(new MenuItem("dz191.bard.combo.useq", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191.bard.combo.distance", "Calculation distance").SetValue(new Slider(250, 100, 450)));
                comboMenu.AddItem(new MenuItem("dz191.bard.combo.accuracy", "Accuracy").SetValue(new Slider(45, 1)));

            }
            rootMenu.AddSubMenu(comboMenu);

            //All Bard needs for now is Q amk

            rootMenu.AddToMainMenu();
        }
    }
}
