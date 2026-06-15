using UnityEngine;
using UnityEngine.InputSystem;

namespace SkyloftGame.Player
{
    /// <summary>
    /// Editörde/PC'de hızlı test için WASD + ok tuşu girdi kaynağı.
    /// Yeni Input System (com.unity.inputsystem) üzerinden okur.
    /// Mobil derlemede joystick birincil kaynaktır; bu yalnızca geliştirme kolaylığıdır.
    /// </summary>
    public class KeyboardInputSource : MovementInputSource
    {
        public override Vector2 Direction
        {
            get
            {
                var kb = Keyboard.current;
                if (kb == null) return Vector2.zero;

                float x = 0f, y = 0f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  y -= 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    y += 1f;

                Vector2 dir = new(x, y);
                return dir.sqrMagnitude > 1f ? dir.normalized : dir;
            }
        }
    }
}
