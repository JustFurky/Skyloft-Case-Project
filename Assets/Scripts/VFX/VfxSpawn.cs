using UnityEngine;
using SkyloftGame.Pool;

namespace SkyloftGame.VFX
{
    [System.Serializable]
    public class VfxSpawn
    {
        [Tooltip("Pool to spawn the VFX from (leave empty to disable).")]
        public PoolId pool;

        [Tooltip("Offset along the origin's forward direction (units).")]
        public float forwardOffset = 0f;

        [Tooltip("Vertical offset above the origin (units).")]
        public float upOffset = 0f;

        [Tooltip("If enabled, the VFX is oriented to face back toward the origin.")]
        public bool faceOrigin = false;

        public void Play(Transform origin)
        {
            if (pool == null || origin == null || ObjectPooler.Instance == null) return;

            Vector3    position = origin.position + origin.forward * forwardOffset + Vector3.up * upOffset;
            Quaternion rotation = faceOrigin ? Quaternion.LookRotation(-origin.forward) : Quaternion.identity;
            ObjectPooler.Instance.Get(pool, position, rotation);
        }
    }
}
