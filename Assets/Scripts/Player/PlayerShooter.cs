using UnityEngine;
using SkyloftGame.Combat;
using SkyloftGame.Enemy;
using SkyloftGame.Pool;
using SkyloftGame.StateMachine;

namespace SkyloftGame.Player
{
    /// <summary>
    /// En yakın düşmana otomatik nişan alıp ateş eden silah denetleyicisi.
    /// Mermiler ObjectPooler'dan alınır (Instantiate/Destroy yok) — büyük çatışmalarda
    /// GC ve oluşturma maliyetini ortadan kaldırır.
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Pool")]
        [SerializeField] private string _projectilePoolKey = "Projectile";

        [Header("Ateş")]
        [SerializeField] private Transform _muzzle;
        [SerializeField] private float _fireRate    = 4f;    // saniyede mermi
        [SerializeField] private float _range        = 12f;
        [SerializeField] private float _projectileSpeed = 18f;

        [Header("Animasyon / VFX (opsiyonel)")]
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private Animator _animator;

        private static readonly int FireHash = Animator.StringToHash("Fire");

        private float _cooldown;

        private void Update()
        {
            if (GameStateManager.Instance == null ||
                GameStateManager.Instance.CurrentState != GameStateType.Playing)
                return;

            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            var target = EnemyRegistry.FindNearest(transform.position, _range);
            if (target == null) return;

            FireAt(target.transform.position);
            _cooldown = 1f / Mathf.Max(0.01f, _fireRate);
        }

        private void FireAt(Vector3 targetPosition)
        {
            Vector3 origin    = _muzzle != null ? _muzzle.position : transform.position;
            Vector3 direction = (targetPosition - origin);
            direction.y = 0f;
            direction.Normalize();

            // Gövdeyi hedefe çevir (nişan hissi).
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            var projectile = ObjectPooler.Instance.Get<Projectile>(
                _projectilePoolKey, origin, Quaternion.LookRotation(direction, Vector3.up));
            projectile?.Launch(direction, _projectileSpeed);

            if (_muzzleFlash != null) _muzzleFlash.Play();
            if (_animator != null)    _animator.SetTrigger(FireHash);
        }
    }
}
