using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SkyloftGame.UI
{
    public class MainMenuView : UIPanel
    {
        [SerializeField] private Button     _startButton;
        [SerializeField] private Button     _newGameButton;
        [SerializeField] private TMP_Text   _progressLabel;

        protected override void Awake()
        {
            base.Awake();
            if (_startButton != null)
                _startButton.onClick.AddListener(() => GameStateManager.Instance.ContinueGame());
            if (_newGameButton != null)
                _newGameButton.onClick.AddListener(() => GameStateManager.Instance.StartNewGame());
        }

        protected override void OnBeforeShow()
        {
            if (_progressLabel == null || GameStateManager.Instance == null) return;

            int total   = GameStateManager.Instance.Score?.TotalKills ?? 0;
            int unlocked = (GameStateManager.Instance.Level?.HighestUnlockedIndex ?? 0) + 1;
            _progressLabel.text = $"Açılan Seviye: {unlocked}\nToplam Öldürme: {total}";
        }
    }
}
