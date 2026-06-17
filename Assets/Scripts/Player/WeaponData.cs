using UnityEngine;
using SkyloftGame.Pool;

namespace SkyloftGame.Player
{
    [CreateAssetMenu(menuName = "SkyloftGame/Weapon Data", fileName = "WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [Tooltip("Projectile pool to fire from.")]
        public PoolId projectile;

        [Tooltip("Number of projectiles fired per second.")]
        [Min(0.01f)] public float fireRate = 4f;

        [Tooltip("Projectile travel speed (units/second).")]
        [Min(0.1f)] public float projectileSpeed = 18f;
    }
}
