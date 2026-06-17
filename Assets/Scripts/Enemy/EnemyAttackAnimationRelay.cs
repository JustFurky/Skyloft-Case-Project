using UnityEngine;

namespace SkyloftGame.Enemy
{
    public class EnemyAttackAnimationRelay : MonoBehaviour
    {
        [Tooltip("If empty, found automatically from the parent chain.")]
        [SerializeField] private EnemyAI _ai;

        private void Awake()
        {
            if (_ai == null) _ai = GetComponentInParent<EnemyAI>();
        }

        public void OnAttackHit() => _ai?.DealAttackDamage();
    }
}
