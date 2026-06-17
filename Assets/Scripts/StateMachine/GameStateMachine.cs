using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkyloftGame.StateMachine
{
    public class GameStateMachine
    {
        private readonly Dictionary<GameStateType, IState> _states = new();

        public IState        CurrentState     { get; private set; }
        public GameStateType CurrentStateType { get; private set; }  = GameStateType.None;
        public GameStateType PreviousStateType{ get; private set; }  = GameStateType.None;

        public event Action<GameStateType, GameStateType> OnStateChanged;

        public void RegisterState(GameStateType type, IState state) => _states[type] = state;

        public void TransitionTo(GameStateType next)
        {
            Debug.Log($"[SM] TransitionTo({next}) istendi. Mevcut={CurrentStateType}");   // TEŞHİS

            if (next == CurrentStateType)
            {
                Debug.LogWarning($"[SM] Zaten {next} durumundayız; geçiş yok sayıldı.");   // TEŞHİS
                return;
            }

            if (!_states.TryGetValue(next, out var nextState))
            {
                Debug.LogError($"[StateMachine] Kayıtsız durum: {next}");
                return;
            }

            CurrentState?.Exit();

            PreviousStateType = CurrentStateType;
            CurrentStateType  = next;
            CurrentState      = nextState;

            CurrentState.Enter();
            OnStateChanged?.Invoke(PreviousStateType, next);
            Debug.Log($"[SM] Geçiş tamam: {PreviousStateType} -> {next}");                 // TEŞHİS
        }

        public void Update()      => CurrentState?.Update();
        public void FixedUpdate() => CurrentState?.FixedUpdate();
    }
}
