using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SkyloftGame.Enemy;
using SkyloftGame.Level;
using SkyloftGame.Pool;
using EnemyUnit = SkyloftGame.Enemy.Enemy;

namespace SkyloftGame.Gameplay
{
    /// <summary>
    /// LevelData'daki dalgaları, oyuncunun etrafındaki NavMesh halkasında üretir.
    ///
    /// Optimizasyon notları:
    ///   - Düşmanlar ObjectPooler üzerinden alınır (Instantiate yok).
    ///   - LevelData.maxAliveEnemies sınırı dolduğunda spawn duraklatılır; bu,
    ///     büyük dalgalarda hem fizik hem AI yükünü tavanda tutar.
    ///   - Canlı sayısı EnemyRegistry'den O(1) okunur.
    /// </summary>
    public class EnemySpawner : MonoBehaviour, IEnemySpawner
    {
        [Tooltip("NavMesh üzerinde geçerli spawn noktası ararken kullanılan örnekleme yarıçapı.")]
        [SerializeField] private float _navSampleRadius = 3f;

        private readonly List<Coroutine> _waveRoutines = new();
        private LevelData _level;
        private Transform _target;
        private int _totalWaves;
        private int _completedWaves;

        public int AliveCount => EnemyRegistry.AliveCount;

        // Tüm dalgalar üretilmiş ve canlı düşman kalmamışsa seviye temizlenmiştir.
        // _totalWaves > 0 koşulu, spawn başlamadan (canlı 0 iken) yanlış zaferi önler.
        public bool IsCleared =>
            _totalWaves > 0 && _completedWaves >= _totalWaves && AliveCount == 0;

        public void BeginLevel(LevelData level, Transform target)
        {
            StopAndClear();

            if (level == null || target == null)
            {
                Debug.LogError("[EnemySpawner] Seviye veya hedef null; spawn başlatılamadı.", this);
                return;
            }

            _level          = level;
            _target         = target;
            _totalWaves     = level.waves?.Length ?? 0;
            _completedWaves = 0;

            if (level.waves == null) return;
            foreach (var wave in level.waves)
                _waveRoutines.Add(StartCoroutine(RunWave(wave)));
        }

        public void StopAndClear()
        {
            foreach (var routine in _waveRoutines)
                if (routine != null) StopCoroutine(routine);
            _waveRoutines.Clear();

            // Sayaçları sıfırla ki durdurulduğunda IsCleared yanlışlıkla true olmasın.
            _totalWaves     = 0;
            _completedWaves = 0;

            // Canlı düşmanları pool'a iade et (snapshot — iade Unregister tetikler).
            foreach (var enemy in EnemyRegistry.Snapshot())
                if (enemy != null) enemy.ReturnToPool();
        }

        private IEnumerator RunWave(Wave wave)
        {
            if (wave.startDelay > 0f)
                yield return new WaitForSeconds(wave.startDelay);

            var interval = new WaitForSeconds(wave.spawnInterval);

            for (int i = 0; i < wave.count; i++)
            {
                // Canlı limiti aşıldıysa yer açılana kadar bekle (optimizasyon kapısı).
                while (AliveCount >= _level.maxAliveEnemies)
                    yield return null;

                SpawnOne(wave.enemyPoolKey);
                yield return interval;
            }

            _completedWaves++;   // bu dalganın tüm düşmanları üretildi
        }

        private void SpawnOne(string poolKey)
        {
            if (!TryGetSpawnPosition(out Vector3 position)) return;

            // Doğar doğmaz oyuncuya dönük gelsin (NavMesh'in yavaş dönüşünü beklemeden).
            Vector3 toTarget = _target.position - position;
            toTarget.y = 0f;
            Quaternion rotation = toTarget.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(toTarget)
                : Quaternion.identity;

            var enemy = ObjectPooler.Instance.Get<EnemyUnit>(poolKey, position, rotation);
            enemy?.SetTarget(_target);   // hedefi enjekte et (FindWithTag yerine)
        }

        /// <summary>Oyuncu etrafındaki halkada NavMesh'e oturan rastgele bir nokta bulur.</summary>
        private bool TryGetSpawnPosition(out Vector3 position)
        {
            Vector2 ring   = Random.insideUnitCircle.normalized *
                             Random.Range(_level.minSpawnDistance, _level.spawnRadius);
            Vector3 candidate = _target.position + new Vector3(ring.x, 0f, ring.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _navSampleRadius, NavMesh.AllAreas))
            {
                position = hit.position;
                return true;
            }

            position = candidate;
            return false;
        }
    }
}
