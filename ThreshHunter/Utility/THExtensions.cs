using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ThreshHunter.Utility
{
    internal static class THExtensions
    {


        public static bool IsEnabledAndReady(this Spell spell, Orbwalking.OrbwalkingMode mode)
        {
            var modeString = mode.ToString().ToLowerInvariant();
            var EnabledInMenu =
                GetItemValue<bool>(string.Format("dz191.thresh.{0}.use{1}", modeString, spell.Slot.GetStringFromSlot()));
            var Ready = spell.IsReady();

            return EnabledInMenu && Ready;
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

        public static T GetItemValue<T>(string item)
        {
            return Thresh.RootMenu.Item(item).GetValue<T>();
        }
    }
}
