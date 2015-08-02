using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

namespace Thresh___Soul_Hunter.Utility
{
    class MenuGenerator
    {
        internal static void Generate(Menu RootMenu)
        {

            var keysMenu = new Menu("dz191.thresh.keys", "Keys");
            {
                keysMenu.Add(new MenuKeyBind("ePull", "E Pull", Keys.T, KeyBindType.Press));
                keysMenu.Add(new MenuKeyBind("ePush", "E Push", Keys.G, KeyBindType.Press));
                RootMenu.Add(keysMenu);
            }

            var comboMenu = new Menu("dz191.thresh.orbwalk", "Combo Options");
            {
                ////Skills
                comboMenu.Add(new MenuSeparator("separatorSkills", "Combo - Skills"));
                comboMenu.Add(new MenuBool("useQ", "Use Q", true));
                comboMenu.Add(new MenuBool("useW", "Use W", true));
                comboMenu.Add(new MenuBool("useE", "Use E", true));
                comboMenu.Add(new MenuBool("useR", "Use R", true));

                ////Mana Manager
                comboMenu.Add(new MenuSeparator("separatorMana", "Combo - Mana Manager"));
                comboMenu.Add(new MenuSlider("QMana", "Q Mana", 15));
                comboMenu.Add(new MenuSlider("WMana", "W Mana", 30));
                comboMenu.Add(new MenuSlider("EMana", "E Mana", 10));
                comboMenu.Add(new MenuSlider("RMana", "R Mana", 15));

                ////Skills Options
                comboMenu.Add(new MenuSeparator("separatorOptions", "Combo - Skill Options"));
                comboMenu.Add(new MenuSlider("rMinEnemies", "Min Enemies for R", 2, 1, 5));
                comboMenu.Add(new MenuBool("pullInUlt", "Pull Enemies into R (Box) with E"));
                RootMenu.Add(comboMenu);
            }

            var harassMenu = new Menu("dz191.thresh.harass", "Harass Options");
            {
                ////Skills
                harassMenu.Add(new MenuSeparator("separatorHSkills", "Harass - Skills"));
                harassMenu.Add(new MenuBool("useQ", "Use Q", true));
                harassMenu.Add(new MenuBool("useE", "Use E", true));

                ////Mana Manager
                harassMenu.Add(new MenuSeparator("separatorHMana", "Harass - Mana Manager"));
                harassMenu.Add(new MenuSlider("QMana", "Q Mana", 15));
                harassMenu.Add(new MenuSlider("EMana", "E Mana", 10));

                RootMenu.Add(harassMenu);
            }

            var lanternMenu = new Menu("dz191.thresh.lantern", "Lantern Options");
            {
                var lanternAllies = new Menu("dz191.thresh.lantern.allies", "Use Lantern On");
                {
                    if (GameObjects.AllyHeroes.Any())
                    {
                        foreach (var ally in GameObjects.AllyHeroes)
                        {
                            lanternAllies.Add(new MenuBool(ally.ChampionName.ToLowerInvariant(), ally.ChampionName, true));
                        }

                        lanternMenu.Add(lanternAllies);
                    }
                }

                ////Usage
                lanternMenu.Add(new MenuSeparator("separatorLanternKeys", "Lantern Usage"));
                lanternMenu.Add(new MenuBool("saveAllies", "Auto Save Allies with Lantern", true));
                lanternMenu.Add(new MenuKeyBind("throwLantern", "Throw Lantern", Keys.S, KeyBindType.Press));

                lanternMenu.Add(new MenuList<string>("prioritizeAlly", "Throw Lantern Priority", new[] { "Health", "Closest" }));

                ////Health and Options
                lanternMenu.Add(new MenuSeparator("separatorLanternOptions", "Lantern Options"));
                lanternMenu.Add(new MenuSlider("allyHealth", "Auto Use Lantern if Ally Health < %", 20));
                lanternMenu.Add(new MenuSlider("enemiesNumber", "And Enemies Around >=", 2, 1, 5));

                ////CC
                lanternMenu.Add(new MenuSeparator("separatorLanternCC", "Lantern on CC"));
                lanternMenu.Add(new MenuBool("lanternCC", "Lantern CC'd allies", true));
                lanternMenu.Add(new MenuSlider("minCC", "Minimum Number of CC on Ally", 2, 1, 5));
                RootMenu.Add(lanternMenu);
            }

            var miscMenu = new Menu("dz191.thresh.misc", "Misc Options");
            {
                ////Antigapcloser and Interrupter
                miscMenu.Add(new MenuSeparator("separatorMiscAGP", "Antigapcloser & Interrupter"));
                miscMenu.Add(new MenuBool("antigapcloser", "AntiGapcloser", true));
                miscMenu.Add(new MenuBool("interrupter", "Interrupter", true));
                miscMenu.Add(new MenuList<string>("interruptskills", "Interrupt Skills", new[] { "Only E", "Only Q", "Q and E" }));
                miscMenu.Add(new MenuBool("xspecial", "The XSpecial", true));
                miscMenu.Add(new MenuList<string>("defaultEMode", "E Mode", new[] { "Push", "Pull" }));

                ////Items & Spells
                miscMenu.Add(new MenuSeparator("separatorMiscItems", "Items & Spells"));
                miscMenu.Add(new MenuBool("exhaust", "Use Exhaust", true));
                miscMenu.Add(new MenuBool("ignite", "Use Ignite", true));

                RootMenu.Add(miscMenu);
            }

            var drawingMenu = new Menu("dz191.thresh.drawing", "Drawing Options");
            {
                drawingMenu.Add(new MenuBool("drawQ", "Draw Q Range"));
                drawingMenu.Add(new MenuBool("drawE", "Draw E Range"));
                drawingMenu.Add(new MenuBool("drawQTarget", "Draw Q Target"));
                RootMenu.Add(drawingMenu);
            }

            RootMenu.Attach();
        }
    }
}
