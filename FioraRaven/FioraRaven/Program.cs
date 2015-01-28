using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace FioraRaven
{
    internal class Fiora
    {
        public static String ChampName = "Fiora";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static Menu Menu;
        public static Obj_AI_Hero Tar;
        public static Dictionary<string, SpellSlot> SpellData;
        public static DZApi Api = new DZApi();
        public static bool FirstQ;
        public static float QCastTime;

        private static void Main(string[] args)
        {
            try
            {
                CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName)
            {
                return;
            }

            SpellData = new Dictionary<string, SpellSlot>();
            Menu = new Menu("Fiora Raven", "FioraRMenu", true);
            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker1"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker1"));
            var ts = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(ts);
            Menu.AddSubMenu(ts);
            Menu.AddSubMenu(new Menu("Fiora Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Menu.AddSubMenu(new Menu("Fiora Misc", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("WBlock", "Use W to block Autos").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("RDodge", "Use R to dodge dangerous").SetValue(true));
            Menu.SubMenu("Misc")
                .AddItem(new MenuItem("SecondQDelay", "Second Q Delay (ms)").SetValue(new Slider(650, 0, 4000)));
            Menu.AddSubMenu(new Menu("Fiora Items", "Item"));
            Menu.SubMenu("Item").AddItem(new MenuItem("Botrk", "Use Botrk").SetValue(true));
            Menu.SubMenu("Item").AddItem(new MenuItem("Youmuu", "Use Youmuu").SetValue(true));
            Menu.SubMenu("Item").AddItem(new MenuItem("Tiamat", "Use Tiamat").SetValue(true));
            Menu.SubMenu("Item").AddItem(new MenuItem("Hydra", "Use Hydra").SetValue(true));
            Menu.SubMenu("Item").AddItem(new MenuItem("OwnHPercBotrk", "Min Own H % Botrk").SetValue(new Slider(50, 1)));
            Menu.SubMenu("Item")
                .AddItem(new MenuItem("EnHPercBotrk", "Min Enemy H % Botrk").SetValue(new Slider(20, 1)));
            Menu.SubMenu("Item").AddItem(new MenuItem("ItInComb", "Use Items In Combo").SetValue(true));
            Menu.AddSubMenu(new Menu("Fiora Dangerous Spells", "DangSpells"));

            var dSpellsDName = Api.GetDanSpellsName();
            foreach (var entry in dSpellsDName)
            {
                Menu.SubMenu("DangSpells").AddItem(new MenuItem(entry.Key, entry.Value).SetValue(true));
            }

            Menu.AddSubMenu(new Menu("Fiora Drawing", "Drawing"));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrQ", "Draw Q").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrR", "Draw R").SetValue(true));

            Game.PrintChat("Fiora The Raven By DZ191 Loaded ");
            Menu.AddToMainMenu();
            Obj_AI_Base.OnProcessSpellCast += GameProcessSpell;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
            Orbwalking.OnAttack += Orbwalking_OnAttack;
            Drawing.OnDraw += Drawing_OnDraw;
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 400f);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (IsEn("DrQ"))
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.MediumPurple);
            }

            if (IsEn("DrR"))
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.MediumPurple);
            }
        }

        public static void GameProcessSpell(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args)
        {
            var name = args.SData.Name;
            var spell = args;
            if (Api.GetDanSpellsName().ContainsKey(args.SData.Name) && IsEn(name))
            {
                //Got Dangerous Spell. Starting Predictions and Custom Evade Logics.
                if (name == "CurseofTheSadMummy")
                {
                    if (Player.Distance(hero.Position) <= 600f)
                    {
                        var tar1 = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                        R.Cast(tar1);
                    }
                }

                if (name == "InfernalGuardian" || name == "UFSlash")
                {
                    if (Player.Distance(spell.End) <= 270f)
                    {
                        var tar1 = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                        R.Cast(tar1);
                    }
                }

                if (name == "BlindMonkRKick" || name == "syndrar" || name == "VeigarPrimordialBurst" ||
                    name == "AlZaharNetherGrasp")
                {
                    if (spell.Target.IsMe)
                    {
                        var tar1 = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                        R.Cast(tar1);
                    }
                }

                if (name == "BusterShot" || name == "ViR")
                {
                    if (spell.Target.IsMe || Player.Distance(spell.Target.Position) <= 50f)
                    {
                        var tar1 = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                        R.Cast(tar1);
                    }
                }

                if (name == "GalioIdolOfDurand")
                {
                    if (Player.Distance(hero.Position) <= 600f)
                    {
                        var tar1 = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                        R.Cast(tar1);
                    }
                }
            }

            if (spell.SData.Name.Contains("Attack") && IsEn("WBlock") && spell.Target.IsMe)
            {
                W.Cast();
            }
        }

        private static void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }

            Tar = (Obj_AI_Hero) target;
            if (IsEn("Botrk") && IsCombo() && target.IsValidTarget() && IsEn("ItInComb"))
            {
                var ownH = Api.GetPlHPer();
                if ((Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
                    ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= Api.GetEnH(Tar))))
                {
                    Api.UseItem(3153, Tar);
                }
            }

            if (IsEn("Youmuu") && IsCombo() && target.IsValidTarget() && IsEn("ItInComb"))
            {
                Api.UseItem(3142, Tar);
            }
        }

        public static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }

            Tar = (Obj_AI_Hero) target;
            if (IsCombo() && E.IsReady() && target.IsValidTarget())
            {
                E.Cast();
            }

            if (Menu.Item("Tiamat").GetValue<bool>() && IsCombo())
            {
                const int itemId = 3077;
                Api.UseItem(itemId);
            }

            if (Menu.Item("Hydra").GetValue<bool>() && IsCombo())
            {
                const int itemId = 3074;
                Api.UseItem(itemId);
            }

            FirstQ = false;
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Q.IsReady())
            {
                FirstQ = false;
            }

            if (!IsCombo())
            {
                return;
            }

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            CastQ(target);
            if ((R.GetDamage(target) >= target.Health))
            {
                CastR(target);
            }
        }

        public static void CastQ(Obj_AI_Hero target)
        {
            if (!target.IsValidTarget())
            {
                return;
            }

            if (!target.IsValidTarget(Q.Range) || !Q.IsReady() || !Q.IsInRange(target.ServerPosition) || FirstQ ||
                !IsEn("UseQ") ||
                !((Game.Time - QCastTime) >= (Menu.Item("SecondQDelay").GetValue<Slider>().Value / 1000)))
            {
                return;
            }

            Q.Cast(target, true);
            FirstQ = true;
            QCastTime = Game.Time;
        }

        public static void CastR(Obj_AI_Hero target)
        {
            if (IsCombo() && target.IsValidTarget() && R.IsInRange(target.ServerPosition) && IsEn("UseR"))
            {
                R.Cast(target, true);
            }
        }

        public static bool IsCombo()
        {
            return Orbwalker.ActiveMode.ToString() == "Combo";
        }

        public static bool IsEn(String item)
        {
            return Menu.Item(item).GetValue<bool>();
        }
    }
}