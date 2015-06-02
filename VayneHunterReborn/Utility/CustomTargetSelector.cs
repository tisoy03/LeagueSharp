namespace VayneHunter_Reborn.Utility
{
    #region
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    #endregion

    internal class CustomTargetSelector
    {
        #region
        private List<PriorityClass> priorityList = new List<PriorityClass>()
        {
            new PriorityClass()
            {
                Name = "Highest",
                Champions = new []
                {
                    "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
                    "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Leblanc",
                    "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon",
                    "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath",
                    "Zed", "Ziggs"
                },
                Priority = Priority.Highest
            },

            new PriorityClass()
            {
                Name = "High",
                Champions = new []
                {
                    "Akali", "Diana", "Ekko", "Fiddlesticks", "Fiora", "Fizz", "Heimerdinger", "Jayce", "Kassadin",
                    "Kayle", "Kha'Zix", "Lissandra", "Mordekaiser", "Nidalee", "Riven", "Shaco", "Vladimir", "Yasuo",
                    "Zilean"
                },
                Priority = Priority.Highest
            },

            new PriorityClass()
            {
                Name = "Medium",
                Champions = new []
                {
                    "Aatrox", "Darius", "Elise", "Evelynn", "Galio", "Gangplank", "Gragas", "Irelia", "Jax",
                    "Lee Sin", "Maokai", "Morgana", "Nocturne", "Pantheon", "Poppy", "Rengar", "Rumble", "Ryze", "Swain",
                    "Trundle", "Tryndamere", "Udyr", "Urgot", "Vi", "XinZhao", "RekSai"
                },
                Priority = Priority.Medium
            },

            new PriorityClass()
            {
                Name = "Low",
                Champions = new []
                {
                    "Alistar", "Amumu", "Bard", "Blitzcrank", "Braum", "Cho'Gath", "Dr. Mundo", "Garen", "Gnar",
                    "Hecarim", "Janna", "Jarvan IV", "Leona", "Lulu", "Malphite", "Nami", "Nasus", "Nautilus", "Nunu",
                    "Olaf", "Rammus", "Renekton", "Sejuani", "Shen", "Shyvana", "Singed", "Sion", "Skarner", "Sona",
                    "Soraka", "Taric", "Thresh", "Volibear", "Warwick", "MonkeyKing", "Yorick", "Zac", "Zyra"
                },
                Priority = Priority.Low
            }
        };
        #endregion

        internal static void OnLoad()
        {
            
        }

        internal Obj_AI_Hero GetTarget(float Range)
        {
            var EnemiesInRange = ObjectManager.Player.GetEnemiesInRange(Range);
            var priorityDictionary = EnemiesInRange.Where(en => en.IsValidTarget(Range)).ToDictionary(Enemy => Enemy, Enemy => GetPriorityByName(Enemy.ChampionName));
            if (priorityDictionary.Any())
            {
                var HighestPriorityTarget = priorityDictionary.OrderBy(pair => pair.Value).First().Key;
                if (HighestPriorityTarget != null && HighestPriorityTarget.IsValidTarget(Range))
                {
                    var HighestPriority = priorityDictionary[HighestPriorityTarget];
                    var numberOfAttacks = HighestPriorityTarget.Health / ObjectManager.Player.GetAutoAttackDamage(HighestPriorityTarget);

                    foreach (var Item in priorityDictionary.Where(item => item.Key != HighestPriorityTarget))
                    {
                        if (Item.Value == HighestPriority)
                        {
                            var attacksNumber = HighestPriorityTarget.Health / ObjectManager.Player.GetAutoAttackDamage(Item.Key);
                            if (attacksNumber < numberOfAttacks)
                            {
                                numberOfAttacks = attacksNumber;
                                HighestPriorityTarget = Item.Key;
                            }
                        }
                    }
                    return HighestPriorityTarget;
                }  
            }
            
            return null;
        }

        internal Priority GetPriorityByName(string name)
        {
            if (priorityList.Any(m => m.Champions.Contains(name)))
            {
                return priorityList.First(m => m.Champions.Contains(name)).Priority;
            }

            return Priority.Low;
        }
    }

    internal class PriorityClass
    {
        public string Name { get; set; }

        public Priority Priority { get; set; }

        public string[] Champions { get; set; } 
    }

    enum Priority
    {
        Highest = 4, 
        High = 3, 
        Medium = 2, 
        Low = 1
    }
}
