using UnityEngine;

namespace SkyloftGame.Player
{
    [CreateAssetMenu(menuName = "SkyloftGame/Health Bar Data", fileName = "HealthBarData")]
    public class HealthBarData : ScriptableObject
    {
        [Tooltip("Seconds for the fill and color to animate to the new value.")]
        [Min(0f)] public float fillDuration = 0.2f;

        [Tooltip("Fill color at full health.")]
        public Color fullHealthColor = new(0.30f, 0.85f, 0.30f, 1f);

        [Tooltip("Fill color at zero health (the bar approaches this as HP drops).")]
        public Color lowHealthColor = new(0.85f, 0.20f, 0.20f, 1f);

        [Tooltip("If enabled, the bar rotates to face the camera (billboard).")]
        public bool faceCamera = true;
    }
}
