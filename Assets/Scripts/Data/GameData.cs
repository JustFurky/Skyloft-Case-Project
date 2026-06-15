using System;

namespace SkyloftGame.Data
{
    /// <summary>
    /// Oyun genelinde kalıcı tutulacak tek veri modeli.
    /// Yeni alan eklemek için yalnızca bu sınıfı değiştirmek yeterlidir.
    /// </summary>
    [Serializable]
    public class GameData
    {
        /// <summary>Tüm zamanlardaki toplam öldürülen düşman (kalıcı).</summary>
        public int totalEnemiesKilled;

        /// <summary>Kalıcı olarak açılmış en yüksek seviye index'i (0 tabanlı).</summary>
        public int highestUnlockedLevel;

        public static GameData Default() => new GameData
        {
            totalEnemiesKilled   = 0,
            highestUnlockedLevel = 0
        };
    }
}
