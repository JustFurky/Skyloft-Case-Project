using UnityEngine;

namespace SkyloftGame.Player
{
    /// <summary>
    /// Oyuncunun menzilindeki en yakın düşmanı bulan algılama bileşeni.
    ///
    /// Physics.OverlapSphereNonAlloc kullanır: yarıçap içindeki çarpıştırıcıları
    /// önceden ayrılmış bir tampona yazar — çerçeve başına çöp (GC) üretmez ve
    /// fiziğin broadphase'i sayesinde yalnızca yakındaki adaylar değerlendirilir.
    /// (Radyal algılama için doğru araç budur; SphereCast ışın boyunca süpürme içindir.)
    ///
    /// Tek bir algılama kaynağı olarak hem nişan alma (PlayerController) hem de
    /// ateş etme (PlayerShooter) bunu paylaşır (DRY).
    /// </summary>
    public class PlayerTargeting : MonoBehaviour
    {
        [Tooltip("Hedef arama yarıçapı.")]
        [SerializeField] private float _range = 12f;

        [Tooltip("Hedef olarak kabul edilecek katman(lar) — Enemy.")]
        [SerializeField] private LayerMask _targetLayers;

        [Tooltip("Aynı anda değerlendirilecek maksimum aday (tampon boyutu).")]
        [Min(1)] [SerializeField] private int _maxCandidates = 32;

        /// <summary>O an kilitlenilen en yakın hedef; yoksa null.</summary>
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
