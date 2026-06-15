using UnityEngine;
using UnityEngine.UI;

namespace SkyloftGame.Player
{
    /// <summary>
    /// Player prefab'ına ait can barı (view). PlayerHealth mantığını dinler ve
    /// doluluk oranını günceller; isteğe bağlı olarak kameraya döner (billboard).
    ///
    /// PlayerHealth, UI'dan habersizdir (SRP). Bar yalnızca durumu yansıtır.
    /// Her şey aynı prefab üzerinde olduğundan inspector bağlaması sahne-arası
    /// referans gerektirmez.
    /// </summary>
    public class PlayerHealthBar : MonoBehaviour
    {
        [Tooltip("Boş bırakılırsa parent zincirinden otomatik bulunur.")]
        [SerializeField] private PlayerHealth _health;

        [Tooltip("Image Type = Filled olan doluluk görseli.")]
        [SerializeField] private Image _fill;

        [SerializeField] private bool _faceCamera = true;

        private Transform _cameraTransform;

        private void Awake()
        {
            if (_health == null) _health = GetComponentInParent<PlayerHealth>();
            if (Camera.main != null) _cameraTransform = Camera.main.transform;
        }

        private void OnEnable()
        {
            if (_health == null) return;
            _health.OnHealthChanged += Refresh;
            Refresh(_health.CurrentHp, _health.MaxHp);
        }

        private void OnDisable()
        {
            if (_health != null) _health.OnHealthChanged -= Refresh;
        }

        private void LateUpdate()
        {
            if (_faceCamera && _cameraTransform != null)
                transform.forward = _cameraTransform.forward;
        }

        private void Refresh(float current, float max)
        {
            if (_fill != null)
                _fill.fillAmount = max > 0f ? current / max : 0f;
        }
    }
}
