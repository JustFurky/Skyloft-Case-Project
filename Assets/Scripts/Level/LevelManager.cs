using UnityEngine;
using Zenject;
using SkyloftGame.Data;

namespace SkyloftGame.Level
{
    public class LevelManager : MonoBehaviour, ILevelService
    {
        [SerializeField] private LevelDatabase _database;

        [Inject] private DataManager _data;

        public LevelData Current      { get; private set; }
        public int       CurrentIndex { get; private set; } = -1;
        public int       Count        => _database != null ? _database.Count : 0;
        public bool      HasNext      => _database != null && CurrentIndex + 1 < _database.Count;

        public int HighestUnlockedIndex => _data != null ? _data.Data.highestUnlockedLevel : 0;

        private void Awake()
        {
            if (_database == null || _database.Count == 0)
                Debug.LogError("[LevelManager] LevelDatabase is not assigned or is empty.", this);
        }

        public void Load(int index)
        {
            if (_database == null || !_database.IsValidIndex(index))
            {
                Debug.LogError($"[LevelManager] Invalid level index: {index}", this);
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
            int nextUnlocked = Mathf.Min(CurrentIndex + 1, Mathf.Max(0, Count - 1));
            _data?.UnlockLevel(nextUnlocked);
        }
    }
}
