using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Muramana
{
    internal class Program
    {
        public static int Muramana = 3042;
        private static Menu _menu;
        private static Obj_AI_Hero _target1;
        private static Dictionary<Obj_SpellMissile, Obj_AI_Hero> _objList;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _menu = new Menu("Muramana Activator", "MMAct", true);
            _menu.AddItem(new MenuItem("useM", "Use Muramana Activator").SetValue(true));
            Game.PrintChat("Muramana Activator By DZ191 Loaded.");

            Orbwalking.OnAttack += OrbwalkingOnAtk;
            GameObject.OnCreate += Obj_SpellMissile_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private static void OrbwalkingOnAtk(AttackableUnit unit, AttackableUnit target)
        {
            var mur = Items.HasItem(Muramana) ? 3042 : 3043;
            if (ObjectManager.Get<Obj_AI_Hero>().Contains(target) && (Items.HasItem(mur)) &&
                (_menu.Item("useM").GetValue<bool>()) && (Items.CanUseItem(mur)))
            {
                Items.UseItem(mur);
                _target1 = (Obj_AI_Hero) target;
            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            var spellMissile = sender as Obj_SpellMissile;
            if (spellMissile == null || spellMissile.IsValid)
            {
                return;
            }

            var missile = spellMissile;
            if (!_objList.ContainsKey(missile))
            {
                return;
            }

            var mur = Items.HasItem(Muramana) ? 3042 : 3043;
            if (_target1.IsValid && ObjectManager.Get<Obj_AI_Hero>().Contains(_objList[missile]) && (Items.HasItem(mur)) &&
                (Items.CanUseItem(mur)) && (_menu.Item("useM").GetValue<bool>()))
            {
                Items.UseItem(mur);
            }

            _objList.Remove(missile);
        }

        private static void Obj_SpellMissile_OnCreate(GameObject sender, EventArgs args)
        {
            var spellMissile = sender as Obj_SpellMissile;
            if (spellMissile == null || !spellMissile.IsValid)
            {
                return;
            }

            var missile = spellMissile;
            if (!(missile.SpellCaster is Obj_AI_Hero) || !missile.SpellCaster.IsMe || !missile.SpellCaster.IsValid ||
                !Orbwalking.IsAutoAttack(missile.SData.Name))
            {
                return;
            }

            _objList.Add(missile, _target1);
            _target1 = null;
        }
    }
}