using UnityEngine;
using SkyloftGame.StateMachine;

namespace SkyloftGame.UI
{
    /// <summary>
    /// Oyun durumunu ilgili panele eşleyen tek yönlendirici (router).
    /// Paneller durumu kendileri dinlemek yerine buradan yönetilir; bu sayede
    /// hangi durumda hangi ekranın görüneceği tek yerde tanımlıdır (KISS).
    /// </summary>
    public class UIController : MonoBehaviour
    {
        [SerializeField] private UIPanel _menuPanel;
        [SerializeField] private UIPanel _hudPanel;
        [SerializeField] private UIPanel _gameWonPanel;
        [SerializeField] private UIPanel _gameLostPanel;

        private void OnEnable()
        {
            if (GameStateManager.Instance == null) return;
            GameStateManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameStateType.None, GameStateManager.Instance.CurrentState);
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameStateType previous, GameStateType next)
        {
            Debug.Log($"[UI] HandleStateChanged {previous}->{next} | menuPanel atanmış mı: {_menuPanel != null}");   // TEŞHİS
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
