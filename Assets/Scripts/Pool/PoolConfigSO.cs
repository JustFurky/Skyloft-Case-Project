using UnityEngine;

namespace SkyloftGame.Pool
{
    /// <summary>
    /// Pool konfigürasyonunu yeniden kullanılabilir bir asset olarak saklayan
    /// ScriptableObject. Tasarımcı dostu: pool ayarları sahneden bağımsız
    /// düzenlenir ve birden çok yerde paylaşılabilir.
    ///
    /// Oluşturma: Assets/Create/SkyloftGame/Pool Config
    /// ObjectPooler bu asset'leri "Config Assets" listesinden okur.
    ///
    /// NOT: Unity, ScriptableObject sınıfının dosya adıyla aynı isimde olmasını
    /// zorunlu kılar — bu yüzden ayrı bir dosyada tutulur.
    /// </summary>
    [CreateAssetMenu(menuName = "SkyloftGame/Pool Config", fileName = "PoolConfig")]
    public class PoolConfigSO : ScriptableObject
    {
        public PoolConfig config;
    }
}
