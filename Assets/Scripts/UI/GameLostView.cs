using UnityEngine;
using TMPro;
using Zenject;

namespace SkyloftGame.UI
{
    public class GameLostView : UIPanel
    {
        [SerializeField] private TMP_Text _runKillsLabel;

        [Inject] private GameStateManager _game;

        protected override void OnBeforeShow()
        {
            if (_runKillsLabel != null && _game != null)
                _runKillsLabel.text = $"Öldürülen: {_game.Score?.RunKills ?? 0}";
        }
    }
}
