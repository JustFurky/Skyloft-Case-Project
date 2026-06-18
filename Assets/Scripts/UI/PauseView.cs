using UnityEngine;
using TMPro;
using Zenject;
using SkyloftGame.Gameplay;

namespace SkyloftGame.UI
{
    public class PauseView : UIPanel
    {
        [SerializeField] private GameObject _resumeButton;
        [SerializeField] private GameObject _menuButton;
        [SerializeField] private TMP_Text   _countdownLabel;

        private PauseController _pause;

        [Inject]
        private void Construct(PauseController pause)
        {
            _pause = pause;
            _pause.OnPauseChanged      += HandlePauseChanged;
            _pause.ResumeCountdownTick += HandleResumeCountdownTick;
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
            if (_resumeButton != null) _resumeButton.SetActive(active);
            if (_menuButton != null)   _menuButton.SetActive(active);
        }
    }
}
