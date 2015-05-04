using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;

namespace VayneHunter_Reborn.MapPosition
{
    class PositionalHelper
    {
        private const float Range = 1200f;
        private const float RangeOffsetAlly = -50f;
        private const float RangeOffsetEnemy = 20f;
        private static Team StrongerTeam
        {
            get
            {
                var enemyCs = HeroManager.Enemies.FindAll(m => m.Distance(ObjectManager.Player) <= Range && m.IsValidTarget(Range, false)).Aggregate(0f, (current, enemy) => current + (enemy.MinionsKilled + enemy.NeutralMinionsKilled)); ;
                var allyCs = HeroManager.Allies.FindAll(m => m.Distance(ObjectManager.Player) <= Range && m.IsValidTarget(Range, false)).Aggregate(0f, (current, ally) => current + (ally.MinionsKilled + ally.NeutralMinionsKilled));
                return allyCs >= enemyCs ? Team.Ally : Team.Enemy;
            }
        }

        private static List<Obj_AI_Hero> AlliesClose
        {
            get
            {
                return
                    HeroManager.Allies.FindAll(
                        m =>
                            m.Distance(ObjectManager.Player) <= Range && m.IsValidTarget(Range, false) &&
                            m.CountAlliesInRange(m.AttackRange + RangeOffsetAlly) > 0);
            }
        }
        private static List<Obj_AI_Hero> EnemiesClose
        {
            get
            {
                return
                    HeroManager.Enemies.FindAll(
                        m =>
                            m.Distance(ObjectManager.Player) <= Range && m.IsValidTarget(Range, false) &&
                            m.CountEnemiesInRange(m.AttackRange + RangeOffsetEnemy) > 0);
            }
        } 
        public static List<Vector2> GetSafeZone()
        {
            var allyList = GetAllyPoints();
            var enemyList = GetEnemyPoints();
            var intersectionList = allyList.Intersect(enemyList);
            return StrongerTeam == Team.Ally ? allyList.Concat(intersectionList).ToList() : allyList.Except(intersectionList).ToList();
        }

        public static List<Vector2> GetAllyPoints()
        {
            var polygonsList = AlliesClose.Select(ally => new Geometry.Circle(ally.ServerPosition.To2D(), ally.AttackRange + RangeOffsetAlly).ToPolygon()).ToList();
            var pathList = Geometry.ClipPolygons(polygonsList);
            var pointList = pathList.SelectMany(path => path, (path, point) => new Vector2(point.X, point.Y)).Where(currentPoint => currentPoint.IsWall()).ToList();
            return pointList;
        }
        public static List<Vector2> GetEnemyPoints()
        {
            var polygonsList = EnemiesClose.Select(enemy => new Geometry.Circle(enemy.ServerPosition.To2D(), enemy.AttackRange + RangeOffsetEnemy).ToPolygon()).ToList();
            var pathList = Geometry.ClipPolygons(polygonsList);
            var pointList = pathList.SelectMany(path => path, (path, point) => new Vector2(point.X, point.Y)).Where(currentPoint => currentPoint.IsWall()).ToList();
            return pointList;
        }

    }

    enum Team
    {
        Ally,Enemy
    }
}
