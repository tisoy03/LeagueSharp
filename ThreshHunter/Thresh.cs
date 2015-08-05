using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using ThreshHunter.Interfaces;

namespace ThreshHunter
{
    class Thresh
    {
        #region Fields
        public static Menu RootMenu { get; set; }

        public static Orbwalking.Orbwalker Orbwalker;

        public static readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 1100f)},
            {SpellSlot.W, new Spell(SpellSlot.Q, 920f)},
            {SpellSlot.E, new Spell(SpellSlot.Q, 400f)},
            {SpellSlot.R, new Spell(SpellSlot.R, 440f)},
        };

       

        public static Obj_AI_Hero HookedUnit { get; set; }

        public static float HookEndTick { get; set; }

        public static List<ITHModule> THModules = new List<ITHModule>() { };

        #endregion

        #region Initializations, Skills, Events
        public static void Init()
        {
            LoadSkills();
            LoadEvents();
        }

        private static void LoadSkills()
        {
            spells[SpellSlot.Q].SetSkillshot(0.500f, 60f, 1900f, true, SkillshotType.SkillshotLine);
        }

        private static void LoadEvents()
        {
            Obj_AI_Base.OnBuffAdd += OnBuffAdd;
            Game.OnUpdate += (args) => { OnUpdate(); };
            Orbwalking.BeforeAttack += OW_BeforeAttack;
            Drawing.OnDraw += OnDraw;
        }

        
        #endregion

        #region Event Delegates
        private static void OnUpdate()
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
            }
        }

        private static void OW_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void OnDraw(EventArgs args)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Combo

        private static void Combo()
        {
            
        }
        #endregion

    }
}
