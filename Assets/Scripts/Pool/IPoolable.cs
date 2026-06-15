namespace SkyloftGame.Pool
{
    /// <summary>
    /// Pool'dan alınan veya pool'a geri verilen nesnelerin uygulaması gereken arayüz.
    ///
    /// Uygulama notu:
    ///   - OnSpawn  → GameObject.SetActive(true)  SONRASINDA çağrılır.
    ///   - OnDespawn → GameObject.SetActive(false) ÖNCESİNDE çağrılır.
    ///
    /// Bu sayede her nesne kendi reset mantığını (pozisyon, hız, HP, vb.) yönetir
    /// ve ObjectPooler hiçbir nesneye özel bilgi taşımak zorunda kalmaz.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Nesne pool'dan alınıp aktif edildiğinde çağrılır.
        /// Başlangıç değerlerini (velocity, hp, vfx vb.) burada sıfırla / uygula.
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Nesne pool'a geri döndürülmeden hemen önce çağrılır.
        /// Coroutine'leri durdur, rigidbody'yi sıfırla, event aboneliklerini kaldır.
        /// </summary>
        void OnDespawn();
    }
}
