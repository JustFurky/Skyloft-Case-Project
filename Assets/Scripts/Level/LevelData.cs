using UnityEngine;
using SkyloftGame.Pool;

namespace SkyloftGame.Level
{
    [CreateAssetMenu(menuName = "SkyloftGame/Level Data", fileName = "LevelData_New")]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("0-based level order. Must match the index in LevelDatabase.")]
        public int levelIndex;
        public string displayName = "Level 1";

        [Header("Duration")]
        [Tooltip("Survival time (seconds). The game is won when the time runs out.")]
        [Min(5f)] public float durationSeconds = 180f;

        [Header("Wave Spawning")]
        [Tooltip("Delay before the very first wave spawns (seconds).")]
        [Min(0f)] public float firstWaveDelay = 2f;

        [Tooltip("Seconds between waves. A new wave keeps spawning on this cadence, " +
                 "looping until the level timer runs out.")]
        [Min(1f)] public float timeBetweenWaves = 6f;

        [Tooltip("Drip interval between individual enemies inside a single wave (seconds).")]
        [Min(0f)] public float spawnInterval = 0.15f;

        [Tooltip("Enemy types and how many of each spawn EVERY wave. " +
                 "Add a row per enemy type; the same set is looped each wave until time runs out.")]
        public EnemySpawnEntry[] enemiesPerWave;

        [Header("Spawn Area")]
        [Tooltip("Outer radius of the ring around the player where enemies appear.")]
        [Min(1f)] public float spawnRadius = 14f;

        [Tooltip("Inner radius that prevents enemies from spawning right next to the player.")]
        [Min(1f)] public float minSpawnDistance = 7f;

        [Header("Optimization")]
        [Tooltip("Maximum number of live enemies allowed in the scene at once. " +
                 "Spawning pauses when the limit is reached, preserving performance during large waves.")]
        [Min(1)] public int maxAliveEnemies = 60;
    }

    [System.Serializable]
    public class EnemySpawnEntry
    {
        [Tooltip("Enemy pool used for this entry (PoolId asset).")]
        public PoolId enemy;

        [Tooltip("How many of this enemy spawn every wave.")]
        [Min(0)] public int count = 5;
    }
}
