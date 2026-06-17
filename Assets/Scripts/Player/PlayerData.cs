using UnityEngine;

namespace SkyloftGame.Player
{
    [CreateAssetMenu(menuName = "SkyloftGame/Player Data", fileName = "PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Header("Health")]
        [Min(1f)] public float maxHp = 100f;

        [Header("Movement")]
        [Min(0.1f)] public float moveSpeed     = 6f;
        public float             rotationSpeed = 720f;
        public float             gravity       = -20f;

        [Header("Arena Boundary")]
        [Tooltip("When enabled, the player is kept within the NavMesh boundaries (won't fall off the map). " +
                 "The boundary is derived automatically from the NavMesh — no per-map center/radius setup needed.")]
        public bool clampToArena = true;

        [Tooltip("How close the player can get to the map edge/corner (units). " +
                 "0 = all the way to the corner; larger values keep the player that far from the edge.")]
        [Min(0f)] public float edgePadding = 1f;
    }
}
