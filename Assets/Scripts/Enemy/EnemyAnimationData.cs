using UnityEngine;

namespace SkyloftGame.Enemy
{
    [CreateAssetMenu(menuName = "SkyloftGame/Enemy Animation Data", fileName = "EnemyAnimationData")]
    public class EnemyAnimationData : ScriptableObject
    {
        [Header("Animator State Names")]
        public string runState    = "Zombie Running";
        public string attackState = "Zombie Punching";

        [Header("Settings")]
        [Min(0f)] public float crossFade = 0.1f;
    }
}
