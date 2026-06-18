using UnityEngine;
using UnityEngine.AI;
using Zenject;
using SkyloftGame.Core;
using SkyloftGame.Gameplay;
using SkyloftGame.StateMachine;

namespace SkyloftGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [Tooltip("The FloatingJoystick in the scene (Joystick Pack). Direction is read directly.")]
        [SerializeField] private Joystick _joystick;

        [Header("Data")]
        [Tooltip("Movement, rotation, gravity and arena boundary parameters.")]
        [SerializeField] private PlayerData _data;

        private GameStateManager _game;
        private PauseController  _pause;

        private CharacterController _cc;
        private PlayerTargeting     _targeting;
        private float _verticalVelocity;
        private bool  _controlEnabled;

        private const float NavSnapMaxDistance = 2.5f;

        private void Awake()
        {
            _cc        = GetComponent<CharacterController>();
            _targeting = GetComponent<PlayerTargeting>();

            if (_data == null)
                Debug.LogError("[PlayerController] PlayerData is not assigned; the player will not move.", this);
        }

        [Inject]
        private void Construct(GameStateManager game, PauseController pause)
        {
            _game  = game;
            _pause = pause;
            _game.OnStateChanged += HandleStateChanged;
            _controlEnabled = _game.CurrentState == GameStateType.Playing;
        }

        private void OnEnable()  => PlayerLocator.Set(transform);
        private void OnDisable() => PlayerLocator.Clear(transform);

        private void OnDestroy()
        {
            if (_game != null) _game.OnStateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            if (_data == null) return;
            if (_pause != null && _pause.IsPaused) return;

            Vector2 input = _controlEnabled && _joystick != null ? _joystick.Direction : Vector2.zero;
            Vector3 move  = new(input.x, 0f, input.y);

            ApplyGravity();
            Move(move);
            if (_data.clampToArena && move.sqrMagnitude > 0f) ClampToArena();
            Rotate(move);
        }

        private void ApplyGravity()
        {
            if (_cc.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
            else _verticalVelocity += _data.gravity * Time.deltaTime;
        }

        private void Move(Vector3 planarDir)
        {
            Vector3 velocity = planarDir * _data.moveSpeed;
            velocity.y = _verticalVelocity;
            _cc.Move(velocity * Time.deltaTime);
        }

        private void Rotate(Vector3 moveDir)
        {
            Vector3 faceDir = AimDirection(moveDir);
            if (faceDir.sqrMagnitude < 0.0001f) return;

            Quaternion target = Quaternion.LookRotation(faceDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, target, _data.rotationSpeed * Time.deltaTime);
        }

        private Vector3 AimDirection(Vector3 moveDir)
        {
            if (_targeting != null && _targeting.CurrentTarget != null)
            {
                Vector3 toTarget = _targeting.CurrentTarget.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.0001f) return toTarget.normalized;
            }
            return moveDir;
        }

        private void ClampToArena()
        {
            Vector3 pos = transform.position;

            if (!NavMesh.SamplePosition(pos, out NavMeshHit sample, NavSnapMaxDistance, NavMesh.AllAreas))
                return;

            Vector3 clamped = sample.position;

            float pad = _data.edgePadding;
            if (pad > 0f && NavMesh.FindClosestEdge(clamped, out NavMeshHit edge, NavMesh.AllAreas)
                         && edge.distance < pad)
            {
                Vector3 inward = clamped - edge.position;
                inward.y = 0f;
                clamped  = edge.position + (inward.sqrMagnitude > 1e-6f ? inward.normalized : edge.normal) * pad;
            }

            clamped.y = pos.y;
            if ((clamped - pos).sqrMagnitude > 1e-6f)
                transform.position = clamped;
        }

        private void HandleStateChanged(GameStateType previous, GameStateType next)
            => _controlEnabled = next == GameStateType.Playing;
    }
}
