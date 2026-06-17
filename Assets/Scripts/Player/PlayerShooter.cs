using UnityEngine;
using SkyloftGame.Combat;
using SkyloftGame.Pool;
using SkyloftGame.StateMachine;

namespace SkyloftGame.Player
{
    /// <summary>
    /// PlayerTargeting'in kilitlediği hedefe otomatik ateş eden silah denetleyicisi.
    /// Hedef algılama PlayerTargeting'e aittir (tek kaynak); burada yalnızca ateş
    /// mantığı vardır (SRP). Mermiler ObjectPooler'dan alınır (Instantiate/Destroy yok).
    /// Nişan (gövdenin hedefe dönmesi) PlayerController tarafından sürdürülür.
    /// </summary>
    [RequireComponent(typeof(PlayerTargeting))]
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Pool")]
        [SerializeField] private string _projectilePoolKey = "Projectile";

        [Header("Ateş")]
        [SerializeField] private Transform _muzzle;
        [SerializeField] private float _fireRate        = 4f;    // saniyede mermi
        [SerializeField] private float _projectileSpeed = 18f;

        [Header("VFX (opsiyonel)")]
        [SerializeField] private ParticleSystem _muzzleFlash;

        private PlayerTargeting _targeting;
        private float _cooldown;

        private void Awake() => _targeting = GetComponent<PlayerTargeting>();

        private void Update()
        {
            if (GameStateManager.Instance == null ||
                GameStateManager.Instance.CurrentState != GameStateType.Playing)
                return;

            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            var target = _targeting.CurrentTarget;
            if (target == null) return;

            FireAt(target.position);
            _cooldown = 1f / Mathf.Max(0.01f, _fireRate);
        }

        private void FireAt(Vector3 targetPosition)
        {
            Vector3 origin    = _muzzle != null ? _muzzle.position : transform.position;
            Vector3 direction = targetPosition - origin;
            direction.y = 0f;
            direction.Normalize();
            if (direction.sqrMagnitude < 0.001f) return;

            var projectile = ObjectPooler.Instance.Get<Projectile>(
                _projectilePoolKey, origin, Quaternion.LookRotation(direction, Vector3.up));
            projectile?.Launch(direction, _projectileSpeed);

            if (_muzzleFlash != null) _muzzleFlash.Play();
        }
    }
}
