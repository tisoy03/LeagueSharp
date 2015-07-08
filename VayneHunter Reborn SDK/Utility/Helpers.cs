using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.Extensions.SharpDX;
using LeagueSharp.SDK.Core.Wrappers;
using SharpDX;

namespace VayneHunter_Reborn_SDK.Utility
{
    static class Helpers
    {
        public static float LastMoveC;
        #region Utility Methods
        public static bool IsJ4FlagThere(Vector3 position, Obj_AI_Hero target)
        {
            return ObjectManager.Get<Obj_AI_Base>().Any(m => m.Distance(position) <= target.BoundingRadius && m.Name == "Beacon");
        }
        public static bool IsFountain(Vector3 position)
        {
            float fountainRange = 750;
            var map = Map.GetMap();
            if (map != null && map.Type == MapType.SummonersRift)
            {
                fountainRange = 1050;
            }
            return ObjectManager.Get<GameObject>().Where(spawnPoint => spawnPoint is Obj_SpawnPoint && spawnPoint.IsAlly).Any(spawnPoint => Vector2.Distance(position.ToVector2(), spawnPoint.Position.ToVector2()) < fountainRange);
        }

        public static bool IsSummonersRift()
        {
            var map = Map.GetMap();
            if (map != null && map.Type == MapType.SummonersRift)
            {
                return true;
            }
            return false;
        }

        public static bool Has2WStacks(this Obj_AI_Hero target)
        {
            return target.Buffs.Any(bu => bu.Name == "vaynesilvereddebuff" && bu.Count == 2);
        }
        public static bool IsPlayerFaded()
        {
            return false;
            return (ObjectManager.Player.HasBuff("vaynetumblefade") && !ObjectManager.Player.UnderTurret(true));
        }
        public static void MoveToLimited(Vector3 where)
        {
            if (Environment.TickCount - LastMoveC < 80)
            {
                return;
            }
            LastMoveC = Environment.TickCount;
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, where);
        }
        public static bool OkToQ(Vector3 position)
        {
            if (position.UnderTurret(true) && !ObjectManager.Player.UnderTurret(true))
                return false;
            var allies = position.CountAlliesInRange(ObjectManager.Player.AttackRange);
            var enemies = position.CountEnemiesInRange(ObjectManager.Player.AttackRange);
            var lhEnemies = GetLhEnemiesNearPosition(position, ObjectManager.Player.AttackRange).Count();

            if (enemies == 1) //It's a 1v1, safe to assume I can E
            {
                return true;
            }

            //Adding 1 for the Player
            return (allies + 1 > enemies - lhEnemies);
        }

        public static bool OkToQ2(Vector3 Position)
        {
            if (!MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "mirin"))
            {
                var closeEnemies =
                    GameObjects.EnemyHeroes.Where(en => en.IsValidTarget(1500f)).OrderBy(en => en.Distance(Position));
                if (closeEnemies.Any())
                {
                    return
                        closeEnemies.All(
                            enemy =>
                                Position.CountEnemiesInRange(
                                    MenuHelper.isMenuEnabled("dz191.vhr.misc.tumble", "dynamicqsafety")
                                        ? enemy.AttackRange
                                        : 405f) < 1);
                }
                return true;
            }
            else
            {
                if (Position.CountEnemiesInRange(360f) >= 1)
                {
                    return false;
                }
                return true;
            }
            
        }
        public static List<Obj_AI_Hero> GetLhEnemiesNearPosition(Vector3 position, float range)
        {
            return GameObjects.EnemyHeroes.Where(hero => hero.IsValidTarget(range, true, position) && hero.HealthPercent <= 15).ToList();
        }

        public static bool UnderAllyTurret(Vector3 Position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(t => t.IsAlly && !t.IsDead);
        }
        #endregion
    }
}
