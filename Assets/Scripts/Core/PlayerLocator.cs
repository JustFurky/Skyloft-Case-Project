using UnityEngine;

namespace SkyloftGame.Core
{
    /// <summary>
    /// Oyuncu Transform'una tek noktadan, taramasız erişim.
    ///
    /// PlayerController etkinleşince kendini buraya yazar; düşmanlar ve mermiler
    /// her spawn'da GameObject.FindWithTag çağırmak yerine buradan okur.
    /// Büyük düşman dalgalarında bu, kayda değer bir spawn maliyeti tasarrufudur.
    /// </summary>
    public static class PlayerLocator
    {
        public static Transform Current { get; private set; }

        public static bool Exists => Current != null;

        public static void Set(Transform player)   => Current = player;
        public static void Clear(Transform player)  { if (Current == player) Current = null; }
    }
}
