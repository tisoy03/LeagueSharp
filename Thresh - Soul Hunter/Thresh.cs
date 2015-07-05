using LeagueSharp.SDK.Core.Utils;

namespace Thresh___Soul_Hunter
{
    #region
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using LeagueSharp.SDK.Core.Wrappers;
    using System.Windows.Forms;
    using LeagueSharp.SDK.Core.Extensions.SharpDX;
    using LeagueSharp.SDK.Core.Math.Prediction;
    using SharpDX;
    using Utility;
    using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;
    #endregion

    internal class Thresh
    {
        #region Fields
        public static Menu RootMenu { get; set; }

        public static Obj_AI_Hero HookedUnit { get; set; }

        public static float HookEndTick { get; set; }

        public const string MenuPrefix = "dz191.thresh.";

        private static readonly Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
        {
            {Spells.Q, new Spell(SpellSlot.Q, 1100f)},
            {Spells.W, new Spell(SpellSlot.Q, 920f)},
            {Spells.E, new Spell(SpellSlot.Q, 400f)},
            {Spells.R, new Spell(SpellSlot.R, 440f)},
        };

        #endregion

        internal static void OnLoad()
        {
            LoadMenu();
            LoadSkills();
            Obj_AI_Base.OnBuffAdd += OnBuffAdd;
            Game.OnUpdate += (args) => { OnUpdate(); };
            Orbwalker.OnAction += Orbwalker_OnAction;
        }

        #region Combo, Harass, Update Methods

        private static void OnCombo()
        {
            
        }

        private static void OnHarass()
        {

        }

        private static void OnUpdate()
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                    OnCombo();
                    break;
                case OrbwalkerMode.Hybrid:
                    OnHarass();
                    break;

            }
        }
        #endregion

        #region Event Delegates
        private static void Orbwalker_OnAction(object sender, Orbwalker.OrbwalkerActionArgs e)
        {
            switch (e.Type)
            {
                case OrbwalkerType.BeforeAttack:
                    //Block AA if no targon stacks.
                    break;
                case OrbwalkerType.AfterAttack:
                    //TODO
                    break;
            }
        }

        private static void OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if ((sender is Obj_AI_Hero) && sender.IsValidTarget() && (args.Buff.Name == "threshqfakeknockup" || args.Buff.Name == "ThreshQ"))
            {
                HookedUnit = (Obj_AI_Hero)sender;
                HookEndTick = Variables.TickCount + 1500f;
                DelayAction.Add((1500+ Game.Ping +250), () =>
                {
                    HookEndTick = 0;
                    HookedUnit = null;
                });
            }
        }
        #endregion

        #region Utility Methods
        private static QStates GetQState()
        {
            if (!spells[Spells.Q].IsReady())
            {
                return QStates.NotReady;
            }
            
            switch (spells[Spells.Q].Instance.Name)
            {
                case "ThreshQ":
                    return QStates.Q1;
                case "threshqleap":
                    return QStates.Q2;
                default:
                    return QStates.Unknown;
            }
        }

        private static void CastFlayPush(OrbwalkerMode Mode)
        {
            var target = TargetSelector.GetTarget(spells[Spells.E].Range);
            if (target.IsValidTarget() && RootMenu[MenuPrefix + Mode.ToString().ToLowerInvariant()]["useE"].GetValue<MenuBool>().Value)
            {
                var targetPrediction = Movement.GetPrediction(target, 0.25f);
                if (targetPrediction.UnitPosition.DistanceSquared(ObjectManager.Player.ServerPosition) <=
                    Math.Pow(spells[Spells.E].Range, 2))
                {
                    var finalPosition = targetPrediction.UnitPosition.Extend(
                        ObjectManager.Player.ServerPosition,
                        ObjectManager.Player.ServerPosition.Distance(targetPrediction.UnitPosition) / 2f);
                    spells[Spells.E].Cast(finalPosition);
                }
            }
        }

        private static void CastFlayPull(OrbwalkerMode Mode)
        {
            var target = TargetSelector.GetTarget(spells[Spells.E].Range);
            if (target.IsValidTarget() && RootMenu[MenuPrefix + Mode.ToString().ToLowerInvariant()]["useE"].GetValue<MenuBool>().Value)
            {
                var targetPrediction = Movement.GetPrediction(target, 0.25f);
                if (targetPrediction.UnitPosition.DistanceSquared(ObjectManager.Player.ServerPosition) <=
                    Math.Pow(spells[Spells.E].Range, 2))
                {
                    var finalPosition = 
                        targetPrediction.UnitPosition.Extend(
                            ObjectManager.Player.ServerPosition,
                            ObjectManager.Player.Distance(targetPrediction.UnitPosition) + 100f
                            );
                    spells[Spells.E].Cast(finalPosition);
                }
            }
        }

        private static Polygon getERectangle(Vector3 finalPosition, float BoundingRadius)
        {
            var halfERange = 150f;
            var eRectangle = new Polygon(
                Polygon.Rectangle(
                    ObjectManager.Player.ServerPosition.ToVector2(), 
                    ObjectManager.Player.ServerPosition.ToVector2().Extend(finalPosition.ToVector2(), halfERange), 
                    BoundingRadius)
                );
            return eRectangle;
        }
        #endregion

        #region Menu, Skills, Events

        private static void LoadSkills()
        {
            spells[Spells.Q].SetSkillshot(0.500f, 70f, 1900f, true, SkillshotType.SkillshotLine);
        }

        private static void LoadMenu()
        {
            RootMenu = new Menu("dz191.thresh", "Thresh - Soul Hunter", true);

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
                comboMenu.Add(new MenuSlider("qManaCombo", "Q Mana", 15));
                comboMenu.Add(new MenuSlider("wManaCombo", "W Mana", 30));
                comboMenu.Add(new MenuSlider("eManaCombo", "E Mana", 10));
                comboMenu.Add(new MenuSlider("rManaCombo", "R Mana", 15));

                ////Skills Options
                comboMenu.Add(new MenuSeparator("separatorOptions", "Combo - Skill Options"));
                comboMenu.Add(new MenuSlider("rMinEnemies", "Min Enemies for R", 2 , 1, 5));
                comboMenu.Add(new MenuList<string>("defaultEMode", "Default E Mode", new[] { "Push", "Pull"}));
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
                harassMenu.Add(new MenuSlider("qManaHarass", "Q Mana", 15));
                harassMenu.Add(new MenuSlider("eManaHarass", "E Mana", 10));

                RootMenu.Add(harassMenu);
            }

            var lanternMenu = new Menu("dz191.thresh.lantern", "Lantern Options");
            {
                var lanternAllies = new Menu("dz191.thresh.lantern.allies", "Use Lantern On");
                {
                    foreach (var ally in GameObjects.AllyHeroes)
                    {
                        lanternAllies.Add(new MenuBool(ally.ChampionName.ToLowerInvariant(), ally.ChampionName, true));
                    }

                    lanternMenu.Add(lanternAllies);
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
        #endregion
    }

    internal enum Spells
    {
        Q, W, E, R
    }

    internal enum QStates
    {
        Q1, Q2, NotReady, Unknown
    }
}
