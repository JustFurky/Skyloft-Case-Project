using System;
using UnityEngine;
using SkyloftGame.StateMachine;

namespace SkyloftGame.Player
{
    /// <summary>
    /// Oyuncunun canı. IDamageable üzerinden düşman saldırılarını alır.
    /// Can biterse oyun kaybedilir. Her yeni tur (Playing'e giriş) başında dolar.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHp = 100f;

        public float MaxHp     => _maxHp;
        public float CurrentHp { get; private set; }
        public bool  IsDead    { get; private set; }

        /// <summary>(current, max) — can barı view'ı (PlayerHealthBar) bunu dinler.</summary>
        public event Action<float, float> OnHealthChanged;

        // Canı Awake'te başlat: prefab üstündeki bar, bileşen sırasına bakmadan
        // OnEnable'da doğru değeri okuyabilsin.
        private void Awake()
        {
            IsDead    = false;
            CurrentHp = _maxHp;
        }

        private void OnEnable()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHp = Mathf.Max(0f, CurrentHp - amount);
            OnHealthChanged?.Invoke(CurrentHp, _maxHp);

            if (CurrentHp <= 0f) Die();
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            GameStateManager.Instance?.LoseGame();
        }

        private void ResetHealth()
        {
            IsDead    = false;
            CurrentHp = _maxHp;
            OnHealthChanged?.Invoke(CurrentHp, _maxHp);
        }

        private void HandleStateChanged(GameStateType previous, GameStateType next)
        {
            if (next == GameStateType.Playing) ResetHealth();
        }
    }
}
