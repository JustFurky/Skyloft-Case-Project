using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SkyloftGame.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        /// <summary>Davranış durumu değiştiğinde yayınlanır (animasyon köprüsü için).</summary>
        public event Action<EnemyStateType> OnStateChanged;

        public NavMeshAgent Agent => _agent;

        private NavMeshAgent _agent;
        private EnemyData    _data;
        private Transform    _target;
        private Coroutine    _pathLoop;

        // Oyuncu bu kare mesafesinden az hareket ettiyse path yeniden hesaplanmaz (~0.2 m²)
        private const float MinMoveSqr = 0.04f;

        private Vector3        _lastDestination;
        private WaitForSeconds _pathWait;

        public EnemyStateType CurrentState { get; private set; } = EnemyStateType.Chase;

        private void Awake() => _agent = GetComponent<NavMeshAgent>();

        public void Init(EnemyData data, Transform target)
        {
            _data   = data;
            _target = target;

            _agent.speed            = data.moveSpeed;
            _agent.angularSpeed     = data.angularSpeed;
            _agent.acceleration     = data.acceleration;
            _agent.stoppingDistance = data.stoppingDistance;
            _agent.autoBraking      = true;
            _pathWait               = new WaitForSeconds(data.pathUpdateInterval);
        }

        public void StartAI()
        {
            if (_pathLoop != null) return;
            CurrentState = EnemyStateType.Chase;

            // isStopped yalnızca ajan NavMesh üstündeyken çağrılabilir; aksi halde hata verir.
            if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
                _agent.isStopped = false;

            _pathLoop = StartCoroutine(PathUpdateLoop());
        }

        public void StopAI()
        {
            if (_pathLoop != null) { StopCoroutine(_pathLoop); _pathLoop = null; }

            if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }
        }

        public void SetTarget(Transform newTarget)
        {
            _target          = newTarget;
            _lastDestination = Vector3.positiveInfinity;
        }

        private void Update()
        {
            if (_pathLoop == null || _data == null) return;

            // Saldırı hasarı artık Update'te değil, punch animasyonunun temas karesinden
            // (Animation Event → DealAttackDamage) tetiklenir.
            if (CurrentState == EnemyStateType.Chase)
                SetDestinationIfMoved();
        }

        private IEnumerator PathUpdateLoop()
        {
            while (true)
            {
                if (_target != null) EvaluateState();
                yield return _pathWait;
            }
        }

        private void EvaluateState()
        {
            float distSqr   = (transform.position - _target.position).sqrMagnitude;
            float attackSqr = _data.attackRange * _data.attackRange;
            TransitionTo(distSqr <= attackSqr ? EnemyStateType.Attack : EnemyStateType.Chase);
        }

        private void TransitionTo(EnemyStateType next)
        {
            if (CurrentState == next) return;
            CurrentState = next;
            OnStateChanged?.Invoke(next);

            if (next == EnemyStateType.Chase)
            {
                _agent.isStopped = false;
                _lastDestination = Vector3.positiveInfinity;
                SetDestinationIfMoved();
            }
            else
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }
        }

        private void SetDestinationIfMoved()
        {
            if (_target == null) return;
            Vector3 pos = _target.position;
            if ((pos - _lastDestination).sqrMagnitude < MinMoveSqr) return;
            if (!NavMesh.SamplePosition(pos, out NavMeshHit hit, 2f, NavMesh.AllAreas)) return;
            _agent.SetDestination(hit.position);
            _lastDestination = pos;
        }

        /// <summary>
        /// Saldırı hasarını uygular. Punch animasyonundaki temas karesine eklenen
        /// Animation Event tarafından (EnemyAttackAnimationRelay üzerinden) çağrılır.
        /// Yalnızca Attack durumundayken ve hedef hâlâ menzildeyken hasar verir.
        /// </summary>
        public void DealAttackDamage()
        {
            if (CurrentState != EnemyStateType.Attack || _target == null || _data == null) return;

            // Animasyon sürerken hedef uzaklaşmış olabilir; küçük toleransla menzil kontrolü.
            float reach = _data.attackRange * 1.2f;
            if ((transform.position - _target.position).sqrMagnitude > reach * reach) return;

            if (_target.TryGetComponent<IDamageable>(out var dmg))
                dmg.TakeDamage(_data.attackDamage);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_data == null) return;
            UnityEditor.Handles.color = new Color(1f, 0.2f, 0.2f, 0.15f);
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, _data.attackRange);
            UnityEditor.Handles.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _data.attackRange);
        }
#endif
    }
}
