using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace DZJayce.Utility
{
    static class Helper
    {
        public static bool IsSpellReady(this Spells spell)
        {
            return SpellHandler.GetCooldown(spell) <= 0f;
        }

        public static bool IsRangedForm(this Obj_AI_Hero player)
        {
            return player.HasBuff("TODO");
        }

        public static bool IsMeleeForm()
        {
            return !ObjectManager.Player.IsRangedForm();
        }
    }
}
