using System;
using UnityEngine;
using SkyloftGame.Data;
using SkyloftGame.Enemy;
using EnemyUnit = SkyloftGame.Enemy.Enemy;

namespace SkyloftGame.Gameplay
{
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
            DataManager.Instance?.AddEnemyKill();
            OnRunKillsChanged?.Invoke(RunKills);
        }
    }
}
