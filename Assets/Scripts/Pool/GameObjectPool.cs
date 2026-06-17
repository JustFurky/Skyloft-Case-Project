using UnityEngine;
using UnityEngine.Pool;
using SkyloftGame.VFX;

namespace SkyloftGame.Pool
{
    public class GameObjectPool
    {
        private readonly PoolId                 _id;
        private readonly Transform              _root;
        private readonly ObjectPool<GameObject> _pool;

        private bool _autoParticleWarned;

        public int CountAll      => _pool.CountAll;
        public int CountActive   => _pool.CountActive;
        public int CountInactive => _pool.CountInactive;

        public GameObjectPool(PoolId id, Transform root)
        {
            _id   = id;
            _root = root;

            _pool = new ObjectPool<GameObject>(
                createFunc:      OnCreate,
                actionOnGet:     null,
                actionOnRelease: OnRelease,
                actionOnDestroy: obj => Object.Destroy(obj),
                collectionCheck: id.collectionChecks,
                defaultCapacity: id.initialSize,
                maxSize:         id.maxSize > 0 ? id.maxSize : int.MaxValue
            );

            Prewarm(id.initialSize);
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            var obj = _pool.Get();
            obj.transform.SetParent(null);
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);

            foreach (var p in obj.GetComponentsInChildren<IPoolable>(true))
                p.OnSpawn();

            return obj;
        }

        public void Release(GameObject obj) => _pool.Release(obj);
        public void Clear()                 => _pool.Clear();

        private GameObject OnCreate()
        {
            var obj    = Object.Instantiate(_id.prefab, _root);
            obj.name   = $"{_id.prefab.name}_{CountAll}";
            var pooled = obj.GetComponent<PooledObject>() ?? obj.AddComponent<PooledObject>();
            pooled.PoolKey = _id;

            EnsureSelfRelease(obj);

            obj.SetActive(false);
            return obj;
        }

        private void EnsureSelfRelease(GameObject obj)
        {
            if (obj.GetComponentInChildren<IPoolable>(true) != null) return;
            if (obj.GetComponent<ParticleSystem>() == null) return;

            obj.AddComponent<PooledParticle>();

            if (!_autoParticleWarned)
            {
                _autoParticleWarned = true;
                Debug.LogWarning($"[Pool] Prefab '{_id.name}' had no PooledParticle; one was added " +
                                 $"automatically (release guaranteed). For a permanent setup, add " +
                                 $"PooledParticle to the prefab root.", _id.prefab);
            }
        }

        private void OnRelease(GameObject obj)
        {
            if (obj.activeSelf)
                foreach (var p in obj.GetComponentsInChildren<IPoolable>(true))
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
