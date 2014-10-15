using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
namespace Muramana
{
    class Program
    {
        public static int Muramana = 3042;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Game.OnGameProcessPacket += Packet_Process; 
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
        }

        private static void Packet_Process(GamePacketEventArgs args)
        {
            
        }

        static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if(unit.IsMe)
            {
                int Mur = Items.HasItem(Muramana) ? 3042 : 3043;
                if (ObjectManager.Get<Obj_AI_Hero>().Contains(target) && (Items.HasItem(Mur)) && (Items.CanUseItem(Mur)))
                {
                    Items.UseItem(Mur);
                }
            }
        }
        public void sendPacket()
        {
            Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, SpellSlot.R, ObjectManager.Player.NetworkId, ObjectManager.Player.Position.X, ObjectManager.Player.Position.Y, Game.CursorPos.X, Game.CursorPos.Y));
        }
        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            int Mur = Items.HasItem(Muramana) ? 3042 : 3043;
            if (ObjectManager.Get<Obj_AI_Hero>().Contains(args.Target) && (Items.HasItem(Mur)) && (Items.CanUseItem(Mur)))
            {
                Items.UseItem(Mur);
            }
        }
         
    }
}
