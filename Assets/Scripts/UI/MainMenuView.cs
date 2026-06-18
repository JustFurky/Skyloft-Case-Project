using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace SkyloftGame.UI
{
    public class MainMenuView : UIPanel
    {
        [SerializeField] private Button     _startButton;
        [SerializeField] private Button     _newGameButton;
        [SerializeField] private TMP_Text   _progressLabel;

        [Inject] private GameStateManager _game;

        protected override void Awake()
        {
            base.Awake();
            if (_startButton != null)
                _startButton.onClick.AddListener(() => _game.ContinueGame());
            if (_newGameButton != null)
                _newGameButton.onClick.AddListener(() => _game.StartNewGame());
        }

        protected override void OnBeforeShow()
        {
            if (_progressLabel == null || _game == null) return;

            int total   = _game.Score?.TotalKills ?? 0;
            int unlocked = (_game.Level?.HighestUnlockedIndex ?? 0) + 1;
            _progressLabel.text = $"Açılan Seviye: {unlocked}\nToplam Öldürme: {total}";
        }
    }
}
