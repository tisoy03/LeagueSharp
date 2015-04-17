using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace DZAhri
{
    class DZAhri
    {
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q, 925f) },
            { SpellSlot.W, new Spell(SpellSlot.W, 700f) },
            { SpellSlot.E, new Spell(SpellSlot.E, 875f) },
            { SpellSlot.R, new Spell(SpellSlot.R, 850f) }
        };
        private delegate void OnOrbwalkingMode();
        private static Dictionary<Orbwalking.OrbwalkingMode, OnOrbwalkingMode> _orbwalkingModesDictionary;

        public static void OnLoad()
        {
            _orbwalkingModesDictionary = new Dictionary<Orbwalking.OrbwalkingMode, OnOrbwalkingMode>
            {
                { Orbwalking.OrbwalkingMode.Combo, Combo },
                { Orbwalking.OrbwalkingMode.Mixed, Harass },
                { Orbwalking.OrbwalkingMode.LastHit, LastHit },
                { Orbwalking.OrbwalkingMode.LaneClear, Laneclear },
                { Orbwalking.OrbwalkingMode.None, () => { } }
            };
            SetUpMenu();
            SetUpSpells();
            SetUpEvents();
        }

        #region Modes Menu
        private static void Combo()
        {
            var comboTarget = TargetSelector.GetTarget(_spells[SpellSlot.E].Range, TargetSelector.DamageType.Magical);
            var charmedUnit = HeroManager.Enemies.Find(h => h.HasBuffOfType(BuffType.Charm) && h.IsValidTarget());
            Obj_AI_Hero target = comboTarget;
            if (charmedUnit != null)
            {
                target = charmedUnit;
            }
            if (target.IsValidTarget())
            {
                if (!target.IsCharmed() && Helpers.IsMenuEnabled("dz191.ahri.combo.usee") && _spells[SpellSlot.E].IsReady() && _spells[SpellSlot.Q].IsReady())
                {
                    _spells[SpellSlot.E].CastIfHitchanceEquals(target, HitChance.High);
                }
                if (Helpers.IsMenuEnabled("dz191.ahri.combo.useq") && _spells[SpellSlot.Q].IsReady() && !_spells[SpellSlot.E].IsReady())
                {
                    _spells[SpellSlot.Q].CastIfHitchanceEquals(target, HitChance.High);
                }
                if (Helpers.IsMenuEnabled("dz191.ahri.combo.usew") && _spells[SpellSlot.W].IsReady() && ObjectManager.Player.Distance(target) <= _spells[SpellSlot.W].Range - 70 && (target.IsCharmed() || (_spells[SpellSlot.W].GetDamage(target) + _spells[SpellSlot.Q].GetDamage(target) > target.Health + 25)))
                {
                    _spells[SpellSlot.W].Cast();
                }
                HandleRCombo(target);
            }
        }

        private static void Harass()
        {
            throw new NotImplementedException();
        }

        private static void LastHit()
        {
            throw new NotImplementedException();
        }

        private static void Laneclear()
        {
            throw new NotImplementedException();
        }
        private static void HandleRCombo(Obj_AI_Hero target)
        {
            if (_spells[SpellSlot.R].IsReady() && Helpers.IsMenuEnabled("dz191.ahri.combo.user"))
            {
                //User chose not to initiate with R.
                if (!Helpers.IsMenuEnabled("dz191.ahri.combo.initr") && Helpers.RStacks() == 0)
                {
                    return;
                }
                //Neither Q or E are ready in <= 2 seconds and we can't kill the enemy with 1 R stack. Don't use R
                if (!_spells[SpellSlot.Q].IsReady(2) && !_spells[SpellSlot.E].IsReady(2) && !(_spells[SpellSlot.R].GetDamage(target) >= target.Health +20))
                {
                    return;
                }
                //Set the test position to the Cursor Position
                var testPosition = Game.CursorPos;
                //Extend from out position towards there
                var extendedPosition = ObjectManager.Player.Position.Extend(testPosition, _spells[SpellSlot.R].Range);
                //Safety checks
                if (extendedPosition.IsSafe())
                {
                    _spells[SpellSlot.R].Cast(extendedPosition);
                }
            }
        }
        #endregion

        #region Event delegates
        static void Game_OnUpdate(EventArgs args)
        {
            _orbwalkingModesDictionary[Orbwalker.ActiveMode]();
        }
        #endregion

        #region Events, Spells, Menu Init
        private static void SetUpEvents()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void SetUpSpells()
        {
            _spells[SpellSlot.Q].SetSkillshot(0.25f, 100, 1600, false, SkillshotType.SkillshotLine);
            _spells[SpellSlot.E].SetSkillshot(0.25f, 60, 1200, true, SkillshotType.SkillshotLine);
        }

        private static void SetUpMenu()
        {
             Menu = new Menu("DZAhri","dz191.ahri",true);
            var orbMenu = new Menu("[Ahri] Orbwalker", "dz191.ahri.orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbMenu);
            Menu.AddSubMenu(orbMenu);
            var tsMenu = new Menu("[Ahri] Target Selector", "dz191.ahri.ts");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);
            var comboMenu = new Menu("[Ahri] Combo", "dz191.ahri.combo");
            {
                comboMenu.AddItem(new MenuItem("dz191.ahri.combo.useq", "Use Q Combo").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191.ahri.combo.usew", "Use W Combo").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191.ahri.combo.usee", "Use E Combo").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191.ahri.combo.user", "Use R Combo").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191.ahri.combo.initr", "Don't Initiate with R").SetValue(false));
                comboMenu.AddItem(new MenuItem("dz191.ahri.combo.mana", "Min Combo Mana").SetValue(new Slider(20)));
            }
            Menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu("[Ahri] Harass", "dz191.ahri.harass");
            {
                harassMenu.AddItem(new MenuItem("dz191.ahri.harass.useq", "Use Q Harass").SetValue(true));
                harassMenu.AddItem(new MenuItem("dz191.ahri.harass.usew", "Use W Harass").SetValue(true));
                harassMenu.AddItem(new MenuItem("dz191.ahri.harass.usee", "Use E Harass").SetValue(true));
                harassMenu.AddItem(new MenuItem("dz191.ahri.harass.onlyqcharm", "Use Q Only when charmed").SetValue(true));
                harassMenu.AddItem(new MenuItem("dz191.ahri.harass.mana", "Min Combo Mana").SetValue(new Slider(20)));
            }
            Menu.AddSubMenu(harassMenu);

            var miscMenu = new Menu("[Ahri] Misc", "dz191.ahri.misc");
            {
                miscMenu.AddItem(new MenuItem("dz191.ahri.misc.egp", "Auto E Gapclosers").SetValue(true));
                miscMenu.AddItem(new MenuItem("dz191.ahri.misc.eint", "Auto E Interrupter").SetValue(true));
                miscMenu.AddItem(new MenuItem("dz191.ahri.misc.rgap", "R away gapclosers if E on CD").SetValue(false));
                miscMenu.AddItem(new MenuItem("dz191.ahri.misc.autoq", "Auto Q Charmed targets").SetValue(false));
            }
            Menu.AddSubMenu(miscMenu);
            Menu.AddToMainMenu();
        }
        #endregion

    }
}
