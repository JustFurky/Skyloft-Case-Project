using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace SkyloftGame.UI
{
    public class GameLostView : UIPanel
    {
        [SerializeField] private TMP_Text _runKillsLabel;
        [SerializeField] private Button   _retryButton;
        [SerializeField] private Button   _menuButton;

        [Inject] private GameStateManager _game;

        protected override void Awake()
        {
            base.Awake();
            if (_retryButton != null) _retryButton.onClick.AddListener(() => _game.ReplayLevel());
            if (_menuButton != null)  _menuButton.onClick.AddListener(() => _game.GoToMenu());
        }

        protected override void OnBeforeShow()
        {
            if (_runKillsLabel != null && _game != null)
                _runKillsLabel.text = $"Öldürülen: {_game.Score?.RunKills ?? 0}";
        }
    }
}
