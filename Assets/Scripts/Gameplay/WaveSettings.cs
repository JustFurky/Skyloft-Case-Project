using UnityEngine;

namespace SkyloftGame.Gameplay
{
    [CreateAssetMenu(menuName = "SkyloftGame/Wave Settings", fileName = "WaveSettings")]
    public class WaveSettings : ScriptableObject
    {
        [Header("HUD Blink")]
        [Tooltip("Total time (seconds) for the wave-countdown text to fade out and back in " +
                 "each time the countdown updates. Cadence itself is configured per level in LevelData.")]
        [Min(0.05f)] public float blinkDuration = 0.4f;
    }
}
