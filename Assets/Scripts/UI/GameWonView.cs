using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace SkyloftGame.UI
{
    public class GameWonView : UIPanel
    {
        [SerializeField] private TMP_Text _runKillsLabel;
        [SerializeField] private TMP_Text _totalKillsLabel;
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private Button   _nextLevelButton;
        [SerializeField] private Button   _replayButton;
        [SerializeField] private Button   _menuButton;

        [Inject] private GameStateManager _game;

        protected override void Awake()
        {
            base.Awake();

            if (_nextLevelButton != null) _nextLevelButton.onClick.AddListener(() => _game.GoToNextLevel());
            if (_replayButton != null)    _replayButton.onClick.AddListener(() => _game.ReplayLevel());
            if (_menuButton != null)      _menuButton.onClick.AddListener(() => _game.GoToMenu());
        }

        protected override void OnBeforeShow()
        {
            var gsm = _game;
            if (gsm == null) return;

            int runKills = gsm.Score?.RunKills ?? 0;

            if (_titleLabel != null)
                _titleLabel.text = gsm.Level?.Current != null
                    ? $"{gsm.Level.Current.displayName} Tamamlandı!" : "Kazandın!";

            if (_runKillsLabel != null)
                Tween.Custom(0f, (float)runKills, 0.6f,
                    onValueChange: (float v) => _runKillsLabel.text = $"Bu Tur: {Mathf.RoundToInt(v)}");

            if (_totalKillsLabel != null)
                _totalKillsLabel.text = $"Toplam: {gsm.Score?.TotalKills ?? 0}";

            if (_nextLevelButton != null)
                _nextLevelButton.gameObject.SetActive(gsm.Level?.HasNext ?? false);
        }
    }
}
