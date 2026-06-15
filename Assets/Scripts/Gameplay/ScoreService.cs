using System;
using UnityEngine;
using SkyloftGame.Data;
using SkyloftGame.Enemy;
using EnemyUnit = SkyloftGame.Enemy.Enemy;

namespace SkyloftGame.Gameplay
{
    /// <summary>
    /// Düşman ölümlerini tek noktadan dinleyip hem tur içi hem kalıcı sayacı günceller.
    ///
    /// Enemy artık DataManager'a doğrudan bağlı değildir; yalnızca
    /// EnemyRegistry.Killed olayını yayınlar. Skor mantığının tek sorumlusu burasıdır
    /// (SRP) ve kalıcı kayıt detayı DataManager'a delege edilir.
    /// </summary>
    public class ScoreService : MonoBehaviour, IScoreService
    {
        public int RunKills   { get; private set; }
        public int TotalKills => DataManager.Instance != null ? DataManager.Instance.Data.totalEnemiesKilled : 0;

        public event Action<int> OnRunKillsChanged;

        private void OnEnable()  => EnemyRegistry.Killed += HandleEnemyKilled;
        private void OnDisable() => EnemyRegistry.Killed -= HandleEnemyKilled;

        public void ResetRun()
        {
            RunKills = 0;
            OnRunKillsChanged?.Invoke(RunKills);
        }

        private void HandleEnemyKilled(EnemyUnit enemy)
        {
            RunKills++;
            DataManager.Instance?.AddEnemyKill();   // kalıcı toplam
            OnRunKillsChanged?.Invoke(RunKills);
        }
    }
}
