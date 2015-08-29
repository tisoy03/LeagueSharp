using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;

namespace DZJayce
{
    class Jayce
    {
        #region
        public static Menu RootMenu { get; set; }

        public static Orbwalking.Orbwalker Orbwalker { get; set; }
        #endregion

        internal static void OnLoad()
        {
            LoadEvents();
            Console.WriteLine("Well hello there");
        }

        private static void LoadEvents()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling())
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                
            }
        }
    }
}
