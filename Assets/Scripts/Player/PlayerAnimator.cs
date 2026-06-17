using UnityEngine;

namespace SkyloftGame.Player
{
    /// <summary>
    /// Oyuncu animasyonlarını Animator state'leri arasında CrossFade ile sürer
    /// (Animator parametresi/transition gerektirmez). Hangi state'in oynayacağına
    /// hareket ve hedef durumuna bakarak karar verir (SRP: yalnızca görsel mantık).
    ///
    /// Öncelik:
    ///   hareket ediyorsa → Rifle Run
    ///   duruyor + hedef varsa → Firing Rifle
    ///   aksi halde → Rifle Idle
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Tooltip("Boşsa child'lardan bulunur (model genelde child objededir).")]
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterController _cc;
        [SerializeField] private PlayerTargeting _targeting;

        [Header("Animator State Adları")]
        [SerializeField] private string _idleState = "Rifle Idle";
        [SerializeField] private string _runState  = "Rifle Run";
        [SerializeField] private string _fireState = "Firing Rifle";

        [Header("Ayar")]
        [SerializeField] private float _crossFade     = 0.1f;
        [SerializeField] private float _moveThreshold = 0.15f;

        private int _idleHash, _runHash, _fireHash, _currentHash;

        private void Awake()
        {
            if (_animator == null)  _animator  = GetComponentInChildren<Animator>();
            if (_cc == null)        _cc        = GetComponent<CharacterController>();
            if (_targeting == null) _targeting = GetComponent<PlayerTargeting>();

            _idleHash = Animator.StringToHash(_idleState);
            _runHash  = Animator.StringToHash(_runState);
            _fireHash = Animator.StringToHash(_fireState);
        }

        private void Update()
        {
            if (_animator == null) return;

            int desired = ResolveState();
            if (desired == _currentHash) return;
            if (!_animator.HasState(0, desired)) return;

            _animator.CrossFadeInFixedTime(desired, _crossFade);
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
            return v.magnitude > _moveThreshold;
        }
    }
}
