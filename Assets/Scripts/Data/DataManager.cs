using UnityEngine;

namespace SkyloftGame.Data
{
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

            if (transform.parent != null) transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            _service ??= new EncryptedJsonDataService();
            Data = _service.Load();
        }

        private void OnApplicationQuit() => _service.Save(Data);
        private void OnApplicationPause(bool paused) { if (paused) _service.Save(Data); }

        public void InjectService(IDataService service)
        {
            _service = service;
            Data     = _service.Load();
        }

        public void AddEnemyKill() => Data.totalEnemiesKilled++;

        public void UnlockLevel(int levelIndex)
        {
            if (levelIndex <= Data.highestUnlockedLevel) { _service.Save(Data); return; }
            Data.highestUnlockedLevel = levelIndex;
            _service.Save(Data);
        }

        public void Save() => _service.Save(Data);

        public void ResetLevelProgress()
        {
            Data.highestUnlockedLevel = 0;
            _service.Save(Data);
        }

        public void ResetData()
        {
            _service.Delete();
            Data = GameData.Default();
        }
    }
}
