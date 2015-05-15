namespace PennyJinxReborn
{
    #region
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    #endregion

    /// <summary>
    /// Penny Jinx Reborn main class
    /// </summary>
    internal class PJR
    {
        /// <summary>
        /// The assembly name, used for welcome messages.
        /// </summary>
        private const string AssemblyName = "PennyJinx Reborn";

        /// <summary>
        /// The assembly name used in the menu fields.
        /// </summary>
        private const string MenuName = "pennyjinx";

        /// <summary>
        /// The prefix of the submenus.
        /// </summary>
        private const string MenuPrefix = "[PJ]";

        /// <summary>
        /// The Spells dictionary, containing the 4 spells.
        /// </summary>
        private static readonly Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q) },
            { SpellSlot.W, new Spell(SpellSlot.W, 1450f) },
            { SpellSlot.E, new Spell(SpellSlot.E, 900f) },
            { SpellSlot.R, new Spell(SpellSlot.R, 2000f) }
        };

        /// <summary>
        /// The Menu variable.
        /// </summary>
        private static Menu menu;

        /// <summary>
        /// The orbwalker instance we are using.
        /// </summary>
        private static Orbwalking.Orbwalker orbwalker;
        
        /// <summary>
        /// Gets a list of Movement Impairing buffs.
        /// </summary>
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

        /// <summary>
        /// OnLoad method, called when the assembly is loaded.
        /// </summary>
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

        /// <summary>
        /// Determines whether or not the assembly should be loaded.
        /// </summary>
        /// <returns>Whether or not the assembly should be loaded</returns>
        internal static bool ShouldBeLoaded()
        {
            return ObjectManager.Player.ChampionName.Equals("Jinx");
        }

        #region Combo/Harass/Farm logic
        /// <summary>
        /// The method called when the orbwalking mode is Combo.
        /// </summary>
        internal static void OnCombo()
        {
            QSwap(orbwalker.ActiveMode);
            WLogic(orbwalker.ActiveMode);
            ELogic(orbwalker.ActiveMode);
            RLogic(orbwalker.ActiveMode);
        }

        /// <summary>
        /// The method called when the orbwalking mode is Mixed.
        /// </summary>
        internal static void OnHarass()
        {
            QSwap(orbwalker.ActiveMode);
            WLogic(orbwalker.ActiveMode);
        }

        /// <summary>
        /// The method called when the orbwalking mode is lasthit/laneclear.
        /// </summary>
        internal static void OnFarm()
        {
            
        }

        /// <summary>
        /// The Q Swapping logic.
        /// </summary>
        /// <param name="currentMode">The current orbwalking mode.</param>
        internal static void QSwap(Orbwalking.OrbwalkingMode currentMode)
        {
            ////Prevents AutoAttack cancelling
            var qEnabled = menu.Item(string.Format("dz191." + MenuName + ".{0}.useq", currentMode).ToLowerInvariant()).GetValue<bool>();
            if (!Spells[SpellSlot.Q].IsReady() || ObjectManager.Player.IsWindingUp || !qEnabled || !ObjectManager.Player.CanAttack)
            {
                return;
            }

            var maxAaRange = GetMinigunRange(null) + GetFishboneRange() + 25f;
            var currentTarget = TargetSelector.GetTarget(maxAaRange, TargetSelector.DamageType.Physical);
            var qMana = menu.Item(string.Format("dz191." + MenuName + ".{0}.mm.q", currentMode).ToLowerInvariant()).GetValue<Slider>().Value;
            if (!currentTarget.IsValidTarget(maxAaRange))
            {
                return;
            }

            if (ObjectManager.Player.ManaPercent < qMana)
            {
                if (IsFishBone())
                {
                    Spells[SpellSlot.Q].Cast();
                }

                return;
            }

            var qSwapMode = menu.Item("dz191." + MenuName + ".settings.q.qmode").GetValue<StringList>().SelectedIndex; ////0 = AOE 1 = Range 2 = Both
            var minEnemiesAoeMode = menu.Item("dz191." + MenuName + ".settings.q.aoeswitch").GetValue<Slider>().Value;
            var qAoeRadius = menu.Item("dz191." + MenuName + ".settings.q.aoeradius").GetValue<Slider>().Value;
            var jinxBaseRange = GetMinigunRange(currentTarget);
            switch (qSwapMode)
            {
                case 0:
                    if (currentTarget.Position.CountEnemiesInRange(qAoeRadius) >= minEnemiesAoeMode)
                    {
                        if (!IsFishBone())
                        {
                            Spells[SpellSlot.Q].Cast();
                        }
                    }
                    else
                    {
                        if (IsFishBone())
                        {
                            Spells[SpellSlot.Q].Cast();
                        }
                    }

                    break;
                case 1:
                    if (IsFishBone())
                    {
                        if (ObjectManager.Player.Distance(currentTarget) <= jinxBaseRange)
                        {
                            Spells[SpellSlot.Q].Cast();
                        }
                    }
                    else
                    {
                        if (ObjectManager.Player.Distance(currentTarget) > jinxBaseRange)
                        {
                            Spells[SpellSlot.Q].Cast();
                        }
                    }

                    break;
                case 2:
                    if (IsFishBone())
                    {
                        if (ObjectManager.Player.Distance(currentTarget) <= jinxBaseRange && !(currentTarget.Position.CountEnemiesInRange(qAoeRadius) >= minEnemiesAoeMode))
                        {
                            Spells[SpellSlot.Q].Cast();
                        }
                    }
                    else
                    {
                        if (ObjectManager.Player.Distance(currentTarget) > jinxBaseRange || (currentTarget.Position.CountEnemiesInRange(qAoeRadius) >= minEnemiesAoeMode))
                        {
                            Spells[SpellSlot.Q].Cast();
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// The W Skill logic
        /// </summary>
        /// <param name="currentMode">The current orbwalking mode.</param>
        internal static void WLogic(Orbwalking.OrbwalkingMode currentMode)
        {
            var wEnabled = menu.Item(string.Format("dz191." + MenuName + ".{0}.usew", currentMode).ToLowerInvariant()).GetValue<bool>();
            if (!Spells[SpellSlot.W].IsReady() || !wEnabled)
            {
                return;
            }

            var minWRange = menu.Item("dz191." + MenuName + ".settings.w.minwrange").GetValue<Slider>().Value;
            var currentTarget = TargetSelector.GetTarget(Spells[SpellSlot.W].Range, TargetSelector.DamageType.Physical);
            var wMana = menu.Item(string.Format("dz191." + MenuName + ".{0}.mm.w", currentMode).ToLowerInvariant()).GetValue<Slider>().Value;
            if (currentTarget.IsValidTarget(minWRange) || !Spells[SpellSlot.W].CanCast(currentTarget) || ObjectManager.Player.ManaPercent < wMana || !currentTarget.IsValidTarget(Spells[SpellSlot.W].Range))
            {
                return;
            }

            var wHitchance = GetHitchanceFromMenu("dz191." + MenuName + ".settings.w.hitchance");
            Spells[SpellSlot.W].CastIfHitchanceEquals(currentTarget, wHitchance);
        }

        /// <summary>
        /// The E Skill logic
        /// </summary>
        /// <param name="currentMode">The current orbwalking mode.</param>
        internal static void ELogic(Orbwalking.OrbwalkingMode currentMode)
        {
            var eEnabled = menu.Item(string.Format("dz191." + MenuName + ".{0}.usee", currentMode).ToLowerInvariant()).GetValue<bool>();
            var eMana = menu.Item(string.Format("dz191." + MenuName + ".{0}.mm.e", currentMode).ToLowerInvariant()).GetValue<Slider>().Value;
            if (!Spells[SpellSlot.E].IsReady() || !eEnabled || ObjectManager.Player.ManaPercent < eMana)
            {
                return;
            }

            var eTarget = orbwalker.GetTarget().IsValid<Obj_AI_Hero>() ? orbwalker.GetTarget() as Obj_AI_Hero : TargetSelector.GetTarget(Spells[SpellSlot.E].Range, TargetSelector.DamageType.Physical);
            if (!eTarget.IsValidTarget() || eTarget == null)
            {
                return;
            }

            var onlyESlowed = menu.Item("dz191." + MenuName + ".settings.e.onlyslow").GetValue<bool>();
            var onlyEStunned = menu.Item("dz191." + MenuName + ".settings.e.onlyimm").GetValue<bool>();
            var eHitchance = GetHitchanceFromMenu("dz191." + MenuName + ".settings.e.hitchance");

            var isTargetSlowed = IsLightlyImpaired(eTarget);
            var isTargetImmobile = IsHeavilyImpaired(eTarget);
            if ((isTargetSlowed && onlyESlowed) || (isTargetImmobile && onlyEStunned))
            {
                if (isTargetSlowed && eTarget.Path.Count() > 1)
                {
                    var slowEndTime = GetSlowEndTime(eTarget);
                    if (slowEndTime >= Spells[SpellSlot.E].Delay + 0.5f + Game.Ping / 2f)
                    {
                        Spells[SpellSlot.E].CastIfHitchanceEquals(eTarget, eHitchance);
                    }
                }

                if (isTargetImmobile)
                {
                    var immobileEndTime = GetImpairedEndTime(eTarget);
                    if (immobileEndTime >= Spells[SpellSlot.E].Delay + 0.5f + Game.Ping / 2f)
                    {
                        Spells[SpellSlot.E].CastIfHitchanceEquals(eTarget, eHitchance);
                    }
                }
            }
        }

        /// <summary>
        /// The R Skill logic
        /// </summary>
        /// <param name="currentMode">The current orbwalking mode.</param>
        internal static void RLogic(Orbwalking.OrbwalkingMode currentMode)
        {
            var rEnabled = menu.Item("dz191." + MenuName + ".combo.user").GetValue<bool>();
            if (!Spells[SpellSlot.R].IsReady() || !rEnabled)
            {
                return;
            }

            var rMana = menu.Item("dz191." + MenuName + ".combo.mm.r").GetValue<Slider>().Value;
            var rTarget = TargetSelector.GetTarget(Spells[SpellSlot.R].Range, TargetSelector.DamageType.Physical);
            if (rTarget.IsValidTarget(Spells[SpellSlot.R].Range) || !Spells[SpellSlot.R].CanCast(rTarget) || ObjectManager.Player.ManaPercent < rMana)
            {
                return;
            }

            var aaBuffer = menu.Item("dz191." + MenuName + ".settings.r.preventoverkill").GetValue<bool>() ? menu.Item("dz191." + MenuName + ".settings.r.aa").GetValue<Slider>().Value : 0f;
            var wDamageBuffer = 0f;
            var aaDamageBuffer = 0f;
            var minRange = menu.Item("dz191." + MenuName + ".settings.r.minrange").GetValue<Slider>().Value;
            var wEnabled = menu.Item(string.Format("dz191." + MenuName + ".{0}.usew", currentMode).ToLowerInvariant()).GetValue<bool>();
            var qEnabled = menu.Item(string.Format("dz191." + MenuName + ".{0}.useq", currentMode).ToLowerInvariant()).GetValue<bool>();
            var currentDistance = rTarget.Distance(ObjectManager.Player.ServerPosition);
            if (currentDistance >= minRange)
            {
                if (currentDistance < Spells[SpellSlot.W].Range && Spells[SpellSlot.W].CanCast(rTarget) && wEnabled)
                {
                    var wHitchance = GetHitchanceFromMenu("dz191." + MenuName + ".settings.w.hitchance");
                    var wPrediction = Spells[SpellSlot.W].GetPrediction(rTarget);
                    if (wPrediction.Hitchance >= wHitchance)
                    {
                        wDamageBuffer = menu.Item("dz191." + MenuName + ".settings.r.preventoverkill").GetValue<bool>() ? Spells[SpellSlot.W].GetDamage(rTarget) : 0f;
                    }
                }

                if (currentDistance < GetMinigunRange(rTarget) + GetFishboneRange() && Spells[SpellSlot.Q].IsReady() &&
                    !ObjectManager.Player.IsWindingUp && ObjectManager.Player.CanAttack && qEnabled)
                {
                    aaDamageBuffer = menu.Item("dz191." + MenuName + ".settings.r.preventoverkill").GetValue<bool>() ? (float)(ObjectManager.Player.GetAutoAttackDamage(rTarget) * aaBuffer) : 0f;
                }

                var targetPredictedHealth = rTarget.Health + 20;
                if (targetPredictedHealth > wDamageBuffer + aaDamageBuffer &&
                    targetPredictedHealth < Spells[SpellSlot.R].GetDamage(rTarget))
                {
                    var rHitchance = GetHitchanceFromMenu("dz191." + MenuName + ".settings.r.hitchance");
                    var actualSpeed = GetRealUltSpeed(rTarget.ServerPosition);
                    Spells[SpellSlot.R].Speed = actualSpeed;
                    var rPrediction = Spells[SpellSlot.R].GetPrediction(rTarget);
                    if (rPrediction.Hitchance >= rHitchance)
                    {
                        Spells[SpellSlot.R].Cast(rPrediction.CastPosition);
                        Spells[SpellSlot.R].Speed = 1700f;
                    }
                }
            }
        }

        #endregion

        #region Utility Methods
        /// <summary>
        /// Determines whether or not FishBone is active.
        /// </summary>
        /// <returns>Range > 565</returns>
        internal static bool IsFishBone()
        {
            return ObjectManager.Player.AttackRange > 565f;
        }

        /// <summary>
        /// Determines the minigun range
        /// </summary>
        /// <param name="target">Current target</param>
        /// <returns>The minigun range</returns>
        internal static float GetMinigunRange(GameObject target)
        {
            return 525f + ObjectManager.Player.BoundingRadius + (target != null ? target.BoundingRadius : 0f);
        }

        /// <summary>
        /// Determines the extra range.
        /// </summary>
        /// <returns>Extra fishbone range</returns>
        internal static float GetFishboneRange()
        {
            return 50f + 25f * Spells[SpellSlot.Q].Level;
        }

        /// <summary>
        /// The Hitchance selected in the menu
        /// </summary>
        /// <param name="menuValue">The menuitem name</param>
        /// <returns>The Hitchance</returns>
        internal static HitChance GetHitchanceFromMenu(string menuValue)
        {
            var possibleValue = menu.Item(menuValue).GetValue<StringList>().SelectedIndex;
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

        /// <summary>
        /// Determines if the target is heavily impaired (stunned/rooted)
        /// </summary>
        /// <param name="enemy">The target</param>
        /// <returns>Whether the target is heavily impaired</returns>
        private static bool IsHeavilyImpaired(Obj_AI_Hero enemy)
        {
            return enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) || enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) || enemy.HasBuffOfType(BuffType.Taunt);
        }

        /// <summary>
        /// Determines if the target is lightly impaired (slowed)
        /// </summary>
        /// <param name="enemy">The target</param>
        /// <returns>Whether the target is lightly impaired</returns>
        private static bool IsLightlyImpaired(Obj_AI_Hero enemy)
        {
            return enemy.HasBuffOfType(BuffType.Slow);
        }

        /// <summary>
        /// Gets the Root/Stun/Immobile buff end time
        /// </summary>
        /// <param name="target">The enemy</param>
        /// <returns>Buff end time</returns>
        private static float GetImpairedEndTime(Obj_AI_Base target)
        {
            return target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => ImpairedBuffTypes.Contains(buff.Type))
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Get the Slow end time
        /// </summary>
        /// <param name="target">The enemy</param>
        /// <returns>Buff end time</returns>
        private static float GetSlowEndTime(Obj_AI_Base target)
        {
            return
                target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Type.Equals(BuffType.Slow))
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Determines the real R speed using acceleration.
        /// </summary>
        /// <param name="endPosition">The End Position (Target position)</param>
        /// <returns>Real ult speed</returns>
        internal static float GetRealUltSpeed(Vector3 endPosition)
        {
            ////Thanks to Beaving - BaseUlt3 - https://github.com/Beaving/LeagueSharp/blob/master/BaseUlt3/
            if (ObjectManager.Player.ServerPosition.Distance(endPosition) > 1350f)
            {
                // ReSharper disable once InconsistentNaming
                const float AccelRate = 0.3f;
                var accelSpace = ObjectManager.Player.ServerPosition.Distance(endPosition) - 1350f;
                if (accelSpace > 150f)
                {
                    accelSpace = 150f;
                }

                var distanceDifference = ObjectManager.Player.ServerPosition.Distance(endPosition) - 1500f;
                var realSpeed = (1350f * Spells[SpellSlot.R].Speed + accelSpace * (Spells[SpellSlot.R].Speed + AccelRate * accelSpace) + distanceDifference * 2200f) / ObjectManager.Player.ServerPosition.Distance(endPosition);
                return realSpeed;
            }

            return Spells[SpellSlot.R].Speed;
        }

        #endregion

        #region Events, Skills And Menu
        /// <summary>
        /// Initializes Events
        /// </summary>
        internal static void LoadEvents()
        {
            Game.OnUpdate += args1 => OnUpdate();
            Drawing.OnDraw += args1 => OnDraw();
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        /// <summary>
        /// Loads the skills. (Using fixed Spelldata)
        /// </summary>
        internal static void LoadSkills()
        {
            Spells[SpellSlot.W].SetSkillshot(0.6f, 60f, 3300f, true, SkillshotType.SkillshotLine);
            Spells[SpellSlot.E].SetSkillshot(1.2f, 1f, 1750f, false, SkillshotType.SkillshotCircle);
            Spells[SpellSlot.R].SetSkillshot(0.6f, 140f, 1700f, false, SkillshotType.SkillshotLine);
        }

        /// <summary>
        /// Loads the Menu and makes it visible.
        /// </summary>
        internal static void LoadMenu()
        {
            menu = new Menu(AssemblyName, "dz191." + MenuName, true);
            var orbwalkerMenu = new Menu(MenuPrefix + " Orbwalker", "dz191." + MenuName + ".orbwalker");
            orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            menu.AddSubMenu(orbwalkerMenu);
            var tsMenu = new Menu(MenuPrefix + " TargetSelector", "dz191." + MenuName + ".targetselector");
            TargetSelector.AddToMenu(tsMenu);
            menu.AddSubMenu(tsMenu);
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
            menu.AddSubMenu(comboMenu);

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
            menu.AddSubMenu(harassMenu);

            var miscMenu = new Menu(MenuPrefix + " Skills Settings", "dz191." + MenuName + ".settings");
            var miscQMenu = new Menu("Q Settings", "dz191." + MenuName + ".settings.q");
            {
                /**Mode Selector*/
                miscQMenu.AddItem(
                    new MenuItem("dz191." + MenuName + ".settings.q.qmode", "Q Switch Mode").SetValue(
                        new StringList(new[] { "AOE", "Range", "Both" }, 2)));
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
                miscWMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.w.immobile", "Auto W Immobile").SetValue(true)); ////TODO
                miscWMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.w.autowmana", "Auto W Mana").SetValue(new Slider(30)));
                /*End*/

                /** Hitchance Selector*/
                miscWMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.w.hitchance", "W Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
                /**End*/
            }

            miscMenu.AddSubMenu(miscWMenu);

            var miscEMenu = new Menu("E Settings", "dz191." + MenuName + ".settings.e");
            {
                ////TODO
                /**Interrupter and AntiGP*/
                miscEMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.e.antigp", "Auto E Antigapcloser").SetValue(false)); ////Done
                miscEMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.e.interrupt", "Auto E Interrupter").SetValue(false)); ////Done

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
                miscRMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.r.aa", "Autoattack buffer").SetValue(new Slider(2, 0, 4))); ////Done
                miscRMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.r.minrange", "Minimum R range").SetValue(new Slider(750, 65, 1500))); ////Done
                miscRMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.r.preventoverkill", "Prevent Overkill (W/AA)").SetValue(false)); ////Done
                miscRMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.r.manualr", "Manual R").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); ////TODO
                /** Hitchance Selector*/
                miscRMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.r.hitchance", "R Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3))); ////Done
                /**End*/
            }

            miscMenu.AddSubMenu(miscRMenu);
            miscMenu.AddItem(new MenuItem("dz191." + MenuName + ".settings.reset", "Reset to default/optimal values").SetValue(false)); ////TODO

            menu.AddSubMenu(miscMenu);
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

            menu.AddSubMenu(drawMenu);

            menu.AddToMainMenu();
        }
        #endregion

        #region Update/Draw/Various Delegates
        /// <summary>
        /// Called every tick.
        /// </summary>
        internal static void OnUpdate()
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            switch (orbwalker.ActiveMode)
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

        /// <summary>
        /// Called when a gapcloser is registered.
        /// </summary>
        /// <param name="gapcloser">The gapcloser parameters</param>
        internal static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (menu.Item("dz191." + MenuName + ".settings.e.antigp").GetValue<bool>() && Spells[SpellSlot.E].IsReady() && gapcloser.Sender.IsValidTarget(Spells[SpellSlot.E].Range))
            {
                Spells[SpellSlot.E].Cast(gapcloser.Sender);
            }
        }

        /// <summary>
        /// Called when a possible interrupt is registered.
        /// </summary>
        /// <param name="sender">The Interruptable target</param>
        /// <param name="args">The interrupt data</param>
        internal static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (menu.Item("dz191." + MenuName + ".settings.e.interrupter").GetValue<bool>() && Spells[SpellSlot.E].IsReady() && sender.IsValidTarget(Spells[SpellSlot.E].Range) && args.DangerLevel >= Interrupter2.DangerLevel.High)
            {
                Spells[SpellSlot.E].Cast(sender);
            }
        }

        /// <summary>
        /// Called before the attack of the orbwalker.
        /// </summary>
        /// <param name="args">The Event args</param>
        internal static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit ||
                orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (IsFishBone())
                {
                    Spells[SpellSlot.Q].Cast();
                }
            }
        }

        /// <summary>
        /// Used to Draw Ranges
        /// </summary>
        internal static void OnDraw()
        {

        }
        #endregion

        #region Automatic Functions
        /// <summary>
        /// Called in OnUpdate, used for Automatic functions.
        /// </summary>
        internal static void OnAuto()
        {
            AutoMinigunSwap();
            AutoE();
        }

        /// <summary>
        /// Used to AutoSwap to minigun.
        /// </summary>
        internal static void AutoMinigunSwap()
        {
            var swapMinigun = menu.Item("dz191." + MenuName + ".settings.q.minigunnoenemies").GetValue<bool>();
            var swapMinigunRange = menu.Item("dz191." + MenuName + ".settings.q.rangeswitch").GetValue<Slider>().Value;
            if (ObjectManager.Player.CountEnemiesInRange(swapMinigunRange) == 0 && swapMinigun && !ObjectManager.Player.IsWindingUp)
            {
                if (IsFishBone() && Spells[SpellSlot.Q].IsReady())
                {
                    Spells[SpellSlot.Q].Cast();
                }
            }
        }

        /// <summary>
        /// Used to AutoW.
        /// </summary>
        internal static void AutoW()
        {
            var autoWEnabled = menu.Item("dz191." + MenuName + ".settings.w.immobile").GetValue<bool>();
            var autoWMana = menu.Item("dz191." + MenuName + ".settings.w.autowmana").GetValue<Slider>().Value;
            if (!autoWEnabled || !Spells[SpellSlot.W].IsReady() || ObjectManager.Player.Mana < autoWMana)
            {
                return;
            }

            var minWRange = menu.Item("dz191." + MenuName + ".settings.w.minwrange").GetValue<Slider>().Value;
            var currentTarget = TargetSelector.GetTarget(Spells[SpellSlot.W].Range, TargetSelector.DamageType.Physical);
            if (currentTarget.IsValidTarget(minWRange) || !Spells[SpellSlot.W].CanCast(currentTarget) || !currentTarget.IsValidTarget(Spells[SpellSlot.W].Range))
            {
                return;
            }

            var wHitchance = GetHitchanceFromMenu("dz191." + MenuName + ".settings.w.hitchance");
            Spells[SpellSlot.W].CastIfHitchanceEquals(currentTarget, wHitchance);
        }

        /// <summary>
        /// Used to Call the 2 AutoE Methods.
        /// </summary>
        internal static void AutoE()
        {
            AutoEStunned();
            AutoESlowed();
        }

        /// <summary>
        /// Auto E Slowed enemies.
        /// </summary>
        internal static void AutoESlowed()
        {
            var eEnabled = menu.Item("dz191." + MenuName + ".settings.e.slowed").GetValue<bool>();
            var eMana = menu.Item("dz191." + MenuName + ".settings.e.autoemana").GetValue<Slider>().Value;
            if (!Spells[SpellSlot.E].IsReady() || !eEnabled || ObjectManager.Player.ManaPercent < eMana)
            {
                return;
            }

            var eTarget = orbwalker.GetTarget().IsValid<Obj_AI_Hero>() ? orbwalker.GetTarget() as Obj_AI_Hero : TargetSelector.GetTarget(Spells[SpellSlot.E].Range, TargetSelector.DamageType.Physical);
            if (!eTarget.IsValidTarget() || eTarget == null)
            {
                return;
            }

            var isTargetSlowed = IsLightlyImpaired(eTarget);
            if (!isTargetSlowed)
            {
                return;
            }

            var eHitchance = GetHitchanceFromMenu("dz191." + MenuName + ".settings.e.hitchance");
            if ( eTarget.Path.Count() > 1)
            {
                    var slowEndTime = GetSlowEndTime(eTarget);
                    if (slowEndTime >= Spells[SpellSlot.E].Delay + 0.5f + Game.Ping / 2f)
                    {
                        Spells[SpellSlot.E].CastIfHitchanceEquals(eTarget, eHitchance);
                    }

            }
        }

        /// <summary>
        /// Auto E Immobile enemies.
        /// </summary>
        internal static void AutoEStunned()
        {
            var eEnabled = menu.Item("dz191." + MenuName + ".settings.e.immobile").GetValue<bool>();
            var eMana = menu.Item("dz191." + MenuName + ".settings.e.autoemana").GetValue<Slider>().Value;
            if (!Spells[SpellSlot.E].IsReady() || !eEnabled || ObjectManager.Player.ManaPercent < eMana)
            {
                return;
            }

            var eTarget = orbwalker.GetTarget().IsValid<Obj_AI_Hero>() ? orbwalker.GetTarget() as Obj_AI_Hero : TargetSelector.GetTarget(Spells[SpellSlot.E].Range, TargetSelector.DamageType.Physical);
            if (!eTarget.IsValidTarget() || eTarget == null)
            {
                return;
            }

            var isTargetImmobile = IsHeavilyImpaired(eTarget);
            if (!isTargetImmobile)
            {
                return;
            }

            var eHitchance = GetHitchanceFromMenu("dz191." + MenuName + ".settings.e.hitchance");
            var immEndTime = GetImpairedEndTime(eTarget);
            if (immEndTime >= Spells[SpellSlot.E].Delay + 0.5f + Game.Ping / 2f)
            {
                 Spells[SpellSlot.E].CastIfHitchanceEquals(eTarget, eHitchance);
            }

        }

        /// <summary>
        /// Called when the user presses the manual R key.
        /// </summary>
        internal static void ManualR()
        {
            
        }
        #endregion

    }
}
