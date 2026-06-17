using UnityEngine;
using SkyloftGame.Core;

namespace SkyloftGame.States
{
    /// <summary>
    /// Aktif oynanış durumu: seviyeyi başlatır, dalgaları üretir ve geri sayımı
    /// her karede ilerletir. Süre dolunca GameStateManager zaferi tetikler.
    /// </summary>
    public class PlayingState : StateMachine.IState
    {
        private readonly GameStateManager _manager;
        public PlayingState(GameStateManager manager) => _manager = manager;

        public void Enter()
        {
            var level = _manager.Level?.Current;
            if (level == null)
            {
                Debug.LogError("[PlayingState] Aktif seviye yok. Önce Level.Load çağrılmalı.");
                return;
            }

            _manager.Score?.ResetRun();
            _manager.Spawner?.BeginLevel(level, PlayerLocator.Current);
            _manager.Timer?.Begin(level.durationSeconds);
        }

        public void Update()
        {
            _manager.Timer?.Tick(Time.deltaTime);

            // Erken zafer: süre dolmadan tüm dalgalar temizlendiyse de seviye tamamlanır.
            // (Timer.OnElapsed zaten süre bitince WinGame'i tetikler.)
            if (_manager.Spawner != null && _manager.Spawner.IsCleared)
                _manager.WinGame();
        }

        public void FixedUpdate() { }

        public void Exit() { }   // temizlik bir sonraki durumun (Won/Lost/Menu) sorumluluğundadır
    }
}
