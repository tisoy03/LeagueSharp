using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace VayneHunterRework
{
    internal class VayneHunterRework
    {
        public static Orbwalking.Orbwalker COrbwalker;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static String CharName = "Vayne";
        public static Spell Q, W, E, R;
        public static Menu Menu;
        public static Vector3 AfterCond = Vector3.Zero;
        public static AttackableUnit Current; // for tower farming
        public static AttackableUnit Last; // for tower farming
        private static float _lastMoveC;
        private static bool _aLInit;
        private static readonly int[] Qwe = { 1, 3, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
        private static readonly int[] Qew = { 1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2 };
        private static readonly int[] Wqe = { 1, 3, 2, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3 };
        private static readonly int[] Weq = { 1, 3, 2, 2, 2, 4, 2, 3, 2, 3, 4, 3, 3, 1, 1, 4, 1, 1 };
        private static readonly int[] Eqw = { 1, 3, 2, 3, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
        private static readonly int[] Ewq = { 1, 3, 2, 3, 3, 4, 3, 2, 3, 2, 4, 2, 2, 1, 1, 4, 1, 1 };

        private static readonly StringList Orders = new StringList(
            new[] { "QWE", "QEW", "WQE", "WEQ", "EQW", "EWQ" }, 2);

        public VayneHunterRework()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private void OrbwalkerAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                AfterAa(target);
            }
        }

        #region Drawing

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            var drawE = Menu.Item("DrawE").GetValue<Circle>();
            var drawCond = Menu.Item("DrawCond").GetValue<Circle>();
            var drawDrake = Menu.Item("DrawDrake").GetValue<Circle>();
            var drawMid = Menu.Item("DrawMid").GetValue<Circle>();
            var midWallQPos = new Vector2(6707.485f, 8802.744f);
            var drakeWallQPos = new Vector2(11514, 4462);
            if (drawDrake.Active && Player.Distance(drakeWallQPos) < 1500f && IsSummonersRift())
            {
                Utility.DrawCircle(new Vector3(12052, 4826, 0f), 75f, drawDrake.Color);
            }

            if (drawMid.Active && Player.Distance(midWallQPos) < 1500f && IsSummonersRift())
            {
                Utility.DrawCircle(new Vector3(6958, 8944, 0f), 75f, drawMid.Color);
            }

            if (drawE.Active)
            {
                Utility.DrawCircle(Player.Position, E.Range, drawE.Color);
            }

            if (drawCond.Active)
            {
                DrawPostCondemn();
            }
        }

        #endregion

        private void AfterAa(AttackableUnit target)
        {
            if (!(target is Obj_AI_Hero))
            {
                return;
            }

            var tar = (Obj_AI_Hero) target;

            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:

                    if (IsMenuEnabled("UseQC"))
                    {
                        SmartQCheck(tar);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (IsMenuEnabled("UseQH"))
                    {
                        SmartQCheck(tar);
                    }
                    break;
            }

            ENextAuto(tar);
            UseItems(tar);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //Cleanser.enableCheck();
            if (Player.IsDead)
            {
                return;
            }

            Obj_AI_Hero tar;

            if (IsMenuEnabled("AutoE") && CondemnCheck(Player.Position, out tar))
            {
                CastE(tar, true);
            }

            if (Menu.Item("WallTumble").GetValue<KeyBind>().Active)
            {
                WallTumble();
            }

            if (Menu.Item("ThreshLantern").GetValue<KeyBind>().Active)
            {
                TakeLantern();
            }

            QFarmCheck();
            FocusTarget();
            NoAaStealth();
            EKs();

            AutoPot();

            //Cleanser
            Cleanser.CleanserByBuffType();
            Cleanser.CleanserBySpell();


            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Obj_AI_Hero tar2;
                    if (IsMenuEnabled("UseEC") && CondemnCheck(Player.ServerPosition, out tar2))
                    {
                        CastE(tar2);
                    }

                    UseIgnite();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Obj_AI_Hero tar3;
                    if (IsMenuEnabled("UseEH") && CondemnCheck(Player.ServerPosition, out tar3))
                    {
                        CastE(tar3);
                    }
                    break;
            }
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != CharName)
            {
                return;
            }

            Cleanser.CreateQssSpellList();

            #region Menu

            Menu = new Menu("VayneHunter Rework", "VHRework", true);
            var orbMenu = new Menu("Orbwalker", "orbwalker");
            COrbwalker = new Orbwalking.Orbwalker(orbMenu);
            Menu.AddSubMenu(orbMenu);
            var tsMenu = new Menu("Target Selector", "TargetSel");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            Menu.AddSubMenu(new Menu("[VH] Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q Combo")).SetValue(true);
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseEC", "Use E Combo").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseRC", "Use R Combo").SetValue(false));
            Menu.SubMenu("Combo").AddItem(new MenuItem("QManaC", "Min Q Mana %").SetValue(new Slider(35, 1)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("EManaC", "Min E Mana %").SetValue(new Slider(20, 1)));
            Menu.SubMenu("Combo")
                .AddItem(new MenuItem("NEnUlt", "Only ult when x enemies").SetValue(new Slider(2, 1, 5)));

            Menu.AddSubMenu(new Menu("[VH] Harrass", "Harrass"));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseQH", "Use Q Harrass")).SetValue(true);
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseEH", "Use E Harrass").SetValue(true));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("QManaH", "Min Q Mana %").SetValue(new Slider(35, 1)));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("EManaH", "Min E Mana %").SetValue(new Slider(20, 1)));

            Menu.AddSubMenu(new Menu("[VH] Farm", "Farm"));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseQLH", "Use Q LastHit")).SetValue(true);
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseQLC", "Use Q Laneclear")).SetValue(true);
            Menu.SubMenu("Farm").AddItem(new MenuItem("QManaLH", "Min Q Mana % LH").SetValue(new Slider(35, 1)));
            Menu.SubMenu("Farm").AddItem(new MenuItem("QManaLC", "Min Q Mana % LC").SetValue(new Slider(35, 1)));

            var miscSubMenu = new Menu("[VH] Misc", "Misc");

            var miscTSubMenu = new Menu("Misc - Tumble", "MiscT");
            {
                miscTSubMenu.AddItem(new MenuItem("SmartQ", "Try to QE First").SetValue(false));
                miscTSubMenu.AddItem(new MenuItem("NoQEn", "Don't Q into enemies").SetValue(true));
                miscTSubMenu.AddItem(new MenuItem("NoAAStealth", "Don't AA while stealthed").SetValue(false));
                miscTSubMenu.AddItem(
                    new MenuItem("WallTumble", "Tumble Over Wall").SetValue(
                        new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));
            }
            var miscCSubMenu = new Menu("Misc - Condemn", "MiscC");
            {
                miscCSubMenu.AddItem(
                    new MenuItem("ENext", "E Next Auto").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
                miscCSubMenu.AddItem(new MenuItem("PushDistance", "E Push Dist").SetValue(new Slider(425, 400, 500)));
                miscCSubMenu.AddItem(new MenuItem("CondemnTurret", "Try to Condemn to turret").SetValue(false));
                miscCSubMenu.AddItem(new MenuItem("CondemnFlag", "Condemn to J4 flag").SetValue(true));
                miscCSubMenu.AddItem(new MenuItem("AutoE", "Auto E").SetValue(false));
                miscCSubMenu.AddItem(new MenuItem("AutoEKS", "Smart E Ks").SetValue(true));
                miscCSubMenu.AddItem(new MenuItem("NoEEnT", "No E Under enemy turret").SetValue(true));
            }
            var miscGSubMenu = new Menu("Misc - General", "MiscG");
            {
                miscGSubMenu.AddItem(new MenuItem("Packets", "Packet Casting").SetValue(true));
                miscGSubMenu.AddItem(new MenuItem("AntiGP", "Anti Gapcloser")).SetValue(true);
                miscGSubMenu.AddItem(new MenuItem("Interrupt", "Interrupter").SetValue(true));
                miscGSubMenu.AddItem(new MenuItem("SpecialFocus", "Focus targets with 2 W marks").SetValue(false));
                miscGSubMenu.AddItem(
                    new MenuItem("ThreshLantern", "Grab Thresh Lantern").SetValue(
                        new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
                miscGSubMenu.AddItem(new MenuItem("UseIgn", "Use Ignite")).SetValue(true);
            }
            miscSubMenu.AddSubMenu(miscTSubMenu);
            miscSubMenu.AddSubMenu(miscCSubMenu);
            miscSubMenu.AddSubMenu(miscGSubMenu);
            Menu.AddSubMenu(miscSubMenu);

            Menu.AddSubMenu(new Menu("[VH] BushRevealer", "BushReveal"));
            //Menu.SubMenu("BushReveal").AddItem(new MenuItem("BushReveal", "Bush Revealer").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Toggle)));
            Menu.SubMenu("BushReveal").AddItem(new MenuItem("BushRevealer", "Trinket bush on condemn").SetValue(true));

            Menu.AddSubMenu(new Menu("[VH] Items", "Items"));
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

            Menu.AddSubMenu(new Menu("[VH] QSS", "QSSMenu"));
            Menu.SubMenu("QSSMenu").AddItem(new MenuItem("UseQSS", "Use QSS").SetValue(true));
            Menu.SubMenu("QSSMenu")
                .AddItem(new MenuItem("QSSMinBuffs", "Min Buffs to QSS").SetValue(new Slider(2, 1, 5)));

            Menu.AddSubMenu(new Menu("[VH] QSS Buff Types", "QSST"));
            Cleanser.CreateTypeQssMenu();
            Menu.AddSubMenu(new Menu("[VH] QSS Spells", "QSSSpell"));
            Cleanser.CreateQssSpellMenu();
            Menu.AddSubMenu(new Menu("[VH] Don't Condemn", "NoCondemn"));
            CreateNoCondemnMenu();

            Menu.AddSubMenu(new Menu("[VH] AutoPot", "AutoPot"));
            Menu.SubMenu("AutoPot").AddItem(new MenuItem("APH", "Health Pot").SetValue(true));
            Menu.SubMenu("AutoPot").AddItem(new MenuItem("APM", "Mana Pot").SetValue(true));
            Menu.SubMenu("AutoPot").AddItem(new MenuItem("APH_Slider", "Health Pot %").SetValue(new Slider(35, 1)));
            Menu.SubMenu("AutoPot").AddItem(new MenuItem("APM_Slider", "Mana Pot %").SetValue(new Slider(35, 1)));
            Menu.SubMenu("AutoPot").AddItem(new MenuItem("APHeal", "Use Heal").SetValue(true));
            Menu.SubMenu("AutoPot").AddItem(new MenuItem("APHeal_Slider", "Heal %").SetValue(new Slider(35, 1)));

            Menu.AddSubMenu(new Menu("[VH] AutoLeveler", "AutoLevel"));
            Menu.SubMenu("AutoLevel").AddItem(new MenuItem("ALSeq", "AutoLevel Seq").SetValue(Orders));
            Menu.SubMenu("AutoLevel").AddItem(new MenuItem("ALAct", "AutoLevel Active").SetValue(false));

            Menu.AddSubMenu(new Menu("[VH] Drawings", "Draw"));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawE", "Draw E").SetValue(new Circle(true, Color.MediumPurple)));
            Menu.SubMenu("Draw")
                .AddItem(new MenuItem("DrawCond", "Draw Pos. Aft. E if Stun").SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Draw")
                .AddItem(new MenuItem("DrawDrake", "Draw Drake Spot").SetValue(new Circle(true, Color.WhiteSmoke)));
            Menu.SubMenu("Draw")
                .AddItem(new MenuItem("DrawMid", "Draw Mid Spot").SetValue(new Circle(true, Color.WhiteSmoke)));

            Menu.AddToMainMenu();

            #endregion

            Game.PrintChat("<font color='#FF0000'>VayneHunter</font> <font color='#FFFFFF'>Rework loaded!</font>");
            Game.PrintChat(
                "By <font color='#FF0000'>DZ</font><font color='#FFFFFF'>191</font>. Special Thanks to: Kurisuu & KonoeChan");
            Game.PrintChat("If you like my assemblies feel free to donate me (link on the forum :) )");

            //Cleanser.cleanUselessSpells();
            Q = new Spell(SpellSlot.Q);
            E = new Spell(SpellSlot.E, 550f);
            R = new Spell(SpellSlot.R);
            E.SetTargetted(0.25f, 1600f);
            Orbwalking.AfterAttack += OrbwalkerAfterAttack;
            Game.OnGameUpdate += Game_OnGameUpdate;

            // Game.OnGameProcessPacket += GameOnOnGameProcessPacket;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += Cleanser.OnCreateObj;
            GameObject.OnDelete += Cleanser.OnDeleteObj;
            Menu.Item("ALAct").ValueChanged += AutoLevelValueChanged;

            #region AutoLeveler

            if (!IsMenuEnabled("ALAct") || _aLInit)
            {
                return;
            }

            var autoLevelI =
                new AutoLevel(
                    GetSequence(
                        Menu.Item("ALSeq").GetValue<StringList>().SList[
                            Menu.Item("ALSeq").GetValue<StringList>().SelectedIndex]));
            _aLInit = true;
        }

        private static void AutoLevelValueChanged(object sender, OnValueChangeEventArgs ev)
        {
            if (IsMenuEnabled("ALAct") && !_aLInit)
            {
                var autoLevelI =
                    new AutoLevel(
                        GetSequence(
                            Menu.Item("ALSeq").GetValue<StringList>().SList[
                                Menu.Item("ALSeq").GetValue<StringList>().SelectedIndex]));
                _aLInit = true;
            }

            AutoLevel.Enabled(ev.GetNewValue<bool>());
        }

        #endregion

        #region Miscellaneous

        private static void ENextAuto(Obj_AI_Hero tar)
        {
            if (!E.IsReady() || !tar.IsValid || !Menu.Item("ENext").GetValue<KeyBind>().Active)
            {
                return;
            }

            CastE(tar, true);
            Menu.Item("ENext").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle));
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gpSender = gapcloser.Sender;
            if (!IsMenuEnabled("AntiGP") || !E.IsReady() || !gpSender.IsValidTarget())
            {
                return;
            }

            CastE(gpSender, true);
        }

        private static void Interrupter_OnPossibleToInterrupt(AttackableUnit unit, InterruptableSpell spell)
        {
            var sender = (Obj_AI_Hero) unit;
            if (!IsMenuEnabled("Interrupt") || !E.IsReady() || !sender.IsValidTarget())
            {
                return;
            }

            CastE(sender, true);
        }

        private static bool CondemnCheck(Vector3 position, out Obj_AI_Hero target)
        {
            if (IsUnderEnTurret(Player.Position) && IsMenuEnabled("NoEEnT"))
            {
                target = null;
                return false;
            }

            foreach (var en in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        hero =>
                            hero.IsEnemy && hero.IsValidTarget() && !IsMenuEnabled("nC" + hero.ChampionName) &&
                            hero.Distance(position) <= E.Range))
            {
                var ePred = E.GetPrediction(en);
                var pushDist = Menu.Item("PushDistance").GetValue<Slider>().Value;
                var finalPosition = ePred.UnitPosition.To2D().Extend(position.To2D(), -pushDist).To3D();
                for (var i = 1; i < pushDist; i += (int) en.BoundingRadius)
                {
                    var loc3 = ePred.UnitPosition.To2D().Extend(position.To2D(), -i).To3D();
                    var orTurret = IsMenuEnabled("CondemnTurret") && IsUnderTurret(finalPosition);
                    var orFlag = IsMenuEnabled("CondemnFlag") && IsJ4FlagThere(loc3, en);
                    var orFountain = IsMenuEnabled("CondemnTurret") && IsFountain(finalPosition);
                    AfterCond = loc3;
                    if (!IsWall(loc3) && !orTurret && !orFlag && !orFountain)
                    {
                        continue;
                    }

                    if (IsMenuEnabled("BushRevealer"))
                    {
                        CheckAndWard(position, loc3, en);
                    }

                    target = en;
                    return true;
                }
            }

            target = null;
            return false;
        }

        private static void QFarmCheck()
        {
            if (!Q.IsReady())
            {
                return;
            }

            var posAfterQ = Player.Position.To2D().Extend(Game.CursorPos.To2D(), 300);
            var minList =
                MinionManager.GetMinions(Player.Position, 550f)
                    .Where(
                        min =>
                            HealthPrediction.GetHealthPrediction(
                                min,
                                (int) (Q.Delay + min.Distance(posAfterQ) / Orbwalking.GetMyProjectileSpeed()) * 1000) +
                            (Game.Ping / 2) <= (Q.GetDamage(min) + Player.GetAutoAttackDamage(min)) &&
                            HealthPrediction.GetHealthPrediction(
                                min,
                                (int) (Q.Delay + min.Distance(posAfterQ) / Orbwalking.GetMyProjectileSpeed()) * 1000) +
                            (Game.Ping / 2) > Player.GetAutoAttackDamage(min)); //Player.GetAutoAttackDamage(min)

            var objAiBases = minList as Obj_AI_Base[] ?? minList.ToArray();
            if (!objAiBases.Any())
            {
                return;
            }

            CastQ(Vector3.Zero, objAiBases.First());
        }


        private static void NoAaStealth()
        {
            var mb = (!IsMenuEnabled("NoAAStealth") || !Player.HasBuff("vaynetumblefade", true));
            COrbwalker.SetAttack(mb);
        }

        private static void FocusTarget()
        {
            if (!IsMenuEnabled("SpecialFocus"))
            {
                return;
            }

            foreach (var hero in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null)))
                    .Where(Has2WStacks))
            {
                COrbwalker.ForceTarget(hero);
                Hud.SelectedUnit = hero;
                return;
            }
        }


        private static IEnumerable<int> GetSequence(String order)
        {
            switch (order)
            {
                case "QWE":
                    return Qwe;
                case "QEW":
                    return Qew;
                case "WQE":
                    return Wqe;
                case "EQW":
                    return Eqw;
                case "WEQ":
                    return Weq;
                case "EWQ":
                    return Ewq;
                default:
                    return null;
            }
        }

        private static void CreateNoCondemnMenu()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                Menu.SubMenu("NoCondemn")
                    .AddItem(new MenuItem("nC" + hero.ChampionName, hero.ChampionName).SetValue(false));
            }
        }

        private static void CheckAndWard(Vector3 sPos, Vector3 endPosition, Obj_AI_Hero target)
        {
            if (!IsGrass(endPosition))
            {
                return;
            }

            var wardSlot = FindBestWardItem();
            if (wardSlot == null)
            {
                return;
            }

            for (var i = 1; i < Vector3.Distance(sPos, endPosition); i += (int) target.BoundingRadius)
            {
                var v = sPos.To2D().Extend(endPosition.To2D(), i).To3D();
                if (!IsGrass(v))
                {
                    continue;
                }

                //WardSlot.UseItem(v);
                Player.Spellbook.CastSpell(wardSlot.SpellSlot, v);
                return;
            }
        }

        private static void DrawPostCondemn()
        {
            var drawCond = Menu.Item("DrawCond").GetValue<Circle>();
            foreach (var en in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        hero =>
                            hero.IsEnemy && hero.IsValidTarget() && !IsMenuEnabled("nC" + hero.ChampionName) &&
                            hero.Distance(Player.Position) <= E.Range))
            {
                var ePred = E.GetPrediction(en);
                var pushDist = Menu.Item("PushDistance").GetValue<Slider>().Value;
                for (var i = 0; i < pushDist; i += (int) en.BoundingRadius)
                {
                    var loc3 = ePred.UnitPosition.To2D().Extend(Player.Position.To2D(), -i).To3D();
                    if (IsWall(loc3))
                    {
                        Utility.DrawCircle(loc3, 100f, drawCond.Color);
                    }
                }
            }
        }

        private static void TakeLantern()
        {
            /**
            foreach (GameObject obj in ObjectManager.Get<GameObject>())
            {
                if (obj.Name.Contains("ThreshLantern") && obj.Position.Distance(ObjectManager.Player.ServerPosition) <= 500 && obj.IsAlly)
                {
                    var InteractPKT = new PKT_InteractReq
                    {
                        NetworkId = Player.NetworkId,
                        TargetNetworkId = obj.NetworkId
                    };
                    //Credits to Trees
                    Game.SendPacket(InteractPKT.Encode(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
                    return;
                }
            }
             * */
        }

        private static void WallTumble()
        {
            if (!IsSummonersRift())
            {
                return;
            }

            var midWallQPos = new Vector2(6707.485f, 8802.744f);
            var drakeWallQPos = new Vector2(11514, 4462);
            if (Player.Distance(midWallQPos) >= Player.Distance(drakeWallQPos))
            {
                if (Player.Position.X < 12000 || Player.Position.X > 12070 || Player.Position.Y < 4800 ||
                    Player.Position.Y > 4872)
                {
                    MoveToLimited(new Vector2(12050, 4827).To3D());
                }
                else
                {
                    MoveToLimited(new Vector2(12050, 4827).To3D());
                    Q.Cast(drakeWallQPos, true);
                }
            }
            else
            {
                if (Player.Position.X < 6908 || Player.Position.X > 6978 || Player.Position.Y < 8917 ||
                    Player.Position.Y > 8989)
                {
                    MoveToLimited(new Vector2(6958, 8944).To3D());
                }
                else
                {
                    MoveToLimited(new Vector2(6958, 8944).To3D());
                    Q.Cast(midWallQPos, true);
                }
            }
        }

        private static void MoveToLimited(Vector3 where)
        {
            if (Environment.TickCount - _lastMoveC < 80)
            {
                return;
            }

            _lastMoveC = Environment.TickCount;
            Player.IssueOrder(GameObjectOrder.MoveTo, where);
        }

        #endregion

        #region Q Region

        private void SmartQCheck(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !target.IsValidTarget())
            {
                return;
            }

            if (!IsMenuEnabled("SmartQ") || !E.IsReady())
            {
                CastQ(Game.CursorPos, target);
            }
            else
            {
                for (var I = 0; I <= 360; I += 65)
                {
                    var f1 =
                        new Vector2(
                            Player.Position.X + (float) (300 * Math.Cos(I * (Math.PI / 180))),
                            Player.Position.Y + (float) (300 * Math.Sin(I * (Math.PI / 180)))).To3D();
                    // var FinalPos = Player.Position.To2D().Extend(F1, 300).To3D();
                    Obj_AI_Hero targ;
                    if (!CondemnCheck(f1, out targ))
                    {
                        continue;
                    }

                    CastTumble(f1, target);
                    CastE(target);
                    return;
                }

                CastQ(Game.CursorPos, target);
            }
        }

        private static void CastQ(Vector3 pos, Obj_AI_Base target, bool customPos = false)
        {
            if (!Q.IsReady() || !target.IsValidTarget())
            {
                return;
            }

            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var manaC = Menu.Item("QManaC").GetValue<Slider>().Value;
                    var enMin = Menu.Item("NEnUlt").GetValue<Slider>().Value;
                    var enemiesList =
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(h => h.IsValid && !h.IsDead && h.Distance(Player.Position) <= 900 && h.IsEnemy)
                            .ToList();
                    if (GetPerValue(true) >= manaC && IsMenuEnabled("UseQC"))
                    {
                        if (IsMenuEnabled("UseRC") && R.IsReady() && enemiesList.Count >= enMin)
                        {
                            R.CastOnUnit(Player);
                        }

                        if (!customPos)
                        {
                            CastTumble(target);
                        }

                        else
                        {
                            CastTumble(pos, target);
                        }
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var manaH = Menu.Item("QManaH").GetValue<Slider>().Value;
                    if (GetPerValue(true) >= manaH && IsMenuEnabled("UseQH"))
                    {
                        if (!customPos)
                        {
                            CastTumble(target);
                        }
                        else
                        {
                            CastTumble(pos, target);
                        }
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    var manaLh = Menu.Item("QManaLH").GetValue<Slider>().Value;
                    if (GetPerValue(true) >= manaLh && IsMenuEnabled("UseQLH"))
                    {
                        if (!customPos)
                        {
                            CastTumble(target);
                        }
                        else
                        {
                            CastTumble(pos, target);
                        }
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    var manaLc = Menu.Item("QManaLC").GetValue<Slider>().Value;
                    if (GetPerValue(true) >= manaLc && IsMenuEnabled("UseQLC"))
                    {
                        if (!customPos)
                        {
                            CastTumble(target);
                        }
                        else
                        {
                            CastTumble(pos, target);
                        }
                    }
                    break;
            }
        }

/*
        private Vector3 GetQVectorMelee()
        {
            if (isMenuEnabled("SmartQ") || !isMenuEnabled("SpecialQMelee"))
            {
                return Player.Position.Extend(Game.CursorPos, Q.Range);
            }

            var position = Game.CursorPos;
            //Standard Q end position
            var extended = Player.Position.Extend(position, Q.Range);

            //Check for Melee enemies in that range
            var heroesThere =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => h.Distance(extended) <= 375f && h.IsValidTarget() && h.IsMelee())
                    .OrderBy(h => h.Distance(Player))
                    .ToList();

            //If the count is 0 return the normal position
            if (heroesThere.Count == 0)
            {
                return extended;
            }

            //Find the closest hero
            var theHero = heroesThere.First();

            //Extend the V3 of the hero radius to my player. Not used atm.
            var heroRadius = theHero.Position.Extend(Player.Position, theHero.AttackRange);

            //Intersection.. Not used atm.
            var intersection = theHero.Position.To2D()
                .Intersection(heroRadius.To2D(), Player.Position.To2D(), extended.To2D());

            //Start angle
            double angle = 0;

            //Step angle
            const int step = 10;

            //The new extended variable
            var newExtended = extended;

            //While the heroes near the new position are > 0 or the angle <= 180
            while (
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => h.Distance(newExtended) <= 375f && h.IsValidTarget() && h.IsMelee())
                    .ToList()
                    .Count > 0 || angle <= 45)
            {
                //Augment the angle by step
                angle += step;

                //Second angle
                var angle2 = -angle;

                //Find the new extended position
                newExtended = new Vector3(
                    extended.X * (float) Math.Cos(Geometry.DegreeToRadian(angle)),
                    extended.Y * (float) Math.Sin(Geometry.DegreeToRadian(angle)), extended.Z);

                //Move in one direction first. Then check:
                //Are enemies still there?
                //If they are then try the other direction
                //If not the while cycle will just end.
                if (
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(h => h.Distance(newExtended) <= 375f && h.IsValidTarget() && h.IsMelee())
                        .ToList()
                        .Count > 0)
                {
                    newExtended = new Vector3(
                        extended.X * (float) Math.Cos(Geometry.DegreeToRadian(angle2)),
                        extended.Y * (float) Math.Sin(Geometry.DegreeToRadian(angle2)), extended.Z);
                }
            }

            //return the end position
            return newExtended;
        }
*/

        private static void CastTumble(Obj_AI_Base target)
        {
            var posAfterTumble = ObjectManager.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if (!(distanceAfterTumble < 550 * 550) || !(distanceAfterTumble > 100 * 100))
            {
                return;
            }

            if (GetEnemiesInRange(posAfterTumble, 500f) >= 3 && GetAlliesInRange(posAfterTumble, 410f) < 3 &&
                IsMenuEnabled("NoQEn"))
            {
                return;
            }

            Q.Cast(Game.CursorPos, IsMenuEnabled("Packets"));
        }

        private static void CastTumble(Vector3 pos, Obj_AI_Base target)
        {
            var posAfterTumble = ObjectManager.Player.ServerPosition.To2D().Extend(pos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if (!(distanceAfterTumble < 550 * 550) || !(distanceAfterTumble > 100 * 100))
            {
                return;
            }

            if (GetEnemiesInRange(posAfterTumble, 500f) >= 3 && GetAlliesInRange(posAfterTumble, 410f) < 3 &&
                IsMenuEnabled("NoQEn"))
            {
                return;
            }

            Q.Cast(pos, IsMenuEnabled("Packets"));
        }

        #endregion

        #region E Region

        private static void CastE(Obj_AI_Hero target, bool isForGapcloser = false)
        {
            if (!E.IsReady() || !target.IsValidTarget())
            {
                return;
            }

            if (isForGapcloser)
            {
                E.Cast(target, IsMenuEnabled("Packets"));
                AfterCond = Vector3.Zero;
                return;
            }

            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var manaC = Menu.Item("EManaC").GetValue<Slider>().Value;
                    if (IsMenuEnabled("UseEC") && GetPerValue(true) >= manaC)
                    {
                        E.Cast(target, IsMenuEnabled("Packets"));
                        AfterCond = Vector3.Zero;
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var manaH = Menu.Item("EManaH").GetValue<Slider>().Value;
                    if (IsMenuEnabled("UseEH") && GetPerValue(true) >= manaH)
                    {
                        E.Cast(target, IsMenuEnabled("Packets"));
                        AfterCond = Vector3.Zero;
                    }
                    break;
            }
        }

        private static void EKs()
        {
            if (Q.IsReady() || Player.CanAttack || !IsMenuEnabled("AutoEKS"))
            {
                return;
            }

            foreach (
                var hero in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(h => h.Distance(Player) > E.Range - 120 && h.IsValidTarget())
                        .Where(Has2WStacks)
                        .Where(
                            hero =>
                                Player.GetSpellDamage(hero, SpellSlot.W) + Player.GetSpellDamage(hero, SpellSlot.E) >=
                                HealthPrediction.GetHealthPrediction(
                                    hero, (int) (Player.Distance(hero) / E.Speed) * 1000)))
            {
                E.Cast(hero, IsMenuEnabled("Packets"));
                return;
            }
        }

        #endregion

        #region Items

        private static void UseItems(Obj_AI_Hero tar)
        {
            var ownH = GetPerValue(false);
            if ((Menu.Item("BotrkC").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) &&
                (Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
                ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= GetPerValueTarget(tar, false))))
            {
                UseItem(3153, tar);
            }

            if ((Menu.Item("BotrkH").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) &&
                (Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
                ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= GetPerValueTarget(tar, false))))
            {
                UseItem(3153, tar);
            }

            if (Menu.Item("YoumuuC").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                UseItem(3142);
            }

            if (Menu.Item("YoumuuH").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                UseItem(3142);
            }

            if (Menu.Item("BilgeC").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                UseItem(3144, tar);
            }

            if (Menu.Item("BilgeH").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                UseItem(3144, tar);
            }
        }

        private static void UseIgnite()
        {
            //Ignite
            var dmg = 50 + 20 * Player.Level;
            var tg = TargetSelector.GetSelectedTarget();
            var ign = Player.GetSpellSlot("summonerdot");
            if (!IsMenuEnabled("UseIgn") || !tg.IsValidTarget() || !(dmg > tg.Health))
            {
                return;
            }

            if (ign != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(ign) == SpellState.Ready)
            {
                Player.Spellbook.CastSpell(ign, tg);
            }
        }

        private static void AutoPot()
        {
            if (ObjectManager.Player.HasBuff("Recall") || Player.InFountain() && Player.InShop())
            {
                return;
            }

            //Health Pots
            if (IsMenuEnabled("APH") && GetPerValue(false) <= Menu.Item("APH_Slider").GetValue<Slider>().Value &&
                !Player.HasBuff("RegenerationPotion", true))
            {
                UseItem(2003);
            }

            //Mana Pots
            if (IsMenuEnabled("APM") && GetPerValue(true) <= Menu.Item("APM_Slider").GetValue<Slider>().Value &&
                !Player.HasBuff("FlaskOfCrystalWater", true))
            {
                UseItem(2004);
            }

            //Summoner Heal
            if (IsMenuEnabled("APHeal") && GetPerValue(false) <= Menu.Item("APHeal_Slider").GetValue<Slider>().Value)
            {
                var heal = Player.GetSpellSlot("summonerheal");
                if (heal != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(heal) == SpellState.Ready)
                {
                    Player.Spellbook.CastSpell(heal);
                }
            }
        }

        #endregion

        #region Utility Methods

        private static bool IsSummonersRift()
        {
            return true;
        }

        private static bool Has2WStacks(Obj_AI_Hero target)
        {
            return target.Buffs.Any(bu => bu.Name == "vaynesilvereddebuff" && bu.Count == 2);
        }

        private static int GetEnemiesInRange(Vector3 point, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => h.IsEnemy && !h.IsDead && h.IsValid && h.Distance(point) <= range)
                    .ToList()
                    .Count;
        }

        private static int GetAlliesInRange(Vector3 point, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => h.IsAlly && !h.IsDead && h.IsValid && h.Distance(point) <= range)
                    .ToList()
                    .Count;
        }

        private static SpellDataInst GetItemSpell(InventorySlot invSlot)
        {
            return ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => (int) spell.Slot == invSlot.Slot + 4);
        }

        private static InventorySlot FindBestWardItem()
        {
            var slot = Items.GetWardSlot();
            if (slot == default(InventorySlot))
            {
                return null;
            }

            var sdi = GetItemSpell(slot);
            if (sdi != default(SpellDataInst) && sdi.State == SpellState.Ready)
            {
                return slot;
            }
            return null;
        }

        public static void UseItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }

        private static bool IsWall(Vector3 pos)
        {
            var cFlags = NavMesh.GetCollisionFlags(pos);
            return (cFlags == CollisionFlags.Wall);
        }

        private static bool IsUnderTurret(Vector3 position)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(turr => turr.IsAlly && (turr.Health != 0))
                    .Any(tur => tur.Distance(position) <= 975f);
        }

        private static bool IsUnderEnTurret(Vector3 position)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(turr => turr.IsEnemy && (turr.Health != 0))
                    .Any(tur => tur.Distance(position) <= 975f);
        }

        public static bool IsMenuEnabled(String val)
        {
            return Menu.Item(val).GetValue<bool>();
        }

        private static float GetPerValue(bool mana)
        {
            if (mana)
            {
                return (Player.Mana / Player.MaxMana) * 100;
            }
            return (Player.Health / Player.MaxHealth) * 100;
        }

        private static float GetPerValueTarget(Obj_AI_Hero target, bool mana)
        {
            if (mana)
            {
                return (target.Mana / target.MaxMana) * 100;
            }
            return (target.Health / target.MaxHealth) * 100;
        }

        private static bool IsGrass(Vector3 pos)
        {
            return NavMesh.IsWallOfGrass(pos, 65);
            //return false; 
        }

        private static bool IsJ4FlagThere(Vector3 position, Obj_AI_Hero target)
        {
            return
                ObjectManager.Get<Obj_AI_Base>()
                    .Any(m => m.Distance(position) <= target.BoundingRadius && m.Name == "Beacon");
        }

        private static bool IsFountain(Vector3 position)
        {
            float fountainRange = 750;
            var map = Utility.Map.GetMap();
            if (map != null && map.Type == Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1050;
            }

            return
                ObjectManager.Get<GameObject>()
                    .Where(spawnPoint => spawnPoint is Obj_SpawnPoint && spawnPoint.IsAlly)
                    .Any(spawnPoint => Vector2.Distance(position.To2D(), spawnPoint.Position.To2D()) < fountainRange);
        }

        #endregion
    }
}