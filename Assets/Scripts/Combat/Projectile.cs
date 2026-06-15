using UnityEngine;
using SkyloftGame.Pool;

namespace SkyloftGame.Combat
{
    /// <summary>
    /// 3B havuzlanan mermi. Düz hareket eder, hedef katmana çarpınca hasar verir,
    /// isabet VFX'i tetikler ve pool'a geri döner.
    ///
    /// Tetikleme algılaması için kinematik Rigidbody + trigger Collider gerekir
    /// (NavMeshAgent'lı düşmanların Rigidbody'si yoktur).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PooledObject))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float     _damage    = 25f;
        [SerializeField] private float     _lifeTime  = 3f;
        [SerializeField] private LayerMask _hitLayers;

        [Tooltip("İsabet anında oynatılacak VFX pool anahtarı (boş bırakılabilir).")]
        [SerializeField] private string _hitVfxKey = "HitVfx";

        private Rigidbody    _rb;
        private PooledObject _pooledObject;
        private Vector3      _velocity;

        private void Awake()
        {
            _rb               = GetComponent<Rigidbody>();
            _rb.isKinematic   = true;
            _rb.useGravity    = false;
            _pooledObject     = GetComponent<PooledObject>();
        }

        public void OnSpawn()  => _pooledObject.ReleaseAfter(_lifeTime);
        public void OnDespawn() => _velocity = Vector3.zero;

        public void Launch(Vector3 direction, float speed)
            => _velocity = direction.normalized * speed;

        private void FixedUpdate()
            => _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);

        private void OnTriggerEnter(Collider other)
        {
            if ((_hitLayers.value & (1 << other.gameObject.layer)) == 0) return;

            if (other.TryGetComponent<IDamageable>(out var damageable))
                damageable.TakeDamage(_damage);

            SpawnHitVfx();
            _pooledObject.Release();
        }

        private void SpawnHitVfx()
        {
            if (string.IsNullOrEmpty(_hitVfxKey) || ObjectPooler.Instance == null) return;
            ObjectPooler.Instance.Get(_hitVfxKey, transform.position, Quaternion.identity);
        }
    }
}
