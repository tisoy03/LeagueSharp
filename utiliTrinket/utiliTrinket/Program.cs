using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace utiliTrinket
{
    internal class Program
    {
        private const int SightStone = 2049;
        private const int Orb = 3363;
        private const int YellowW = 3340;
        private const int TrinketRed = 3341;
        private const int QuillCoat = 3204;
        private const int Wriggle = 3154;
        public static Menu Menu;
        public static Obj_AI_Base Player = ObjectManager.Player;
        //static bool boughtSweepS = false;
        //static bool boughtSweepW = false;
        //static bool boughtSweepQ = false;
        //static bool boughtYellow = false;
        //static bool boughtSweep = false;
        //static bool boughtBlue = false;
        //static int trinketSlot = 134;
        //static Vector3 position;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("utiliTrinket", "UtiliTrkMenu", true);
            Menu.AddItem(new MenuItem("ward", "Buy WTotem at start").SetValue(true));
            Menu.AddItem(new MenuItem("EnSW", "Buy Sweeper").SetValue(true));
            Menu.AddItem(new MenuItem("timer", "Buy Sw at x min").SetValue(new Slider(15, 1, 30)));
            Menu.AddItem(new MenuItem("orb", "Buy Orb").SetValue(true));
            Menu.AddItem(new MenuItem("timer2", "Buy Orb at x min").SetValue(new Slider(40, 30, 60)));
            Menu.AddItem(new MenuItem("sweeperS", "Buy Sw On Sightstone").SetValue(true));
            Menu.AddItem(new MenuItem("sweeperQ", "Buy Sw QuillCoat").SetValue(true));
            Menu.AddItem(new MenuItem("sweeperW", "Buy Sw on Wriggle").SetValue(true));
            Game.PrintChat("utiliTrinket By DZ191 based on PewPewPew's Script Loaded!");
            Menu.AddToMainMenu();
            Game.OnGameUpdate += OnTick;
        }

        private static void OnTick(EventArgs args)
        {
            var player1 = (Obj_AI_Hero) Player;
            if (!Player.IsDead && !player1.InShop())
            {
                return;
            }

            //Game.PrintChat(hasItem(YellowW).ToString());
            if (GetTimer() < 1 && !HasItem(YellowW) && IsEn("ward"))
            {
                Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(YellowW, ObjectManager.Player.NetworkId))
                    .Send();
            }

            if (HasItem(SightStone) && IsEn("sweeperS") && !HasItem(TrinketRed))
            {
                Packet.C2S.SellItem.Encoded(new Packet.C2S.SellItem.Struct(SpellSlot.Trinket, Player.NetworkId)).Send();
                Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(TrinketRed, ObjectManager.Player.NetworkId))
                    .Send();
            }

            if (HasItem(QuillCoat) && IsEn("sweeperQ") && !HasItem(TrinketRed))
            {
                Packet.C2S.SellItem.Encoded(new Packet.C2S.SellItem.Struct(SpellSlot.Trinket, Player.NetworkId)).Send();
                Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(TrinketRed, ObjectManager.Player.NetworkId))
                    .Send();
            }

            if (HasItem(Wriggle) && IsEn("sweeperW") && !HasItem(TrinketRed))
            {
                Packet.C2S.SellItem.Encoded(new Packet.C2S.SellItem.Struct(SpellSlot.Trinket, Player.NetworkId)).Send();
                player1.SellItem((int) SpellSlot.Trinket);
                Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(TrinketRed, ObjectManager.Player.NetworkId))
                    .Send();
            }

            if (IsEn("orb") && (GetTimer() >= Menu.Item("timer2").GetValue<Slider>().Value) && !HasItem(Orb))
            {
                Packet.C2S.SellItem.Encoded(new Packet.C2S.SellItem.Struct(SpellSlot.Trinket, Player.NetworkId)).Send();
                Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(Orb, ObjectManager.Player.NetworkId)).Send();
            }

            if (IsEn("EnSW") && HasItem(YellowW) && (GetTimer() >= Menu.Item("timer").GetValue<Slider>().Value) &&
                (GetTimer() < Menu.Item("timer2").GetValue<Slider>().Value) && !HasItem(TrinketRed))
            {
                // Game.PrintChat("Called");
                Packet.C2S.SellItem.Encoded(new Packet.C2S.SellItem.Struct(SpellSlot.Trinket, Player.NetworkId)).Send();
                Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(TrinketRed, ObjectManager.Player.NetworkId))
                    .Send();
            }
        }

        public static float GetTimer()
        {
            return Game.Time / 60;
        }

        public static bool HasItem(int id)
        {
            return Items.HasItem(id, (Obj_AI_Hero) Player);
        }

        public static bool IsEn(String op)
        {
            return Menu.Item(op).GetValue<bool>();
        }
    }
}