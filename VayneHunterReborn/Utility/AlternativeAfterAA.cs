using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace VayneHunter_Reborn.Utility
{
    class AlternativeAfterAA
    {
        public delegate void OnAADelegate(Obj_AI_Base sender);
        public static event OnAADelegate OnAlternativeAfterAA;

        static AlternativeAfterAA()
        {
            GameObject.OnCreate += Obj_SpellMissile_OnCreate;
        }

        static void Obj_SpellMissile_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is Obj_SpellMissile)
            {
                var missile = (Obj_SpellMissile)sender;
                if (Orbwalking.IsAutoAttack(missile.SData.Name) && (missile.SpellCaster is Obj_AI_Hero))
                {
                    //Game.PrintChat("AfterAA by " + missile.SpellCaster);
                    var resolveTest = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(h => h.Distance(missile.EndPosition) <= 60);
                    if (resolveTest != null)
                    {
                       // Game.PrintChat("ResolveTest: " + resolveTest.ChampionName);
                        Console.WriteLine("Not null");

                    }
                    if (OnAlternativeAfterAA != null)
                    {
                        OnAlternativeAfterAA(missile.SpellCaster);
                    }
                }
            }
        }
    }
}
