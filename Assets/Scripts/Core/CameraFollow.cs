using UnityEngine;

namespace SkyloftGame.Core
{
    /// <summary>
    /// Kamerayı, verilen offset kadar uzaktan hedefi (oyuncu) yumuşakça takip ettirir.
    ///
    /// Hedef Inspector'dan atanabilir; atanmazsa PlayerLocator üzerinden otomatik
    /// bulunur (sahnedeki oyuncuya doğrudan referans gerekmez — gevşek bağlılık).
    ///
    /// LateUpdate'te çalışır: oyuncu o karede hareket ettikten SONRA kamerayı
    /// günceller; böylece titreme (jitter) olmaz.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("Takip edilecek hedef. Boşsa PlayerLocator.Current kullanılır.")]
        [SerializeField] private Transform _target;

        [Tooltip("Hedefe göre dünya-uzayı offset (örn. tepeden-açılı için (0, 14, -8)).")]
        [SerializeField] private Vector3 _offset = new(0f, 14f, -8f);

        [Tooltip("Konum takibi yumuşaklığı (saniye). 0 = anında kilitlen.")]
        [Min(0f)] [SerializeField] private float _smoothTime = 0.15f;

        [Tooltip("Açıkken kamera her zaman hedefe bakar; kapalıyken sabit yönelimi korur.")]
        [SerializeField] private bool _lookAtTarget = true;

        [Tooltip("Bakış noktasının hedeften yukarı ofseti (ayak yerine gövdeye bakmak için).")]
        [SerializeField] private float _lookHeightOffset = 1.5f;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            Transform target = ResolveTarget();
            if (target == null) return;

            Vector3 desired = target.position + _offset;

            transform.position = _smoothTime > 0f
                ? Vector3.SmoothDamp(transform.position, desired, ref _velocity, _smoothTime)
                : desired;

            if (_lookAtTarget)
                transform.LookAt(target.position + Vector3.up * _lookHeightOffset);
        }

        private Transform ResolveTarget() => _target != null ? _target : PlayerLocator.Current;
    }
}
