using UnityEngine;

namespace SkyloftGame.Level
{
    [CreateAssetMenu(menuName = "SkyloftGame/Level Database", fileName = "LevelDatabase")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] private LevelData[] _levels;

        public int Count => _levels != null ? _levels.Length : 0;

        public bool IsValidIndex(int index) => index >= 0 && index < Count;

        public LevelData Get(int index) => IsValidIndex(index) ? _levels[index] : null;
    }
}
