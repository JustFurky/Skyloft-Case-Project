using UnityEngine;
using SkyloftGame.Pool;

namespace SkyloftGame.Combat
{
    [CreateAssetMenu(menuName = "SkyloftGame/Projectile Data", fileName = "ProjectileData")]
    public class ProjectileData : ScriptableObject
    {
        [Header("Damage")]
        [Min(0f)] public float damage = 25f;

        [Header("Lifetime")]
        [Tooltip("Time the projectile lives before returning to the pool (seconds).")]
        [Min(0.05f)] public float lifeTime = 3f;

        [Header("Collision")]
        [Tooltip("Layers the projectile can damage (usually Enemy).")]
        public LayerMask hitLayers;

        [Header("Hit VFX")]
        [Tooltip("VFX pool played on hit (can be left empty).")]
        public PoolId hitVfx;

        [Tooltip("Distance the hit VFX is placed in front of the enemy (forward direction), in units.")]
        public float hitVfxForwardOffset = 0.1f;
    }
}
