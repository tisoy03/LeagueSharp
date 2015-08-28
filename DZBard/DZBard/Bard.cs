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
                    if (spells[SpellSlot.Q].IsReady() && GetItemValue<bool>(string.Format("dz191.bard.{0}.useq", BardOrbwalker.ActiveMode)) &&
                        ComboTarget.IsValidTarget())
                    {
                        HandleQ(ComboTarget);
                    }

                    if (GetItemValue<bool>(string.Format("dz191.bard.{0}.usew", BardOrbwalker.ActiveMode)))
                    {
                        HandleW();
                    }

                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (spells[SpellSlot.Q].IsReady() && GetItemValue<bool>(string.Format("dz191.bard.{0}.useq", BardOrbwalker.ActiveMode)) &&
                        ComboTarget.IsValidTarget())
                    {
                        HandleQ(ComboTarget);
                    }
                    break;
            }
        }

        private static void HandleQ(Obj_AI_Hero comboTarget)
        {
                var QPrediction = spells[SpellSlot.Q].GetPrediction(comboTarget);

                if (QPrediction.Hitchance >= HitChance.High)
                {
                    if (spells[SpellSlot.Q].GetDamage(comboTarget) > comboTarget.Health + 15)
                    {
                        spells[SpellSlot.Q].Cast(QPrediction.CastPosition);
                        return;
                    }

                    var QPushDistance = GetItemValue<Slider>("dz191.bard.misc.distance").Value;
                    var QAccuracy = GetItemValue<Slider>("dz191.bard.misc.accuracy").Value;
                    var PlayerPosition = ObjectManager.Player.ServerPosition;

                    var BeamStartPositions = new List<Vector3>()
                    {
                        QPrediction.CastPosition,
                        QPrediction.UnitPosition,
                        comboTarget.ServerPosition,
                        comboTarget.Position
                    };

                    if (comboTarget.IsDashing())
                    {
                        BeamStartPositions.Add(comboTarget.GetDashInfo().EndPos.To3D());
                    }

                    var PositionsList = new List<Vector3>();
                    var CollisionPositions = new List<Vector3>();

                    foreach (var position in BeamStartPositions)
                    {
                        var collisionableObjects = spells[SpellSlot.Q].GetCollision(position.To2D(),
                            new List<Vector2>() {position.Extend(PlayerPosition, -QPushDistance).To2D()});

                        if (collisionableObjects.Any())
                        {
                            if (collisionableObjects.Any(h => h is Obj_AI_Hero) &&
                                (collisionableObjects.All(h => h.IsValidTarget())))
                            {
                                spells[SpellSlot.Q].Cast(QPrediction.CastPosition);
                                break;
                            }

                            for (var i = 0; i < QPushDistance; i += (int) comboTarget.BoundingRadius)
                            {
                                CollisionPositions.Add(position.Extend(PlayerPosition, -i));
                            }
                        }

                        for (var i = 0; i < QPushDistance; i += (int) comboTarget.BoundingRadius)
                        {
                            PositionsList.Add(position.Extend(PlayerPosition, -i));
                        }
                    }

                    if (PositionsList.Any())
                    {
                        //We don't want to divide by 0 Kappa
                        var WallNumber = PositionsList.Count(p => p.IsWall())*1.3f;
                        var CollisionPositionCount = CollisionPositions.Count;
                        var Percent = (WallNumber + CollisionPositionCount)/PositionsList.Count;
                        var AccuracyEx = QAccuracy/100f;
                        if (Percent >= AccuracyEx)
                        {
                            spells[SpellSlot.Q].Cast(QPrediction.CastPosition);
                        }
                    }
                }
        }


        private static void HandleW()
        {
            if (ObjectManager.Player.IsRecalling() || ObjectManager.Player.InShop() || !spells[SpellSlot.W].IsReady())
            {
                return;
            }

            if (ObjectManager.Player.HealthPercent <= 20)
            {
                var castPosition = ObjectManager.Player.ServerPosition.Extend(Game.CursorPos, 65);
                spells[SpellSlot.W].Cast(castPosition);
                return;
            }

            var LowHealthAlly = HeroManager.Allies
                .Where(ally => ally.IsValidTarget(spells[SpellSlot.W].Range, false)
                    && ally.HealthPercent <= 25
                    && GetItemValue<bool>(string.Format("dz191.bard.wtarget.{0}", ally.ChampionName.ToLower())))
                .OrderBy(TargetSelector.GetPriority)
                .ThenBy(ally => ally.Health)
                .FirstOrDefault();

            if (LowHealthAlly != null)
            {
                var movementPrediction = Prediction.GetPrediction(LowHealthAlly, 0.25f);
                spells[SpellSlot.W].Cast(movementPrediction.UnitPosition);
            }
        }


        private static T GetItemValue<T>(string item)
        {
            return BardMenu.Item(item).GetValue<T>();
        }
    }
}
