using UnityEngine;

namespace SkyloftGame.Player
{
    /// <summary>
    /// Hareket girdisinin kaynağından bağımsız sözleşmesi.
    /// PlayerController yalnızca bu arayüze bağlıdır; girdi ister sanal joystick,
    /// ister klavye, ister yapay (test) bir kaynaktan gelsin fark etmez (DIP).
    /// </summary>
    public interface IMoveInput
    {
        /// <summary>Normalize edilmiş hareket yönü (XY). x = sağ/sol, y = ileri/geri.</summary>
        Vector2 Direction { get; }
    }
}
