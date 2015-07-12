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
using LeagueSharp.SDK.Core.Wrappers;
using SharpDX;
using VHR_SDK.Interfaces;
using VHR_SDK.Modules;
using VHR_SDK.Utility;

namespace VHR_SDK
{
    using LeagueSharp.SDK.Core.UI.IMenu;

    class VHR
    {
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
            new TestModule()
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
            if (e.Target.IsValidTarget() && e.Sender.IsMe && (e.Target is Obj_AI_Base))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Orbwalk:
                        PreliminaryQCheck((Obj_AI_Base)e.Target, OrbwalkerMode.Orbwalk);
                        break;

                }
            }
        }

        private static void OnBeforeAttack(Orbwalker.OrbwalkerActionArgs e)
        {

        }
        #endregion

        #region Skills Usage

        #region Tumble

        private static void PreliminaryQCheck(Obj_AI_Base target, OrbwalkerMode mode)
        {
            if (spells[SpellSlot.Q].IsEnabledAndReady(mode))
            {
                TryToQE(target);
            }
        }

        private static void TryToQE(Obj_AI_Base Target)
        {
            
        }

        private static void UseTumble()
        {
            
        }

        #endregion

        #region Condemn

        private static Obj_AI_Base GetCondemnTarget(Vector3 FromPosition)
        {
            if (TickLimiter.CanTick("CondemnLimiter"))
            {
                switch (VHRMenu["dz191.vhr.misc.condemn"]["condemnmethod"].GetValue<MenuList<string>>().Index)
                {
                    case 0:
                        //VHR SDK
                        #region VHR SDK Method

                        var HeroList = GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(spells[SpellSlot.E].Range) && !h.HasBuffOfType(BuffType.SpellShield) && !h.HasBuffOfType(BuffType.SpellImmunity));
                        var NumberOfChecks = VHRMenu["dz191.vhr.misc.condemn"]["predictionNumber"].GetValue<MenuSlider>().Value;
                        var MinChecksPercent = (VHRMenu["dz191.vhr.misc.condemn"]["accuracy"].GetValue<MenuSlider>().Value);
                        var PushDistance = VHRMenu["dz191.vhr.misc.condemn"]["pushdistance"].GetValue<MenuSlider>().Value;
                        var NextPrediction = (VHRMenu["dz191.vhr.misc.condemn"]["nextprediction"].GetValue<MenuSlider>().Value);
                        var PredictionsList = new List<Vector3>();
                        var interval = NextPrediction / NumberOfChecks;
                        var currentInterval = interval;
                        var LastUnitPosition = Vector3.Zero;

                        foreach (var Hero in HeroList)
                        {
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

                            var WallListCount = ExtendedList.Count(h => h.IsWall());
                            var TotalListCount = ExtendedList.Count();
                            if ((WallListCount / TotalListCount) * 100 >= MinChecksPercent)
                            {
                                return Hero;
                            }
                        }
                        #endregion

                        break;
                    case 1:
                        //Marksman/Gosu
                        #region VH/Gosu Method

                        #endregion
                        break;
                }
            }
            return null;
        }
        #endregion

        #endregion

    }
}
