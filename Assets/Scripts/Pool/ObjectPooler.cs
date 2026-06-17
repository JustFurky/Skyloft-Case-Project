using System.Collections.Generic;
using UnityEngine;

namespace SkyloftGame.Pool
{
    [DefaultExecutionOrder(-150)]
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [Tooltip("Pools to prewarm at startup (optional). Pools not listed " +
                 "are created automatically on first Get call.")]
        [SerializeField] private List<PoolId> _prewarm = new();

        private readonly Dictionary<PoolId, GameObjectPool> _pools = new();
        private Transform _poolRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (transform.parent != null) transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            _poolRoot = new GameObject("[PoolRoot]").transform;
            _poolRoot.SetParent(transform);
            _poolRoot.gameObject.SetActive(false);

            foreach (var id in _prewarm)
                GetOrCreatePool(id);
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public GameObject Get(PoolId id, Vector3 position, Quaternion rotation)
        {
            var pool = GetOrCreatePool(id);
            return pool != null ? pool.Spawn(position, rotation) : null;
        }

        public GameObject Get(PoolId id, Vector3 position) => Get(id, position, Quaternion.identity);
        public GameObject Get(PoolId id)                   => Get(id, Vector3.zero, Quaternion.identity);

        public T Get<T>(PoolId id, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = Get(id, position, rotation);
            return obj != null ? obj.GetComponent<T>() : null;
        }

        public void Release(PoolId id, GameObject obj)
        {
            if (obj == null) return;
            if (_pools.TryGetValue(id, out var pool)) pool.Release(obj);
            else Destroy(obj);
        }

        private GameObjectPool GetOrCreatePool(PoolId id)
        {
            if (id == null) { Debug.LogError("[ObjectPooler] PoolId is null."); return null; }
            if (_pools.TryGetValue(id, out var pool)) return pool;

            if (id.prefab == null)
            {
                Debug.LogError($"[ObjectPooler] No prefab assigned on PoolId '{id.name}'.", id);
                return null;
            }

            var root = new GameObject($"Pool_{id.name}").transform;
            root.SetParent(_poolRoot);
            pool = new GameObjectPool(id, root);
            _pools[id] = pool;
            return pool;
        }

#if UNITY_EDITOR
        [ContextMenu("Print Pool Stats")]
        private void PrintStats()
        {
            var sb = new System.Text.StringBuilder("=== Pool Stats ===\n");
            foreach (var (id, pool) in _pools)
                sb.AppendLine($"  {id.name}: active={pool.CountActive} inactive={pool.CountInactive} total={pool.CountAll}");
            Debug.Log(sb);
        }
#endif
    }
}
