using UnityEngine;
using UnityEngine.Pool;

namespace SkyloftGame.Pool
{
    public class GameObjectPool
    {
        private readonly PoolConfig             _config;
        private readonly Transform              _root;
        private readonly ObjectPool<GameObject> _pool;

        public int CountAll      => _pool.CountAll;
        public int CountActive   => _pool.CountActive;
        public int CountInactive => _pool.CountInactive;

        public GameObjectPool(PoolConfig config, Transform root)
        {
            _config = config;
            _root   = root;

            _pool = new ObjectPool<GameObject>(
                createFunc:      OnCreate,
                actionOnGet:     OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: obj => Object.Destroy(obj),
                collectionCheck: config.collectionChecks,
                defaultCapacity: config.initialSize,
                maxSize:         config.maxSize > 0 ? config.maxSize : int.MaxValue
            );

            Prewarm(config.initialSize);
        }

        public GameObject Get()               => _pool.Get();
        public void       Release(GameObject obj) => _pool.Release(obj);
        public void       Clear()             => _pool.Clear();

        private GameObject OnCreate()
        {
            var obj    = Object.Instantiate(_config.prefab, _root);
            obj.name   = $"{_config.prefab.name}_{CountAll}";
            var pooled = obj.GetComponent<PooledObject>() ?? obj.AddComponent<PooledObject>();
            pooled.PoolKey = _config.key;
            obj.SetActive(false);
            return obj;
        }

        private static void OnGet(GameObject obj)
        {
            obj.SetActive(true);
            foreach (var p in obj.GetComponentsInChildren<IPoolable>(true))
                p.OnSpawn();
        }

        private void OnRelease(GameObject obj)
        {
            foreach (var p in obj.GetComponentsInChildren<IPoolable>(false))
                p.OnDespawn();
            obj.SetActive(false);
            obj.transform.SetParent(_root);
        }

        private void Prewarm(int count)
        {
            if (count <= 0) return;
            var buffer = new GameObject[count];
            for (int i = 0; i < count; i++) buffer[i] = _pool.Get();
            for (int i = 0; i < count; i++) _pool.Release(buffer[i]);
        }
    }
}
