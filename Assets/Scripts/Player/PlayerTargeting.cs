using UnityEngine;

namespace SkyloftGame.Player
{
    public class PlayerTargeting : MonoBehaviour
    {
        [Tooltip("Target search radius.")]
        [SerializeField] private float _range = 12f;

        [Tooltip("Layer(s) accepted as targets — Enemy.")]
        [SerializeField] private LayerMask _targetLayers;

        [Tooltip("Maximum candidates evaluated at once (buffer size).")]
        [Min(1)] [SerializeField] private int _maxCandidates = 32;

        public Transform CurrentTarget { get; private set; }

        public float Range => _range;

        private Collider[] _buffer;

        private void Awake() => _buffer = new Collider[_maxCandidates];

        private void Update() => CurrentTarget = FindNearest();

        private Transform FindNearest()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, _range, _buffer, _targetLayers, QueryTriggerInteraction.Ignore);

            Transform nearest = null;
            float bestSqr = float.MaxValue;
            Vector3 origin = transform.position;

            for (int i = 0; i < count; i++)
            {
                var col = _buffer[i];
                if (col == null) continue;

                float sqr = (col.transform.position - origin).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = col.transform;
                }
            }

            return nearest;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, _range);
        }
#endif
    }
}
