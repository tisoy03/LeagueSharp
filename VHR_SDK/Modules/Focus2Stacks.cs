using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using VHR_SDK.Utility.Helpers;

namespace VHR_SDK.Modules
{
    using LeagueSharp;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using Interfaces;
    using Utility;

    class Focus2Stacks : IVHRModule
    {
        public bool ShouldBeLoaded()
        {
            return true;
        }

        public string GetModuleName()
        {
            return "Focus Targets with 2 W marks Module";
        }

        public void OnLoad()
        {
            VHRDebug.WriteInfo("Focus Targets with 2 W marks Module loaded successfully!");
        }

        public bool ShouldRun()
        {
            return VHR.VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.general"]["specialfocus"].GetValue<MenuBool>().Value && (Orbwalker.ActiveMode == OrbwalkerMode.Hybrid || Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk);
        }

        public void Run()
        {
            var TwoStacksTarget = VHRExtensions.GetHeroWith2WStacks();
            if (TwoStacksTarget != null && TwoStacksTarget.IsValidTarget(ObjectManager.Player.AttackRange + 65) &&
                TwoStacksTarget != Orbwalker.OrbwalkTarget && !ObjectManager.Player.IsWindingUp)
            {
                Orbwalker.OrbwalkTarget = TwoStacksTarget;
            }
        }
    }
}
