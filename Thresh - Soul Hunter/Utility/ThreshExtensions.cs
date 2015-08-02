using LeagueSharp;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;

namespace Thresh___Soul_Hunter.Utility
{
    static class ThreshExtensions
    {
        public static bool IsEnabledAndReady(this Spell spell, OrbwalkerMode mode)
        {
            var modeString = mode.ToString().ToLowerInvariant();
            var EnabledInMenu =
                Thresh.RootMenu[string.Format("dz191.thresh.{0}", modeString)][
                    string.Format("use{0}", spell.Slot.GetStringFromSlot())].GetValue<MenuBool>().Value;

            var Ready = spell.IsReady();

            var ManaManagerCheck = ObjectManager.Player.ManaPercent >= Thresh.RootMenu[string.Format("dz191.vhr.{0}", modeString)][
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
