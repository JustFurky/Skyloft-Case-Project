using UnityEngine;
using TMPro;
using SkyloftGame.Gameplay;

namespace SkyloftGame.UI
{
    /// <summary>
    /// Oynanış sırasındaki HUD: geri sayım ve öldürme sayısı.
    /// Servislere olay-tabanlı bağlanır; her karede polling yapmaz.
    ///
    /// Can barı artık Player prefab'ına ait (bkz. PlayerHealthBar); böylece HUD'un
    /// sahnedeki oyuncuya doğrudan referans tutma zorunluluğu ortadan kalkar.
    /// </summary>
    public class HudView : UIPanel
    {
        [SerializeField] private TMP_Text _timerLabel;
        [SerializeField] private TMP_Text _killsLabel;

        private ICountdownTimer _timer;
        private IScoreService   _score;

        private void OnEnable()
        {
            // Servisleri gösterim anında çöz (init sırası güvenli).
            if (GameStateManager.Instance != null)
            {
                _timer = GameStateManager.Instance.Timer;
                _score = GameStateManager.Instance.Score;
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
        }

        private void OnDisable()
        {
            if (_timer != null) _timer.OnTick -= UpdateTimer;
            if (_score != null) _score.OnRunKillsChanged -= UpdateKills;
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
    }
}
