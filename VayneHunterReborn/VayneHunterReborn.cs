using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VayneHunter_Reborn.MapPosition;
using VayneHunter_Reborn.Utility;
using Color = System.Drawing.Color;
using Geometry = LeagueSharp.Common.Geometry;

namespace VayneHunter_Reborn
{
    class VayneHunterReborn
    {
        public static Menu Menu;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Orbwalking.Orbwalker Orbwalker;
        private static Vector3 _predictedPosition;
        private static Vector3 _predictedEndPosition;
        private static float _lastCheckTick;
        private static float _lastCheckTick2;
        private static readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q) },
            { SpellSlot.W, new Spell(SpellSlot.W) },
            { SpellSlot.E, new Spell(SpellSlot.E, 590f) },
            { SpellSlot.R, new Spell(SpellSlot.R) }
        };

        private static Spell trinketSpell;
        private static readonly Notification CondemnNotification = new Notification("Condemned",5500);

        public static void OnLoad()
        {
            if (ObjectManager.Player.ChampionName != "Vayne")
            {
                return;
            }
            Menu = new Menu("VayneHunter Reborn","VHR",true);
            var owMenu = new Menu("[VHR] Orbwalker", "dz191.vhr.orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(owMenu);
            Menu.AddSubMenu(owMenu);

            var tgMenu = new Menu("[VHR] Target Selector", "dz191.vhr.targetselector");
            TargetSelector.AddToMenu(tgMenu);
            Menu.AddSubMenu(tgMenu);

            var comboMenu = new Menu("[VHR] Combo", "dz191.vhr.combo");
            comboMenu.AddModeMenu(Mode.Combo,new []{SpellSlot.Q,SpellSlot.E,SpellSlot.R},new []{true,true,false});
            comboMenu.AddManaManager(Mode.Combo, new[] { SpellSlot.Q, SpellSlot.E, SpellSlot.R }, new[] { 0,0,0 });
            comboMenu.AddItem(new MenuItem("dz191.vhr.combo.r.minenemies", "Min R Enemies").SetValue(new Slider(2, 1, 5)));
            Menu.AddSubMenu(comboMenu);
            var harassMenu = new Menu("[VHR] Harass", "dz191.vhr.harass");
            harassMenu.AddModeMenu(Mode.Harrass, new[] { SpellSlot.Q, SpellSlot.E }, new[] { true, true });
            harassMenu.AddManaManager(Mode.Harrass, new[] { SpellSlot.Q, SpellSlot.E}, new[] { 25, 20 });
            Menu.AddSubMenu(harassMenu);
            var farmMenu = new Menu("[VHR] Farm", "dz191.vhr.farm");
            farmMenu.AddModeMenu(Mode.Farm, new[] { SpellSlot.Q}, new[] { true, true });
            farmMenu.AddManaManager(Mode.Farm, new[] { SpellSlot.Q }, new[] { 40 });
            Menu.AddSubMenu(farmMenu);
            
            var miscMenu = new Menu("[VHR] Misc", "dz191.vhr.misc");
            var miscQMenu = new Menu("Misc - Tumble", "dz191.vhr.misc.tumble");
            {
                miscQMenu.AddItem(new MenuItem("dz191.vhr.misc.tumble.smartq", "Try to QE First").SetValue(false));
                miscQMenu.AddItem(new MenuItem("dz191.vhr.misc.tumble.noqenemies", "Don't Q into enemies").SetValue(true));
                miscQMenu.AddItem(new MenuItem("dz191.vhr.misc.tumble.noaastealth", "Don't AA while stealthed").SetValue(false));
                miscQMenu.AddItem(new MenuItem("dz191.vhr.misc.tumble.qspam", "Ignore Q checks").SetValue(false));
                miscQMenu.AddItem(new MenuItem("dz191.vhr.misc.tumble.walltumble", "Tumble Over Wall").SetValue(new KeyBind("Y".ToCharArray()[0],KeyBindType.Press)));
            }
            var miscEMenu = new Menu("Misc - Condemn", "dz191.vhr.misc.condemn");
            {
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.enextauto", "E Next Auto").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.condemnmethod", "Condemn Method").SetValue(new StringList(new []{"VH Reborn","Marksman/Gosu", "VH Rework"})));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.pushdistance", "E Push Dist").SetValue(new Slider(375, 350, 500)));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.condemnturret", "Try to Condemn to turret").SetValue(false));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.condemnflag", "Condemn to J4 flag").SetValue(true));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.autoe", "Auto E").SetValue(false));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.eks", "Smart E Ks").SetValue(false));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.ethird", "E 3rd proc in Harass").SetValue(false));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.noeturret", "No E Under enemy turret").SetValue(true));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.lowlifepeel", "Peel with E when low").SetValue(false));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.onlystuncurrent", "Only stun current target").SetValue(false));
                miscEMenu.AddItem(new MenuItem("dz191.vhr.misc.condemn.trinketbush", "Trinket Bush on Condemn").SetValue(true));
            }
            var miscGeneralSubMenu = new Menu("Misc - General", "dz191.vhr.misc.general");
            {
                miscGeneralSubMenu.AddItem(new MenuItem("dz191.vhr.misc.general.antigp", "Anti Gapcloser")).SetValue(true);
                miscGeneralSubMenu.AddItem(new MenuItem("dz191.vhr.misc.general.interrupt", "Interrupter").SetValue(true));
                miscGeneralSubMenu.AddItem(new MenuItem("dz191.vhr.misc.general.specialfocus", "Focus targets with 2 W marks").SetValue(false));
                miscGeneralSubMenu.AddItem(new MenuItem("dz191.vhr.misc.general.reveal", "Stealth Reveal (Pink Ward)").SetValue(false));
            }
            miscMenu.AddSubMenu(miscQMenu);
            miscMenu.AddSubMenu(miscEMenu);
            miscMenu.AddSubMenu(miscGeneralSubMenu);
            Menu.AddSubMenu(miscMenu);

            var drawMenu = new Menu("[VHR] Drawing", "dz191.vhr.drawing");
            drawMenu.AddDrawMenu(_spells,Color.Red);
            drawMenu.AddItem(new MenuItem("dz191.vhr.drawing.drawstun", "Draw Stunnable").SetValue(true));
            drawMenu.AddItem(new MenuItem("dz191.vhr.drawing.drawspots", "Draw Spots").SetValue(true));
            Menu.AddSubMenu(drawMenu);

            Menu.AddToMainMenu();
            //Game.PrintChat("<b><font color='#FF0000'>[VH]</font></b><font color='#FFFFFF'> Vayne Hunter Reborn loaded! Version: {0} </font>",Assembly.GetExecutingAssembly().GetName().Version);
            SetUpEvents();
            SetUpSkills();
            trinketSpell = new Spell(SpellSlot.Trinket);
        }

        static void SetUpSkills()
        {
            _spells[SpellSlot.E].SetTargetted(0.25f,2000f);
        }

        static void SetUpEvents()
        {
            Cleanser.OnLoad();
            PotionManager.OnLoad(Menu);
            ItemManager.OnLoad(Menu);

            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Stealth.OnStealth += Stealth_OnStealth;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnPlayAnimation += Obj_AI_Hero_OnPlayAnimation;
            GameObject.OnCreate += GameObject_OnCreate;
        }

        static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general.antigp") && _spells[SpellSlot.E].IsReady())
            {
                if (sender.IsEnemy && sender.Name == "Rengar_LeapSound.troy")
                {
                    var rengarEntity = HeroManager.Enemies.Find(h => h.ChampionName.Equals("Rengar") && h.IsValidTarget(_spells[SpellSlot.E].Range));
                    if (rengarEntity != null)
                    {
                        _spells[SpellSlot.E].Cast(rengarEntity);
                    }
                }
            }
        }

        static void Obj_AI_Hero_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            return;
            if (!sender.IsMe)
            {
                return;
            }
            //Console.WriteLine(args.Animation.ToString());
            if (args.Animation.Contains("Attack"))
            {
                LeagueSharp.Common.Utility.DelayAction.Add((25), () =>
                {
                    if (ObjectManager.Player.IsAttackingPlayer)
                    {
                        LeagueSharp.Common.Utility.DelayAction.Add((int)(ObjectManager.Player.AttackCastDelay * 1000 + 15), () => OrbwalkingAfterAttack(ObjectManager.Player, Orbwalker.GetTarget()));
                    }
                });
            }
        }

        static void Stealth_OnStealth(Stealth.OnStealthEventArgs obj)
        {
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general.reveal"))
            {
                if (obj.Sender.Distance(ObjectManager.Player) <= 600f && obj.IsStealthed)
                {
                    if (Items.HasItem(2043) && Items.CanUseItem(2043))
                    {
                        Items.UseItem(2043,obj.Sender.ServerPosition.Extend(ObjectManager.Player.ServerPosition, 400f));
                    }
                }
            }
        }
        static void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harrass();
                    break;

            }
            OnUpdateFunctions();
        }

        static private void OnUpdateFunctions()
        {
            if (Environment.TickCount - _lastCheckTick < 150)
            {
                return;
            }
            _lastCheckTick = Environment.TickCount;
            #region Auto E
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.autoe") && _spells[SpellSlot.E].IsReady())
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
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general.specialfocus"))
            {
                var target = HeroManager.Enemies.Find(en => en.IsValidTarget(ObjectManager.Player.AttackRange) && en.Has2WStacks());
                if (target != null)
                {
                   Orbwalker.ForceTarget(target);
                   Hud.SelectedUnit = target;
                }
            }
            #endregion

            #region Disable AA Stealth
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble.noaastealth"))
            {
                Orbwalker.SetAttack(!Helpers.IsPlayerFaded());
            }
            #endregion

            #region Condemn KS
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.eks") && _spells[SpellSlot.E].IsReady())
            {
                var target = HeroManager.Enemies.Find(en => en.IsValidTarget(_spells[SpellSlot.E].Range) && en.Has2WStacks());
                if (target != null && target.Health + 60 <=(_spells[SpellSlot.E].GetDamage(target) + _spells[SpellSlot.W].GetDamage(target)) && (target is Obj_AI_Hero))
                {
                    _spells[SpellSlot.E].Cast(target);
                }
            }
            #endregion

            #region WallTumble
            if (Menu.Item("dz191.vhr.misc.tumble.walltumble").GetValue<KeyBind>().Active && _spells[SpellSlot.Q].IsReady())
            {
                WallTumble();
            }
            #endregion

            #region Low Life Peel

            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.lowlifepeel") && ObjectManager.Player.HealthPercentage() <= 20 && _spells[SpellSlot.E].IsReady())
            {
                var meleeEnemies = ObjectManager.Player.GetEnemiesInRange(350f).FindAll(m => m.IsMelee());
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

        }

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
                       //Notifications.AddNotification(new Notification("Condemned " + target.ChampionName, 1000));
                    }
                }
            }
        }

        private static void Harrass()
        {
            if (_spells[SpellSlot.E].IsEnabledAndReady(Mode.Harrass))
            {
                var possibleTarget = HeroManager.Enemies.Find(enemy => enemy.IsValidTarget(_spells[SpellSlot.E].Range) && enemy.Has2WStacks());
                if (possibleTarget != null && MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.ethird") && (possibleTarget is Obj_AI_Hero))
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

        private static void Farm()
        {
            if (!_spells[SpellSlot.Q].IsEnabledAndReady(Mode.Farm))
            {
                return;
            }
            var minionsInRange = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Player.AttackRange).FindAll(m => m.Health <= Player.GetAutoAttackDamage(m) + _spells[SpellSlot.Q].GetDamage(m)).ToList();
            if (!minionsInRange.Any())
            {
                return;
            }
            if (minionsInRange.Count > 1)
            {
                var firstMinion = minionsInRange.OrderBy(m => m.HealthPercentage()).First();
                CastTumble(firstMinion);
                Orbwalker.ForceTarget(firstMinion);
            }
        }

        static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {

            if (!(target is Obj_AI_Base) || !unit.IsMe)
            {
                return;
            }
            var tg = (Obj_AI_Base) target;
            if (MenuHelper.getKeybindValue("dz191.vhr.misc.condemn.enextauto") &&
                _spells[SpellSlot.E].CanCast(tg) && (tg is Obj_AI_Hero))
            {
                _spells[SpellSlot.E].Cast(tg);
                Menu.Item("dz191.vhr.misc.condemn.enextauto").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle));
            }
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (_spells[SpellSlot.Q].IsEnabledAndReady(Mode.Combo))
                    {
                        CastQ(tg);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (_spells[SpellSlot.Q].IsEnabledAndReady(Mode.Harrass))
                    {
                        CastQ(tg);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Farm();
                    break;
                default:
                    return;
            }
        }


        static void Drawing_OnDraw(EventArgs args)
        {
            //PositionalHelper.DrawMyZone();
            //PositionalHelper.DrawSafeZone();
            //PositionalHelper.DrawAllyZone();
            //PositionalHelper.DrawEnemyZone();
            //PositionalHelper.DrawIntersection();
            var drawE = Menu.Item("VayneDrawE").GetValue<Circle>();
            var midWallQPos = new Vector2(6707.485f, 8802.744f);
            var drakeWallQPos = new Vector2(11514, 4462);
            if (drawE.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position,_spells[SpellSlot.E].Range,drawE.Color);
            }
            if (MenuHelper.isMenuEnabled("dz191.vhr.drawing.drawstun"))
            {
                Obj_AI_Hero myTarget;
                /**
                if (CondemnCheck(ObjectManager.Player.ServerPosition, out myTarget))
                {
                    if (myTarget != null)
                    {
                        Drawing.DrawText(myTarget.Position.X, myTarget.Position.Y, Color.Aqua, "Stunnable!");
                    }
                }
                 * */
            }
            if (MenuHelper.isMenuEnabled("dz191.vhr.drawing.drawspots"))
            {
                if (ObjectManager.Player.Distance(midWallQPos) <= 1500f)
                {
                    Render.Circle.DrawCircle(midWallQPos.To3D2(), 65f, Color.AliceBlue);
                }
                if (ObjectManager.Player.Distance(drakeWallQPos) <= 1500f)
                {
                    Render.Circle.DrawCircle(drakeWallQPos.To3D2(), 65f, Color.AliceBlue);
                }
            }
           
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general.antigp"))
            {
                if (gapcloser.Sender.IsValidTarget(_spells[SpellSlot.E].Range) && gapcloser.End.Distance(ObjectManager.Player.ServerPosition) <= 375f && (gapcloser.Sender is Obj_AI_Hero))
                {
                    _spells[SpellSlot.E].Cast(gapcloser.Sender);
                }
            }
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.general.interrupt"))
            {
                if (args.DangerLevel == Interrupter2.DangerLevel.High && sender.IsValidTarget(_spells[SpellSlot.E].Range) && (sender is Obj_AI_Hero))
                {
                    _spells[SpellSlot.E].Cast(sender);
                }
            }
        }
        

        #region Tumble Region
        static private void CastQ(Obj_AI_Base target)
        {
            if (Environment.TickCount - _spells[SpellSlot.E].LastCastAttemptT <= 265)
            {
                return;
            }
            var myPosition = Game.CursorPos;
            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble.smartq") && _spells[SpellSlot.E].IsReady()) 
            {
                const int currentStep = 30;
                var direction = ObjectManager.Player.Direction.To2D().Perpendicular();
                for (var i = 0f; i < 360f; i += currentStep)
                {
                    var angleRad = Geometry.DegreeToRadian(i);
                    var rotatedPosition = ObjectManager.Player.Position.To2D() + (300 * direction.Rotated(angleRad));
                    Obj_AI_Hero myTarget;
                    if (CondemnCheck(rotatedPosition.To3D(), out myTarget) && Helpers.OkToQ(rotatedPosition.To3D()))
                    {
                        myPosition = rotatedPosition.To3D();
                        break;
                    }
                }
            }
            if (_spells[SpellSlot.R].IsEnabledAndReady(Mode.Combo) && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) && ObjectManager.Player.CountEnemiesInRange(Orbwalking.GetRealAutoAttackRange(null)) >= MenuHelper.getSliderValue("dz191.vhr.combo.r.minenemies"))
            {
                _spells[SpellSlot.R].Cast();
            }
            CastTumble(myPosition,target);
        }

        static void CastTumble(Obj_AI_Base target)
        {
            if (!_spells[SpellSlot.Q].IsReady())
            {
                return;
            }
            var posAfterTumble =
                ObjectManager.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if ((distanceAfterTumble < 550 * 550 && distanceAfterTumble > 100 * 100) || (MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble.qspam")))
            {
                if (!Helpers.OkToQ2(posAfterTumble) && MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble.noqenemies"))
                {
                    if(!(MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble.qspam")))
                    {
                          return;
                    }
                }
                _spells[SpellSlot.Q].Cast(Game.CursorPos);
            }
        }
        static void CastTumble(Vector3 pos, Obj_AI_Base target)
        {
            if (!_spells[SpellSlot.Q].IsReady())
            {
                return;
            }
            var posAfterTumble =
                ObjectManager.Player.ServerPosition.To2D().Extend(pos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if ((distanceAfterTumble < 550 * 550 && distanceAfterTumble > 100 * 100) || (MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble.qspam")))
            {
                if (!Helpers.OkToQ2(posAfterTumble) && MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble.noqenemies"))
                {
                    if(!(MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble.qspam")))
                    {
                          return;
                    }
                }
                _spells[SpellSlot.Q].Cast(pos);
            }
        }

        #endregion

        #region E Region

        static bool CondemnCheck(Vector3 fromPosition, out Obj_AI_Hero tg)
        {
            if ((fromPosition.UnderTurret(true) && MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.noeturret"))|| !_spells[SpellSlot.E].IsReady())
            {
                tg = null;
                return false;
            }
            if (
                !HeroManager.Enemies.Any(
                    h =>
                        h.IsValidTarget(_spells[SpellSlot.E].Range) && !h.HasBuffOfType(BuffType.SpellShield) &&
                        !h.HasBuffOfType(BuffType.SpellImmunity)))
            {
                tg = null;
                return false;
            }
            switch (Menu.Item("dz191.vhr.misc.condemn.condemnmethod").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    //VHReborn Condemn Code
                    foreach (var target in HeroManager.Enemies.Where(h => h.IsValidTarget(_spells[SpellSlot.E].Range) && !h.HasBuffOfType(BuffType.SpellShield) && !h.HasBuffOfType(BuffType.SpellImmunity)))
                    {
                        var pushDistance = MenuHelper.getSliderValue("dz191.vhr.misc.condemn.pushdistance");
                        var targetPosition = _spells[SpellSlot.E].GetPrediction(target).UnitPosition;
                        var finalPosition = targetPosition.Extend(fromPosition, -pushDistance);
                        var numberOfChecks = (float)Math.Ceiling(pushDistance / 30f);
                        for (var i = 1; i <= 30; i++)
                        {
                            var v3 = (targetPosition - fromPosition).Normalized();
                            var extendedPosition = targetPosition + v3 * (numberOfChecks * i); 
                            //var extendedPosition2 = targetPosition.Extend(fromPosition, -(i * target.BoundingRadius + target.BoundingRadius/4));
                            //var extendedPosition3 = targetPosition.Extend(fromPosition, -(i * target.BoundingRadius - target.BoundingRadius / 4));
                            var underTurret = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.condemnturret") && (Helpers.UnderAllyTurret(finalPosition) || Helpers.IsFountain(finalPosition));
                            var j4Flag = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.condemnflag") && (Helpers.IsJ4FlagThere(extendedPosition, target));
                            if ((extendedPosition.IsWall() || j4Flag) && (target.Path.Count() < 2) && !target.IsDashing())
                            {
                                if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.onlystuncurrent") &&
                                    !target.Equals(Orbwalker.GetTarget()))
                                {
                                    tg = null;
                                    return false;
                                }
                                if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.trinketbush") &&
                                    NavMesh.IsWallOfGrass(extendedPosition,25) && trinketSpell != null)
                                {
                                    var wardPosition = ObjectManager.Player.ServerPosition.Extend(
                                        extendedPosition,
                                        ObjectManager.Player.ServerPosition.Distance(extendedPosition) - 25f);
                                    LeagueSharp.Common.Utility.DelayAction.Add(250,() => trinketSpell.Cast(wardPosition));
                                }
                                CondemnNotification.Text = "Condemned " + target.ChampionName;
                                _predictedEndPosition = extendedPosition;
                                _predictedPosition = targetPosition;
                                //Notifications.AddNotification(condemnNotification);
                                tg = target;
                                return true;
                            }
                        }
                    }
                    break;
                case 1:
                    //Marksman/Gosu Condemn Code
                    foreach (var target in HeroManager.Enemies.Where(h => h.IsValidTarget(_spells[SpellSlot.E].Range)))
                    {
                        var pushDistance = MenuHelper.getSliderValue("dz191.vhr.misc.condemn.pushdistance");
                        var targetPosition = _spells[SpellSlot.E].GetPrediction(target).UnitPosition;
                        var finalPosition = targetPosition.Extend(fromPosition, -pushDistance);
                        var finalPosition2 = targetPosition.Extend(fromPosition, -(pushDistance/2f));
                        var underTurret = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.condemnturret") && (finalPosition.UnderTurret(false) || Helpers.IsFountain(finalPosition));
                        var j4Flag = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.condemnflag") && (Helpers.IsJ4FlagThere(finalPosition, target) || Helpers.IsJ4FlagThere(finalPosition2, target));
                        if (finalPosition.IsWall() || finalPosition2.IsWall() || underTurret || j4Flag)
                        {
                            if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.onlystuncurrent") &&
                                    !target.Equals(Orbwalker.GetTarget()))
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
                        int pushDist = Menu.Item("dz191.vhr.misc.condemn.pushdistance").GetValue<Slider>().Value;
                        for (int i = 0; i < pushDist; i += (int)en.BoundingRadius)
                        {
                            Vector3 loc3 = ePred.UnitPosition.To2D().Extend(fromPosition.To2D(), -i).To3D();
                            var orTurret = MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.condemnturret") && Helpers.UnderAllyTurret(loc3);
                            if (loc3.IsWall() || orTurret)
                            {
                                if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.onlystuncurrent") &&
                                    !en.Equals(Orbwalker.GetTarget()))
                                {
                                    tg = null;
                                    return false;
                                }
                                if (MenuHelper.isMenuEnabled("dz191.vhr.misc.condemn.trinketbush") &&
                                    NavMesh.IsWallOfGrass(loc3, 25) && trinketSpell != null)
                                {
                                    var wardPosition = ObjectManager.Player.ServerPosition.Extend(
                                        loc3,
                                        ObjectManager.Player.ServerPosition.Distance(loc3) - 25f);
                                    LeagueSharp.Common.Utility.DelayAction.Add(250, () => trinketSpell.Cast(wardPosition));
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

        static bool CondemnBeta(Vector3 fromPosition, out Obj_AI_Hero tg)
        {
            foreach (var target in from target in HeroManager.Enemies.Where(
                h =>
                    h.IsValidTarget(_spells[SpellSlot.E].Range) && !h.HasBuffOfType(BuffType.SpellShield) &&
                    !h.HasBuffOfType(BuffType.SpellImmunity)) let pushDistance = MenuHelper.getSliderValue("dz191.vhr.misc.condemn.pushdistance") let targetPosition = _spells[SpellSlot.E].GetPrediction(target).UnitPosition let finalPosition = targetPosition.Extend(fromPosition, -pushDistance) let condemnRectangle = new Polygon(Polygon.Rectangle(targetPosition.To2D(), finalPosition.To2D(), target.BoundingRadius)) where condemnRectangle.Points.Any(point => point.IsWall()) select target) {
                        tg = target;
                        return true;
                    }
            tg = null;
            return false;
        }
        #endregion

        #region WallTumble
        static void WallTumble()
        {
            Vector2 midWallQPos = new Vector2(6707.485f, 8802.744f);
            Vector2 drakeWallQPos = new Vector2(11514, 4462);
            if (Player.Distance(midWallQPos) >= Player.Distance(drakeWallQPos))
            {

                if (Player.Position.X < 12000 || Player.Position.X > 12070 || Player.Position.Y < 4800 ||
                    Player.Position.Y > 4872)
                {
                    Helpers.MoveToLimited(new Vector2(12050, 4827).To3D());
                }
                else
                {
                    Helpers.MoveToLimited(new Vector2(12050, 4827).To3D());
                    _spells[SpellSlot.Q].Cast(drakeWallQPos, true);
                }
            }
            else
            {
                if (Player.Position.X < 6908 || Player.Position.X > 6978 || Player.Position.Y < 8917 ||
                    Player.Position.Y > 8989)
                {
                    Helpers.MoveToLimited(new Vector2(6958, 8944).To3D());
                }
                else
                {
                    Helpers.MoveToLimited(new Vector2(6958, 8944).To3D());
                    _spells[SpellSlot.Q].Cast(midWallQPos, true);
                }
            }
        }

        #endregion


    }
}
