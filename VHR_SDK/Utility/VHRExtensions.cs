using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;

namespace VHR_SDK.Utility
{
    static class VHRExtensions
    {
        public static bool IsEnabledAndReady(this Spell spell, OrbwalkerMode mode)
        {
            var modeString = mode.ToString().ToLowerInvariant();

            var EnabledInMenu =
                VHR.VHRMenu[string.Format("dz191.vhr.{0}", modeString)][
                    string.Format("Use{0}", spell.Slot.GetStringFromSlot())].GetValue<MenuBool>().Value;

            var Ready = spell.IsReady();

            var ManaManagerCheck = ObjectManager.Player.ManaPercent >= VHR.VHRMenu[string.Format("dz191.vhr.{0}", modeString)][
                    string.Format("{0}Mana", spell.Slot.GetStringFromSlot())].GetValue<MenuSlider>().Value;

            return EnabledInMenu && Ready && ManaManagerCheck;
        }

        public static string GetStringFromSlot(this SpellSlot slot)
        {
            switch (slot)
            {
                case SpellSlot.Q:
                    return "Q";
                case SpellSlot.E:
                    return "E";
                case SpellSlot.R:
                    return "R";
                case SpellSlot.Trinket:
                    return "Trinket";
                default:
                    return "Unk";
            }
        }
    }
}
