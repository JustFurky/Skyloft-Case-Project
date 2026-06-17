using UnityEngine;

namespace SkyloftGame.Pool
{
    [CreateAssetMenu(menuName = "SkyloftGame/Pool Id", fileName = "PoolId")]
    public class PoolId : ScriptableObject
    {
        [Tooltip("Prefab to pool.")]
        public GameObject prefab;

        [Tooltip("Number of objects to create at startup (prewarm).")]
        [Min(0)] public int initialSize = 10;

        [Tooltip("Maximum inactive objects kept in the pool. Returned objects beyond this are destroyed.\n" +
                 "0 = unlimited (the pool grows to its peak and always reuses — no churn).")]
        [Min(0)] public int maxSize = 0;

        [Tooltip("Unity ObjectPool collection check to catch errors like double release.")]
        public bool collectionChecks = true;
    }
}
