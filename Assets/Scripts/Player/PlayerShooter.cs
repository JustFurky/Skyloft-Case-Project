using UnityEngine;
using SkyloftGame.Audio;
using SkyloftGame.Combat;
using SkyloftGame.Pool;
using SkyloftGame.StateMachine;

namespace SkyloftGame.Player
{
    [RequireComponent(typeof(PlayerTargeting))]
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Weapon")]
        [Tooltip("Data holding fire rate, projectile speed and the projectile pool key.")]
        [SerializeField] private WeaponData _weapon;

        [Header("Fire")]
        [SerializeField] private Transform _muzzle;

        [Header("VFX (optional)")]
        [SerializeField] private ParticleSystem _muzzleFlash;

        private PlayerTargeting _targeting;
        private float _cooldown;

        private void Awake()
        {
            _targeting = GetComponent<PlayerTargeting>();
            if (_weapon == null)
                Debug.LogError("[PlayerShooter] WeaponData is not assigned; cannot fire.", this);
        }

        private void Update()
        {
            if (_weapon == null) return;
            if (GameStateManager.Instance == null ||
                GameStateManager.Instance.CurrentState != GameStateType.Playing)
                return;

            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            var target = _targeting.CurrentTarget;
            if (target == null) return;

            FireAt(target.position);
            _cooldown = 1f / Mathf.Max(0.01f, _weapon.fireRate);
        }

        private void FireAt(Vector3 targetPosition)
        {
            Vector3 origin    = _muzzle != null ? _muzzle.position : transform.position;
            Vector3 direction = targetPosition - origin;
            direction.y = 0f;
            direction.Normalize();
            if (direction.sqrMagnitude < 0.001f) return;

            var projectile = ObjectPooler.Instance.Get<Projectile>(
                _weapon.projectile, origin, Quaternion.LookRotation(direction, Vector3.up));
            projectile?.Launch(direction, _weapon.projectileSpeed);

            if (_muzzleFlash != null) _muzzleFlash.Play();
            AudioEvents.Play(AudioCue.Shoot);
        }
    }
}
