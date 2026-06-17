using System;
using UnityEngine;
using SkyloftGame.StateMachine;

namespace SkyloftGame.Gameplay
{
    [DefaultExecutionOrder(-80)]
    public class PauseController : MonoBehaviour
    {
        public static PauseController Instance { get; private set; }

        public bool IsPaused { get; private set; }

        public event Action<bool> OnPauseChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                Time.timeScale = 1f;
            }
        }

        public void TogglePause()
        {
            if (IsPaused) Resume();
            else          Pause();
        }

        public void Pause()
        {
            if (IsPaused) return;
            if (GameStateManager.Instance == null ||
                GameStateManager.Instance.CurrentState != GameStateType.Playing) return;

            IsPaused       = true;
            Time.timeScale = 0f;
            OnPauseChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (!IsPaused) return;

            IsPaused       = false;
            Time.timeScale = 1f;
            OnPauseChanged?.Invoke(false);
        }

        private void HandleStateChanged(GameStateType previous, GameStateType next)
        {
            if (next != GameStateType.Playing)
            {
                if (IsPaused) Resume();
                else          Time.timeScale = 1f;
            }
        }
    }
}
