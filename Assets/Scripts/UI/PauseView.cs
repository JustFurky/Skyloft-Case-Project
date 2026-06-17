using UnityEngine;
using UnityEngine.UI;
using SkyloftGame.Gameplay;

namespace SkyloftGame.UI
{
    public class PauseView : UIPanel
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _menuButton;

        protected override void Awake()
        {
            base.Awake();

            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(() => PauseController.Instance?.Resume());

            if (_menuButton != null)
                _menuButton.onClick.AddListener(() =>
                {
                    PauseController.Instance?.Resume();
                    GameStateManager.Instance?.GoToMenu();
                });

            if (PauseController.Instance != null)
                PauseController.Instance.OnPauseChanged += HandlePauseChanged;
        }

        private void OnDestroy()
        {
            if (PauseController.Instance != null)
                PauseController.Instance.OnPauseChanged -= HandlePauseChanged;
        }

        private void HandlePauseChanged(bool paused)
        {
            if (paused) Show();
            else        Hide();
        }
    }
}
