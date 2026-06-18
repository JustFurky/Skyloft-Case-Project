using UnityEngine;
using UnityEngine.AI;
using Zenject;
using SkyloftGame.Audio;
using SkyloftGame.Combat;
using SkyloftGame.Gameplay;
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

        [Tooltip("Layers that block the shot (e.g. EnvironmentBlocker). " +
                 "If anything on these layers is between muzzle and target, the player holds fire.")]
        [SerializeField] private LayerMask _shotBlockers;

        [Header("VFX (optional)")]
        [SerializeField] private ParticleSystem _muzzleFlash;

        private GameStateManager _game;
        private PauseController  _pause;

        private PlayerTargeting _targeting;
        private float _cooldown;

        [Inject]
        private void Construct(GameStateManager game, PauseController pause)
        {
            _game  = game;
            _pause = pause;
        }

        private void Awake()
        {
            _targeting = GetComponent<PlayerTargeting>();
            if (_weapon == null)
                Debug.LogError("[PlayerShooter] WeaponData is not assigned; cannot fire.", this);
        }

        private void Update()
        {
            if (_weapon == null) return;
            if (_game == null || _game.CurrentState != GameStateType.Playing) return;
            if (_pause != null && _pause.IsPaused) return;

            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            var target = _targeting.CurrentTarget;
            if (target == null) return;

            Vector3 origin = _muzzle != null ? _muzzle.position : transform.position;
            if (IsShotBlocked(origin, target.position)) return;   // hold fire, retry next frame

            FireAt(origin, target.position);
            _cooldown = 1f / Mathf.Max(0.01f, _weapon.fireRate);
        }

        private bool IsShotBlocked(Vector3 origin, Vector3 targetPosition)
        {
            if (Physics.Linecast(origin, targetPosition, _shotBlockers, QueryTriggerInteraction.Ignore))
                return true;

            if (NavMesh.Raycast(transform.position, targetPosition, out _, NavMesh.AllAreas))
                return true;

            return false;
        }

        private void FireAt(Vector3 origin, Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - origin;
            direction.y = 0f;
            direction.Normalize();
            if (direction.sqrMagnitude < 0.001f) return;

            if (ObjectPooler.Instance == null) return;

            var projectile = ObjectPooler.Instance.Get<Projectile>(
                _weapon.projectile, origin, Quaternion.LookRotation(direction, Vector3.up));
            projectile?.Launch(direction, _weapon.projectileSpeed);

            if (_muzzleFlash != null) _muzzleFlash.Play();
            AudioEvents.Play(AudioCue.Shoot);
        }
    }
}
