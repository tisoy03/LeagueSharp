using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;

namespace VayneHunter_Reborn_SDK.Utility
{
    static class MenuHelper
    {
        public static bool IsEnabledAndReady(this Spell spell, Mode mode)
        {
            if (ObjectManager.Player.IsDead)
                return false;
            try
            {
                var manaPercentage = getSliderValue("dz191.vhr." + ObjectManager.Player.ChampionName.ToLowerInvariant() + ".manamanager" , GetStringFromSpellSlot(spell.Slot).ToLowerInvariant() + "mana" + GetStringFromMode(mode).ToLowerInvariant());
                var enabledCondition = isMenuEnabled("dz191.vhr." + ObjectManager.Player.ChampionName.ToLowerInvariant() , "use" + GetStringFromSpellSlot(spell.Slot).ToLowerInvariant() + GetStringFromMode(mode));
                return spell.IsReady() && (ObjectManager.Player.ManaPercent >= manaPercentage) && enabledCondition;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }

        public static void AddManaManager(this Menu menu, Mode mode, SpellSlot[] spellList, int[] manaCosts)
        {
            var mmMenu = new Menu("Mana Manager", "dz191.vhr." + ObjectManager.Player.ChampionName.ToLowerInvariant() + ".mm." + GetStringFromMode(mode));
            for (var i = 0; i < spellList.Count(); i++)
            {
                mmMenu.Add(
                    new MenuSlider(
                        "dz191.vhr." + ObjectManager.Player.ChampionName.ToLowerInvariant() + ".manamanager." + GetStringFromSpellSlot(spellList[i]).ToLowerInvariant() + "mana" + GetStringFromMode(mode).ToLowerInvariant(),
                        GetStringFromSpellSlot(spellList[i]) + " Mana", 
                        manaCosts[i]));
            }
            menu.Add(mmMenu);
        }

        public static void AddModeMenu(this Menu menu, Mode mode, SpellSlot[] spellList, bool[] values)
        {
            for (var i = 0; i < spellList.Count(); i++)
            {
                menu.Add(
                    new MenuBool(
                        "dz191.vhr." + ObjectManager.Player.ChampionName.ToLowerInvariant() + ".use" + GetStringFromSpellSlot(spellList[i]).ToLowerInvariant() + GetStringFromMode(mode),
                        "Use " + GetStringFromSpellSlot(spellList[i]) + " " + GetFullNameFromMode(mode), 
                        values[i]));
            }
        }

        public static void AddDrawMenu(this Menu menu, Dictionary<SpellSlot, Spell> dictionary, SharpDX.Color myColor)
        {
            foreach (var entry in dictionary)
            {
                var slot = entry.Key;
                if (entry.Value.Range < 5000f)
                {
                    menu.Add(new MenuColor(ObjectManager.Player.ChampionName + "Draw" + GetStringFromSpellSlot(slot), "Draw " + GetStringFromSpellSlot(slot), myColor));
                }
            }
        }

        public static void AddNoCondemnMenu(this Menu menu, bool allies)
        {
            var _menu = new Menu("Don't E", "dz191.vhr." + ObjectManager.Player.ChampionName.ToLowerInvariant() + ".noe");
            foreach (var player in ObjectManager.Get<Obj_AI_Hero>().Where(h => !h.IsMe && allies ? h.IsAlly : h.IsEnemy))
            {
                _menu.Add(new MenuBool("dz191.vhr." + ObjectManager.Player.ChampionName.ToLowerInvariant() + ".noe." + player.ChampionName, player.ChampionName, false));
            }
            menu.Add(_menu);
        }

        public static bool isMenuEnabled(String submenu, String item)
        {
            var startString = item.StartsWith("Use") ? ObjectManager.Player.ChampionName : "";
            return VayneHunterReborn.Menu[submenu][item].GetValue<MenuBool>().Value;
        }

        public static int getSliderValue(String submenu, String item)
        {
            return VayneHunterReborn.Menu[submenu][item] != null ? VayneHunterReborn.Menu[submenu][item].GetValue<MenuSlider>().Value : -1;
        }

        public static bool getKeybindValue(String submenu, String item)
        {
            return VayneHunterReborn.Menu[submenu][item].GetValue<MenuKeyBind>().Active;
        }

        public static String GetStringFromSpellSlot(SpellSlot sp)
        {
            //TODO Test if this works
            //return sp.ToString();
            switch (sp)
            {
                case SpellSlot.Q:
                    return "Q";
                case SpellSlot.W:
                    return "W";
                case SpellSlot.E:
                    return "E";
                case SpellSlot.R:
                    return "R";
                default:
                    return "unk";
            }
        }
        static String GetStringFromMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.Combo:
                    return "C";
                case Mode.Harrass:
                    return "H";
                case Mode.Lasthit:
                    return "LH";
                case Mode.Laneclear:
                    return "LC";
                case Mode.Farm:
                    return "F";
                default:
                    return "unk";
            }
        }
        static String GetFullNameFromMode(Mode mode)
        {
            return mode.ToString();
        }
    }

    enum Mode
    {
        Combo,
        Harrass,
        Lasthit,
        Laneclear,
        Farm
    }
}
