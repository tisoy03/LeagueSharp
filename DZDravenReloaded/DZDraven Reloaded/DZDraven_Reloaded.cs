using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace DZDraven_Reloaded
{
    internal class DZDraven_Reloaded
    {
        private static String _champName = "Draven";
        public static Menu Menu;
        public static Spell Q, W, E, R;
        private static xSLxOrbwalker _xSLx;
        private static Obj_AI_Hero _player;
        private static readonly List<PossibleReticle> Axes = new List<PossibleReticle>();

        public static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (_player.ChampionName != "Draven")
            {
                return;
            }

            Menu = new Menu("Draven Reloaded", "DravenReloaded", true);
            var xSLxMenu = new Menu("Orbwalker", "Orbwalker1");
            xSLxOrbwalker.AddToMenu(xSLxMenu);
            Menu.AddSubMenu(xSLxMenu);
            var ts = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(ts);
            Menu.AddSubMenu(ts);

            Menu.AddSubMenu(new Menu("[Draven]Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q Combo").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseWC", "Use W Combo").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseEC", "Use E Combo").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseRC", "Use R Combo").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("QManaC", "Q Mana in Combo").SetValue(new Slider(30, 1)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("WManaC", "W Mana in Combo").SetValue(new Slider(25, 1)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("EManaC", "E Mana in Combo").SetValue(new Slider(20, 1)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("RManaC", "R Mana in Combo").SetValue(new Slider(5, 1)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CAC", "Catch Axes Combo").SetValue(true));

            Menu.AddSubMenu(new Menu("[Draven]Harrass", "Harrass"));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseQH", "Use Q Harrass").SetValue(true));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseWH", "Use W Harrass").SetValue(true));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseEH", "Use E Harrass").SetValue(true));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseRH", "Use R Harrass").SetValue(true));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("QManaH", "Q Mana in Harrass").SetValue(new Slider(30, 1)));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("WManaH", "W Mana in Harrass").SetValue(new Slider(25, 1)));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("EManaH", "E Mana in Harrass").SetValue(new Slider(20, 1)));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("RManaH", "R Mana in Harrass").SetValue(new Slider(5, 1)));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("CAH", "Catch Axes Harrass").SetValue(true));

            Menu.AddSubMenu(new Menu("[Draven]Farm", "Farm"));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseQLC", "Use Q Laneclear").SetValue(true));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseELC", "Use E Laneclear").SetValue(true));
            Menu.SubMenu("Farm").AddItem(new MenuItem("QManaLC", "Q Mana in Laneclear").SetValue(new Slider(25, 1)));
            Menu.SubMenu("Farm").AddItem(new MenuItem("EManaLC", "E Mana in Laneclear").SetValue(new Slider(25, 1)));
            Menu.SubMenu("Farm").AddItem(new MenuItem("CAF", "Catch Axes Farm").SetValue(true));

            Menu.AddSubMenu(new Menu("[Draven]Axe Settings", "Axes"));
            Menu.SubMenu("Axes").AddItem(new MenuItem("MaxAxeC", "Max Axe Number Combo").SetValue(new Slider(2, 1, 4)));
            Menu.SubMenu("Axes")
                .AddItem(new MenuItem("MaxAxeH", "Max Axe Number Harrass").SetValue(new Slider(2, 1, 4)));
            Menu.SubMenu("Axes").AddItem(new MenuItem("MaxAxeF", "Max Axe Number Farm").SetValue(new Slider(2, 1, 4)));
            Menu.SubMenu("Axes")
                .AddItem(new MenuItem("CatchRadius", "Axe catch radius").SetValue(new Slider(600, 200, 1000)));
            Menu.SubMenu("Axes").AddItem(new MenuItem("SafeZone", "Axe Safezone").SetValue(new Slider(125, 0, 325)));
            Menu.AddSubMenu(new Menu("[Draven]Misc", "Misc"));
            Menu.SubMenu("Misc")
                .AddItem(
                    new MenuItem("ManualR", "Manual R Cast").SetValue(
                        new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Packets", "Packet Casting").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("AntiGP", "Anti Gapcloser")).SetValue(true);
            Menu.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "Interrupter").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("WCatch", "Use W to catch axes").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("WCatchCombo", "Only in combo").SetValue(false));

            Menu.AddSubMenu(new Menu("[Draven] Items", "Items"));
            Menu.SubMenu("Items").AddItem(new MenuItem("BotrkC", "Botrk Combo").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("BotrkH", "Botrk Harrass").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("YoumuuC", "Youmuu Combo").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("YoumuuH", "Youmuu Harrass").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("BilgeC", "Cutlass Combo").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("BilgeH", "Cutlass Harrass").SetValue(false));
            Menu.SubMenu("Items")
                .AddItem(new MenuItem("OwnHPercBotrk", "Min Own H. % Botrk").SetValue(new Slider(50, 1)));
            Menu.SubMenu("Items")
                .AddItem(new MenuItem("EnHPercBotrk", "Min Enemy H. % Botrk").SetValue(new Slider(20, 1)));

            Menu.AddSubMenu(new Menu("[Draven] QSS", "QSSMenu"));
            Menu.SubMenu("QSSMenu").AddItem(new MenuItem("UseQSS", "Use QSS").SetValue(true));
            Menu.AddSubMenu(new Menu("[Draven] QSS Buff Types", "QSST"));
            Cleanser.CreateTypeQssMenu();
            Menu.AddSubMenu(new Menu("[Draven] QSS Spells", "QSSSpell"));
            Cleanser.CreateQssSpellMenu();

            Menu.AddSubMenu(new Menu("[Draven]Drawings", "Drawing"));

            //Drawings Menu
            Menu.SubMenu("Drawing")
                .AddItem(new MenuItem("DrawE", "Draw E range").SetValue(new Circle(true, Color.MediumPurple)));
            Menu.SubMenu("Drawing")
                .AddItem(new MenuItem("DrawCRange", "Draw CatchRange").SetValue(new Circle(true, Color.RoyalBlue)));
            Menu.SubMenu("Drawing")
                .AddItem(new MenuItem("DrawRet", "Draw Reticles").SetValue(new Circle(true, Color.Yellow)));
            Menu.SubMenu("Drawing")
                .AddItem(new MenuItem("DrawNextRet", "Draw Next Ret to catch").SetValue(new Circle(true, Color.Orange)));
            Menu.AddToMainMenu();

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 20000);
            E.SetSkillshot(250f, 130f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            xSLxOrbwalker.AfterAttack += XSlxOrbwalkerAfterAttack;
            Game.PrintChat("<font color='#FF0000'>DZDraven</font> ReLoaded!");
            Game.PrintChat(
                "By <font color='#FF0000'>DZ</font><font color='#FFFFFF'>191</font>. Special Thanks to: Lexxes");
            Game.PrintChat("If you like my assemblies feel free to donate me (link on the forum :) )");
        }

        private static void XSlxOrbwalkerAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            //Game.PrintChat("Registered");
            if (!(target is Obj_AI_Hero))
            {
                return;
            }

            if (!unit.IsMe || !target.IsValidTarget())
            {
                return;
            }

            _player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            CastW();
            CastItems((Obj_AI_Hero) target);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_player.IsDead)
            {
                return;
            }

            MinionThere();
            if (Axes.Count == 0)
            {
                xSLxOrbwalker.CustomOrbwalkMode = false;
            }

            //Game.PrintChat(hasWBuff().ToString());
            var target = TargetSelector.GetTarget(
                xSLxOrbwalker.GetAutoAttackRange(), TargetSelector.DamageType.Physical);
            var etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var rTarget = TargetSelector.GetTarget(2000f, TargetSelector.DamageType.Physical);
            CatchAxes();
            Cleanser.CleanserBySpell();
            Cleanser.CleanserByBuffType();

            if (target.IsValidTarget())
            {
                CastQ();
            }

            if (etarget.IsValidTarget())
            {
                CastE(etarget);
            }

            if (rTarget.IsValidTarget())
            {
                CastRExecute(rTarget);
            }

            if (xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.LaneClear)
            {
                EFarmCheck();
            }

            if (rTarget.IsValidTarget() && Menu.Item("ManualR").GetValue<KeyBind>().Active)
            {
                RExecute(rTarget);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawCatch = Menu.Item("DrawCRange").GetValue<Circle>();
            var radius = Menu.Item("CatchRadius").GetValue<Slider>().Value;
            if (drawCatch.Active)
            {
                Utility.DrawCircle(Game.CursorPos, radius, drawCatch.Color);
            }

            var drawE = Menu.Item("DrawE").GetValue<Circle>();
            if (drawE.Active)
            {
                Utility.DrawCircle(_player.Position, E.Range, drawE.Color);
            }

            var drawRet = Menu.Item("DrawRet").GetValue<Circle>();
            var drawNextRet = Menu.Item("DrawNextRet").GetValue<Circle>();
            bool shouldUseW;
            var nextRet = GetClosestAxe(out shouldUseW);

            if (drawRet.Active && Axes.Count > 0)
            {
                foreach (var r in Axes.Where(ret => Game.CursorPos.Distance(ret.Position) <= radius && ret != nextRet))
                {
                    Utility.DrawCircle(r.Position, 100f, drawRet.Color);
                }
            }

            if (drawNextRet.Active && nextRet != null)
            {
                Utility.DrawCircle(nextRet.Position, 100f, drawNextRet.Color);
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gpSender = gapcloser.Sender;
            if (!IsMenuEnabled("AntiGP") || !E.IsReady() || !gpSender.IsValidTarget())
            {
                return;
            }

            CastEHitchance(gpSender);
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            var sender = (Obj_AI_Hero) unit;
            if (!IsMenuEnabled("Interrupt") || !E.IsReady() || !sender.IsValidTarget())
            {
                return;
            }

            CastEHitchance(sender);
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Q_reticle_self"))
            {
                return;
            }

            var ret = Axes.Where(a => a.NetworkId == sender.NetworkId);
            foreach (var axe in ret)
            {
                Axes.Remove(axe);
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Q_reticle_self"))
            {
                return;
            }

            Axes.Add(new PossibleReticle(sender));
        }

        public static PossibleReticle GetClosestAxe(out bool useW)
        {
            if (Axes.Count <= 0)
            {
                useW = false;
                return null;
            }

            var catchRange = Menu.Item("CatchRadius").GetValue<Slider>().Value;
            var UseW = IsMenuEnabled("WCatch");

            bool shouldUseW;
            if ((IsMenuEnabled("WCatchCombo") && xSLxOrbwalker.CurrentMode != xSLxOrbwalker.Mode.Combo))
            {
                UseW = false;
            }

            var Axe =
                Axes.Where(axe => axe.AxeGameObject.IsValid && axe.Position.Distance(Game.CursorPos) <= catchRange)
                    .OrderBy(axe => axe.Distance())
                    .First();
            if (Axe.CanCatch(UseW, out shouldUseW))
            {
                useW = shouldUseW;
                return Axe;
            }

            useW = false;
            return null;
        }

        public static void CatchAxes()
        {
            bool shouldUseWForIt;
            //Game.PrintChat("I'm Combo");
            if (Axes.Count == 0)
            {
                xSLxOrbwalker.CustomOrbwalkMode = false;
                return;
            }

            var axe = GetClosestAxe(out shouldUseWForIt);

            if (axe == null)
            {
                xSLxOrbwalker.CustomOrbwalkMode = false;
                return;
            }

            //  if (shouldUseWForIt) { xSLxOrbwalker.SetAttack(false); } else { xSLxOrbwalker.SetAttack(true);}
            switch (xSLxOrbwalker.CurrentMode)
            {
                case xSLxOrbwalker.Mode.Combo:
                    var catchCombo = IsMenuEnabled("CAC");
                    if (!catchCombo)
                    {
                        return;
                    }

                    Catch(shouldUseWForIt, axe);
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    var catchHarass = IsMenuEnabled("CAH");
                    if (!catchHarass)
                    {
                        return;
                    }

                    Catch(shouldUseWForIt, axe);
                    break;
                case xSLxOrbwalker.Mode.Lasthit:
                    var catchLastHit = IsMenuEnabled("CAF");
                    if (!catchLastHit)
                    {
                        return;
                    }

                    Catch(shouldUseWForIt, axe);
                    break;
                case xSLxOrbwalker.Mode.LaneClear:
                    var catchLaneClear = IsMenuEnabled("CAF");
                    if (!catchLaneClear)
                    {
                        return;
                    }

                    Catch(shouldUseWForIt, axe);
                    break;
            }
        }

        public static void CastEHitchance(Obj_AI_Hero target)
        {
            var pred = E.GetPrediction(target);
            if (pred.Hitchance >= HitChance.Medium)
            {
                E.Cast(target, IsMenuEnabled("Packets"));
            }
        }

        public static void Catch(bool shouldUseWForIt, PossibleReticle axe)
        {
            if (shouldUseWForIt && W.IsReady() && !axe.IsCatchingNow())
            {
                W.Cast();
            }

            xSLxOrbwalker.CustomOrbwalkMode = true;
            xSLxOrbwalker.Orbwalk(
                PosAfterRange(axe.Position, Game.CursorPos, 49 + _player.BoundingRadius / 2),
                xSLxOrbwalker.GetPossibleTarget());
        }

        public static void CastQ()
        {
            switch (xSLxOrbwalker.CurrentMode)
            {
                case xSLxOrbwalker.Mode.Combo:
                    if (!IsMenuEnabled("UseQC"))
                    {
                        return;
                    }

                    var manaQCombo = Menu.Item("QManaC").GetValue<Slider>().Value;
                    var qMax = Menu.Item("MaxAxeC").GetValue<Slider>().Value;
                    if (GetPerValue(true) >= manaQCombo && GetQStacks() + 1 <= qMax)
                    {
                        Q.Cast(IsMenuEnabled("Packets"));
                    }
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    if (!IsMenuEnabled("UseQC"))
                    {
                        return;
                    }

                    var manaQHarass = Menu.Item("QManaH").GetValue<Slider>().Value;
                    var qMaxH = Menu.Item("MaxAxeH").GetValue<Slider>().Value;
                    if (GetPerValue(true) >= manaQHarass && GetQStacks() + 1 <= qMaxH)
                    {
                        Q.Cast(IsMenuEnabled("Packets"));
                    }
                    break;
                case xSLxOrbwalker.Mode.Lasthit:
                    if (!IsMenuEnabled("UseQF"))
                    {
                        return;
                    }

                    var manaQlh = Menu.Item("QManaLH").GetValue<Slider>().Value;
                    var qMaxLh = Menu.Item("MaxAxeF").GetValue<Slider>().Value;
                    if (GetPerValue(true) >= manaQlh && GetQStacks() + 1 <= qMaxLh && MinionThere())
                    {
                        Q.Cast(IsMenuEnabled("Packets"));
                    }
                    break;
                case xSLxOrbwalker.Mode.LaneClear:
                    if (!IsMenuEnabled("UseQLC"))
                    {
                        return;
                    }

                    var manaQlc = Menu.Item("QManaLC").GetValue<Slider>().Value;
                    var qlc = Menu.Item("MaxAxeF").GetValue<Slider>().Value;
                    if (GetPerValue(true) >= manaQlc && GetQStacks() + 1 <= qlc && MinionThere())
                    {
                        Q.Cast(IsMenuEnabled("Packets"));
                    }
                    break;
            }
        }

        private static void CastW()
        {
            if (HasWBuff() || !W.IsReady())
            {
                return;
            }

            switch (xSLxOrbwalker.CurrentMode)
            {
                case xSLxOrbwalker.Mode.Combo:
                    if (!IsMenuEnabled("UseWC"))
                    {
                        return;
                    }

                    var mwc = Menu.Item("WManaC").GetValue<Slider>().Value;
                    if (GetPerValue(true) >= mwc)
                    {
                        W.Cast(IsMenuEnabled("Packets"));
                    }
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    if (!IsMenuEnabled("UseWC"))
                    {
                        return;
                    }

                    var mwh = Menu.Item("WManaH").GetValue<Slider>().Value;
                    if (GetPerValue(true) >= mwh)
                    {
                        W.Cast(IsMenuEnabled("Packets"));
                    }
                    break;
            }
        }

        private static void CastE(Obj_AI_Hero target)
        {
            if (!E.IsReady() || !target.IsValidTarget())
            {
                return;
            }

            switch (xSLxOrbwalker.CurrentMode)
            {
                case xSLxOrbwalker.Mode.Combo:
                    if (!IsMenuEnabled("UseEC"))
                    {
                        return;
                    }

                    var mec = Menu.Item("EManaC").GetValue<Slider>().Value;
                    if (GetPerValue(true) >= mec)
                    {
                        CastEHitchance(target);
                    }
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    if (!IsMenuEnabled("UseEH"))
                    {
                        return;
                    }

                    var meh = Menu.Item("EManaH").GetValue<Slider>().Value;
                    if (GetPerValue(true) >= meh)
                    {
                        CastEHitchance(target);
                    }
                    break;
            }
        }

        private static void EFarmCheck()
        {
            if (!IsMenuEnabled("UseEF"))
            {
                return;
            }

            var minionsE = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);
            var eFarmLocation = E.GetLineFarmLocation(minionsE);
            var melc = Menu.Item("EManaLC").GetValue<Slider>().Value;
            if (GetPerValue(true) >= melc && eFarmLocation.MinionsHit > 2)
            {
                E.Cast(eFarmLocation.Position, IsMenuEnabled("Packets"));
            }
        }

        private static void CastRExecute(Obj_AI_Hero rTarget)
        {
            var pred = R.GetPrediction(rTarget);
            if (!rTarget.IsValidTarget() || pred.Hitchance < HitChance.Medium || !R.IsReady())
            {
                return;
            }

            switch (xSLxOrbwalker.CurrentMode)
            {
                case xSLxOrbwalker.Mode.Combo:
                    if (!IsMenuEnabled("UseRC"))
                    {
                        return;
                    }

                    var manaR = Menu.Item("RManaC").GetValue<Slider>().Value;
                    if (GetUnitsInPath(_player, rTarget, R) && GetPerValue(true) >= manaR &&
                        !_player.HasBuff("dravenrdoublecast", true))
                    {
                        R.Cast(rTarget, IsMenuEnabled("Packets"));
                    }
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    if (!IsMenuEnabled("UseRH"))
                    {
                        return;
                    }

                    var manaRh = Menu.Item("RManaH").GetValue<Slider>().Value;
                    if (GetUnitsInPath(_player, rTarget, R) && GetPerValue(true) >= manaRh &&
                        !_player.HasBuff("dravenrdoublecast", true))
                    {
                        R.Cast(rTarget, IsMenuEnabled("Packets"));
                    }
                    break;
            }
        }

        private static void RExecute(Obj_AI_Hero rTarget)
        {
            var pred = R.GetPrediction(rTarget);
            if (!rTarget.IsValidTarget() || pred.Hitchance < HitChance.Medium || !R.IsReady())
            {
                return;
            }

            if (GetUnitsInPath(_player, rTarget, R) && !_player.HasBuff("dravenrdoublecast", true))
            {
                R.Cast(rTarget, IsMenuEnabled("Packets"));
            }
        }

        private static void CastItems(Obj_AI_Hero tar)
        {
            var ownH = GetPerValue(false);
            if ((Menu.Item("BotrkC").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo) &&
                (Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
                ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= GetPerValueTarget(tar, false))))
            {
                UseItem(3153, tar);
            }

            if ((Menu.Item("BotrkH").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Harass) &&
                (Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
                ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= GetPerValueTarget(tar, false))))
            {
                UseItem(3153, tar);
            }

            if (Menu.Item("YoumuuC").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo)
            {
                UseItem(3142);
            }

            if (Menu.Item("YoumuuH").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Harass)
            {
                UseItem(3142);
            }

            if (Menu.Item("BilgeC").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo)
            {
                UseItem(3144, tar);
            }

            if (Menu.Item("BilgeH").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Harass)
            {
                UseItem(3144, tar);
            }
        }

        private static bool HasWBuff()
        {
            //dravenfurybuff
            //DravenFury
            return _player.HasBuff("DravenFury", true) || _player.HasBuff("dravenfurybuff", true);
        }

        public static bool MinionThere()
        {
            var list =
                MinionManager.GetMinions(_player.Position, xSLxOrbwalker.GetAutoAttackRange())
                    .Where(
                        m =>
                            HealthPrediction.GetHealthPrediction(
                                m, (int) (_player.Distance(m) / Orbwalking.GetMyProjectileSpeed()) * 1000) <=
                            Q.GetDamage(m) + _player.GetAutoAttackDamage(m))
                    .ToList();
            // Game.PrintChat("QDmg "+Q.GetDamage(List.FirstOrDefault()));

            return list.Count > 0;
        }

        public static Vector3 PosAfterRange(Vector3 p1, Vector3 finalp2, float range)
        {
            var pos2 = Vector3.Normalize(finalp2 - p1);

            return p1 + (pos2 * range);
        }

        public static int GetQStacks()
        {
            var buff = ObjectManager.Player.Buffs.FirstOrDefault(buff1 => buff1.Name.Equals("dravenspinningattack"));

            return buff != null ? buff.Count : 0;
        }

        #region Utility Methods

        public static bool IsMenuEnabled(String val)
        {
            return Menu.Item(val).GetValue<bool>();
        }

        private static float GetPerValue(bool mana)
        {
            if (mana)
            {
                return (_player.Mana / _player.MaxMana) * 100;
            }

            return (_player.Health / _player.MaxHealth) * 100;
        }

        private static float GetPerValueTarget(Obj_AI_Hero target, bool mana)
        {
            if (mana)
            {
                return (target.Mana / target.MaxMana) * 100;
            }

            return (target.Health / target.MaxHealth) * 100;
        }

        public static void UseItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }

        public static bool IsUnderEnTurret(Vector3 position)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(turr => turr.IsEnemy && (turr.Health != 0))
                    .Any(tur => tur.Distance(position) <= 975f);
        }

        private static bool GetUnitsInPath(Obj_AI_Hero player, Obj_AI_Hero target, Spell spell)
        {
            var distance = player.Distance(target);
            var minionList = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, spell.Range, MinionTypes.All, MinionTeam.NotAlly);
            var numberOfMinions = (from Obj_AI_Minion minion in minionList
                let skillshotPosition =
                    V2E(
                        player.Position,
                        V2E(
                            player.Position, target.Position,
                            Vector3.Distance(player.Position, target.Position) - spell.Width + 1).To3D(),
                        Vector3.Distance(player.Position, minion.Position))
                where skillshotPosition.Distance(minion) < spell.Width
                select minion).Count();
            var numberOfChamps = (from minion in ObjectManager.Get<Obj_AI_Hero>()
                let skillshotPosition =
                    V2E(
                        player.Position,
                        V2E(
                            player.Position, target.Position,
                            Vector3.Distance(player.Position, target.Position) - spell.Width + 1).To3D(),
                        Vector3.Distance(player.Position, minion.Position))
                where skillshotPosition.Distance(minion) < spell.Width && minion.IsEnemy
                select minion).Count();
            var totalUnits = numberOfChamps + numberOfMinions - 1;
            // total number of champions and minions the projectile will pass through.
            if (totalUnits == -1)
            {
                return false;
            }

            var damageReduction = ((totalUnits > 7)) ? 0.4 : (totalUnits == 0) ? 1.0 : (1 - ((totalUnits) / 12.5));
            // the damage reduction calculations minus percentage for each unit it passes through!

            return spell.GetDamage(target) * damageReduction >= (target.Health + (distance / 2000) * target.HPRegenRate);
            // - 15 is a safeguard for certain kill.
        }

        private static Vector2 V2E(Vector3 from, Vector3 direction, float distance)
        {
            return from.To2D() + distance * Vector3.Normalize(direction - from).To2D();
        }

        #endregion
    }

    internal class PossibleReticle
    {
        public GameObject AxeGameObject;
        public double CreationTime;
        public double EndTime;
        public int NetworkId;
        public Vector3 Position;

        public PossibleReticle(GameObject axe)
        {
            AxeGameObject = axe;
            NetworkId = axe.NetworkId;
            Position = axe.Position;
            CreationTime = Game.Time;
            EndTime = Game.Time + 1.20;
        }

        public bool CanCatch(bool useW, out bool shouldUseW)
        {
            var enemyHeroesCount =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        h =>
                            h.IsEnemy && h.IsValidTarget() &&
                            h.Distance(Position) <= DZDraven_Reloaded.Menu.Item("SafeZone").GetValue<Slider>().Value)
                    .ToList();
            if ((DZDraven_Reloaded.IsUnderEnTurret(Position) &&
                 !DZDraven_Reloaded.IsUnderEnTurret(ObjectManager.Player.Position)) || enemyHeroesCount.Count > 0)
            {
                shouldUseW = false;
                return false;
            }
            var distance = ObjectManager.Player.GetPath(Position).ToList().To2D().PathLength();
            var catchNormal = distance / ObjectManager.Player.MoveSpeed + Game.Time < EndTime;
                // Not buffed with W, Normal
            var additionalSpeed = (5 * DZDraven_Reloaded.W.Level + 35) * 0.01 * ObjectManager.Player.MoveSpeed;
            var catchBuff = distance / (ObjectManager.Player.MoveSpeed + additionalSpeed + Game.Time) < EndTime;
                //Buffed with W
            if (catchNormal)
            {
                shouldUseW = false;
                return catchNormal;
            }
            if (useW && !catchNormal && catchBuff)
            {
                shouldUseW = true;
                return catchBuff;
            }
            shouldUseW = false;

            return false;
        }

        public float Distance()
        {
            return Vector3.Distance(Position, ObjectManager.Player.Position);
        }

        public bool IsCatchingNow()
        {
            return Distance() < 49 + (ObjectManager.Player.BoundingRadius / 2) + 50; //Taken from PUC Draven
        }
    }
}