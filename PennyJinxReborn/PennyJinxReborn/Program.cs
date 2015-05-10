using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace PennyJinxReborn
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnLoad;
        }

        private static void GameOnLoad(EventArgs args)
        {
            PJR.OnLoad();
        }
    }
}
