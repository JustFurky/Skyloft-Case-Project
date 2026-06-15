using UnityEngine;
using SkyloftGame.Core;
using SkyloftGame.StateMachine;

namespace SkyloftGame.Player
{
    /// <summary>
    /// CharacterController tabanlı 3B oyuncu hareketi. Girdiyi IMoveInput
    /// kaynağından alır; yalnızca Playing durumunda kontrol açıktır.
    ///
    /// Sorumluluğu sadece hareket + yönelimdir (SRP). Saldırı PlayerShooter'da,
    /// can PlayerHealth'tedir.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Girdi")]
        [SerializeField] private MovementInputSource _moveSource;

        [Header("Hareket")]
        [SerializeField] private float _moveSpeed      = 6f;
        [SerializeField] private float _rotationSpeed  = 720f;   // derece/saniye
        [SerializeField] private float _gravity        = -20f;

        [Header("Animasyon (opsiyonel)")]
        [SerializeField] private Animator _animator;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private CharacterController _cc;
        private float _verticalVelocity;
        private bool  _controlEnabled;

        private void Awake() => _cc = GetComponent<CharacterController>();

        private void OnEnable()
        {
            PlayerLocator.Set(transform);

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
                _controlEnabled = GameStateManager.Instance.CurrentState == GameStateType.Playing;
            }
        }

        private void OnDisable()
        {
            PlayerLocator.Clear(transform);
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            Vector2 input = _controlEnabled && _moveSource != null ? _moveSource.Direction : Vector2.zero;
            Vector3 move  = new(input.x, 0f, input.y);

            ApplyGravity();
            Move(move);
            Rotate(move);
            UpdateAnimator(move);
        }

        private void ApplyGravity()
        {
            if (_cc.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
            else _verticalVelocity += _gravity * Time.deltaTime;
        }

        private void Move(Vector3 planarDir)
        {
            Vector3 velocity = planarDir * _moveSpeed;
            velocity.y = _verticalVelocity;
            _cc.Move(velocity * Time.deltaTime);
        }

        private void Rotate(Vector3 planarDir)
        {
            if (planarDir.sqrMagnitude < 0.0001f) return;
            Quaternion target = Quaternion.LookRotation(planarDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, target, _rotationSpeed * Time.deltaTime);
        }

        private void UpdateAnimator(Vector3 planarDir)
        {
            if (_animator != null)
                _animator.SetFloat(SpeedHash, planarDir.magnitude);
        }

        private void HandleStateChanged(GameStateType previous, GameStateType next)
            => _controlEnabled = next == GameStateType.Playing;
    }
}
