using UnityEngine;

namespace SkyloftGame.Gameplay
{
    [CreateAssetMenu(menuName = "SkyloftGame/Wave Settings", fileName = "WaveSettings")]
    public class WaveSettings : ScriptableObject
    {
        [Header("Between-Wave Countdown")]
        [Tooltip("Seconds to count down after a wave is cleared, before the next wave begins.")]
        [Min(0)] public int betweenWaveCountdown = 3;

        [Tooltip("Should the countdown also run before the first wave?")]
        public bool countdownBeforeFirstWave = true;

        [Header("HUD Blink")]
        [Tooltip("Total time (seconds) for the text to fade out and back in each time the countdown updates.")]
        [Min(0.05f)] public float blinkDuration = 0.4f;
    }
}
