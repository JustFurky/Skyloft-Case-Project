using PrimeTween;
using UnityEngine;
using TMPro;
using Zenject;

namespace SkyloftGame.UI
{
    public class GameWonView : UIPanel
    {
        [SerializeField] private TMP_Text   _runKillsLabel;
        [SerializeField] private TMP_Text   _totalKillsLabel;
        [SerializeField] private TMP_Text   _titleLabel;
        [Tooltip("Next-level button object; hidden when there is no next level.")]
        [SerializeField] private GameObject _nextLevelButton;

        [Inject] private GameStateManager _game;

        protected override void OnBeforeShow()
        {
            if (_game == null) return;

            int runKills = _game.Score?.RunKills ?? 0;

            if (_titleLabel != null)
                _titleLabel.text = _game.Level?.Current != null
                    ? $"{_game.Level.Current.displayName} Tamamlandı!" : "Kazandın!";

            if (_runKillsLabel != null)
                Tween.Custom(0f, (float)runKills, 0.6f,
                    onValueChange: (float v) => _runKillsLabel.text = $"Bu Tur: {Mathf.RoundToInt(v)}");

            if (_totalKillsLabel != null)
                _totalKillsLabel.text = $"Toplam: {_game.Score?.TotalKills ?? 0}";

            if (_nextLevelButton != null)
                _nextLevelButton.SetActive(_game.Level?.HasNext ?? false);
        }
    }
}
