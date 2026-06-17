using UnityEngine;

namespace SkyloftGame.Core
{
    public static class PlayerLocator
    {
        public static Transform Current { get; private set; }

        public static bool Exists => Current != null;

        public static void Set(Transform player)   => Current = player;
        public static void Clear(Transform player)  { if (Current == player) Current = null; }
    }
}
