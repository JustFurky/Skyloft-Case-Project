using UnityEngine;

namespace SkyloftGame.Enemy
{
    /// <summary>
    /// Punch animasyonunun temas karesindeki Animation Event'i, root'taki EnemyAI'ya iletir.
    ///
    /// Neden ayrı köprü? Unity, Animation Event'leri yalnızca Animator'ın bulunduğu
    /// GameObject üzerindeki bileşenlere gönderir. Animator genelde model child'ındadır;
    /// EnemyAI ise root'tadır. Bu bileşen model child'ına eklenir ve olayı yukarı iletir.
    ///
    /// Kurulum:
    ///   1) Bu bileşeni Animator'ın olduğu (model) GameObject'e ekle.
    ///   2) "Zombie Punching" klibinde yumruğun isabet ettiği kareye bir Animation Event ekle.
    ///   3) Function olarak OnAttackHit seç.
    /// </summary>
    public class EnemyAttackAnimationRelay : MonoBehaviour
    {
        [Tooltip("Boşsa parent zincirinden otomatik bulunur.")]
        [SerializeField] private EnemyAI _ai;

        private void Awake()
        {
            if (_ai == null) _ai = GetComponentInParent<EnemyAI>();
        }

        /// <summary>Animation Event tarafından (temas karesinde) çağrılır.</summary>
        public void OnAttackHit() => _ai?.DealAttackDamage();
    }
}
