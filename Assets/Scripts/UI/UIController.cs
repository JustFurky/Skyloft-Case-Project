using UnityEngine;
using Zenject;
using SkyloftGame.StateMachine;

namespace SkyloftGame.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private UIPanel _menuPanel;
        [SerializeField] private UIPanel _hudPanel;
        [SerializeField] private UIPanel _gameWonPanel;
        [SerializeField] private UIPanel _gameLostPanel;

        [Inject] private GameStateManager _game;

        private void Start()
        {
            if (_game == null) return;
            _game.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameStateType.None, _game.CurrentState);
        }

        private void OnDestroy()
        {
            if (_game != null) _game.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameStateType previous, GameStateType next)
        {
            Toggle(_menuPanel,     next == GameStateType.Menu);
            Toggle(_hudPanel,      next == GameStateType.Playing);
            Toggle(_gameWonPanel,  next == GameStateType.GameWon);
            Toggle(_gameLostPanel, next == GameStateType.GameLost);
        }

        private static void Toggle(UIPanel panel, bool show)
        {
            if (panel == null) return;
            if (show) panel.Show();
            else      panel.Hide();
        }
    }
}
