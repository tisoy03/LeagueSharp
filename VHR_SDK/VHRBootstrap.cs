using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.SDK.Core;
using VHR_SDK.Interfaces;
using VHR_SDK.Utility;

namespace VHR_SDK
{
    class VHRBootstrap
    {
        public static void Init()
        {
            MenuGenerator.SetUp();
            VHR.OnLoad();
        }
    }
}
