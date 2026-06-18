using System;
using UnityEngine;
using Zenject;
using SkyloftGame.Data;
using SkyloftGame.Enemy;
using EnemyUnit = SkyloftGame.Enemy.Enemy;

namespace SkyloftGame.Gameplay
{
    public class ScoreService : MonoBehaviour, IScoreService
    {
        [Inject] private DataManager _data;

        public int RunKills   { get; private set; }
        public int TotalKills => _data != null ? _data.Data.totalEnemiesKilled : 0;

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
            _data?.AddEnemyKill();
            OnRunKillsChanged?.Invoke(RunKills);
        }
    }
}
