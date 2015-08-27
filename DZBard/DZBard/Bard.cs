using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace DZBard
{
    class Bard
    {
        public static Menu BardMenu;

        public static Orbwalking.Orbwalker BardOrbwalker { get; set; }
        
        //DO YOU HAVE A MOMENT TO TALK ABOUT DIKTIONARIESS!=!=!=!==??!?!? -Everance 2k15
        public static Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>()
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 950f)}//TODO Check
        };

        internal static void OnLoad()
        {
            LoadEvents();
            LoadSpells();
        }

        private static void LoadSpells()
        {
            spells[SpellSlot.Q].SetSkillshot(0.25f, 65f, 1600f, false, SkillshotType.SkillshotLine);
        }

        private static void LoadEvents()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            var ComboTarget = TargetSelector.GetTarget(spells[SpellSlot.Q].Range / 1.3f, TargetSelector.DamageType.Magical);
            switch (BardOrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (spells[SpellSlot.Q].IsReady() && GetItemValue<bool>("dz191.bard.combo.useq") && ComboTarget.IsValidTarget())
                    {
                        var QPrediction = spells[SpellSlot.Q].GetPrediction(ComboTarget);

                        if (QPrediction.Hitchance >= HitChance.VeryHigh)
                        {
                            var QPushDistance = GetItemValue<Slider>("dz191.bard.combo.distance").Value;
                            var QAccuracy = GetItemValue<Slider>("dz191.bard.combo.accuracy").Value;
                            var PlayerPosition = ObjectManager.Player.ServerPosition;

                            var BeamStartPositions = new List<Vector3>()
                            {
                                QPrediction.CastPosition,
                                QPrediction.UnitPosition,
                                ComboTarget.ServerPosition,
                                ComboTarget.Position
                            };

                            if (ComboTarget.IsDashing())
                            {
                                BeamStartPositions.Add(ComboTarget.GetDashInfo().EndPos.To3D());    
                            }

                            var PositionsList = new List<Vector3>();
                            var CollisionPositions = new List<Vector3>();

                            foreach (var position in BeamStartPositions)
                            {
                                var collisionableObjects = spells[SpellSlot.Q].GetCollision(position.To2D(),
                                    new List<Vector2>() {position.Extend(PlayerPosition, -QPushDistance).To2D()});

                                if (collisionableObjects.Any())
                                {
                                    for (var i = 0; i < QPushDistance; i += (int) ComboTarget.BoundingRadius)
                                    {
                                        CollisionPositions.Add(position.Extend(PlayerPosition, -i));
                                    }
                                }

                                for (var i = 0; i < QPushDistance; i += (int) ComboTarget.BoundingRadius)
                                {
                                    PositionsList.Add(position.Extend(PlayerPosition, -i));
                                }
                            }

                            if (PositionsList.Any())
                            {
                                //We don't want to divide by 0 Kappa
                                var WallNumber = PositionsList.Count(p => p.IsWall());
                                var CollisionPositionCount = CollisionPositions.Count;
                                var Percent = (WallNumber + CollisionPositionCount) / PositionsList.Count;
                                var AccuracyEx = QAccuracy/100f;
                                if (Percent >= AccuracyEx)
                                {
                                    spells[SpellSlot.Q].Cast(QPrediction.CastPosition);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private static T GetItemValue<T>(string item)
        {
            return BardMenu.Item(item).GetValue<T>();
        }
    }
}
