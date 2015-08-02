using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.Utils;

namespace Thresh___Soul_Hunter
{
    #region
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using LeagueSharp.SDK.Core.Wrappers;
    using System.Windows.Forms;
    using LeagueSharp.SDK.Core.Extensions.SharpDX;
    using LeagueSharp.SDK.Core.Math.Prediction;
    using SharpDX;
    using Utility;
    using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;
    #endregion

    internal class Thresh
    {
        #region Fields
        public static Menu RootMenu { get; set; }

        public static Obj_AI_Hero HookedUnit { get; set; }

        public static float HookEndTick { get; set; }

        public const string MenuPrefix = "dz191.thresh.";

        private static readonly Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
        {
            {Spells.Q, new Spell(SpellSlot.Q, 1100f)},
            {Spells.W, new Spell(SpellSlot.Q, 920f)},
            {Spells.E, new Spell(SpellSlot.Q, 400f)},
            {Spells.R, new Spell(SpellSlot.R, 440f)},
        };

        #endregion

        internal static void OnLoad()
        {
            LoadMenu();
            LoadSkills();
            Obj_AI_Base.OnBuffAdd += OnBuffAdd;
            Game.OnUpdate += (args) => { OnUpdate(); };
            Orbwalker.OnAction += Orbwalker_OnAction;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(20f, 20f, System.Drawing.Color.White, GetQState().ToString());
        }

        #region Combo, Harass, Update Methods

        private static void OnCombo()
        {
            
            var target = HookedUnit ?? TargetSelector.GetTarget(spells[Spells.Q].Range, DamageType.Magical);
            if (spells[Spells.E].IsEnabledAndReady(OrbwalkerMode.Orbwalk) && target.IsValidTarget() &&
                (GameObjects.Player.DistanceSquared(target) <= Math.Pow(spells[Spells.E].Range, 2)))
            {
                switch (GetEMode())
                {
                    case EMode.Pull:
                        CastFlayPull(target, OrbwalkerMode.Orbwalk);
                        break;
                    case EMode.Push:
                        CastFlayPush(target, OrbwalkerMode.Orbwalk);
                        break;
                }
            }

           var QStage = GetQState();
           if (spells[Spells.Q].IsEnabledAndReady(OrbwalkerMode.Orbwalk) && target.IsValidTarget(spells[Spells.Q].Range))
           {
                 switch (QStage)
                 {
                     case QStates.Q1:
                         var prediction = spells[Spells.Q].GetPrediction(target);
                         if (prediction.Hitchance >= HitChance.VeryHigh)
                         {
                             var endPosition = prediction.CastPosition;
                             
                             spells[Spells.Q].Cast(endPosition);
                         }
                         break;
                     case QStates.Q2:
                         Console.WriteLine("Casted 2");

                         ////vvv TODO Redundant and just for debugging purpouses. Removed it for final release.
                         if (target == HookedUnit && HookEndTick - Variables.TickCount < 650 && IsSafePosition(target.ServerPosition))
                         {
                             ////TODO Lantern ally in before flying if lantern is enabled (Thresh Pain train)

                            // spells[Spells.Q].Cast();
                         }
                         break;
                     case QStates.Unknown:
                         Console.WriteLine("Q Spell State is unknown?!");
                         break;
                 }
            }
            
        }

        private static void OnHarass()
        {

        }

        private static void CastLantern(LanternMode Mode)
        {
            
        }

        private static void OnUpdate()
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            //threshqfakeknockup -> The buff gained as soon as the enemy is hooked
            //ThreshQ -> The Buff gained when the enemy has the hook on him

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                    OnCombo();
                    break;
                case OrbwalkerMode.Hybrid:
                    OnHarass();
                    break;

            }
        }
        #endregion

        #region Event Delegates
        private static void Orbwalker_OnAction(object sender, Orbwalker.OrbwalkerActionArgs e)
        {
            switch (e.Type)
            {
                case OrbwalkerType.BeforeAttack:
                    if (ObjectManager.Player.GetBuffCount("talentreaperdisplay") == 0 && Orbwalker.ActiveMode == OrbwalkerMode.Hybrid)
                    {
                        e.Process = false;
                    }
                    break;
                case OrbwalkerType.AfterAttack:
                    //TODO
                    break;
            }
        }

        private static void OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if ((sender is Obj_AI_Hero) && sender.IsValidTarget(float.MaxValue, false))
            {
                var h = (Obj_AI_Hero)sender;
                Console.WriteLine("{0} - {1}",h.ChampionName, args.Buff.Name);
            }

            if ((sender is Obj_AI_Hero) && sender.IsValidTarget() && (args.Buff.Name == "threshqfakeknockup" || args.Buff.Name == "ThreshQ"))
            {
                HookedUnit = (Obj_AI_Hero)sender;
                HookEndTick = Variables.TickCount + 1500f;
                DelayAction.Add((1500+ Game.Ping +250), () =>
                {
                    HookEndTick = 0;
                    HookedUnit = null;
                });
            }
        }
        #endregion

        #region Utility Methods
        private static bool IsSafePosition(Vector3 position)
        {
            return true;
        }

        private static EMode GetEMode()
        {
            switch (RootMenu[MenuPrefix + "misc"]["defaultEMode"].GetValue<MenuList>().Index)
            {
                case 0:
                    return EMode.Pull;
                case 1:
                    return EMode.Pull;
                default:
                    return EMode.Push;
            }
        }

        private static QStates GetQState()
        {
            if (!spells[Spells.Q].IsReady())
            {
                return QStates.NotReady;
            }
            
            switch (spells[Spells.Q].Instance.Name)
            {
                case "ThreshQ":
                    return QStates.Q1;
                case "threshqleap":
                    if (HookedUnit != null)
                    {
                        return QStates.Q2;
                    }
                    return QStates.Q1;
                default:
                    return QStates.Unknown;
            }
        }

        private static void CastFlayPush(Obj_AI_Hero target, OrbwalkerMode Mode)
        {
            if (target.IsValidTarget() && RootMenu[MenuPrefix + Mode.ToString().ToLowerInvariant()]["useE"].GetValue<MenuBool>().Value)
            {
                var targetPrediction = Movement.GetPrediction(target, 0.25f);
                if (targetPrediction.UnitPosition.DistanceSquared(ObjectManager.Player.ServerPosition) <=
                    Math.Pow(spells[Spells.E].Range, 2))
                {
                    var finalPosition = targetPrediction.UnitPosition.Extend(
                        ObjectManager.Player.ServerPosition,
                        ObjectManager.Player.ServerPosition.Distance(targetPrediction.UnitPosition) / 2f);
                    spells[Spells.E].Cast(finalPosition);
                }
            }
        }

        private static void CastFlayPull(Obj_AI_Hero target, OrbwalkerMode Mode)
        {
            if (target.IsValidTarget(spells[Spells.E].Range) && RootMenu[MenuPrefix + Mode.ToString().ToLowerInvariant()]["useE"].GetValue<MenuBool>().Value)
            {
                var targetPrediction = Movement.GetPrediction(target, 0.25f);

                    var finalPosition = 
                        targetPrediction.UnitPosition.Extend(
                            ObjectManager.Player.ServerPosition,
                            ObjectManager.Player.Distance(targetPrediction.UnitPosition) + 100f
                            );
                    spells[Spells.E].Cast(finalPosition);
            }
        }

        private static Polygon getERectangle(Vector3 finalPosition, float BoundingRadius)
        {
            var halfERange = 150f;
            var eRectangle = new Polygon(
                Polygon.Rectangle(
                    ObjectManager.Player.ServerPosition.ToVector2(), 
                    ObjectManager.Player.ServerPosition.ToVector2().Extend(finalPosition.ToVector2(), halfERange), 
                    BoundingRadius)
                );
            return eRectangle;
        }
        #endregion

        #region Menu, Skills, Events

        private static void LoadSkills()
        {
            spells[Spells.Q].SetSkillshot(0.500f, 60f, 1900f, true, SkillshotType.SkillshotLine);
        }

        private static void LoadMenu()
        {
            RootMenu = new Menu("dz191.thresh", "Thresh - Soul Hunter", true);

            MenuGenerator.Generate(RootMenu);
        }
        #endregion
    }

    internal enum Spells
    {
        Q, W, E, R
    }

    internal enum QStates
    {
        Q1, Q2, NotReady, Unknown
    }

    internal enum EMode
    {
        Pull, Push
    }

    internal enum LanternMode
    {
        LowestHealth, Closest
    }
}
