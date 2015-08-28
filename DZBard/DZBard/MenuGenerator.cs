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

            var TSMenu = new Menu("TargetSelector","dz191.bard.ts");

            TargetSelector.AddToMenu(TSMenu);

            rootMenu.AddSubMenu(TSMenu);

            var comboMenu = new Menu("Combo","dz191.bard.combo");
            {
                comboMenu.AddItem(new MenuItem("dz191.bard.combo.useq", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191.bard.combo.distance", "Calculation distance").SetValue(new Slider(250, 100, 450)));
                comboMenu.AddItem(new MenuItem("dz191.bard.combo.accuracy", "Accuracy").SetValue(new Slider(20, 1, 50)));

            }
            rootMenu.AddSubMenu(comboMenu);

            //All Bard needs for now is Q amk

            rootMenu.AddToMainMenu();
        }
    }
}
