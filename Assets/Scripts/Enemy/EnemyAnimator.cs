using UnityEngine;

namespace SkyloftGame.Enemy
{
    /// <summary>
    /// Düşman AI durumunu Animator parametrelerine bağlayan köprü (SRP).
    /// AI mantığı ile görsel mantık ayrıdır; bu bileşen yalnızca animasyon parametresi yazar.
    ///
    /// Beklenen Animator parametreleri:
    ///   - float "Speed"  : ajanın hız büyüklüğü (Idle/Run blend)
    ///   - trigger "Attack": saldırı durumuna geçiş
    ///   - trigger "Die"   : ölüm
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(EnemyAI))]
    [RequireComponent(typeof(Enemy))]
    public class EnemyAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash  = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int DieHash    = Animator.StringToHash("Die");

        private Animator _animator;
        private EnemyAI  _ai;
        private Enemy    _enemy;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _ai       = GetComponent<EnemyAI>();
            _enemy    = GetComponent<Enemy>();
        }

        private void OnEnable()
        {
            _ai.OnStateChanged += HandleStateChanged;
            _enemy.OnDied      += HandleDied;
        }

        private void OnDisable()
        {
            _ai.OnStateChanged -= HandleStateChanged;
            _enemy.OnDied      -= HandleDied;
        }

        private void Update()
        {
            if (_ai.Agent != null)
                _animator.SetFloat(SpeedHash, _ai.Agent.velocity.magnitude);
        }

        private void HandleStateChanged(EnemyStateType state)
        {
            if (state == EnemyStateType.Attack) _animator.SetTrigger(AttackHash);
        }

        private void HandleDied(Enemy _) => _animator.SetTrigger(DieHash);
    }
}
