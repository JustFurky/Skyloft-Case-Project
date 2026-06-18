using UnityEngine;
using SkyloftGame.VFX;

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
        public VfxSpawn hitVfx = new() { forwardOffset = 0.1f, upOffset = 0.5f, faceOrigin = true };
    }
}
