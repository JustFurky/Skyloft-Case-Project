using PrimeTween;
using UnityEngine;
using TMPro;
using Zenject;
using SkyloftGame.Gameplay;

namespace SkyloftGame.UI
{
    public class HudView : UIPanel
    {
        [SerializeField] private TMP_Text _timerLabel;
        [SerializeField] private TMP_Text _killsLabel;

        [Header("Wave Countdown")]
        [Tooltip("Text showing the between-wave countdown (blinks every second).")]
        [SerializeField] private TMP_Text     _waveCountdownLabel;
        [Tooltip("Blink duration is read from here (same asset as EnemySpawner).")]
        [SerializeField] private WaveSettings _waveSettings;

        [Inject] private GameStateManager _game;

        private ICountdownTimer _timer;
        private IScoreService   _score;
        private IEnemySpawner   _spawner;
        private Sequence        _blink;

        private void OnEnable()
        {
            if (_game != null)
            {
                _timer   = _game.Timer;
                _score   = _game.Score;
                _spawner = _game.Spawner;
            }

            if (_timer != null)
            {
                _timer.OnTick += UpdateTimer;
                UpdateTimer(_timer.Remaining);
            }
            if (_score != null)
            {
                _score.OnRunKillsChanged += UpdateKills;
                UpdateKills(_score.RunKills);
            }
            if (_spawner != null)
            {
                _spawner.WaveCountdownTick += ShowWaveCountdown;
                _spawner.WaveStarted       += HideWaveCountdown;
            }

            SetCountdownAlpha(0f);
        }

        private void OnDisable()
        {
            if (_timer != null)   _timer.OnTick -= UpdateTimer;
            if (_score != null)   _score.OnRunKillsChanged -= UpdateKills;
            if (_spawner != null)
            {
                _spawner.WaveCountdownTick -= ShowWaveCountdown;
                _spawner.WaveStarted       -= HideWaveCountdown;
            }
            _blink.Stop();
        }

        private void UpdateTimer(float remaining)
        {
            if (_timerLabel == null) return;
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            _timerLabel.text = $"{minutes:00}:{seconds:00}";
        }

        private void UpdateKills(int kills)
        {
            if (_killsLabel != null) _killsLabel.text = kills.ToString();
        }
        

        private void ShowWaveCountdown(int secondsRemaining)
        {
            if (_waveCountdownLabel == null) return;

            float half = (_waveSettings != null ? _waveSettings.blinkDuration : 0.4f) * 0.5f;

            _blink.Stop();
            _blink = Sequence.Create()
                .Chain(Tween.Alpha(_waveCountdownLabel, 0f, half))
                .ChainCallback(() => _waveCountdownLabel.text = $"Next Wave Start At {secondsRemaining}!")
                .Chain(Tween.Alpha(_waveCountdownLabel, 1f, half));
        }

        private void HideWaveCountdown()
        {
            if (_waveCountdownLabel == null) return;
            _blink.Stop();
            SetCountdownAlpha(0f);
        }

        private void SetCountdownAlpha(float a)
        {
            if (_waveCountdownLabel == null) return;
            Color c = _waveCountdownLabel.color;
            c.a = a;
            _waveCountdownLabel.color = c;
        }
    }
}
