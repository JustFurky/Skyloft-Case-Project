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

        [Header("Enemy Waves")]
        public Wave[] waves;

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
    public class Wave
    {
        [Tooltip("Enemy pool used by this wave (PoolId asset).")]
        public PoolId enemy;

        [Min(1)] public int count = 10;

        [Tooltip("Spawn interval between enemies in the same wave (seconds). Smaller = faster flow.")]
        [Min(0.02f)] public float spawnInterval = 0.1f;

        [Tooltip("How many seconds after the level start this wave is triggered.")]
        [Min(0f)] public float startDelay = 0f;
    }
}
