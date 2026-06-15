using UnityEngine;
using SkyloftGame.Data;

namespace SkyloftGame.Level
{
    /// <summary>
    /// LevelDatabase üzerinden seviye yükleyen ve ilerlemeyi DataManager ile
    /// kalıcı tutan servis. State makinesi seviye geçişlerinde bunu kullanır.
    /// </summary>
    public class LevelManager : MonoBehaviour, ILevelService
    {
        [SerializeField] private LevelDatabase _database;

        public LevelData Current      { get; private set; }
        public int       CurrentIndex { get; private set; } = -1;
        public int       Count        => _database != null ? _database.Count : 0;
        public bool      HasNext      => _database != null && CurrentIndex + 1 < _database.Count;

        public int HighestUnlockedIndex =>
            DataManager.Instance != null ? DataManager.Instance.Data.highestUnlockedLevel : 0;

        private void Awake()
        {
            if (_database == null || _database.Count == 0)
                Debug.LogError("[LevelManager] LevelDatabase atanmamış veya boş.", this);
        }

        public void Load(int index)
        {
            if (_database == null || !_database.IsValidIndex(index))
            {
                Debug.LogError($"[LevelManager] Geçersiz seviye index'i: {index}", this);
                return;
            }

            CurrentIndex = index;
            Current      = _database.Get(index);
        }

        public bool TryAdvance()
        {
            if (!HasNext) return false;
            Load(CurrentIndex + 1);
            return true;
        }

        public void MarkCurrentCompleted()
        {
            // Bir sonraki seviyeyi kalıcı olarak aç (mevcut + 1, sınır içinde).
            int nextUnlocked = Mathf.Min(CurrentIndex + 1, Mathf.Max(0, Count - 1));
            DataManager.Instance?.UnlockLevel(nextUnlocked);
        }
    }
}
