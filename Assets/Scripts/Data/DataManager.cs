using UnityEngine;
using Zenject;

namespace SkyloftGame.Data
{
    public class DataManager : MonoBehaviour
    {
        public GameData Data { get; private set; }

        private IDataService _service;

        [Inject]
        public void Construct(IDataService service)
        {
            _service = service;
            Data     = _service.Load();
        }

        private void OnApplicationQuit() => _service?.Save(Data);
        private void OnApplicationPause(bool paused) { if (paused) _service?.Save(Data); }

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
