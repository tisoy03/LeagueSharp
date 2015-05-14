#region
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace PennyJinxReborn
{
    // ReSharper disable once InconsistentNaming
    class PJR
    {
        public static Menu Menu;
        private static Orbwalking.Orbwalker _orbwalker;
        private const String AssemblyName = "PennyJinx Reborn";
        private const String MenuName = "pennyjinx";
        private const String MenuPrefix = "[PJ]";
        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q) },
            { SpellSlot.W, new Spell(SpellSlot.W, 1450f) },
            { SpellSlot.E, new Spell(SpellSlot.E, 900f) },
            { SpellSlot.R, new Spell(SpellSlot.R, 2000f) }
        };

        private static List<BuffType> ImpairedBuffTypes
        {
            get
            {
                return new List<BuffType>
                {
                    BuffType.Stun,
                    BuffType.Snare,
                    BuffType.Charm,
                    BuffType.Fear,
                    BuffType.Taunt,
                    BuffType.Slow
                };
            }
        }

        internal static void OnLoad()
        {
            if (!ShouldBeLoaded())
            {
                return;
            }
            LoadMenu();
            LoadSkills();
            LoadEvents();
            Notifications.AddNotification(new Notification(AssemblyName + " Loaded!", 2750, true));
        }

        internal static bool ShouldBeLoaded()
        {
            return ObjectManager.Player.ChampionName.Equals("Jinx");
        }

        #region Combo/Harass/Farm logic
        internal static void OnCombo()
        {
            QSwap(_orbwalker.ActiveMode);
            WLogic(_orbwalker.ActiveMode);
            RLogic(_orbwalker.ActiveMode);
        }
        internal static void OnHarass()
        {
            QSwap(_orbwalker.ActiveMode);
            WLogic(_orbwalker.ActiveMode);
        }
        internal static void OnFarm()
        {
            
        }
        internal static void QSwap(Orbwalking.OrbwalkingMode currentMode)
        {
            //Prevents AutoAttack cancelling
            var qEnabled = Menu.Item(String.Format("dz191." + MenuName + ".{0}.useq", currentMode).ToLowerInvariant()).GetValue<bool>();
            if (!_spells[SpellSlot.Q].IsReady() || ObjectManager.Player.IsWindingUp || !qEnabled || !ObjectManager.Player.CanAttack)
            {
                return;
            }
            var maxAaRange = GetMinigunRange(null) + GetFishboneRange() + 25f;
            var currentTarget = TargetSelector.GetTarget(maxAaRange, TargetSelector.DamageType.Physical);
            var qMana = Menu.Item(String.Format("dz191." + MenuName + ".{0}.mm.q", currentMode).ToLowerInvariant()).GetValue<Slider>().Value;
            if (!currentTarget.IsValidTarget(maxAaRange))
            {
                return;
            }
            if (ObjectManager.Player.ManaPercent < qMana)
            {
                if (IsFishBone())
                {
                    _spells[SpellSlot.Q].Cast();
                }
                return;
            }
            var qSwapMode = Menu.Item("dz191." + MenuName + ".settings.q.qmode").GetValue<StringList>().SelectedIndex; //0 = AOE 1 = Range 2 = Both
            var minEnemiesAoeMode = Menu.Item("dz191." + MenuName + ".settings.q.aoeswitch").GetValue<Slider>().Value;
            var qAoeRadius = Menu.Item("dz191." + MenuName + ".settings.q.aoeradius").GetValue<Slider>().Value;
            var jinxBaseRange = GetMinigunRange(currentTarget);
            switch (qSwapMode)
            {
                case 0:
                    if (currentTarget.Position.CountEnemiesInRange(qAoeRadius) >= minEnemiesAoeMode)
                    {
                        if (!IsFishBone())
                        {
                            _spells[SpellSlot.Q].Cast();
                        }
                    }
                    else
                    {
                        if (IsFishBone())
                        {
                            _spells[SpellSlot.Q].Cast();
                        }
                    }
                    break;
                case 1:
                    if (IsFishBone())
                    {
                        if (ObjectManager.Player.Distance(currentTarget) <= jinxBaseRange)
                        {
                            _spells[SpellSlot.Q].Cast();
                        }
                    }
                    else
                    {
                        if (ObjectManager.Player.Distance(currentTarget) > jinxBaseRange)
                        {
                            _spells[SpellSlot.Q].Cast();
                        }
                    }
                    break;
                case 2:
                    if (IsFishBone())
                    {
                        if (ObjectManager.Player.Distance(currentTarget) <= jinxBaseRange && !(currentTarget.Position.CountEnemiesInRange(qAoeRadius) >= minEnemiesAoeMode))
                        {
                            _spells[SpellSlot.Q].Cast();
                        }
                    }
                    else
                    {
                        if (ObjectManager.Player.Distance(currentTarget) > jinxBaseRange || (currentTarget.Position.CountEnemiesInRange(qAoeRadius) >= minEnemiesAoeMode))
                        {
                            _spells[SpellSlot.Q].Cast();
                        }
                    }
                    break;
            }
        }
        internal static void WLogic(Orbwalking.OrbwalkingMode currentMode)
        {
            var wEnabled = Menu.Item(String.Format("dz191." + MenuName + ".{0}.usew", currentMode).ToLowerInvariant()).GetValue<bool>();
            if (!_spells[SpellSlot.W].IsReady() || !wEnabled)
            {
                return;
            }
            var minWRange = Menu.Item("dz191." + MenuName + ".settings.w.minwrange").GetValue<Slider>().Value;
            var currentTarget = TargetSelector.GetTarget(_spells[SpellSlot.W].Range, TargetSelector.DamageType.Physical);
            var wMana = Menu.Item(String.Format("dz191." + MenuName + ".{0}.mm.w", currentMode).ToLowerInvariant()).GetValue<Slider>().Value;
            if (currentTarget.IsValidTarget(minWRange) || !_spells[SpellSlot.W].CanCast(currentTarget) || ObjectManager.Player.ManaPercent < wMana || !currentTarget.IsValidTarget(_spells[SpellSlot.W].Range))
            {
                return;
            }
            var wHitchance = GetHitchanceFromMenu("dz191." + MenuName + ".settings.w.hitchance");
            _spells[SpellSlot.W].CastIfHitchanceEquals(currentTarget, wHitchance);
        }

        internal static void ELogic(Orbwalking.OrbwalkingMode currentMode)
        {
            var eEnabled = Menu.Item(String.Format("dz191." + MenuName + ".{0}.usee", currentMode).ToLowerInvariant()).GetValue<bool>();
            var eMana = Menu.Item(String.Format("dz191." + MenuName + ".{0}.mm.e", currentMode).ToLowerInvariant()).GetValue<Slider>().Value;
            if (!_spells[SpellSlot.E].IsReady() || !eEnabled || ObjectManager.Player.ManaPercent < eMana)
            {
                return;
            }
            var eTarget = _orbwalker.GetTarget().IsValid<Obj_AI_Hero>()?_orbwalker.GetTarget() as Obj_AI_Hero:TargetSelector.GetTarget(_spells[SpellSlot.E].Range, TargetSelector.DamageType.Physical);
            if (!eTarget.IsValidTarget())
            {
                return;
            }
            var onlyESlowed = Menu.Item("dz191." + MenuName + ".settings.e.onlyslow").GetValue<bool>();
            var onlyEStunned = Menu.Item("dz191." + MenuName + ".settings.e.onlyimm").GetValue<bool>();
            var eHitchance = GetHitchanceFromMenu("dz191." + MenuName + ".settings.e.hitchance");

            var isTargetSlowed = IsLightlyImpaired(eTarget);
            var isTargetImmobile = IsHeavilyImpaired(eTarget);
            if ((isTargetSlowed && onlyESlowed) || (isTargetImmobile && onlyEStunned))
            {
                if (isTargetSlowed && eTarget.Path.Count() > 1)
                {
                    var slowEndTime = GetSlowEndTime(eTarget);
                    if (slowEndTime >= _spells[SpellSlot.E].Delay + 0.5f + Game.Ping / 2f)
                    {
                        _spells[SpellSlot.E].CastIfHitchanceEquals(eTarget, eHitchance);
                    }
                }
                if (isTargetImmobile)
                {
                    var immobileEndTime = GetImpairedEndTime(eTarget);
                    if (immobileEndTime >= _spells[SpellSlot.E].Delay + 0.5f + Game.Ping / 2f)
                    {
                        _spells[SpellSlot.E].CastIfHitchanceEquals(eTarget, eHitchance);
                    }
                }
            }
        }

        internal static void RLogic(Orbwalking.OrbwalkingMode currentMode)
        {
            var rEnabled = Menu.Item("dz191." + MenuName + ".combo.user").GetValue<bool>();
            if (!_spells[SpellSlot.R].IsReady() || !rEnabled)
            {
                return;
            }
            var rMana = Menu.Item("dz191." + MenuName + ".combo.mm.r").GetValue<Slider>().Value;
            var rTarget = TargetSelector.GetTarget(_spells[SpellSlot.R].Range, TargetSelector.DamageType.Physical);
            if (rTarget.IsValidTarget(_spells[SpellSlot.R].Range) || !_spells[SpellSlot.R].CanCast(rTarget) || ObjectManager.Player.ManaPercent < rMana)
            {
                return;
            }
            var aaBuffer = Menu.Item("dz191." + MenuName + ".settings.r.preventoverkill").GetValue<bool>() ? Menu.Item("dz191." + MenuName + ".settings.r.aa").GetValue<Slider>().Value : 0f;
            var wDamageBuffer = 0f;
            var aaDamageBuffer = 0f;
            var minRange = Menu.Item("dz191." + MenuName + ".settings.r.minrange").GetValue<Slider>().Value;
            var wEnabled = Menu.Item(String.Format("dz191." + MenuName + ".{0}.usew", currentMode).ToLowerInvariant()).GetValue<bool>();
            var qEnabled = Menu.Item(String.Format("dz191." + MenuName + ".{0}.useq", currentMode).ToLowerInvariant()).GetValue<bool>();
            var currentDistance = rTarget.Distance(ObjectManager.Player.ServerPosition);
            if (currentDistance >= minRange)
            {
                if (currentDistance < _spells[SpellSlot.W].Range && _spells[SpellSlot.W].CanCast(rTarget) && wEnabled)
                {
                    var wHitchance = GetHitchanceFromMenu("dz191." + MenuName + ".settings.w.hitchance");
                    var wPrediction = _spells[SpellSlot.W].GetPrediction(rTarget);
                    if (wPrediction.Hitchance >= wHitchance)
                    {
                        wDamageBuffer = Menu.Item("dz191." + MenuName + ".settings.r.preventoverkill").GetValue<bool>() ? _spells[SpellSlot.W].GetDamage(rTarget) : 0f;
                    }
                }
                if (currentDistance < GetMinigunRange(rTarget) + GetFishboneRange() && _spells[SpellSlot.Q].IsReady() &&
                    !ObjectManager.Player.IsWindingUp && ObjectManager.Player.CanAttack && qEnabled)
                {
                    aaDamageBuffer = Menu.Item("dz191." + MenuName + ".settings.r.preventoverkill").GetValue<bool>() ? (float)(ObjectManager.Player.GetAutoAttackDamage(rTarget) * aaBuffer) : 0f;
                }
                var targetPredictedHealth = rTarget.Health + 20;
                if (targetPredictedHealth > wDamageBuffer + aaDamageBuffer &&
                    targetPredictedHealth < _spells[SpellSlot.R].GetDamage(rTarget))
                {
                    var rHitchance = GetHitchanceFromMenu("dz191." + MenuName + ".settings.r.hitchance");
                    var actualSpeed = GetRealUltSpeed(rTarget.ServerPosition);
                    _spells[SpellSlot.R].Speed = actualSpeed;
                    var rPrediction = _spells[SpellSlot.R].GetPrediction(rTarget);
                    if (rPrediction.Hitchance >= rHitchance)
                    {
                        _spells[SpellSlot.R].Cast(rPrediction.CastPosition);
                        _spells[SpellSlot.R].Speed = 1700f;
                    }
                }
            }
        }
        #endregion

        #region Utility Methods
        internal static bool IsFishBone()
        {
            return ObjectManager.Player.AttackRange > 565f;
        }
        internal static float GetMinigunRange(GameObject target)
        {
            return 525f + ObjectManager.Player.BoundingRadius + (target != null ? target.BoundingRadius : 0f);
        }
        internal static float GetFishboneRange()
        {
            return 50f + 25f * _spells[SpellSlot.Q].Level;
        }

        internal static HitChance GetHitchanceFromMenu(String menuValue)
        {
            var possibleValue = Menu.Item(menuValue).GetValue<StringList>().SelectedIndex;
            switch (possibleValue)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.High;
            }
        }
        private static bool IsHeavilyImpaired(Obj_AI_Hero enemy)
        {
            return (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                    enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                    enemy.HasBuffOfType(BuffType.Taunt) || IsLightlyImpaired(enemy));
        }

        private static bool IsLightlyImpaired(Obj_AI_Hero enemy)
        {
            return (enemy.HasBuffOfType(BuffType.Slow));
        }
        private static float GetImpairedEndTime(Obj_AI_Base target)
        {
            return target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => ImpairedBuffTypes.Contains(buff.Type))
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }
        private static float GetSlowEndTime(Obj_AI_Base target)
        {
            return
                target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Type.Equals(BuffType.Slow))
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }
        internal static float GetRealUltSpeed(Vector3 endPosition)
        {
            //Thanks to Beaving - BaseUlt3 - https://github.com/Beaving/LeagueSharp/blob/master/BaseUlt3/
            if (ObjectManager.Player.ServerPosition.Distance(endPosition) > 1350f)
            {
                const float accelRate = 0.3f;
                var accelSpace = ObjectManager.Player.ServerPosition.Distance(endPosition) - 1350f;
                if (accelSpace > 150f)
                {
                    accelSpace = 150f;
                }
                var distanceDifference = ObjectManager.Player.ServerPosition.Distance(endPosition) - 1500f;
                var realSpeed = (1350f * _spells[SpellSlot.R].Speed + accelSpace * (_spells[SpellSlot.R].Speed + accelRate * accelSpace) + distanceDifference * 2200f) / ObjectManager.Player.ServerPosition.Distance(endPosition);
                return realSpeed;
            }
            return _spells[SpellSlot.R].Speed;
        }
        #endregion

        #region Events, Skills And Menu
        internal static void LoadEvents()
        {
            Game.OnUpdate += args1 => OnUpdate();
            Drawing.OnDraw += args1 => OnDraw();
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        internal static void LoadSkills()
        {
            _spells[SpellSlot.W].SetSkillshot(0.6f, 60f, 3300f, true, SkillshotType.SkillshotLine);
            _spells[SpellSlot.E].SetSkillshot(1.2f, 1f, 1750f, false, SkillshotType.SkillshotCircle);
            _spells[SpellSlot.R].SetSkillshot(0.6f, 140f, 1700f, false, SkillshotType.SkillshotLine);
        }
        internal static void LoadMenu()
        {
            Menu = new Menu(AssemblyName,"dz191."+MenuName,true);
            var orbwalkerMenu = new Menu(MenuPrefix + " Orbwalker", "dz191." + MenuName+".orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);
            var tsMenu = new Menu(MenuPrefix + " TargetSelector", "dz191." + MenuName + ".targetselector");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);
            var comboMenu = new Menu(MenuPrefix + " Combo", "dz191." + MenuName + ".combo");
            {
                comboMenu.AddItem(new MenuItem("dz191." + MenuName + ".combo.useq", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191." + MenuName + ".combo.usew", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191." + MenuName + ".combo.usee", "Use E").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191." + MenuName + ".combo.user", "Use R").SetValue(true));
            }
            var comboManaManager = new Menu("Mana Manager", "dz191." + MenuName + ".combo.mm");
            {
                comboManaManager.AddItem(new MenuItem("dz191." + MenuName + ".combo.mm.q", "Q").SetValue(new Slider(10)));
                comboManaManager.AddItem(new MenuItem("dz191." + MenuName + ".combo.mm.w", "W").SetValue(new Slider(15)));
                comboManaManager.AddItem(new MenuItem("dz191." + MenuName + ".combo.mm.e", "E").SetValue(new Slider(25)));
                comboManaManager.AddItem(new MenuItem("dz191." + MenuName + ".combo.mm.r", "R").SetValue(new Slider(5)));
                comboManaManager.AddItem(new MenuItem("dz191." + MenuName + ".combo.mm.auto", "Auto Mana Manager").SetValue(false));
            }
            comboMenu.AddSubMenu(comboManaManager);
            Menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu(MenuPrefix + " Harass", "dz191." + MenuName + ".mixed");
            {
                harassMenu.AddItem(new MenuItem("dz191." + MenuName + ".mixed.useq", "Use Q").SetValue(true));
                harassMenu.AddItem(new MenuItem("dz191." + MenuName + ".mixed.usew", "Use W").SetValue(true));
            }
            var harassManaManager = new Menu("Mana Manager", "dz191." + MenuName + ".mixed.mm");
            {
                harassManaManager.AddItem(new MenuItem("dz191." + MenuName + ".mixed.mm.q", "Q").SetValue(new Slider(10)));
                harassManaManager.AddItem(new MenuItem("dz191." + MenuName + ".mixed.mm.w", "W").SetValue(new Slider(15)));
                harassManaManager.AddItem(new MenuItem("dz191." + MenuName + ".mixed.mm.auto", "Auto Mana Manager").SetValue(false));
            }
            harassMenu.AddSubMenu(harassManaManager);
            Menu.AddSubMenu(harassMenu);

            var miscMenu = new Menu(MenuPrefix + " Skills Settings", "dz191." + MenuName + ".settings");
            var miscQMenu = new Menu("Q Settings", "dz191." + MenuName + ".settings.q");
            {
                /**Mode Selector*/
                miscQMenu.AddItem(
                    new MenuItem("dz191." + MenuName + ".settings.q.qmode", "Q Switch Mode").SetValue(
                        new StringList(new[] { "AOE", "Range", "Both" },2)));
                /**End*/

                /**AOE Options*/
                miscQMenu.AddItem(
                    new MenuItem("dz191." + MenuName + ".settings.q.aoeswitch", "Min enemies for AOE switch").SetValue(
                        new Slider(2, 1, 5)));
                miscQMenu.AddItem(
                    new MenuItem("dz191." + MenuName + ".settings.q.farmminions", "Min AOE Minions for Q Farm Switch").SetValue(
                        new Slider(2, 1, 5)));
                miscQMenu.AddItem(
                    new MenuItem("dz191." + MenuName + ".settings.q.aoeradius", "AOE Radius").SetValue(
                        new Slider(160, 65, 200)));
                /** End of AOE Options*/

                /** Q Switch no enemies in range*/
                miscQMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.q.minigunnoenemies", "AutoSwitch to minigun if no enemies in \"X\" range").SetValue(false));
                miscQMenu.AddItem(
                    new MenuItem("dz191." + MenuName + ".settings.q.rangeswitch", "^ \"X\" Range for Q to minigun switch").SetValue(
                        new Slider(650, 100, 1500)));
                /**End*/


            }
            miscMenu.AddSubMenu(miscQMenu);

            var miscWMenu = new Menu("W Settings", "dz191." + MenuName + ".settings.w");
            {
                /**Minimum W Range*/
                miscWMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.w.minwrange", "Combo/Harass: Minimum W Range").SetValue(new Slider(188, 65, 800)));
                /**End*/

                /** Auto W*/
                miscWMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.w.immobile", "Auto W Immobile").SetValue(true)); //TODO
                miscWMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.w.autowmana", "Auto W Mana").SetValue(new Slider(30)));
                /*End*/

                /** Hitchance Selector*/
                miscWMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.w.hitchance", "W Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
                /**End*/
            }
            miscMenu.AddSubMenu(miscWMenu);

            var miscEMenu = new Menu("E Settings", "dz191." + MenuName + ".settings.e");
            {
                //TODO
                /**Interrupter and AntiGP*/
                miscEMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.e.antigp", "Auto E Antigapcloser").SetValue(false)); //Done
                miscEMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.e.interrupt", "Auto E Interrupter").SetValue(false)); //Done

                /**End*/
                /** Auto E*/
                miscEMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.e.slowed", "Auto E Slowed Enemies").SetValue(false));
                miscEMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.e.immobile", "Auto E Immobile").SetValue(true)); 
                miscEMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.e.tp", "Auto E Teleport").SetValue(true));
                miscEMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.e.autoemana", "Auto E Mana").SetValue(new Slider(30)));
                /*End*/

                miscEMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.e.onlyslow", "Combo: Only E Slowed").SetValue(false));
                miscEMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.e.onlyimm", "Combo: Only E Immobile").SetValue(false));

                /** Hitchance Selector*/
                miscEMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.e.hitchance", "E Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
                /**End*/
            }
            miscMenu.AddSubMenu(miscEMenu);

            var miscRMenu = new Menu("R Settings", "dz191." + MenuName + ".settings.r");
            {
                miscRMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.r.aa", "Autoattack buffer").SetValue(new Slider(2,0,4))); //Done
                miscRMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.r.minrange", "Minimum R range").SetValue(new Slider(750,65,1500))); //Done
                miscRMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.r.preventoverkill", "Prevent Overkill (W/AA)").SetValue(false)); //Done
                miscRMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.r.manualr", "Manual R").SetValue(new KeyBind("T".ToCharArray()[0],KeyBindType.Press))); //TODO
                /** Hitchance Selector*/
                miscRMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.r.hitchance", "R Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3))); //Done
                /**End*/
            }
            miscMenu.AddSubMenu(miscRMenu);
            miscMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.reset", "Reset to default/optimal values").SetValue(false)); //TODO

            Menu.AddSubMenu(miscMenu);
            var drawMenu = new Menu(MenuPrefix + " Harass", "dz191." + MenuName + ".drawings");
            {
                drawMenu.AddItem(
                    new MenuItem("dz191." + MenuName + ".drawings.q", "Draw Q").SetValue(
                        new Circle(true, System.Drawing.Color.Red)));
                drawMenu.AddItem(
                    new MenuItem("dz191." + MenuName + ".drawings.w", "Draw W").SetValue(
                        new Circle(true, System.Drawing.Color.Cyan)));
                drawMenu.AddItem(
                                    new MenuItem("dz191." + MenuName + ".drawings.e", "Draw E").SetValue(
                                        new Circle(true, System.Drawing.Color.Yellow)));
                drawMenu.AddItem(
                    new MenuItem("dz191." + MenuName + ".drawings.rsprite", "R Sprite drawing").SetValue(false));
            }
            Menu.AddSubMenu(drawMenu);

            Menu.AddToMainMenu();
        }
        #endregion

        #region Update/Draw/Various Delegates
        internal static void OnUpdate()
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }
            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    OnFarm();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    OnFarm();
                    break;
            }
            OnAuto();
        }
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Menu.Item("dz191." + MenuName + ".settings.e.antigp").GetValue<bool>() && _spells[SpellSlot.E].IsReady() && gapcloser.Sender.IsValidTarget(_spells[SpellSlot.E].Range))
            {
                _spells[SpellSlot.E].Cast(gapcloser.Sender);
            }
        }
        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Menu.Item("dz191." + MenuName + ".settings.e.interrupter").GetValue<bool>() && _spells[SpellSlot.E].IsReady() && sender.IsValidTarget(_spells[SpellSlot.E].Range) && args.DangerLevel >= Interrupter2.DangerLevel.High)
            {
                _spells[SpellSlot.E].Cast(sender);
            }
        }
        static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit ||
                _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (IsFishBone())
                {
                    _spells[SpellSlot.Q].Cast();
                }
            }
        }
        internal static void OnDraw()
        {

        }
        #endregion

        #region Automatic Functions
        internal static void OnAuto()
        {
            AutoMinigunSwap();
            AutoE();
        }

        internal static void AutoMinigunSwap()
        {
            var swapMinigun = Menu.Item("dz191." + MenuName + ".settings.q.minigunnoenemies").GetValue<bool>();
            var swapMinigunRange = Menu.Item("dz191." + MenuName + ".settings.q.rangeswitch").GetValue<Slider>().Value;
            if (ObjectManager.Player.CountEnemiesInRange(swapMinigunRange) == 0 && swapMinigun && !ObjectManager.Player.IsWindingUp)
            {
                if (IsFishBone() && _spells[SpellSlot.Q].IsReady())
                {
                    _spells[SpellSlot.Q].Cast();
                }
            }
        }

        internal static void AutoW()
        {
            
        }

        internal static void AutoE()
        {
            AutoEStunned();
            AutoESlowed();
        }

        internal static void AutoESlowed()
        {
            
        }

        internal static void AutoEStunned()
        {
            
        }

        internal static void ManualR()
        {
            
        }
        #endregion

    }
}
