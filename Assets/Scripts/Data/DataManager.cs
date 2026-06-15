using UnityEngine;

namespace SkyloftGame.Data
{
    /// <summary>
    /// Singleton MonoBehaviour — oyun verisinin tek erişim noktası.
    ///
    /// Bağımlılık enjeksiyonu:
    ///   Varsayılan servis EncryptedJsonDataService'dir.
    ///   Test veya farklı platform için Awake öncesinde InjectService() çağrılabilir.
    ///
    /// Kullanım:
    ///   DataManager.Instance.AddEnemyKill();
    ///   int total = DataManager.Instance.Data.totalEnemiesKilled;
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        public GameData Data { get; private set; }

        private IDataService _service;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _service ??= new EncryptedJsonDataService();
            Data = _service.Load();
        }

        private void OnApplicationQuit() => _service.Save(Data);
        private void OnApplicationPause(bool paused) { if (paused) _service.Save(Data); }

        /// <summary>Servis bağımlılığını değiştir (test / platform senaryoları için).</summary>
        public void InjectService(IDataService service)
        {
            _service = service;
            Data     = _service.Load();
        }

        /// <summary>
        /// Öldürülen düşman sayısını artırır. Performans için her ölümde diske
        /// yazmaz; kalıcılık seviye sonu (Save), uygulama duraklama/çıkışında garanti edilir.
        /// Büyük dalgalarda yüzlerce PlayerPrefs yazımını önler.
        /// </summary>
        public void AddEnemyKill() => Data.totalEnemiesKilled++;

        /// <summary>Verilen seviyeyi (yalnızca daha yüksekse) kalıcı olarak açar ve kaydeder.</summary>
        public void UnlockLevel(int levelIndex)
        {
            if (levelIndex <= Data.highestUnlockedLevel) { _service.Save(Data); return; }
            Data.highestUnlockedLevel = levelIndex;
            _service.Save(Data);
        }

        /// <summary>Mevcut veriyi diske yazar (seviye sonu vb. açık kayıt noktaları).</summary>
        public void Save() => _service.Save(Data);

        /// <summary>Tüm kayıtlı veriyi siler ve modeli sıfırlar.</summary>
        public void ResetData()
        {
            _service.Delete();
            Data = GameData.Default();
        }
    }
}
