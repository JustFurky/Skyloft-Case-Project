using UnityEngine;
using TMPro;
using Zenject;

namespace SkyloftGame.UI
{
    public class MainMenuView : UIPanel
    {
        [SerializeField] private TMP_Text _progressLabel;

        [Inject] private GameStateManager _game;

        protected override void OnBeforeShow()
        {
            if (_progressLabel == null || _game == null) return;

            int total    = _game.Score?.TotalKills ?? 0;
            int unlocked = (_game.Level?.HighestUnlockedIndex ?? 0) + 1;
            _progressLabel.text = $"Açılan Seviye: {unlocked}\nToplam Öldürme: {total}";
        }
    }
}
