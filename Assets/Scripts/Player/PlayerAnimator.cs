using UnityEngine;

namespace SkyloftGame.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        [Tooltip("If empty, found in children (the model is usually a child object).")]
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterController _cc;
        [SerializeField] private PlayerTargeting _targeting;

        [Header("Data")]
        [Tooltip("Animator state names and transition settings.")]
        [SerializeField] private PlayerAnimationData _animData;

        private int _idleHash, _runHash, _fireHash, _currentHash;

        private void Awake()
        {
            if (_animator == null)  _animator  = GetComponentInChildren<Animator>();
            if (_cc == null)        _cc        = GetComponent<CharacterController>();
            if (_targeting == null) _targeting = GetComponent<PlayerTargeting>();

            if (_animData == null)
            {
                Debug.LogError("[PlayerAnimator] PlayerAnimationData is not assigned.", this);
                return;
            }

            _idleHash = Animator.StringToHash(_animData.idleState);
            _runHash  = Animator.StringToHash(_animData.runState);
            _fireHash = Animator.StringToHash(_animData.fireState);
        }

        private void Update()
        {
            if (_animator == null || _animData == null) return;

            int desired = ResolveState();
            if (desired == _currentHash) return;
            if (!_animator.HasState(0, desired)) return;

            _animator.CrossFadeInFixedTime(desired, _animData.crossFade);
            _currentHash = desired;
        }

        private int ResolveState()
        {
            if (IsMoving())                       return _runHash;
            if (_targeting != null && _targeting.CurrentTarget != null) return _fireHash;
            return _idleHash;
        }

        private bool IsMoving()
        {
            if (_cc == null) return false;
            Vector3 v = _cc.velocity;
            v.y = 0f;
            return v.magnitude > _animData.moveThreshold;
        }
    }
}
