using UnityEngine;
using SkyloftGame.Audio;
using SkyloftGame.Pool;

namespace SkyloftGame.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PooledObject))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        [Tooltip("Projectile parameters (damage, lifetime, collision layer, hit VFX).")]
        [SerializeField] private ProjectileData _data;

        private const float FallbackLifeTime = 3f;

        private Rigidbody    _rb;
        private PooledObject _pooledObject;
        private Vector3      _velocity;

        private void Awake()
        {
            _rb             = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.useGravity  = false;
            _pooledObject   = GetComponent<PooledObject>();

            if (_data == null)
                Debug.LogError("[Projectile] ProjectileData not assigned; projectile deals no damage.", this);
        }

        public void OnSpawn()
            => _pooledObject.ReleaseAfter(_data != null ? _data.lifeTime : FallbackLifeTime);

        public void OnDespawn() => _velocity = Vector3.zero;

        public void Launch(Vector3 direction, float speed)
            => _velocity = direction.normalized * speed;

        private void FixedUpdate()
            => _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);

        private void OnTriggerEnter(Collider other)
        {
            if (_data == null) return;
            if ((_data.hitLayers.value & (1 << other.gameObject.layer)) == 0) return;

            if (other.TryGetComponent<IDamageable>(out var damageable))
                damageable.TakeDamage(_data.damage);

            AudioEvents.Play(AudioCue.Hit);
            _data.hitVfx.Play(other.transform);
            _pooledObject.Release();
        }
    }
}
