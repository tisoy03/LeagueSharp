using System.Windows.Forms;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.Extensions.SharpDX;
using LeagueSharp.SDK.Core.IDrawing;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.UI.INotifications;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.SDK.Core.Wrappers;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;
using MenuItem = LeagueSharp.SDK.Core.UI.IMenu.MenuItem;

namespace VayneHunter_Reborn_SDK
{
    #region References
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp;
    using SharpDX;
    using MapPosition;
    using Utility;

    using Color = System.Drawing.Color;
    using Geometry = LeagueSharp.SDK.Core.Math.Geometry;
    using ActiveGapcloser = VayneHunter_Reborn_SDK.Utility.ActiveGapcloser;
    using System.Reflection;

    #endregion

    class VayneHunterReborn
    {
        /// <summary>
        /// TODO Log:
        /// Rewrite most of the core part of the code, since it is so ugly.
        /// 
        /// Add Support for xSalice Orbwalker   
        /// Antigapcloser/Interrupter List
        /// </summary>
       
        public static Menu Menu;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Orbwalker Orbwalker;
        private static Vector3 _predictedPosition;
        private static Vector3 _predictedEndPosition;
        private static float _lastCheckTick;
        private static float _lastCheckTick2;
        private static float _lastCondemnCheck;

        private static readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q) },
            { SpellSlot.W, new Spell(SpellSlot.W) },
            { SpellSlot.E, new Spell(SpellSlot.E, 590f) },
            { SpellSlot.R, new Spell(SpellSlot.R) }
        };

        private static Spell trinketSpell;
       // private static readonly Notification CondemnNotification = new Notification("Condemned",5500);
       
        /// <summary>
        /// Method Called when the Assembly is loaded.
        /// </summary>
        public static void OnLoad(string[] args)
        {
            if (ObjectManager.Player.ChampionName != "Vayne")
            {
                return;
            }
            Bootstrap.Init(args);
            SetUpMenu();
            SetUpEvents();
            SetUpSkills();
        }

        /// <summary>
        /// Sets up the Menu
        /// </summary>
        private static void SetUpMenu()
        {
            Menu = new Menu("VayneHunter Reborn", "VHR", true);

            CustomTargetSelector.OnLoad(Menu);

            var comboMenu = new Menu("[VHR] Combo", "dz191.vhr.combo");
            comboMenu.AddModeMenu(Mode.Combo, new[] { SpellSlot.Q, SpellSlot.E, SpellSlot.R }, new[] { true, true, false });
            comboMenu.AddManaManager(Mode.Combo, new[] { SpellSlot.Q, SpellSlot.E, SpellSlot.R }, new[] { 0, 0, 0 });
            comboMenu.Add(new MenuSlider("dz191.vhr.combo.r.minenemies", "Min R Enemies", 2, 1, 5));
            Menu.Add(comboMenu);

            var harassMenu = new Menu("[VHR] Harass", "dz191.vhr.harass");
            harassMenu.AddModeMenu(Mode.Harrass, new[] { SpellSlot.Q, SpellSlot.E }, new[] { true, true });
            harassMenu.AddManaManager(Mode.Harrass, new[] { SpellSlot.Q, SpellSlot.E }, new[] { 25, 20 });
            Menu.Add(harassMenu);

            var farmMenu = new Menu("[VHR] Farm", "dz191.vhr.farm");
            farmMenu.AddModeMenu(Mode.Farm, new[] { SpellSlot.Q }, new[] { true, true });
            farmMenu.AddManaManager(Mode.Farm, new[] { SpellSlot.Q }, new[] { 40 });
            Menu.Add(farmMenu);

            var miscMenu = new Menu("[VHR] Misc", "dz191.vhr.misc");
            var miscQMenu = new Menu("Misc - Tumble", "dz191.vhr.misc.tumble");
            {
                miscQMenu.Add(new MenuList<string>("dz191.vhr.misc.condemn.qlogic", "Q Logic", new[] { "Normal", "Away from enemies" }));
                miscQMenu.Add(new MenuBool("dz191.vhr.misc.tumble.smartq", "Try to QE First"));
                miscQMenu.Add(new MenuBool("dz191.vhr.misc.tumble.noaastealth", "Don't AA while stealthed"));
                miscQMenu.Add(new MenuBool("dz191.vhr.misc.tumble.noqenemies", "Don't Q into enemies", true));
                miscQMenu.Add(new MenuBool("dz191.vhr.misc.tumble.dynamicqsafety", "Dynamic Q Safety Distance"));
                miscQMenu.Add(new MenuBool("dz191.vhr.misc.tumble.qspam", "Ignore Q checks"));
                miscQMenu.Add(new MenuBool("dz191.vhr.misc.tumble.qinrange", "Q In Range if Enemy Health < Q+AA Dmg", true));
                miscQMenu.Add(new MenuKeyBind("dz191.vhr.misc.tumble.walltumble", "Tumble Over Wall (WallTumble)", System.Windows.Forms.Keys.Y, KeyBindType.Press));
                miscQMenu.Add(new MenuBool("dz191.vhr.misc.tumble.mirin", "Enable this if you're Mirin"));
            }

            var miscEMenu = new Menu("Misc - Condemn", "dz191.vhr.misc.condemn");
            {
                miscEMenu.Add(new MenuList<string>("dz191.vhr.misc.condemn.condemnmethod", "Condemn Method", new[] { "VH Reborn", "Marksman/Gosu", "VH Rework" }));
                miscEMenu.Add(new MenuSlider("dz191.vhr.misc.condemn.pushdistance", "E Push Distance", 375, 350, 500));
                miscEMenu.Add(new MenuKeyBind("dz191.vhr.misc.condemn.enextauto", "E Next Auto", Keys.T, KeyBindType.Toggle));
                miscEMenu.Add(new MenuBool("dz191.vhr.misc.condemn.onlystuncurrent", "Only stun current target"));
                miscEMenu.Add(new MenuBool("dz191.vhr.misc.condemn.autoe", "Auto E"));
                miscEMenu.Add(new MenuBool("dz191.vhr.misc.condemn.eks", "Smart E Ks"));
                miscEMenu.Add(new MenuSlider("dz191.vhr.misc.condemn.noeaa", "Don't E if Target can be killed in X AA", 1, 0, 4));

                miscEMenu.Add(new MenuBool("dz191.vhr.misc.condemn.trinketbush", "Trinket Bush on Condemn", true));
                miscEMenu.Add(new MenuBool("dz191.vhr.misc.condemn.ethird", "E 3rd proc in Harass"));
                miscEMenu.Add(new MenuBool("dz191.vhr.misc.condemn.lowlifepeel", "Peel with E when low"));

                miscEMenu.Add(new MenuBool("dz191.vhr.misc.condemn.condemnturret", "Try to Condemn to turret"));
                miscEMenu.Add(new MenuBool("dz191.vhr.misc.condemn.condemnflag", "Condemn to J4 flag", true));
                miscEMenu.Add(new MenuBool("dz191.vhr.misc.condemn.noeturret", "No E Under enemy turret", true));
            }

            var miscGeneralSubMenu = new Menu("Misc - General", "dz191.vhr.misc.general");
            {
                miscGeneralSubMenu.Add(new MenuBool("dz191.vhr.misc.general.antigp", "Anti Gapcloser", true));
                miscGeneralSubMenu.Add(new MenuBool("dz191.vhr.misc.general.interrupt", "Interrupter", true));
                miscGeneralSubMenu.Add(new MenuSlider("dz191.vhr.misc.general.antigpdelay", "Anti Gapcloser Delay (ms)", 0, 0, 1000));
                miscGeneralSubMenu.Add(new MenuBool("dz191.vhr.misc.general.specialfocus", "Focus targets with 2 W marks"));
                miscGeneralSubMenu.Add(new MenuBool("dz191.vhr.misc.general.reveal", "Stealth Reveal (Pink Ward)"));
                miscGeneralSubMenu.Add(new MenuBool("dz191.vhr.misc.general.disablemovement", "Disable Orbwalker Movement"));
                /**
                miscGeneralSubMenu.AddItem(new MenuItem("dz191.vhr.misc.general.permashow", "PermaShow").SetShared().SetValue(true)).ValueChanged += (s, args) =>
                {
                    if (args.GetNewValue<bool>())
                    {
                        Menu.Item("dz191.vhr.misc.condemn.enextauto").Permashow(true, "E Next Auto");
                    }
                    else
                    {
                        Menu.Item("dz191.vhr.misc.condemn.enextauto").Permashow(false);
                    }
                };
                 * */
            }
            miscMenu.Add(miscQMenu);
            miscMenu.Add(miscEMenu);
            miscMenu.Add(miscGeneralSubMenu);
            Menu.Add(miscMenu);


            //AntiGP.BuildMenu(Menu);

            /**
            var drawMenu = new Menu("[VHR] Drawing", "dz191.vhr.drawing");
            drawMenu.AddDrawMenu(_spells, Color.Red);
            drawMenu.AddItem(new MenuItem("dz191.vhr.drawing.drawstun", "Draw Stunnable").SetValue(true));
            drawMenu.AddItem(new MenuItem("dz191.vhr.drawing.drawspots", "Draw Spots").SetValue(true));
            Menu.AddSubMenu(drawMenu);

            Menu.AddItem(new MenuItem("dz191.vhr.info", "VHR by Asuna v." + Assembly.GetExecutingAssembly().GetName().Version));

            Menu.Item("dz191.vhr.misc.condemn.enextauto").Permashow(Menu.Item("dz191.vhr.misc.condemn.enextauto").GetValue<KeyBind>().Active, "E Next Auto");
            */
            Menu.Attach();
        }

        /// <summary>
        /// Sets up the Spell's Data
        /// </summary>
        private static void SetUpSkills()
        {
            _spells[SpellSlot.E].SetTargetted(0.25f,2000f);
            trinketSpell = new Spell(SpellSlot.Trinket);
        }

        /// <summary>
        /// Registers the Events.
        /// </summary>
        private static void SetUpEvents()
        {
            Cleanser.OnLoad();
            PotionManager.OnLoad(Menu);
            ItemManager.OnLoad(Menu);
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalker.OnAction += Orbwalker_OnAction;
            AntiGP.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            InterruptableSpell.OnInterruptableTarget += Interrupter_OnInterrupt;
            Stealth.OnStealth += Stealth_OnStealth;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Hero_OnPlayAnimation;
            GameObject.OnCreate += GameObject_OnCreate;
            if (CustomTargetSelector.IsActive())
            {
                CustomTargetSelector.RegisterEvents();
            }
        }

        #region Delegate Methods.

        /// <summary>
        /// Delegate called when a Game Object is created.
        /// </summary>
        /// <param name="sender">The Created Gameobject.</param>
        /// <param name="args">The event args'</param>
        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general", "antigp") && _spells[SpellSlot.E].IsReady())
            {
                if (sender.IsEnemy && sender.Name == "Rengar_LeapSound.troy")
                {
                    var rengarEntity = GameObjects.EnemyHeroes.Find(h => h.ChampionName.Equals("Rengar") && h.IsValidTarget(_spells[SpellSlot.E].Range));
                    if (rengarEntity != null)
                    {
                        _spells[SpellSlot.E].Cast(rengarEntity);
                    }
                }
            }
        }

        #region
        static void Obj_AI_Hero_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            /**
            return;
            if (!sender.IsMe)
            {
                return;
            }
            if (args.Animation.Contains("Attack"))
            {
                LeagueSharp.Common.Utility.DelayAction.Add((25), () =>
                {
                    if (ObjectManager.Player.IsAttackingPlayer)
                    {
                        LeagueSharp.Common.Utility.DelayAction.Add((int)(ObjectManager.Player.AttackCastDelay * 1000 + 15), () => OrbwalkingAfterAttack(ObjectManager.Player, VayneHunterReborn.Orbwalker.GetTarget()));
                    }
                });
            }
             * */
        }
        #endregion

        /// <summary>
        /// Called when an unit goes on stealth.
        /// </summary>
        /// <param name="obj">The Event's arguments</param>
        private static void Stealth_OnStealth(Stealth.OnStealthEventArgs obj)
        {
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general","reveal"))
            {
                if (obj.Sender.Distance(ObjectManager.Player) <= 600f && obj.IsStealthed && !obj.Sender.IsMe)
                {
                    if (Items.HasItem(2043) && Items.CanUseItem(2043))
                    {
                        Items.UseItem(2043, obj.Sender.ServerPosition.Extend(ObjectManager.Player.ServerPosition, 400f));
                    }
                }
            }
        }

        /// <summary>
        /// Method called each tick.
        /// </summary>
        /// <param name="args">The Event's args</param>
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            if (CustomTargetSelector.IsActive())
            {
                if (CustomTargetSelector.GetTarget(GetRealAARange()) != null)
                {
                    Orbwalker.OrbwalkTarget = CustomTargetSelector.GetTarget(GetRealAARange());
                }
                else
                {
                    Orbwalker.OrbwalkTarget = (TargetSelector.GetTarget(GetRealAARange()));

                }
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                    Combo();
                    break;
                case OrbwalkerMode.Hybrid:
                    Harrass();
                    break;
            }

            OnUpdateFunctions();
        }

        private static float GetRealAARange(Obj_AI_Hero target = null)
        {
            var result = ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius;
            if (target.IsValidTarget() && target != null)
            {
                return result + target.BoundingRadius;
            }
            return result;
        }
        /// <summary>
        /// Called when an unit finishes the attack.
        /// </summary>
        /// <param name="unit">The Unit</param>
        /// <param name="target">The target of the attack</param>
        private static void Orbwalker_OnAction(object sender, LeagueSharp.SDK.Core.Orbwalker.OrbwalkerActionArgs e)
        {
            if (e.Type == OrbwalkerType.AfterAttack)
            {
                if (!(e.Target is Obj_AI_Base) || !e.Sender.IsMe)
                {
                    return;
                }

                var tg = (Obj_AI_Base)e.Target;

                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Orbwalk:
                        if (_spells[SpellSlot.Q].IsEnabledAndReady(Mode.Combo))
                        {
                            CastQ(tg);
                        }
                        break;
                    case OrbwalkerMode.Hybrid:
                        if (_spells[SpellSlot.Q].IsEnabledAndReady(Mode.Harrass) && (tg is Obj_AI_Hero))
                        {
                            CastQ(tg);
                        }
                        break;
                    case OrbwalkerMode.LastHit:
                        Farm();
                        break;
                    case OrbwalkerMode.LaneClear:
                        Farm();
                        break;
                }

                if (MenuHelper.getKeybindValue("dz191.vhr.misc.condemn", "enextauto") &&
                    _spells[SpellSlot.E].CanCast(tg) && (tg is Obj_AI_Hero))
                {
                    _spells[SpellSlot.E].Cast(tg);
                    //TODO
                    //Menu.Item("dz191.vhr.misc.condemn.enextauto").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle));
                }
            }
            
        }

        /// <summary>
        /// Delegate used for drawings.
        /// </summary>
        /// <param name="args">The event's args.</param>
        private static void Drawing_OnDraw(EventArgs args)
        {
            /**
            var drawE = Menu.Item("VayneDrawE").GetValue<Circle>();
            var drakeWallQPos = new Vector2(11514, 4462);

            if (drawE.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _spells[SpellSlot.E].Range, drawE.Color);
            }

            if (MenuHelper.isMenuEnabled("dz191.vhr.drawing.drawstun"))
            {
                Obj_AI_Hero myTarget;
            }

            if (MenuHelper.isMenuEnabled("dz191.vhr.drawing.drawspots"))
            {
                if (ObjectManager.Player.Distance(drakeWallQPos) <= 1500f && Helpers.IsSummonersRift())
                {
                    Render.Circle.DrawCircle(drakeWallQPos.To3D2(), 65f, Color.AliceBlue);
                }
            }
             * */
        }
        
        /// <summary>
        /// Called when an unit gapcloses onto the Player.
        /// </summary>
        /// <param name="gapcloser">The Event's args</param>
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general","antigp"))
            {
               DelayAction.Add((float)MenuHelper.getSliderValue("dz191.vhr.misc.general", "antigpdelay"),
                    () =>
                    {
                        if (gapcloser.Sender.IsValidTarget(_spells[SpellSlot.E].Range) 
                            && gapcloser.End.Distance(ObjectManager.Player.ServerPosition) <= 400f 
                            && (gapcloser.Sender is Obj_AI_Hero)
                            && MenuHelper.isMenuEnabled(string.Format("dz191.vhr.agplist.{0}", gapcloser.Sender.ChampionName.ToLowerInvariant()), gapcloser.SpellName))
                        {
                            _spells[SpellSlot.E].Cast(gapcloser.Sender);
                        }
                    });
            }
        }

        /// <summary>
        /// Called when an unit casts an interruptable spell.
        /// </summary>
        /// <param name="sender">The sender of the spell.</param>
        /// <param name="args">The event's args.</param>
        private static void Interrupter_OnInterrupt(object sender, InterruptableSpell.InterruptableTargetEventArgs e)
        {
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general","interrupt"))
            {
                if (e.DangerLevel == DangerLevel.High && e.Sender.IsValidTarget(_spells[SpellSlot.E].Range) && (sender is Obj_AI_Hero))
                {
                    _spells[SpellSlot.E].Cast(e.Sender);
                }
            }
        }

        #endregion

        #region Modes
        /// <summary>
        /// The Combo Method.
        /// </summary>
        private static void Combo()
        {
            if (Environment.TickCount - _lastCheckTick2 < 80)
            {
                return;
            }

            _lastCheckTick2 = Environment.TickCount;

            if (_spells[SpellSlot.E].IsEnabledAndReady(Mode.Combo))
            {
                Obj_AI_Hero target;

                if (CondemnCheck(ObjectManager.Player.ServerPosition, out target))
                {
                    if (target.IsValidTarget(_spells[SpellSlot.E].Range) && (target is Obj_AI_Hero))
                    {
                        _spells[SpellSlot.E].Cast(target);
                    }
                }
            }

            if (_spells[SpellSlot.Q].IsEnabledAndReady(Mode.Combo))
            {
                CheckAndCastKSQ();
            }
        }

        /// <summary>
        /// The Harass Method
        /// </summary>
        private static void Harrass()
        {
            if (_spells[SpellSlot.E].IsEnabledAndReady(Mode.Harrass))
            {
                var possibleTarget = GameObjects.EnemyHeroes.Find(enemy => enemy.IsValidTarget(_spells[SpellSlot.E].Range) && enemy.Has2WStacks());
                if (possibleTarget != null && MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "ethird") && (possibleTarget is Obj_AI_Hero) && !ObjectManager.Player.HasBuff("vaynetumblebonus"))
                {
                    _spells[SpellSlot.E].Cast(possibleTarget);
                }

                Obj_AI_Hero target;
                if (CondemnCheck(ObjectManager.Player.ServerPosition, out target))
                {
                    if (target.IsValidTarget(_spells[SpellSlot.E].Range) && (target is Obj_AI_Hero))
                    {
                        _spells[SpellSlot.E].Cast(target);
                    }
                }
            }
        }

        /// <summary>
        /// The Farm Method.
        /// </summary>
        private static void Farm()
        {
            if (!_spells[SpellSlot.Q].IsEnabledAndReady(Mode.Farm))
            {
                return;
            }

            var minionsInRange = GameObjects.EnemyMinions.Where(m => m.Health <= Player.GetAutoAttackDamage(m) &&
                m.DistanceSquared(ObjectManager.Player) <= Math.Pow(GameObjects.Player.AttackRange, 2)).ToList();
                //&& _spells[SpellSlot.Q].GetDamage(m)); TODO When Dmg Lib Bik
            if (!minionsInRange.Any())
            {
                return;
            }

            if (minionsInRange.Count > 1)
            {
                var firstMinion = minionsInRange.OrderBy(m => m.HealthPercent).First();
                CastTumble(firstMinion);
                Orbwalker.OrbwalkTarget = firstMinion;
                
            }
        }

        /// <summary>
        /// Method called to execute the OnUpdate Functions (KS, AutoE, Etc).
        /// </summary>
        private static void OnUpdateFunctions()
        {
            if (Environment.TickCount - _lastCheckTick < 150)
            {
                return;
            }

            _lastCheckTick = Environment.TickCount;

            #region Auto E
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "autoe") && _spells[SpellSlot.E].IsReady())
            {
                Obj_AI_Hero target;
                if (CondemnCheck(ObjectManager.Player.ServerPosition, out target))
                {
                    if (target.IsValidTarget(_spells[SpellSlot.E].Range) && (target is Obj_AI_Hero))
                    {
                        _spells[SpellSlot.E].Cast(target);
                    }
                }
            }
            #endregion

            #region Focus 2 W stacks
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general", "specialfocus"))
            {
                var target = GameObjects.EnemyHeroes.Find(en => en.IsValidTarget(ObjectManager.Player.AttackRange) && en.Has2WStacks());
                if (target != null)
                {
                    Orbwalker.OrbwalkTarget = target;
                    CustomTargetSelector.scriptSelectedHero = target;
                    Hud.SelectedUnit = target;
                }
            }
            #endregion

            #region Disable AA Stealth
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "noaastealth") && ObjectManager.Player.CountEnemiesInRange(1000f) > 1)
            {
                Orbwalker.Attack = !Helpers.IsPlayerFaded();
            }
            #endregion

            #region Condemn KS
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "eks") && _spells[SpellSlot.E].IsReady() && !ObjectManager.Player.HasBuff("vaynetumblebonus"))
            {
                var target = GameObjects.EnemyHeroes.Find(en => en.IsValidTarget(_spells[SpellSlot.E].Range) && en.Has2WStacks());
                //TODO
                if (target != null && target.Health + 60 <= (ObjectManager.Player.GetSpellDamage(target, SpellSlot.E) + ObjectManager.Player.GetSpellDamage(target, SpellSlot.W)) && (target is Obj_AI_Hero))
                {
                    _spells[SpellSlot.E].Cast(target);
                }
            }
            #endregion

            #region WallTumble
            if (Menu.GetValue<MenuKeyBind>("dz191.vhr.misc.tumble.walltumble").Active && _spells[SpellSlot.Q].IsReady())
            {
                WallTumble();
            }
            #endregion

            #region Low Life Peel
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "lowlifepeel") && ObjectManager.Player.HealthPercent <= 20 && _spells[SpellSlot.E].IsReady())
            {
                var meleeEnemies = ObjectManager.Player.GetEnemiesInRange(375f).FindAll(m => m.IsMelee());
                if (meleeEnemies.Any())
                {
                    var mostDangerous = meleeEnemies.OrderByDescending(m => m.GetAutoAttackDamage(ObjectManager.Player)).First();
                    if (mostDangerous is Obj_AI_Hero)
                    {
                        _spells[SpellSlot.E].Cast(mostDangerous);
                    }
                }
            }
            #endregion

            #region Disable Movement
            Orbwalker.Movement = !MenuHelper.isMenuEnabled("dz191.vhr.misc.general", "disablemovement");
            #endregion

        }

        #endregion

        #region Tumble Region

        private static void CheckAndCastKSQ()
        {
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "qinrange") && _spells[SpellSlot.Q].IsReady())
            {
                var currentTarget = TargetSelector.GetTarget(GetRealAARange() + 240f);
                if (!currentTarget.IsValidTarget())
                {
                    return;
                }

                if (currentTarget.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <=
                   GetRealAARange(null))
                {
                    return;
                }

                if (currentTarget.Health + 15 <
                    ObjectManager.Player.GetAutoAttackDamage(currentTarget) +
                    ObjectManager.Player.GetSpellDamage(currentTarget, SpellSlot.Q))
                {
                    var extendedPosition = ObjectManager.Player.ServerPosition.Extend(
                        currentTarget.ServerPosition, 300f);
                    if (Helpers.OkToQ(extendedPosition))
                    {
                        _spells[SpellSlot.Q].Cast(extendedPosition);
                        Orbwalker.ResetAutoAttackTimer();
                        Orbwalker.OrbwalkTarget = currentTarget;
                        CustomTargetSelector.scriptSelectedHero = currentTarget;
                    }
                }
            }
        }

        private static void CastQ(Obj_AI_Base target)
        {
            var myPosition = Game.CursorPos;
            Obj_AI_Hero myTarget = null;
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "smartq") && _spells[SpellSlot.E].IsReady()) 
            {
                const int currentStep = 30;
                var direction = ObjectManager.Player.Direction.ToVector2().Perpendicular();
                for (var i = 0f; i < 360f; i += currentStep)
                {
                    var angleRad = (i) * (float)(Math.PI / 180f);
                    var rotatedPosition = ObjectManager.Player.Position.ToVector2() + (300f * direction.Rotated(angleRad));
                    if (CondemnCheck(rotatedPosition.ToVector3(), out myTarget) && Helpers.OkToQ(rotatedPosition.ToVector3()))
                    {
                        myPosition = rotatedPosition.ToVector3();
                        break;
                    }
                }
            }

            if (_spells[SpellSlot.R].IsEnabledAndReady(Mode.Combo) && (Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk) && ObjectManager.Player.CountEnemiesInRange(GetRealAARange() + 200f) >= MenuHelper.getSliderValue("dz191.vhr.combo.r", "minenemies"))
            {
                _spells[SpellSlot.R].Cast();
            }

            CastTumble(myPosition,target);

            if (myPosition != Game.CursorPos && myTarget != null && myTarget.IsValidTarget(300f + _spells[SpellSlot.E].Range) && _spells[SpellSlot.E].IsReady())
            {
                    DelayAction.Add((int)(Game.Ping / 2f + _spells[SpellSlot.Q].Delay * 1000 + 300f / 1500f + 50f),
                        () =>
                        {
                            if (!_spells[SpellSlot.Q].IsReady())
                            {
                                _spells[SpellSlot.E].Cast(myTarget);
                            }
                        });
            }
        }

        private static void CastTumble(Obj_AI_Base target)
        {
            if (!_spells[SpellSlot.Q].IsReady())
            {
                return;
            }

            var posAfterTumble =
                ObjectManager.Player.ServerPosition.ToVector2().Extend(Game.CursorPos.ToVector2(), 300f).ToVector3();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if ((distanceAfterTumble <= 550 * 550 && distanceAfterTumble >= 100 * 100) || (MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "qspam")))
            {
                if (!Helpers.OkToQ2(posAfterTumble) && MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "noqenemies"))
                {
                    if(!(MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "qspam")))
                    {
                          return;
                    }
                }
                _spells[SpellSlot.Q].Cast(Game.CursorPos);
            }
        }

        private static void CastTumble(Vector3 pos, Obj_AI_Base target)
        {
            if (!_spells[SpellSlot.Q].IsReady())
            {
                return;
            }
            var posAfterTumble =
                ObjectManager.Player.ServerPosition.ToVector2().Extend(pos.ToVector2(), 300f).ToVector3();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if ((distanceAfterTumble < 550 * 550 && distanceAfterTumble > 100 * 100) ||
                (MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "qspam")))
            {

                switch (Menu["dz191.vhr.misc.tumble"]["qlogic"].GetValue<MenuList<string>>().Index)
                {
                    case 0:
                        if (!Helpers.OkToQ2(posAfterTumble) &&
                            MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "noqenemies"))
                        {
                            if (!(MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "qspam")))
                            {
                                return;
                            }
                        }
                        _spells[SpellSlot.Q].Cast(pos);
                        break;

                    case 1:
                        if (PositionalHelper.MeleeEnemiesTowardsMe.Any() &&
                            !PositionalHelper.MeleeEnemiesTowardsMe.All(m => m.HealthPercent <= 15))
                        {
                            var Closest =
                                PositionalHelper.MeleeEnemiesTowardsMe.OrderBy(m => m.Distance(ObjectManager.Player))
                                    .First();
                            var whereToQ = Closest.ServerPosition.Extend(
                                ObjectManager.Player.ServerPosition, Closest.Distance(ObjectManager.Player) + 300f);
                            if ((Helpers.OkToQ2(whereToQ) ||
                                 (!Helpers.OkToQ2(whereToQ) && MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "qspam"))) &&
                                !whereToQ.UnderTurret(true))
                            {
                                _spells[SpellSlot.Q].Cast(whereToQ);
                                return;
                            }
                        }

                        if (!Helpers.OkToQ2(posAfterTumble) &&
                            MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "noqenemies"))
                        {
                            if (!(MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "qspam")))
                            {
                                return;
                            }
                        }

                        _spells[SpellSlot.Q].Cast(pos);
                        break;
                }
            }
        }

        #endregion

        #region E Region
        /// <summary>
        /// The E logic checker method.
        /// </summary>
        /// <param name="fromPosition">The start position</param>
        /// <param name="tg">The target which can be condemned.</param>
        /// <returns>Whether the target is condemnable or not.</returns>
        private static bool CondemnCheck(Vector3 fromPosition, out Obj_AI_Hero tg)
        {
            if (Environment.TickCount - _lastCondemnCheck < 150)
            {
                tg = null;
                return false;
            }
            _lastCondemnCheck = Environment.TickCount;

            if ((fromPosition.UnderTurret(true) && MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "noeturret"))|| !_spells[SpellSlot.E].IsReady())
            {
                tg = null;
                return false;
            }
            if (
                !GameObjects.EnemyHeroes.Any(
                    h =>
                        h.IsValidTarget(_spells[SpellSlot.E].Range) && !h.HasBuffOfType(BuffType.SpellShield) &&
                        !h.HasBuffOfType(BuffType.SpellImmunity)))
            {
                tg = null;
                return false;
            }
            switch (Menu["dz191.vhr.misc.condemn"]["condemnmethod"].GetValue<MenuList<string>>().Index)
            {
                case 0:
                    //VHReborn Condemn Code
                    foreach (var target in GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(_spells[SpellSlot.E].Range) && !h.HasBuffOfType(BuffType.SpellShield) && !h.HasBuffOfType(BuffType.SpellImmunity)))
                    {
                        var pushDistance = MenuHelper.getSliderValue("dz191.vhr.misc.condemn", "pushdistance");
                        var targetPosition = target.ServerPosition;
                        var finalPosition = targetPosition.Extend(fromPosition, -pushDistance);
                        var numberOfChecks = (float)Math.Ceiling(pushDistance / 30f);

                        if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "onlystuncurrent") &&
                                    !target.Equals(Orbwalker.OrbwalkTarget))
                        {
                            continue;
                        }

                        for (var i = 1; i <= 30; i++)
                        {
                            var v3 = (targetPosition - fromPosition).Normalized();
                            var extendedPosition = targetPosition + v3 * (numberOfChecks * i); 
                            //var underTurret = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.condemnturret") && (Helpers.UnderAllyTurret(finalPosition) || Helpers.IsFountain(finalPosition));
                            var j4Flag = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "condemnflag") && (Helpers.IsJ4FlagThere(extendedPosition, target));
                            if ((extendedPosition.IsWall() || j4Flag) && (target.Path.Count() < 2) && !target.IsDashing())
                            {
                                
                                if (target.Health + 10 <=
                                    ObjectManager.Player.GetAutoAttackDamage(target) *
                                    MenuHelper.getSliderValue("dz191.vhr.misc.condemn", "noeaa"))
                                {
                                    tg = null;
                                    return false;
                                }

                                if (extendedPosition.UnderTurret(true))
                                {
                                    tg = null;
                                    return false;
                                }

                                if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "trinketbush") &&
                                    NavMesh.IsWallOfGrass(extendedPosition,25) && trinketSpell != null)
                                {
                                    var wardPosition = ObjectManager.Player.ServerPosition.Extend(
                                        extendedPosition,
                                        ObjectManager.Player.ServerPosition.Distance(extendedPosition) - 25f);
                                    DelayAction.Add(250,() => trinketSpell.Cast(wardPosition));
                                }

                                _predictedEndPosition = extendedPosition;
                                _predictedPosition = targetPosition;
                                
                                tg = target;
                                return true;
                            }
                        }
                    }
                    break;
                case 1:
                    //Marksman/Gosu Condemn Code
                    foreach (var target in GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(_spells[SpellSlot.E].Range)))
                    {
                        var pushDistance = MenuHelper.getSliderValue("dz191.vhr.misc.condemn", "pushdistance");
                        var targetPosition = _spells[SpellSlot.E].GetPrediction(target).UnitPosition;
                        var finalPosition = targetPosition.Extend(fromPosition, -pushDistance);
                        var finalPosition2 = targetPosition.Extend(fromPosition, -(pushDistance/2f));
                        var underTurret = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "condemnturret") && (finalPosition.UnderTurret(false) || Helpers.IsFountain(finalPosition));
                        var j4Flag = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "condemnflag") && (Helpers.IsJ4FlagThere(finalPosition, target) || Helpers.IsJ4FlagThere(finalPosition2, target));
                        if (finalPosition.IsWall() || finalPosition2.IsWall() || underTurret || j4Flag)
                        {
                            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "onlystuncurrent") &&
                                    !target.Equals(Orbwalker.OrbwalkTarget))
                            {
                                tg = null;
                                return false;
                            }

                            if (target.Health + 10 <=
                                    ObjectManager.Player.GetAutoAttackDamage(target) *
                                    MenuHelper.getSliderValue("dz191.vhr.misc.condemn", "noeaa"))
                            {
                                tg = null;
                                return false;
                            }

                            tg = target;
                            return true;
                        }
                    }
                    break;
                case 2:
                    //Vayne Hunter Rework
                    foreach (var en in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValidTarget() && hero.Distance(Player.Position)<= _spells[SpellSlot.E].Range))
                    {
                        var ePred = _spells[SpellSlot.E].GetPrediction(en);
                        int pushDist = MenuHelper.getSliderValue("dz191.vhr.misc.condemn", "pushdistance");
                        for (int i = 0; i < pushDist; i += (int)en.BoundingRadius)
                        {
                            Vector3 loc3 = ePred.UnitPosition.ToVector2().Extend(fromPosition.ToVector2(), -i).ToVector3();
                            var orTurret = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "condemnturret") && Helpers.UnderAllyTurret(loc3);
                            if (loc3.IsWall() || orTurret)
                            {
                                if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "onlystuncurrent") &&
                                    !en.Equals(Orbwalker.OrbwalkTarget))
                                {
                                    tg = null;
                                    return false;
                                }

                                if (en.Health + 10 <=
                                    ObjectManager.Player.GetAutoAttackDamage(en) *
                                    MenuHelper.getSliderValue("dz191.vhr.misc.condemn", "noeaa"))
                                {
                                    tg = null;
                                    return false;
                                }

                                if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn", "trinketbush") &&
                                    NavMesh.IsWallOfGrass(loc3, 25) && trinketSpell != null)
                                {
                                    var wardPosition = ObjectManager.Player.ServerPosition.Extend(
                                        loc3,
                                        ObjectManager.Player.ServerPosition.Distance(loc3) - 25f);
                                    DelayAction.Add(250, () => trinketSpell.Cast(wardPosition));
                                }

                                tg = en;
                                return true; 
                            }
                        }
                    }
                    break;
            }
            tg = null;
            return false;
        }

        private static bool CondemnBeta(Vector3 fromPosition, out Obj_AI_Hero tg)
        {
            foreach (var target in from target in GameObjects.EnemyHeroes.Where(
                h =>
                    h.IsValidTarget(_spells[SpellSlot.E].Range) && !h.HasBuffOfType(BuffType.SpellShield) &&
                    !h.HasBuffOfType(BuffType.SpellImmunity)) let pushDistance = MenuHelper.getSliderValue("dz191.vhr.misc.condemn", "pushdistance") let targetPosition = _spells[SpellSlot.E].GetPrediction(target).UnitPosition let finalPosition = targetPosition.Extend(fromPosition, -pushDistance) let condemnRectangle = new Polygon(Polygon.Rectangle(targetPosition.ToVector2(), finalPosition.ToVector2(), target.BoundingRadius)) where condemnRectangle.Points.Any(point => point.IsWall()) select target) {
                        tg = target;
                        return true;
                    }
            tg = null;
            return false;
        }
        #endregion

        #region WallTumble
        private static void WallTumble()
        {
            if (!Helpers.IsSummonersRift())
            {
                return;
            }
            
            Vector2 drakeWallQPos = new Vector2(11514, 4462);
            
            if (Player.Position.X < 12000 || Player.Position.X > 12070 || Player.Position.Y < 4800 ||
                Player.Position.Y > 4872)
            {
                Helpers.MoveToLimited(new Vector2(12050, 4827).ToVector3());
            }
            else
            {
                Helpers.MoveToLimited(new Vector2(12050, 4827).ToVector3());
                _spells[SpellSlot.Q].Cast(drakeWallQPos);
            }
        }

        #endregion

    }
}
