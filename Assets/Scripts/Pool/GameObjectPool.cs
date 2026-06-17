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

            // actionOnGet bilerek atanmadı: nesne, KONUMLANDIRILDIKTAN sonra aktive
            // edilip OnSpawn alacak (bkz. Spawn). Bu sıra, NavMeshAgent gibi
            // bileşenlerin doğru pozisyonda NavMesh'e yerleşmesi için kritiktir.
            _pool = new ObjectPool<GameObject>(
                createFunc:      OnCreate,
                actionOnGet:     null,
                actionOnRelease: OnRelease,
                actionOnDestroy: obj => Object.Destroy(obj),
                collectionCheck: config.collectionChecks,
                defaultCapacity: config.initialSize,
                maxSize:         config.maxSize > 0 ? config.maxSize : int.MaxValue
            );

            Prewarm(config.initialSize);
        }

        /// <summary>
        /// Havuzdan nesne alır, önce konumlandırır, SONRA aktive edip OnSpawn çağırır.
        /// Önce-konumlandır-sonra-aktive et sırası NavMeshAgent yerleşimi için şarttır.
        /// </summary>
        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            var obj = _pool.Get();                                   // pasif gelir (kök inaktif)
            obj.transform.SetParent(null);                          // aktif hiyerarşiye çıkar
            obj.transform.SetPositionAndRotation(position, rotation); // önce yerleştir
            obj.SetActive(true);                                     // sonra aktive et

            foreach (var p in obj.GetComponentsInChildren<IPoolable>(true))
                p.OnSpawn();

            return obj;
        }

        public void Release(GameObject obj) => _pool.Release(obj);
        public void Clear()                 => _pool.Clear();

        private GameObject OnCreate()
        {
            var obj    = Object.Instantiate(_config.prefab, _root);
            obj.name   = $"{_config.prefab.name}_{CountAll}";
            var pooled = obj.GetComponent<PooledObject>() ?? obj.AddComponent<PooledObject>();
            pooled.PoolKey = _config.key;
            obj.SetActive(false);
            return obj;
        }

        private void OnRelease(GameObject obj)
        {
            // Yalnızca gerçekten spawn olmuş (aktif) nesnelerde OnDespawn çağır;
            // prewarm sırasında nesne hiç aktive edilmediğinden atlanır.
            if (obj.activeSelf)
                foreach (var p in obj.GetComponentsInChildren<IPoolable>(true))
                    p.OnDespawn();

            obj.SetActive(false);
            obj.transform.SetParent(_root);
        }

        private void Prewarm(int count)
        {
            if (count <= 0) return;
            // Nesneler pasif kalır; OnSpawn tetiklenmez (NavMeshAgent erken Resume hatası önlenir).
            var buffer = new GameObject[count];
            for (int i = 0; i < count; i++) buffer[i] = _pool.Get();
            for (int i = 0; i < count; i++) _pool.Release(buffer[i]);
        }
    }
}
