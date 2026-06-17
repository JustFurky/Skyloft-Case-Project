using UnityEngine;

namespace SkyloftGame.Player
{
    [CreateAssetMenu(menuName = "SkyloftGame/Player Animation Data", fileName = "PlayerAnimationData")]
    public class PlayerAnimationData : ScriptableObject
    {
        [Header("Animator State Names")]
        public string idleState = "Rifle Idle";
        public string runState  = "Rifle Run";
        public string fireState = "Firing Rifle";

        [Header("Settings")]
        [Min(0f)] public float crossFade     = 0.1f;
        [Min(0f)] public float moveThreshold = 0.15f;
    }
}
