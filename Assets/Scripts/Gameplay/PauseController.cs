using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using SkyloftGame.StateMachine;

namespace SkyloftGame.Gameplay
{
    public class PauseController : MonoBehaviour
    {
        [Tooltip("Seconds counted down before the game resumes after Continue.")]
        [SerializeField] private int _resumeCountdownSeconds = 3;

        public bool IsPaused { get; private set; }

        public event Action<bool> OnPauseChanged;
        public event Action<int>  ResumeCountdownTick;

        private GameStateManager         _game;
        private CancellationTokenSource  _resumeCts;

        [Inject]
        private void Construct(GameStateManager game)
        {
            _game = game;
            _game.OnStateChanged += HandleStateChanged;
        }

        private void OnDestroy()
        {
            if (_game != null) _game.OnStateChanged -= HandleStateChanged;
            CancelResumeCountdown();
            Time.timeScale = 1f;
        }

        public void Pause()
        {
            if (IsPaused) return;
            if (_game == null || _game.CurrentState != GameStateType.Playing) return;

            CancelResumeCountdown();
            IsPaused       = true;
            Time.timeScale = 0f;
            OnPauseChanged?.Invoke(true);
        }

        public void RequestResume()
        {
            if (!IsPaused || _resumeCts != null) return;
            _resumeCts = new CancellationTokenSource();
            ResumeCountdownAsync(_resumeCts.Token).Forget();
        }

        private async UniTaskVoid ResumeCountdownAsync(CancellationToken token)
        {
            for (int s = Mathf.Max(1, _resumeCountdownSeconds); s > 0; s--)
            {
                ResumeCountdownTick?.Invoke(s);

                bool canceled = await UniTask
                    .Delay(TimeSpan.FromSeconds(1), DelayType.Realtime, cancellationToken: token)
                    .SuppressCancellationThrow();

                if (canceled) return;
            }
            Resume();
        }

        public void Resume()
        {
            CancelResumeCountdown();
            if (!IsPaused) return;

            IsPaused       = false;
            Time.timeScale = 1f;
            OnPauseChanged?.Invoke(false);
        }

        private void CancelResumeCountdown()
        {
            if (_resumeCts == null) return;
            _resumeCts.Cancel();
            _resumeCts.Dispose();
            _resumeCts = null;
        }

        private void HandleStateChanged(GameStateType previous, GameStateType next)
        {
            if (next != GameStateType.Playing)
            {
                CancelResumeCountdown();
                if (IsPaused) Resume();
                else          Time.timeScale = 1f;
            }
        }
    }
}
