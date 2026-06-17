using UnityEngine;

namespace SkyloftGame.Enemy
{
    /// <summary>
    /// Düşman animasyonlarını AI durumuna göre CrossFade ile sürer
    /// (Animator parametresi/transition gerektirmez). AI mantığından ayrıdır (SRP).
    ///
    ///   Chase  → Zombie Running
    ///   Attack → Zombie Punching
    ///
    /// Havuzdan her çıkışta (OnEnable) mevcut duruma göre yeniden senkronize olur.
    /// </summary>
    [RequireComponent(typeof(EnemyAI))]
    public class EnemyAnimator : MonoBehaviour
    {
        [Tooltip("Boşsa child'lardan bulunur (model genelde child objededir).")]
        [SerializeField] private Animator _animator;

        [Header("Animator State Adları")]
        [SerializeField] private string _runState    = "Zombie Running";
        [SerializeField] private string _attackState = "Zombie Punching";

        [SerializeField] private float _crossFade = 0.1f;

        private EnemyAI _ai;
        private int _runHash, _attackHash, _currentHash;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            _ai = GetComponent<EnemyAI>();

            _runHash    = Animator.StringToHash(_runState);
            _attackHash = Animator.StringToHash(_attackState);
        }

        private void OnEnable()
        {
            _currentHash = 0;                       // pool'dan çıkışta yeniden değerlendir
            _ai.OnStateChanged += Apply;
            Apply(_ai.CurrentState);
        }

        private void OnDisable() => _ai.OnStateChanged -= Apply;

        private void Apply(EnemyStateType state)
        {
            if (_animator == null) return;

            int desired = state == EnemyStateType.Attack ? _attackHash : _runHash;
            if (desired == _currentHash || !_animator.HasState(0, desired)) return;

            _animator.CrossFadeInFixedTime(desired, _crossFade);
            _currentHash = desired;
        }
    }
}
