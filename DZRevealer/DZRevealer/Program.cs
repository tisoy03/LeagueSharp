using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace DZRevealer
{
    //Inspired from the discontinued BoL script Disclosures. Many thanks to the Autor for his Trinket\Ward place calcs.

    internal class Program
    {
        public static Dictionary<String, String> Dict;
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static Spell E;
        public static Menu Menu;
        public static int VisionWard = 2043;
        public static int TrinketRed = 3364;
        public static float WardRange = 600f;
        public static float TrinketRange = 600f;
        public static bool Debug = false;

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
            Menu = new Menu("DZReveal!", "DZReveal", true);
            Menu.AddItem(new MenuItem("doRev", "Reveal").SetValue(true));
            Menu.AddItem(new MenuItem("revDesc1", "Priority:"));
            Menu.AddItem(new MenuItem("prior", "ON: Pink OFF: Trinket").SetValue(true));
            if (Player.BaseSkinName == "LeeSin")
            {
                Menu.AddItem(new MenuItem("leeE", "Lee Sin: Use E").SetValue(true));
                E = new Spell(SpellSlot.E, 350f);
            }

            Game.PrintChat("DZReveal Loaded");
            Menu.AddToMainMenu();
            FillDict();
            Game.PrintChat(Player.BaseSkinName);
            Game.OnGameUpdate += GameGameUpdate;
        }

        private static void GameGameUpdate(EventArgs args)
        {
            if (!IsEn("doRev"))
            {
                return;
            }

            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsEnemy)
                        .Where(enemy => enemy.HasBuffOfType(BuffType.Invisibility) && enemy.BaseSkinName != "Evelynn"))
            {
                Reveal(enemy);
            }
        }

        private static void Reveal(Obj_AI_Hero enemy)
        {
            if (Player.BaseSkinName == "LeeSin" && E.IsReady() && Player.Distance(enemy) <= E.Range && IsEn("leeE"))
            {
                E.Cast();
            }
            else
            {
                if (IsEn("prior"))
                {
                    //W
                    if (!(Player.Distance(enemy) <= WardRange + 300f))
                    {
                        return;
                    }

                    if (Player.Distance(enemy) <= WardRange)
                    {
                        UseItem(VisionWard, enemy.Position);
                    }
                    else
                    {
                        var pos1 = Vector3.Lerp(Player.Position, enemy.Position, WardRange / Player.Distance(enemy));
                        UseItem(VisionWard, pos1);
                    }
                }
                else
                {
                    //Trink
                    if (!(Player.Distance(enemy) <= TrinketRange + 300f))
                    {
                        return;
                    }

                    if (Player.Distance(enemy) <= TrinketRange)
                    {
                        UseItem(TrinketRed, enemy.Position);
                    }
                    else
                    {
                        var pos1 = Vector3.Lerp(Player.Position, enemy.Position, TrinketRange / Player.Distance(enemy));
                        UseItem(TrinketRed, pos1);
                    }
                }
            }
        }

        public static bool IsEn(String item)
        {
            return Menu.Item(item).GetValue<bool>();
        }

        private static void FillDict()
        {
            Dict = new Dictionary<String, String>
            {
                { "Vayne", "VayneTumbleFade" },
                { "Twitch", "TwitchHideInShadows" },
                { "Rengar", "RengarR" },
                { "MonkeyKing", "monkeykingdecoystealth" },
                { "Khazix", "khazixrstealth" },
                { "Talon", "talonshadowassaultbuff" },
                { "Akali", "akaliwstealth" }
            };
        }

        public static void UseItem(int id, Vector3 position)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, position);
            }
        }
    }
}