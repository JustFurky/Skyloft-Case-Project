using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace SkyloftGame.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        public event Action<EnemyStateType> OnStateChanged;

        public NavMeshAgent Agent => _agent;

        private NavMeshAgent _agent;
        private EnemyData    _data;
        private Transform    _target;
        private CancellationTokenSource _aiCts;

        private const float MinMoveSqr = 0.04f;

        private Vector3 _lastDestination;

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
        }

        public void StartAI()
        {
            if (_aiCts != null) return;
            CurrentState = EnemyStateType.Chase;

            if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
                _agent.isStopped = false;

            _aiCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            PathUpdateLoopAsync(_aiCts.Token).Forget();
        }

        public void StopAI()
        {
            if (_aiCts != null) { _aiCts.Cancel(); _aiCts.Dispose(); _aiCts = null; }

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
            if (_aiCts == null || _data == null) return;

            if (CurrentState == EnemyStateType.Chase)
                SetDestinationIfMoved();
        }

        private async UniTaskVoid PathUpdateLoopAsync(CancellationToken token)
        {
            while (true)
            {
                if (_target != null) EvaluateState();
                await UniTask.Delay(TimeSpan.FromSeconds(_data.pathUpdateInterval),
                                    cancellationToken: token);
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

        public void DealAttackDamage()
        {
            if (CurrentState != EnemyStateType.Attack || _target == null || _data == null) return;

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
