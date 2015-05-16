using SharpDX;

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
        /// The R sprite instance.
        /// </summary>
        private static Render.Sprite _sprite;

        /// <summary>
        /// Gets the current target we will draw the sprite on
        /// </summary>
        private static Obj_AI_Hero RTarget
        {
            get
            {
                var HeroList =
                    HeroManager.Enemies.FindAll(
                        h =>
                            h.Health + 20 < PJR.GetSpellsDictionary()[SpellSlot.R].GetDamage(h) &&
                            PJR.GetSpellsDictionary()[SpellSlot.R].CanCast(h)).ToList().OrderBy(h => h.Health);
                return HeroList.Any() ? HeroList.First() : null;
            }
        }

        /// <summary>
        /// Gets the R target position on the screen.
        /// </summary>
        private static Vector2 RTargetPosition
        {
            get
            {
                return RTarget != null?
                    new Vector2(
                        Drawing.WorldToScreen(RTarget.Position).X - RTarget.BoundingRadius * 2 +
                        RTarget.BoundingRadius / 2.5f,
                        Drawing.WorldToScreen(RTarget.Position).Y - RTarget.BoundingRadius * 2) : new Vector2(0,0);
            }
        }

        /// <summary>
        /// Gets the Draw condition for the sprite.
        /// </summary>
        private static bool DrawCondition
        {
            get { return PJR.GetSpellsDictionary()[SpellSlot.R].IsReady() && RTarget != null && RTargetPosition.IsOnScreen() && PJR.GetMenu().Item("dz191." + PJR.GetMenuName() + ".drawings.rsprite").GetValue<bool>(); }
        }

        /// <summary>
        /// Initializes the sprite reference. To be called when the assembly is loaded.
        /// </summary>
        internal static void InitalizeSprite()
        {
            _sprite = new Render.Sprite(Properties.Resources.ScopeSprite, new Vector2());
            {
                _sprite.Scale = new Vector2(0.65f, 0.65f);
                _sprite.PositionUpdate = () => RTargetPosition;
                _sprite.VisibleCondition = s => DrawCondition;
            }
            _sprite.Add();
        }
    }
}
