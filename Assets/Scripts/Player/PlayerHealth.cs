using System;
using UnityEngine;
using Zenject;
using SkyloftGame.StateMachine;

namespace SkyloftGame.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Tooltip("Health (maxHp) is read from here. Assign the same asset as PlayerController.")]
        [SerializeField] private PlayerData _data;

        private GameStateManager _game;

        private const float FallbackMaxHp = 100f;

        public float MaxHp     => _data != null ? _data.maxHp : FallbackMaxHp;
        public float CurrentHp { get; private set; }
        public bool  IsDead    { get; private set; }

        public event Action<float, float> OnHealthChanged;

        private void Awake()
        {
            if (_data == null)
                Debug.LogError("[PlayerHealth] PlayerData is not assigned; using default health.", this);

            IsDead    = false;
            CurrentHp = MaxHp;
        }

        [Inject]
        private void Construct(GameStateManager game)
        {
            _game = game;
            _game.OnStateChanged += HandleStateChanged;
        }

        private void OnDestroy()
        {
            if (_game != null) _game.OnStateChanged -= HandleStateChanged;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHp = Mathf.Max(0f, CurrentHp - amount);
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);

            if (CurrentHp <= 0f) Die();
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            _game?.LoseGame();
        }

        private void ResetHealth()
        {
            IsDead    = false;
            CurrentHp = MaxHp;
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        private void HandleStateChanged(GameStateType previous, GameStateType next)
        {
            if (next == GameStateType.Playing) ResetHealth();
        }
    }
}
