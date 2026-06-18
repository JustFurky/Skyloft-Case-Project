using System;
using UnityEngine;

namespace SkyloftGame.UI
{
    [CreateAssetMenu(menuName = "SkyloftGame/Button Catalog", fileName = "ButtonCatalog")]
    public class ButtonCatalog : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public ButtonId id;
            public string   label;
            public Sprite   icon;
        }

        [SerializeField] private Entry[] _entries;

        public bool TryGet(ButtonId id, out Entry entry)
        {
            if (_entries != null)
                foreach (var e in _entries)
                    if (e.id == id) { entry = e; return true; }

            entry = default;
            return false;
        }
    }
}
