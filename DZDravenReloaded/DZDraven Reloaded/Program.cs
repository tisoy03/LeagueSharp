using LeagueSharp.Common;

namespace DZDraven_Reloaded
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += DZDraven_Reloaded.Game_OnGameLoad;
        }
    }
}