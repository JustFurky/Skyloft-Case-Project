using UnityEngine;

namespace SkyloftGame.Pool
{
    /// <summary>
    /// Inspector'dan veya kod ile ayarlanabilen pool konfigürasyonu.
    /// ObjectPooler.PoolEntry içinde kullanılır; ayrıca bağımsız bir
    /// ScriptableObject olarak da oluşturulabilir.
    /// </summary>
    [System.Serializable]
    public class PoolConfig
    {
        [Tooltip("Bu pool'un benzersiz kimliği. Get / Release çağrılarında kullanılır.")]
        public string key;

        [Tooltip("Havuzlanacak prefab.")]
        public GameObject prefab;

        [Tooltip("Oyun başlangıcında önceden oluşturulacak nesne adedi.")]
        [Min(0)] public int initialSize = 10;

        [Tooltip("Havuzun aynı anda tutabileceği maksimum pasif nesne sayısı.\n" +
                 "Aşıldığında fazla nesneler yok edilir (Destroy). 0 = limitsiz.")]
        [Min(0)] public int maxSize = 50;

        [Tooltip("Etkin (active) nesneler limiti aşsa bile yeni nesne yaratılsın mı?")]
        public bool collectionChecks = true;
    }
}
