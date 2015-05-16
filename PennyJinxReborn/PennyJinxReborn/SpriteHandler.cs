namespace PennyJinxReborn
{
    #region
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    #endregion

    /// <summary>
    /// Handles the R sprite drawing.
    /// </summary>
    internal class SpriteHandler
    {
        /// <summary>
        /// Gets the current target we will draw the sprite on
        /// </summary>
        private static Obj_AI_Hero RTarget
        {
            get
            {
                return HeroManager.Enemies.Find(h => h.Health < PJR.GetSpellsDictionary()[SpellSlot.R].GetDamage(h) && PJR.GetSpellsDictionary()[SpellSlot.R].CanCast(h));
            }
        }

        /// <summary>
        /// Draws the scope sprite onto the target.
        /// </summary>
        internal static void DrawSprite()
        {
            
        }
    }
}
