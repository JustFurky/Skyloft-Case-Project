using System.Collections.Generic;
using UnityEngine;

namespace SkyloftGame.Pool
{
    [DefaultExecutionOrder(-150)]
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [Tooltip("Sahneye gömülü (inline) pool konfigürasyonları.")]
        [SerializeField] private List<PoolConfig> _configs = new();

        [Tooltip("Yeniden kullanılabilir asset olarak tanımlı pool konfigürasyonları " +
                 "(Assets/Create/SkyloftGame/Pool Config).")]
        [SerializeField] private List<PoolConfigSO> _configAssets = new();

        private readonly Dictionary<string, GameObjectPool> _pools = new();
        private Transform _poolRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _poolRoot = new GameObject("[PoolRoot]").transform;
            _poolRoot.SetParent(transform);

            foreach (var cfg in _configs)
                RegisterPool(cfg);

            foreach (var so in _configAssets)
                if (so != null) RegisterPool(so.config);
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void RegisterPool(PoolConfig config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.key))
            {
                Debug.LogWarning("[ObjectPooler] Geçersiz konfigürasyon atlandı.", this);
                return;
            }
            if (_pools.ContainsKey(config.key)) return;

            var root = new GameObject($"Pool_{config.key}").transform;
            root.SetParent(_poolRoot);
            _pools[config.key] = new GameObjectPool(config, root);
        }

        public void UnregisterPool(string key)
        {
            if (_pools.Remove(key, out var pool)) pool.Clear();
        }

        public GameObject Get(string key, Vector3 position, Quaternion rotation)
        {
            if (!TryGetPool(key, out var pool)) return null;
            var obj = pool.Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        public GameObject Get(string key, Vector3 position) => Get(key, position, Quaternion.identity);
        public GameObject Get(string key)                   => Get(key, Vector3.zero, Quaternion.identity);

        public T Get<T>(string key, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = Get(key, position, rotation);
            return obj != null ? obj.GetComponent<T>() : null;
        }

        public void Release(string key, GameObject obj)
        {
            if (obj == null) return;
            if (!TryGetPool(key, out var pool)) { Destroy(obj); return; }
            pool.Release(obj);
        }

        private bool TryGetPool(string key, out GameObjectPool pool)
        {
            if (_pools.TryGetValue(key, out pool)) return true;
            Debug.LogError($"[ObjectPooler] Kayıtsız pool: '{key}'");
            return false;
        }

#if UNITY_EDITOR
        [ContextMenu("Print Pool Stats")]
        private void PrintStats()
        {
            var sb = new System.Text.StringBuilder("=== Pool Stats ===\n");
            foreach (var (key, pool) in _pools)
                sb.AppendLine($"  {key}: active={pool.CountActive} inactive={pool.CountInactive} total={pool.CountAll}");
            Debug.Log(sb);
        }
#endif
    }
}
