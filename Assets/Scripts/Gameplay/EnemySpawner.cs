using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using SkyloftGame.Enemy;
using SkyloftGame.Level;
using SkyloftGame.Pool;
using EnemyUnit = SkyloftGame.Enemy.Enemy;

namespace SkyloftGame.Gameplay
{
    public class EnemySpawner : MonoBehaviour, IEnemySpawner
    {
        [Tooltip("Sampling radius used when searching for a valid spawn point on the NavMesh.")]
        [SerializeField] private float _navSampleRadius = 3f;

        [Tooltip("How many candidate points to try when finding a valid NavMesh spawn point.")]
        [SerializeField] private int _spawnPositionAttempts = 8;

        [Tooltip("How many seconds before a wave spawns the HUD countdown warning appears. " +
                 "The wait itself is LevelData.timeBetweenWaves; only the last N seconds are shown.")]
        [Min(0)] [SerializeField] private int _warnSecondsBeforeWave = 3;

        private CancellationTokenSource _cts;
        private LevelData _level;
        private Transform _target;

        public event Action<int> WaveCountdownTick;
        public event Action       WaveStarted;

        public int AliveCount => EnemyRegistry.AliveCount;

        private void OnDestroy() => CancelRun();

        public void BeginLevel(LevelData level, Transform target)
        {
            StopAndClear();

            if (level == null || target == null)
            {
                Debug.LogError("[EnemySpawner] Level or target is null; spawning could not start.", this);
                return;
            }

            _level  = level;
            _target = target;

            if (!HasSpawnableEntries(level))
            {
                Debug.LogWarning($"[EnemySpawner] '{level.displayName}' has no enemy entries; nothing will spawn.", this);
                return;
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            RunLevelAsync(_cts.Token).Forget();
        }

        public void StopAndClear()
        {
            CancelRun();

            WaveStarted?.Invoke();   // hide any HUD countdown

            foreach (var enemy in EnemyRegistry.Snapshot())
                if (enemy != null) enemy.ReturnToPool();
        }

        private void CancelRun()
        {
            if (_cts == null) return;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        // Spawns waves on a fixed time cadence and loops them until the level ends.
        // The owning state (GameWon/GameLost/Menu) calls StopAndClear, which cancels this token.
        private async UniTaskVoid RunLevelAsync(CancellationToken token)
        {
            await UniTask.Yield(token);

            await CountdownAsync(_level.firstWaveDelay, token);

            while (!token.IsCancellationRequested)
            {
                await SpawnWaveAsync(token);
                await CountdownAsync(_level.timeBetweenWaves, token);
            }
        }

        // Waits the full duration, but only fires the HUD "next wave" warning during the
        // last _warnSecondsBeforeWave seconds; then signals the wave start.
        private async UniTask CountdownAsync(float seconds, CancellationToken token)
        {
            for (int s = Mathf.FloorToInt(seconds); s > 0; s--)
            {
                if (s <= _warnSecondsBeforeWave)
                    WaveCountdownTick?.Invoke(s);
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            }
            WaveStarted?.Invoke();
        }

        // One wave = every configured enemy entry, "count" copies each, dripped by spawnInterval.
        private async UniTask SpawnWaveAsync(CancellationToken token)
        {
            int maxAlive = Mathf.Max(1, _level.maxAliveEnemies);
            var entries  = _level.enemiesPerWave;
            if (entries == null) return;

            foreach (var entry in entries)
            {
                if (entry == null || entry.enemy == null || entry.count <= 0) continue;

                for (int i = 0; i < entry.count; i++)
                {
                    await UniTask.WaitUntil(() => AliveCount < maxAlive, cancellationToken: token);

                    SpawnOne(entry.enemy);

                    if (_level.spawnInterval > 0f)
                        await UniTask.Delay(TimeSpan.FromSeconds(_level.spawnInterval), cancellationToken: token);
                }
            }
        }

        private static bool HasSpawnableEntries(LevelData level)
        {
            if (level.enemiesPerWave == null) return false;
            foreach (var entry in level.enemiesPerWave)
                if (entry != null && entry.enemy != null && entry.count > 0) return true;
            return false;
        }

        private void SpawnOne(PoolId pool)
        {
            if (pool == null || !TryGetSpawnPosition(out Vector3 position)) return;
            if (ObjectPooler.Instance == null) return;

            Vector3 toTarget = _target.position - position;
            toTarget.y = 0f;
            Quaternion rotation = toTarget.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(toTarget)
                : Quaternion.identity;

            var enemy = ObjectPooler.Instance.Get<EnemyUnit>(pool, position, rotation);
            enemy?.SetTarget(_target);
        }

        private bool TryGetSpawnPosition(out Vector3 position)
        {
            int attempts = Mathf.Max(1, _spawnPositionAttempts);
            for (int i = 0; i < attempts; i++)
            {
                Vector2 ring      = UnityEngine.Random.insideUnitCircle.normalized *
                                    UnityEngine.Random.Range(_level.minSpawnDistance, _level.spawnRadius);
                Vector3 candidate = _target.position + new Vector3(ring.x, 0f, ring.y);

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _navSampleRadius, NavMesh.AllAreas))
                {
                    position = hit.position;
                    return true;
                }
            }

            position = _target.position;
            return false;
        }
    }
}
