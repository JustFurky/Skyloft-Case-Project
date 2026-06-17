using System;

namespace SkyloftGame.Data
{
    [Serializable]
    public class GameData
    {
        public int totalEnemiesKilled;

        public int highestUnlockedLevel;

        public static GameData Default() => new GameData
        {
            totalEnemiesKilled   = 0,
            highestUnlockedLevel = 0
        };
    }
}
