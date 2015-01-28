using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace FioraRaven
{
    internal class DZApi
    {
        public static Obj_AI_Base Player = ObjectManager.Player;
        private readonly Dictionary<String, String> dSpellsName = new Dictionary<String, String>();
        private readonly Dictionary<int, String> itemNames = new Dictionary<int, String>();
        private string[] dSpellsNames;

        public DZApi()
        {
            FillDSpellList();
        }

        public Dictionary<String, String> GetDanSpellsName()
        {
            return dSpellsName;
        }

        public Dictionary<int, String> GetItemNames()
        {
            return itemNames;
        }

        public void AddSpell(String name, String displayName)
        {
            dSpellsName.Add(name, displayName);
        }

        public void FillDSpellList()
        {
            AddSpell("CurseofTheSadMummy", "Amumu R");
            AddSpell("InfernalGuardian", "Annie Tibbers");
            AddSpell("BlindMonkRKick", "Lee Sin R");
            AddSpell("GalioIdolOfDurand", "Galio R");
            AddSpell("syndrar", "Syndra R");
            AddSpell("BusterShot", "Trist R");
            AddSpell("UFSlash", "Malphite R");
            AddSpell("VeigarPrimordialBurst", "Veigar R");
            AddSpell("ViR", "Vi R");
            AddSpell("AlZaharNetherGrasp", "Malzahar R");
        }

        public float GetEnH(Obj_AI_Hero target)
        {
            var h = (target.Health / target.MaxHealth) * 100;

            return h;
        }

        public float GetManaPer()
        {
            var mana = (Player.Mana / Player.MaxMana) * 100;

            return mana;
        }

        public float GetPlHPer()
        {
            var h = (Player.Health / Player.MaxHealth) * 100;

            return h;
        }

        public void UseItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }
    }
}