namespace VHR_SDK
{
    #region
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.Extensions.SharpDX;
    using LeagueSharp.SDK.Core.Math.Prediction;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using LeagueSharp.SDK.Core.Utils;
    using LeagueSharp.SDK.Core.Wrappers;
    using SharpDX;
    using Interfaces;
    using Modules;
    using Utility;
    using Utility.Helpers;
    using LeagueSharp.SDK.Core.UI.IMenu;
    #endregion

    class VHR
    {

        /**
         * This code is patented under Australian IP Laws Kappa.
         * */

        /**
         * Special Credits and mentions:
         * Exory - Being a great guy and helping me test, as well as giving constant feedback!
         * */

        #region Variables and fields
        public static Menu VHRMenu { get; set; }

        public static Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>()
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q) },
            { SpellSlot.W, new Spell(SpellSlot.W) },
            { SpellSlot.E, new Spell(SpellSlot.E, 590f) },
            { SpellSlot.R, new Spell(SpellSlot.R) }
        };

        private static Spell TrinketSpell = new Spell(SpellSlot.Trinket);

        public static List<IVHRModule> VhrModules = new List<IVHRModule>()
        {
            new AutoEModule(),
            new EKSModule(),
            new LowLifePeel()
        };
        #endregion

        #region Initialization, Public Methods and operators
        public static void OnLoad()
        {
            LoadSpells();
            LoadModules();
            LoadEvents();

            TickLimiter.Add("CondemnLimiter", 250);
            TickLimiter.Add("ModulesLimiter", 300);
            TickLimiter.Add("ComboLimiter", 80);
        }

        private static void LoadSpells()
        {
            spells[SpellSlot.E].SetTargetted(0.25f, 2000f);
        }

        private static void LoadEvents()
        {
            Orbwalker.OnAction += Orbwalker_OnAction;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void LoadModules()
        {
            foreach (var module in VhrModules.Where(module => module.ShouldBeLoaded()))
            {
                try
                {
                    module.OnLoad();
                }
                catch (Exception exception)
                {
                    VHRDebug.WriteError(string.Format("Failed to load module! Module name: {0} - Exception: {1} ", module.GetModuleName(), exception));
                }
            }
        }
        #endregion

        #region Event Delegates
        private static void Game_OnUpdate(EventArgs args)
        {
            if (!TickLimiter.CanTick("ModulesLimiter"))
            {
                return;
            }

            Orbwalker.Attack = !VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.general"]["disableaa"].GetValue<MenuBool>().Value;
            Orbwalker.Movement = !VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.general"]["disablemovement"].GetValue<MenuBool>().Value;

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                        var condemnTarget = GetCondemnTarget(ObjectManager.Player.ServerPosition);
                        if (spells[SpellSlot.E].IsEnabledAndReady(OrbwalkerMode.Orbwalk) && condemnTarget.IsValidTarget())
                        {
                            spells[SpellSlot.E].Cast(condemnTarget);
                        }
                    break;
                case OrbwalkerMode.Hybrid:
                        var condemnTarget_Harass = GetCondemnTarget(ObjectManager.Player.ServerPosition);
                        if (spells[SpellSlot.E].IsEnabledAndReady(OrbwalkerMode.Hybrid) && condemnTarget_Harass.IsValidTarget())
                        {
                            spells[SpellSlot.E].Cast(condemnTarget_Harass);
                        }
                    break;
            }

            foreach (var Module in VhrModules.Where(module => module.ShouldRun()))
            {
                Module.Run();
            }

        }

        private static void Orbwalker_OnAction(object sender, Orbwalker.OrbwalkerActionArgs e)
        {
            switch (e.Type)
            {
                case OrbwalkerType.AfterAttack:
                    //AfterAttack Delegate. Q Spells Usage Here.
                    OnAfterAttack(e);
                    break;
                case OrbwalkerType.BeforeAttack:
                    //BeforeAttack Delegate, focus target with W stacks here.
                    OnBeforeAttack(e);
                    break;
            }
        }
        #endregion

        #region Private Methods and operators
        private static void OnAfterAttack(Orbwalker.OrbwalkerActionArgs e)
        {

            if (e.Target.IsValidTarget() && (e.Target is Obj_AI_Base))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Orbwalk:
                        PreliminaryQCheck((Obj_AI_Base) e.Target, OrbwalkerMode.Orbwalk);
                        break;
                    case OrbwalkerMode.Hybrid:
                        PreliminaryQCheck((Obj_AI_Base) e.Target, OrbwalkerMode.Hybrid);
                        break;
                }
            }
        }

        private static void OnBeforeAttack(Orbwalker.OrbwalkerActionArgs e)
        {
            /**
            if (VHRMenu["dz191.vhr.misc.general"]["specialfocus"].GetValue<MenuBool>().Value)
            {
                var currentTarget = e.Target;
                if (currentTarget is Obj_AI_Hero)
                {
                    var target = (Obj_AI_Hero) currentTarget;
                    var TwoStacksTarget = VHRExtensions.GetHeroWith2WStacks();
                    if (TwoStacksTarget != null && TwoStacksTarget != target)
                    {
                        Orbwalker.OrbwalkTarget = TwoStacksTarget;
                    }
                }
            }
             * */
        }
        #endregion

        #region Skills Usage

        #region Tumble

        private static void PreliminaryQCheck(Obj_AI_Base target, OrbwalkerMode mode)
        {
            ////TODO Try to reset AA faster by doing Q against a wall if possible

            if (spells[SpellSlot.Q].IsEnabledAndReady(mode))
            {
                if (VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.tumble"]["smartq"].GetValue<MenuBool>().Value && GetQEPosition() != Vector3.Zero)
                {
                    UseTumble(GetQEPosition(), target);

                    DelayAction.Add(
                        (int) (Game.Ping / 2f + spells[SpellSlot.Q].Delay * 1000 + 300f / 1200f + 50f), () =>
                        {
                            if (!spells[SpellSlot.Q].IsReady())
                            {
                                spells[SpellSlot.E].Cast(target);
                            }
                        });
                }
                else
                {
                    UseTumble(target);
                }
            }
        }

        #region Q-E Combo Calculation
        private static Vector3 GetQEPosition()
        {
            if (spells[SpellSlot.E].IsReady())
            {
                const int currentStep = 30;
                var direction = ObjectManager.Player.Direction.ToVector2().Perpendicular();
                for (var i = 0f; i < 360f; i += currentStep)
                {
                    var angleRad = (i) * (float)(Math.PI / 180f);
                    var rotatedPosition = ObjectManager.Player.Position.ToVector2() + (300f * direction.Rotated(angleRad));
                    if (GetCondemnTarget(rotatedPosition.ToVector3()) != null && rotatedPosition.ToVector3().IsSafePosition())
                    {
                        return rotatedPosition.ToVector3();
                    }
                }
                return Vector3.Zero;
            }
            return Vector3.Zero;

        }
        #endregion

        #region Tumble Overloads
        private static void UseTumble(Obj_AI_Base Target)
        {
            var Position = Game.CursorPos;
            var extendedPosition = ObjectManager.Player.ServerPosition.Extend(Position, 300f);
            var distanceAfterTumble = Vector3.DistanceSquared(extendedPosition, Target.ServerPosition);

            if (VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.tumble"]["limitQ"].GetValue<MenuBool>().Value)
            {
                if ((distanceAfterTumble <= 550 * 550 && distanceAfterTumble >= 100 * 100))
                {
                    if (extendedPosition.IsSafePosition() && extendedPosition.PassesNoQIntoEnemiesCheck())
                    {
                        RealQCast(extendedPosition);
                    }
                }
                else
                {
                    if (VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.tumble"]["qspam"].GetValue<MenuBool>().Value)
                    {
                        if (extendedPosition.IsSafePosition() && extendedPosition.PassesNoQIntoEnemiesCheck())
                        {
                            RealQCast(extendedPosition);
                        }
                    }
                }
            }
            else
            {
                if (extendedPosition.IsSafePosition() && extendedPosition.PassesNoQIntoEnemiesCheck())
                {
                    RealQCast(extendedPosition);
                }
            }
        }

        private static void UseTumble(Vector3 Position, Obj_AI_Base Target = null)
        {
            var extendedPosition = ObjectManager.Player.ServerPosition.Extend(Position, 300f);
            var distanceAfterTumble = Vector3.DistanceSquared(extendedPosition, Target.ServerPosition);

            if (VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.tumble"]["limitQ"].GetValue<MenuBool>().Value)
            {
                if ((distanceAfterTumble <= 550 * 550 && distanceAfterTumble >= 100 * 100))
                {
                    if (extendedPosition.IsSafePosition() && extendedPosition.PassesNoQIntoEnemiesCheck())
                    {
                        RealQCast(extendedPosition);
                    }
                }
                else
                {
                    if (VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.tumble"]["qspam"].GetValue<MenuBool>().Value)
                    {
                        if (extendedPosition.IsSafePosition() && extendedPosition.PassesNoQIntoEnemiesCheck())
                        {
                            RealQCast(extendedPosition);
                        }
                    }
                }
            }
            else
            {
                if (extendedPosition.IsSafePosition() && extendedPosition.PassesNoQIntoEnemiesCheck())
                {
                    RealQCast(extendedPosition);
                }
            }
        }

        private static void RealQCast(Vector3 Position)
        {
            spells[SpellSlot.Q].Cast(Position);
            DelayAction.Add((int)(Game.Ping / 2f + spells[SpellSlot.Q].Delay * 1000 + 300f / 1650f + 50f), Orbwalker.ResetAutoAttackTimer);
        }
        #endregion

        #endregion

        #region Condemn

        public static Obj_AI_Base GetCondemnTarget(Vector3 FromPosition)
        {
            if (TickLimiter.CanTick("CondemnLimiter"))
            {
                if (ObjectManager.Player.IsWindingUp)
                {
                    return null;
                }

                switch (VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["condemnmethod"].GetValue<MenuList<string>>().Index)
                {

                    case 0:
                        ////VHR SDK Condemn Method

                        if (!VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.general"]["lightweight"].GetValue<MenuBool>().Value)
                        {
                            #region VHR SDK Method (Non LW Method)
                            var HeroList =
                                GameObjects.EnemyHeroes.Where(
                                    h =>
                                        h.IsValidTarget(spells[SpellSlot.E].Range) &&
                                        !h.HasBuffOfType(BuffType.SpellShield) &&
                                        !h.HasBuffOfType(BuffType.SpellImmunity));
                            var NumberOfChecks =
                                VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["predictionNumber"].GetValue<MenuSlider>().Value;
                            var MinChecksPercent =
                                (VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["accuracy"].GetValue<MenuSlider>().Value);
                            var PushDistance =
                                VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["pushdistance"].GetValue<MenuSlider>().Value;
                            var NextPrediction =
                                (VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["nextprediction"].GetValue<MenuSlider>().Value);
                            var interval = NextPrediction / NumberOfChecks;
                            var currentInterval = interval;
                            var LastUnitPosition = Vector3.Zero;

                            foreach (var Hero in HeroList)
                            {
                                if (!TargetPassesCondemnExtraChecks(Hero))
                                {
                                    continue;
                                }

                                var PredictionsList = new List<Vector3>();

                                PredictionsList.Add(Hero.ServerPosition);

                                for (var i = 0; i < NumberOfChecks; i++)
                                {
                                    var Prediction = Movement.GetPrediction(Hero, currentInterval);
                                    var UnitPosition = Prediction.UnitPosition;
                                    if (UnitPosition.DistanceSquared(LastUnitPosition) >=
                                        Hero.BoundingRadius * Hero.BoundingRadius)
                                    {
                                        PredictionsList.Add(UnitPosition);
                                        LastUnitPosition = UnitPosition;
                                        currentInterval += interval;
                                    }
                                }

                                var ExtendedList = new List<Vector3>();

                                foreach (var position in PredictionsList)
                                {
                                    ExtendedList.Add(position.Extend(FromPosition, -PushDistance / 4f));
                                    ExtendedList.Add(position.Extend(FromPosition, -PushDistance / 2f));
                                    ExtendedList.Add(position.Extend(FromPosition, -(PushDistance * 0.75f)));
                                    ExtendedList.Add(position.Extend(FromPosition, -PushDistance));
                                }

                                var WallListCount = ExtendedList.Count(h => h.IsWall() || IsJ4Flag(h, Hero));

                                if (((float)WallListCount / (float)ExtendedList.Count) >= MinChecksPercent / 100f)
                                {
                                    return Hero;
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            #region VHR SDK Method (LW Method)
                            //// ReSharper disable once LoopCanBePartlyConvertedToQuery
                            foreach (
                                var target in
                                    GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(spells[SpellSlot.E].Range)))
                            {
                                var PushDistance =
                                VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["pushdistance"].GetValue<MenuSlider>().Value;
                                var FinalPosition = target.ServerPosition.Extend(FromPosition, -PushDistance);
                                var AlternativeFinalPosition = target.ServerPosition.Extend(FromPosition, -(PushDistance/2f));
                                if (FinalPosition.IsWall() || AlternativeFinalPosition.IsWall())
                                {
                                    return target;
                                }
                            }
                            #endregion
                        }
                        
                        break;
                    case 1:
                        ////Marksman/Gosu
                        
                        #region Marksman/Gosu Method
                        //// ReSharper disable once LoopCanBePartlyConvertedToQuery
                        foreach (
                            var target in
                                GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(spells[SpellSlot.E].Range)))
                        {
                            var PushDistance =
                            VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["pushdistance"].GetValue<MenuSlider>().Value;
                            var FinalPosition = target.ServerPosition.Extend(FromPosition, -PushDistance);
                            var AlternativeFinalPosition = target.ServerPosition.Extend(FromPosition, -(PushDistance / 2f));
                            if (FinalPosition.IsWall() || AlternativeFinalPosition.IsWall() || (IsJ4Flag(FinalPosition, target) || IsJ4Flag(AlternativeFinalPosition, target)))
                            {
                                return target;
                            }
                        }
                        #endregion

                        break;
                }
            }
            return null;
        }

        #region Condemn Utility Methods
        private static bool IsJ4Flag(Vector3 endPosition, Obj_AI_Base Target)
        {
            return VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["condemnflag"].GetValue<MenuBool>().Value && GameObjects.AllGameObjects.Any(m => m.DistanceSquared(endPosition) <= Target.BoundingRadius * Target.BoundingRadius && m.Name == "Beacon"); ;
        }

        private static bool TargetPassesCondemnExtraChecks(Obj_AI_Hero target)
        {
            var NoEAANumber = VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["noeaa"].GetValue<MenuSlider>().Value;

            if (target.Health <= ObjectManager.Player.GetAutoAttackDamage(target) * NoEAANumber)
            {
                return false;
            }

            var OnlyCurrent = VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["onlystuncurrent"].GetValue<MenuBool>().Value;

            if (OnlyCurrent && target != Orbwalker.OrbwalkTarget)
            {
                return false;
            }

            //noeturret

            var UnderEnemyTurretToggle = VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["noeturret"].GetValue<MenuBool>().Value;

            if (UnderEnemyTurretToggle && ObjectManager.Player.ServerPosition.IsUnderTurret(true))
            {
                return false;
            }

            return true;

        }
        #endregion

        #endregion

        #endregion

    }
}
