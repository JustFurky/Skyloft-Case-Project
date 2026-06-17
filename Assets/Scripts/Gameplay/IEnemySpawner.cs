using UnityEngine;
using SkyloftGame.Level;

namespace SkyloftGame.Gameplay
{
    /// <summary>
    /// Seviye dalgalarını üreten ve temizleyen spawner sözleşmesi.
    /// </summary>
    public interface IEnemySpawner
    {
        int AliveCount { get; }

        /// <summary>Tüm dalgalar üretildi VE sahnede canlı düşman kalmadı mı?
        /// (Süre dolmadan erken zafer koşulu.)</summary>
        bool IsCleared { get; }

        /// <summary>Verilen seviyenin tüm dalgalarını hedef etrafında üretmeye başlar.</summary>
        void BeginLevel(LevelData level, Transform target);

        /// <summary>Spawn'ı durdurur ve sahnedeki tüm canlı düşmanları pool'a iade eder.</summary>
        void StopAndClear();
    }
}
