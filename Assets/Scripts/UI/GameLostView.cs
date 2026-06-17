using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SkyloftGame.UI
{
    /// <summary>
    /// "Game Over" menüsü: oyuncu canı bittiğinde gösterilir.
    /// Aynı seviyeyi tekrar oynama veya menüye dönme seçeneği sunar.
    /// </summary>
    public class GameLostView : UIPanel
    {
        [SerializeField] private TMP_Text _runKillsLabel;
        [SerializeField] private Button   _retryButton;
        [SerializeField] private Button   _menuButton;

        protected override void Awake()
        {
            base.Awake();
            if (_retryButton != null) _retryButton.onClick.AddListener(() => GameStateManager.Instance.ReplayLevel());
            if (_menuButton != null)  _menuButton.onClick.AddListener(() => GameStateManager.Instance.GoToMenu());
        }

        protected override void OnBeforeShow()
        {
            if (_runKillsLabel != null && GameStateManager.Instance != null)
                _runKillsLabel.text = $"Öldürülen: {GameStateManager.Instance.Score?.RunKills ?? 0}";
        }
    }
}
