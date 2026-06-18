using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using SkyloftGame.Gameplay;

namespace SkyloftGame.UI
{
    public class PauseView : UIPanel
    {
        [SerializeField] private Button   _resumeButton;
        [SerializeField] private Button   _menuButton;
        [SerializeField] private TMP_Text _countdownLabel;

        private PauseController  _pause;
        private GameStateManager _game;

        [Inject]
        public void Construct(PauseController pause, GameStateManager game)
        {
            _pause = pause;
            _game  = game;
            _pause.OnPauseChanged      += HandlePauseChanged;
            _pause.ResumeCountdownTick += HandleResumeCountdownTick;
        }

        protected override void Awake()
        {
            base.Awake();

            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(() => _pause?.RequestResume());

            if (_menuButton != null)
                _menuButton.onClick.AddListener(() =>
                {
                    _pause?.Resume();
                    _game?.GoToMenu();
                });
        }

        private void OnDestroy()
        {
            if (_pause != null)
            {
                _pause.OnPauseChanged      -= HandlePauseChanged;
                _pause.ResumeCountdownTick -= HandleResumeCountdownTick;
            }
        }

        private void HandlePauseChanged(bool paused)
        {
            if (paused)
            {
                SetButtonsActive(true);
                if (_countdownLabel != null) _countdownLabel.gameObject.SetActive(false);
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void HandleResumeCountdownTick(int seconds)
        {
            SetButtonsActive(false);
            if (_countdownLabel != null)
            {
                _countdownLabel.gameObject.SetActive(true);
                _countdownLabel.text = seconds.ToString();
            }
        }

        private void SetButtonsActive(bool active)
        {
            if (_resumeButton != null) _resumeButton.gameObject.SetActive(active);
            if (_menuButton != null)   _menuButton.gameObject.SetActive(active);
        }
    }
}
