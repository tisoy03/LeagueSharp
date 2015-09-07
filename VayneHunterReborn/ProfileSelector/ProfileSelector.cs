using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VayneHunter_Reborn.ProfileSelector
{
    class ProfileSelector
    {

    }

    internal class ProfileSettings
    {
        public String ProfileName { get; set; }

        public List<ProfileOption> Options { get; set; } 
    }

    internal class ProfileOption
    {
        public MajorCategories MajorCategory { get; set; }

        public MinorCategories MinorCategory { get; set; }

        public String ExtraOptions { get; set; }

        public Value Value { get; set; }

    }

    internal enum MajorCategories
    {
        Tumble, Condemn, General
    }

    internal enum MinorCategories
    {
        QLogic, SmartQ, NoAAStealth, NoQEnemies, DynamicQSafety, QSpam, QInRange, Mirin, WallTumble

    }

    internal enum ValueTypes
    {
        Keybind, Boolean, Slider
    }

    abstract class Value
    {
        public abstract ValueTypes GetValueType();

        public abstract T GetValue<T>();

    }

    /**
    internal class SliderValue : Value
    {
        public override ValueTypes GetValueType()
        {
            return ValueTypes.Slider;
        }

        public override T GetValue<T>()
        {
            return null;
            //return new Tuple<int, int, int>(0, 0, 0);
        }
    }
    */
}
