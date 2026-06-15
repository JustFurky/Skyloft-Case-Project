using UnityEngine;

namespace SkyloftGame.Player
{
    /// <summary>
    /// IMoveInput uygulayan tüm MonoBehaviour girdi kaynakları için ortak temel.
    ///
    /// Unity arayüz alanlarını Inspector'da serileştiremediğinden, PlayerController
    /// bu soyut tipi referans alır; böylece joystick ve klavye kaynakları
    /// polimorfik olarak takılıp değiştirilebilir (OCP).
    /// </summary>
    public abstract class MovementInputSource : MonoBehaviour, IMoveInput
    {
        public abstract Vector2 Direction { get; }
    }
}
