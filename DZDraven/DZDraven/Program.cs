using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace DZDraven
{
    internal class Program
    {
        public static String CharName = "Draven"; //Not Draven,Draaaaaaaaaaven
        public static List<Reticle> ReticleList = new List<Reticle>();
        public static List<Obj_AI_Turret> TowerPos = new List<Obj_AI_Turret>();
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static Menu Menu;
        public static bool IsCatching;
        public static float AutoRange = 550f;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != CharName)
            {
                return;
            }
            Game.PrintChat("DZDraven is Outdated! Please use DZDraven Reloaded!");
            return;

            Menu = new Menu("DZDraven", "DZdrvenMenu", true);
            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker1"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker1"));
            var ts = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(ts);
            Menu.AddSubMenu(ts);
            Menu.AddSubMenu(new Menu("[Draven]Skill Q", "QMenu"));
            //Q Menu

            Menu.SubMenu("QMenu").AddItem(new MenuItem("QC", "Use Q Combo").SetValue(true));

            Menu.SubMenu("QMenu").AddItem(new MenuItem("QM", "Use Q Mixed").SetValue(false));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QLH", "Use Q LastHit").SetValue(false));

            Menu.SubMenu("QMenu").AddItem(new MenuItem("QLC", "Use Q LaneClear").SetValue(false));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QKs", "Use Q Ks").SetValue(true));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("MaxQNum", "Max n of Q").SetValue(new Slider(2, 1, 4)));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("SafeZone", "BETA SafeZone").SetValue(new Slider(100, 0, 400)));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QRadius", "Catch Radius").SetValue(new Slider(600, 200, 800)));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QManaC", "Min Q Mana in Combo").SetValue(new Slider(10, 1)));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QManaM", "Min Q Mana in Mixed").SetValue(new Slider(10, 1)));
            //menu.SubMenu("QMenu").AddItem(new MenuItem("UseAARet", "Use AA while orbwalking to reticle").SetValue(true));
            Menu.SubMenu("QMenu")
                .AddItem(
                    new MenuItem("QRefresh", "Refresh List (if bug)").SetValue(
                        new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            Menu.AddSubMenu(new Menu("[Draven]Skill W", "WMenu"));

            //W Menu
            Menu.SubMenu("WMenu").AddItem(new MenuItem("WC", "Use W Combo").SetValue(true));
            Menu.SubMenu("WMenu").AddItem(new MenuItem("WM", "Use W Mixed").SetValue(true));
            Menu.SubMenu("WMenu").AddItem(new MenuItem("WLH", "Use W LastHit").SetValue(false));
            Menu.SubMenu("WMenu").AddItem(new MenuItem("WLC", "Use W LaneClear").SetValue(false));
            Menu.SubMenu("WMenu").AddItem(new MenuItem("WManaC", "Min W Mana in Combo").SetValue(new Slider(60, 1)));
            Menu.SubMenu("WMenu").AddItem(new MenuItem("WManaM", "Min W Mana in Mixed").SetValue(new Slider(60, 1)));


            Menu.AddSubMenu(new Menu("[Draven]Skill E", "EMenu"));

            //E Menu
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EC", "Use E Combo").SetValue(true));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EM", "Use E Mixed").SetValue(false));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EKs", "Use E Ks").SetValue(true));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EGapCloser", "Use E AntiGapcloser").SetValue(true));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EInterrupt", "Use E Interrupt").SetValue(true));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EManaC", "Min E Mana in Combo").SetValue(new Slider(20, 1)));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EManaM", "Min R Mana in Mixed").SetValue(new Slider(20, 1)));


            Menu.AddSubMenu(new Menu("[Draven]Skill R (2000un)", "RMenu"));

            //R Menu
            Menu.SubMenu("RMenu").AddItem(new MenuItem("RC", "Use R Combo").SetValue(false));
            Menu.SubMenu("RMenu").AddItem(new MenuItem("RM", "Use R Mixed").SetValue(false));
            Menu.SubMenu("RMenu").AddItem(new MenuItem("RKs", "Use R Ks").SetValue(true));
            //menu.SubMenu("RMenu").AddItem(new MenuItem("ManualR", "Manual R Cast").SetValue(new KeyBind("T".ToCharArray()[0],KeyBindType.Press)));
            Menu.SubMenu("RMenu").AddItem(new MenuItem("RManaC", "Min R Mana in Combo").SetValue(new Slider(5, 1)));
            Menu.SubMenu("RMenu").AddItem(new MenuItem("RManaM", "Min R Mana in Mixed").SetValue(new Slider(5, 1)));

            //Axe Catcher
            Menu.AddSubMenu(new Menu("[Draven]Axe Catcher", "AxeCatcher"));

            Menu.SubMenu("AxeCatcher").AddItem(new MenuItem("ACC", "AxeC Combo").SetValue(true));
            Menu.SubMenu("AxeCatcher").AddItem(new MenuItem("ACM", "AxeC Mixed").SetValue(true));
            Menu.SubMenu("AxeCatcher").AddItem(new MenuItem("ACLH", "Axe CLastHit").SetValue(true));
            Menu.SubMenu("AxeCatcher").AddItem(new MenuItem("ACLC", "AxeC LaneClear").SetValue(true));

            Menu.AddSubMenu(new Menu("[Draven]Items", "Items"));

            //Items Menu
            Menu.SubMenu("Items").AddItem(new MenuItem("BOTRK", "Use BOTRK").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("Youmuu", "Use Youmuu").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("SOTD", "Use SOTD if Oneshot").SetValue(true));


            Menu.AddSubMenu(new Menu("[Draven]Drawing", "Drawing"));

            //Drawings Menu
            Menu.SubMenu("Drawing")
                .AddItem(
                    new MenuItem("DrawCRange", "Draw CatchRange").SetValue(
                        new Circle(true, Color.FromArgb(80, 255, 0, 255))));
            Menu.SubMenu("Drawing")
                .AddItem(new MenuItem("DrawRet", "Draw Reticles").SetValue(new Circle(true, Color.Yellow)));


            Menu.AddToMainMenu();
            Game.PrintChat("DZDraven 1.23 Loaded.");
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 20000);
            E.SetSkillshot(250f, 130f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);
            CompileTowerArray();
            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
            AntiGapcloser.OnEnemyGapcloser += OnGapcloser;
            Interrupter.OnPossibleToInterrupt += OnInterruptCreate;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var qRadius = Menu.Item("QRadius").GetValue<Slider>().Value;
            var drawCatch = Menu.Item("DrawCRange").GetValue<Circle>();
            var drawRet = Menu.Item("DrawRet").GetValue<Circle>();
            if (drawCatch.Active)
            {
                Drawing.DrawCircle(Game.CursorPos, qRadius, drawCatch.Color);
            }

            if (!drawRet.Active)
            {
                return;
            }

            foreach (var r in ReticleList.Where(r => r.GetObj().IsValid))
            {
                Drawing.DrawCircle(r.GetPosition(), 100, drawRet.Color);
            }
        }

        private static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }

            var tar = (Obj_AI_Hero) target;
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (IsEn("QC"))
                    {
                        CastQ();
                    }

                    if (IsEn("WC") &&
                        (ObjectManager.Player.Buffs.FirstOrDefault(
                            buff => buff.Name == "dravenfurybuff" || buff.Name == "DravenFury") == null))
                    {
                        var wManaCombo = Menu.Item("WManaC").GetValue<Slider>().Value;
                        if (GetManaPer() >= wManaCombo)
                        {
                            W.Cast();
                        }
                    }

                    //Botrk
                    if (IsEn("BOTRK"))
                    {
                        UseItem(3153, (Obj_AI_Hero) target);
                    }

                    //Youmuu
                    if (IsEn("Youmuu"))
                    {
                        UseItem(3142);
                    }

                    //SOTD
                    if (IsEn("SOTD"))
                    {
                        var hasIe = Items.HasItem(3031);
                        var coeff = hasIe ? 2.5 : 2.0;
                        if ((Player.GetAutoAttackDamage(tar) * coeff * 3 >= target.Health))
                        {
                            UseItem(3131);
                            Orbwalker.ForceTarget(tar);
                        }
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (IsEn("QM"))
                    {
                        CastQ();
                    }

                    if (IsEn("WM") &&
                        (ObjectManager.Player.Buffs.FirstOrDefault(
                            buff => buff.Name == "dravenfurybuff" || buff.Name == "DravenFury") == null))
                    {
                        var wManaMix = Menu.Item("WManaM").GetValue<Slider>().Value;
                        if (GetManaPer() >= wManaMix)
                        {
                            W.Cast();
                        }
                    }
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    if (IsEn("QLH"))
                    {
                        CastQ();
                    }

                    if (IsEn("WLH") &&
                        (ObjectManager.Player.Buffs.FirstOrDefault(
                            buff => buff.Name == "dravenfurybuff" || buff.Name == "DravenFury") == null))
                    {
                        W.Cast();
                    }
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (IsEn("WLC") &&
                        (ObjectManager.Player.Buffs.FirstOrDefault(
                            buff => buff.Name == "dravenfurybuff" || buff.Name == "DravenFury") == null))
                    {
                        W.Cast();
                    }

                    if (IsEn("QLC"))
                    {
                        CastQ();
                    }
                    break;
                default:
                    return;
            }
        }

/*
        private static bool PlayerInTurretRange()
        {
            foreach (var val in TowerPos.Where(val => val.Health == 0)) 
            {
                TowerPos.Remove(val);
            }

            return TowerPos.Any(val => Player.Distance(val) < 975f);
        }
*/

        private static bool RetInTurretRange(Vector3 retPosition)
        {
            foreach (var val in TowerPos.Where(val => val.Health == 0))
            {
                TowerPos.Remove(val);
            }

            return TowerPos.Any(val => Vector3.Distance(retPosition, val.Position) < 975f);
        }

        private static void CompileTowerArray()
        {
            foreach (var tower in ObjectManager.Get<Obj_AI_Turret>().Where(tower => tower.IsEnemy))
            {
                TowerPos.Add(tower);
            }
        }

        private static bool IsZoneSafe(Vector3 v, float dist)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(enemy => enemy.IsEnemy)
                    .All(enemy => !(Vector3.Distance(enemy.Position, v) < dist) || enemy.IsDead);
        }

/*
        private static Obj_AI_Hero ClosestHero()
        {
             Obj_AI_Hero[] clhero = { null };
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy).Where(hero => !hero.IsDead && hero.IsVisible && Player.Distance(hero)<Player.Distance(clhero[0]))) {
                clhero[0] = hero;
            }
            return clhero[0];
        }
*/

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var safeZone = Menu.Item("SafeZone").GetValue<Slider>().Value;
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                if (IsEn("EKs"))
                {
                    if (E.GetHealthPrediction(hero) <= 0)
                    {
                        E.Cast(hero);
                        break;
                    }
                }

                if (IsEn("QKs"))
                {
                    if (Q.GetDamage(hero) + Player.GetAutoAttackDamage(hero) >= hero.Health)
                    {
                        if (GetQNumber() < 1)
                        {
                            Q.Cast();
                        }
                        Orbwalker.SetAttack(true);
                        Orbwalker.ForceTarget(hero);
                        break;
                    }
                }

                if (IsEn("RKs"))
                {
                    if (!(R.GetHealthPrediction(hero) <= 0) || !(Player.Distance(hero) <= 2000f))
                    {
                        continue;
                    }

                    R.Cast(hero);
                    break;
                }
            }

            if (Menu.Item("QRefresh").GetValue<KeyBind>().Active)
            {
                ReticleList.Clear();
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:

                    if (IsEn("EC"))
                    {
                        CastE(eTarget);
                    }

                    if (IsEn("RC"))
                    {
                        CastR(rTarget);
                    }

                    if (IsEn("ACC"))
                    {
                        OrbWalkToReticle(safeZone, 100);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (IsEn("EM"))
                    {
                        CastE(eTarget);
                    }

                    if (IsEn("RM"))
                    {
                        CastR(rTarget);
                    }

                    if (IsEn("ACM"))
                    {
                        OrbWalkToReticle(safeZone, 100);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    if (IsEn("ACLH"))
                    {
                        OrbWalkToReticle(safeZone, 100);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (IsEn("ACLC"))
                    {
                        OrbWalkToReticle(safeZone, 100);
                    }
                    break;
            }
        }

        public static void UseItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Q_reticle_self"))
            {
                return;
            }

            ReticleList.Add(new Reticle(sender, Game.Time, sender.Position, Game.Time + 1.20, sender.NetworkId));
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Q_reticle_self"))
            {
                return;
            }

            foreach (var ret in ReticleList)
            {
                if (Player.ServerPosition.Distance(ret.GetPosition()) <= 100 && ret.GetNetworkId() == sender.NetworkId)
                {
                    IsCatching = false;
                }

                if (ret.GetNetworkId() == sender.NetworkId)
                {
                    ReticleList.Remove(ret);
                }
            }
        }

        private static void OnGapcloser(ActiveGapcloser gapcloser)
        {
            if (!IsEn("EGapCloser"))
            {
                return;
            }

            if (!(gapcloser.End.Distance(Player.ServerPosition) <= 50f))
            {
                return;
            }

            var ePred = E.GetPrediction(gapcloser.Sender);
            if (ePred.Hitchance >= HitChance.Medium)
            {
                E.Cast(ePred.CastPosition);
            }
        }

        private static void OnInterruptCreate(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!IsEn("EInterrupt"))
            {
                return;
            }

            var ePred = E.GetPrediction(unit);
            if (ePred.Hitchance >= HitChance.Medium)
            {
                E.Cast(ePred.CastPosition);
            }
        }

/*
        private static bool IsInStandRange()
        {
            return (Vector3.Distance(Game.CursorPos,Player.Position)<220);
        }
*/

        /// <summary>
        ///     Orbwalker to catch the axes.
        /// </summary>
        /// <param name="safeZone">Heroes safe zone,def 100</param>
        /// <param name="retSafeZone">Reticle safe zone, def 100</param>
        private static void OrbWalkToReticle(int safeZone, int retSafeZone)
        {
            Reticle closestRet = null;
            var qRadius = Menu.Item("QRadius").GetValue<Slider>().Value;
            foreach (var r in ReticleList.Where(r => !r.GetObj().IsValid))
            {
                ReticleList.Remove(r);
            }

            if (ReticleList.Count > 0)
            {
                float[] closestDist = { float.MaxValue };

                foreach (var r in
                    ReticleList.Where(
                        r =>
                            r.GetPosition().Distance(Game.CursorPos) <= qRadius &&
                            Player.Distance(r.GetPosition()) < closestDist[0])
                        .Where(r => IsZoneSafe(r.GetPosition(), retSafeZone) && IsZoneSafe(Player.Position, safeZone)))
                {
                    closestRet = r;
                    closestDist[0] = Player.Distance(r.GetPosition());
                }
            }

            if (closestRet == null || RetInTurretRange(closestRet.GetPosition()))
            {
                return;
            }

            var qDist1 = Player.GetPath(closestRet.GetPosition()).ToList().To2D().PathLength();
            var canReachRet = (qDist1 / Player.MoveSpeed + Game.Time) < (closestRet.GetEndTime());
            var canReachRetWBonus = (qDist1 / (Player.MoveSpeed + (Player.MoveSpeed * (GetMoveSpeedBonusW() / 100))) +
                                     Game.Time) < (closestRet.GetEndTime());
            var wNeeded = false;
            if (canReachRetWBonus && !canReachRet)
            {
                W.Cast();
                wNeeded = true;
            }
            if ((!canReachRet && !wNeeded))
            {
                return;
            }

            if (Player.Distance(closestRet.GetPosition()) >= 100)
            {
                Orbwalker.SetOrbwalkingPoint(
                    closestRet.GetPosition() != Game.CursorPos ? closestRet.GetPosition() : Game.CursorPos);
            }

            Console.WriteLine("Orbwalking to " + closestRet.GetPosition());
        }

        public static int GetMoveSpeedBonusW()
        {
            switch (W.Level)
            {
                case 1:
                    return 40;
                case 2:
                    return 45;
                case 3:
                    return 50;
                case 4:
                    return 55;
                case 5:
                    return 60;
                default:
                    return 0;
            }
        }

        public static void CastE(Obj_AI_Base unit)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var eManaCombo = Menu.Item("EManaC").GetValue<Slider>().Value;
                    if ((GetManaPer() >= eManaCombo))
                    {
                        E.Cast(unit);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var eManaMix = Menu.Item("EManaM").GetValue<Slider>().Value;
                    if ((GetManaPer() >= eManaMix))
                    {
                        E.Cast(unit);
                    }
                    break;
            }
        }

        public static void CastR(Obj_AI_Base unit)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var rManaCombo = Menu.Item("RManaC").GetValue<Slider>().Value;
                    if ((GetManaPer() >= rManaCombo) && Player.Distance(unit) < 2000f)
                    {
                        R.Cast(unit);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var rManaMix = Menu.Item("RManaM").GetValue<Slider>().Value;
                    if ((GetManaPer() >= rManaMix) && Player.Distance(unit) < 2000f)
                    {
                        R.Cast(unit);
                    }
                    break;
            }
        }

        public static void CastQ()
        {
            var qNumberOnPlayer = GetQNumber();
            if (ReticleList.Count + 1 > Menu.Item("MaxQNum").GetValue<Slider>().Value)
            {
                return;
            }

            if (qNumberOnPlayer > 2)
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var qManaCombo = Menu.Item("QManaC").GetValue<Slider>().Value;
                    if (GetManaPer() >= qManaCombo)
                    {
                        Q.Cast();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var qManaMix = Menu.Item("QManaM").GetValue<Slider>().Value;
                    if (GetManaPer() >= qManaMix)
                    {
                        Q.Cast();
                    }
                    break;
                default:
                    Q.Cast();
                    break;
            }
        }

        public static int GetQNumber()
        {
            var buff = ObjectManager.Player.Buffs.FirstOrDefault(buff1 => buff1.Name.Equals("dravenspinningattack"));

            return buff != null ? buff.Count : 0;
        }

        private static bool IsEn(String opt)
        {
            return Menu.Item(opt).GetValue<bool>();
        }

        public static float GetManaPer()
        {
            return (Player.Mana / Player.MaxMana) * 100;
        }
    }
}