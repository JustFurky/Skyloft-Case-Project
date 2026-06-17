using UnityEngine;

namespace SkyloftGame.Enemy
{
    [RequireComponent(typeof(EnemyAI))]
    public class EnemyAnimator : MonoBehaviour
    {
        [Tooltip("If empty, found in children (the model is usually a child object).")]
        [SerializeField] private Animator _animator;

        [Header("Data")]
        [Tooltip("Animator state names and transition duration.")]
        [SerializeField] private EnemyAnimationData _animData;

        private EnemyAI _ai;
        private int _runHash, _attackHash, _currentHash;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            _ai = GetComponent<EnemyAI>();

            if (_animData == null)
            {
                Debug.LogError("[EnemyAnimator] EnemyAnimationData is not assigned.", this);
                return;
            }

            _runHash    = Animator.StringToHash(_animData.runState);
            _attackHash = Animator.StringToHash(_animData.attackState);
        }

        private void OnEnable()
        {
            _currentHash = 0;
            _ai.OnStateChanged += Apply;
            Apply(_ai.CurrentState);
        }

        private void OnDisable() => _ai.OnStateChanged -= Apply;

        private void Apply(EnemyStateType state)
        {
            if (_animator == null || _animData == null) return;

            int desired = state == EnemyStateType.Attack ? _attackHash : _runHash;
            if (desired == _currentHash || !_animator.HasState(0, desired)) return;

            _animator.CrossFadeInFixedTime(desired, _animData.crossFade);
            _currentHash = desired;
        }
    }
}
