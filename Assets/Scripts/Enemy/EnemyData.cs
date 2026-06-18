using UnityEngine;
using SkyloftGame.VFX;

namespace SkyloftGame.Enemy
{
    [CreateAssetMenu(menuName = "SkyloftGame/Enemy Data", fileName = "EnemyData_New")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique name of this enemy type.")]
        public string enemyName = "Enemy";

        [Header("Health")]
        [Min(1f)] public float maxHp        = 50f;
        [Min(0f)] public float armor        = 0f;

        [Header("Movement (NavMesh)")]
        [Min(0.1f)] public float moveSpeed          = 3.5f;
        [Min(0.1f)] public float angularSpeed       = 120f;
        [Min(0f)]   public float acceleration       = 8f;
        [Min(0f)]   public float stoppingDistance   = 1.2f;

        [Tooltip("How often (in seconds) to recalculate the target position. " +
                 "Lower value = more precise but more expensive. Recommended: 0.1-0.3")]
        [Range(0.05f, 1f)]
        public float pathUpdateInterval = 0.2f;

        [Header("Attack")]
        [Min(0f)] public float attackDamage    = 10f;
        [Min(0f)] public float attackRange     = 1.5f;
        [Min(0.1f)] public float attackCooldown = 1f;

        [Header("VFX")]
        public VfxSpawn spawnVfx = new();
        public VfxSpawn deathVfx = new() { upOffset = 0.5f };

        [Header("Hit Flash")]
        [Tooltip("Tint applied briefly when the enemy takes damage (slight red).")]
        public Color hitFlashColor = new(1f, 0.5f, 0.5f, 1f);

        [Tooltip("How long the hit flash lasts before returning to the base color (seconds).")]
        [Min(0f)] public float hitFlashDuration = 0.08f;

        [Header("Reward")]
        [Min(0)]  public int scoreValue       = 10;
        [Min(0f)] public float dropChance     = 0.2f;
    }
}
