using UnityEngine;
using SkyloftGame.Core;

namespace SkyloftGame.States
{
    public class PlayingState : StateMachine.IState
    {
        private readonly GameStateManager _manager;
        public PlayingState(GameStateManager manager) => _manager = manager;

        public void Enter()
        {
            var level = _manager.Level?.Current;
            if (level == null)
            {
                Debug.LogError("[PlayingState] No active level. Level.Load must be called first.");
                return;
            }

            _manager.Score?.ResetRun();
            _manager.Spawner?.BeginLevel(level, PlayerLocator.Current);
            _manager.Timer?.Begin(level.durationSeconds);
        }

        public void Update()
        {
            // Win is purely time-based: enemies keep coming until the timer elapses.
            _manager.Timer?.Tick(Time.deltaTime);
        }

        public void FixedUpdate() { }

        public void Exit() { }
    }
}
