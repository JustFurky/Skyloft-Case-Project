using UnityEngine;

namespace SkyloftGame.Level
{
    /// <summary>
    /// Tek bir seviyenin veriye dayalı tanımı (ScriptableObject).
    ///
    /// Zorluk artışı tamamen veriden yönetilir: daha fazla/uzun dalga, daha kısa
    /// spawn aralığı veya daha güçlü düşman pool anahtarları girilerek yeni kod
    /// yazmadan üç (veya N) seviye üretilebilir — OCP'ye uygun.
    ///
    /// Oluşturma: Assets/Create/SkyloftGame/Level Data
    /// </summary>
    [CreateAssetMenu(menuName = "SkyloftGame/Level Data", fileName = "LevelData_New")]
    public class LevelData : ScriptableObject
    {
        [Header("Kimlik")]
        [Tooltip("0 tabanlı seviye sırası. LevelDatabase içindeki index ile eşleşmelidir.")]
        public int levelIndex;
        public string displayName = "Level 1";

        [Header("Süre")]
        [Tooltip("Hayatta kalma süresi (saniye). Süre dolduğunda oyun kazanılır.")]
        [Min(5f)] public float durationSeconds = 180f;   // 3 dakika

        [Header("Düşman Dalgaları")]
        public Wave[] waves;

        [Header("Spawn Alanı")]
        [Tooltip("Oyuncunun etrafında düşmanların belireceği halkanın dış yarıçapı.")]
        [Min(1f)] public float spawnRadius = 14f;

        [Tooltip("Düşmanların oyuncuya doğrudan bitişik doğmasını engelleyen iç yarıçap.")]
        [Min(1f)] public float minSpawnDistance = 7f;

        [Header("Optimizasyon")]
        [Tooltip("Aynı anda sahnede tutulabilecek maksimum canlı düşman. " +
                 "Limit dolduğunda spawn duraklatılır — büyük dalgalarda performansı korur.")]
        [Min(1)] public int maxAliveEnemies = 60;
    }

    /// <summary>
    /// Bir seviye içindeki tek bir düşman dalgası.
    /// Birden çok dalga, başlangıç gecikmeleriyle üst üste binerek
    /// kademeli yoğunluk artışı oluşturur.
    /// </summary>
    [System.Serializable]
    public class Wave
    {
        [Tooltip("Bu dalgada kullanılacak düşman pool anahtarı (ObjectPooler key).")]
        public string enemyPoolKey = "Enemy";

        [Min(1)] public int count = 10;

        [Tooltip("Aynı dalgadaki düşmanlar arası spawn aralığı (saniye).")]
        [Min(0.02f)] public float spawnInterval = 0.3f;

        [Tooltip("Bu dalga, seviye başlangıcından kaç saniye sonra tetiklensin.")]
        [Min(0f)] public float startDelay = 0f;
    }
}
