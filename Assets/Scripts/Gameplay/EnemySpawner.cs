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

        [Tooltip("Between-wave countdown and blink settings.")]
        [SerializeField] private WaveSettings _waveSettings;

        private CancellationTokenSource _cts;
        private LevelData _level;
        private Transform _target;
        private int _totalWaves;
        private int _completedWaves;

        public event Action<int> WaveCountdownTick;
        public event Action       WaveStarted;

        public int AliveCount => EnemyRegistry.AliveCount;

        public bool IsCleared =>
            _totalWaves > 0 && _completedWaves >= _totalWaves && AliveCount == 0;

        private void OnDestroy() => CancelRun();

        public void BeginLevel(LevelData level, Transform target)
        {
            StopAndClear();

            if (level == null || target == null)
            {
                Debug.LogError("[EnemySpawner] Level or target is null; spawning could not start.", this);
                return;
            }

            _level          = level;
            _target         = target;
            _totalWaves     = level.waves?.Length ?? 0;
            _completedWaves = 0;

            if (_totalWaves == 0) return;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            RunLevelAsync(_cts.Token).Forget();
        }

        public void StopAndClear()
        {
            CancelRun();

            _totalWaves     = 0;
            _completedWaves = 0;

            WaveStarted?.Invoke();

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

        private async UniTaskVoid RunLevelAsync(CancellationToken token)
        {
            await UniTask.Yield(token);

            for (int i = 0; i < _level.waves.Length; i++)
            {
                if (ShouldCountdownBefore(i))
                    await CountdownAsync(_waveSettings.betweenWaveCountdown, token);
                else
                    WaveStarted?.Invoke();

                await SpawnWaveAsync(_level.waves[i], token);
                await UniTask.WaitUntil(() => AliveCount == 0, cancellationToken: token);

                _completedWaves++;
            }
        }

        private bool ShouldCountdownBefore(int waveIndex)
            => _waveSettings != null
               && _waveSettings.betweenWaveCountdown > 0
               && (waveIndex > 0 || _waveSettings.countdownBeforeFirstWave);

        private async UniTask CountdownAsync(int seconds, CancellationToken token)
        {
            for (int s = seconds; s > 0; s--)
            {
                WaveCountdownTick?.Invoke(s);
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            }
            WaveStarted?.Invoke();
        }

        private async UniTask SpawnWaveAsync(Wave wave, CancellationToken token)
        {
            if (wave.startDelay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(wave.startDelay), cancellationToken: token);

            for (int i = 0; i < wave.count; i++)
            {
                await UniTask.WaitUntil(() => AliveCount < _level.maxAliveEnemies, cancellationToken: token);

                SpawnOne(wave.enemy);
                await UniTask.Delay(TimeSpan.FromSeconds(wave.spawnInterval), cancellationToken: token);
            }
        }

        private void SpawnOne(PoolId pool)
        {
            if (pool == null || !TryGetSpawnPosition(out Vector3 position)) return;

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
