using UnityEngine;
using UnityEngine.UI;

namespace SkyloftGame.Player
{
    public class PlayerHealthBar : MonoBehaviour
    {
        [Tooltip("If left empty, found automatically from the parent chain.")]
        [SerializeField] private PlayerHealth _health;

        [Tooltip("Fill image with Image Type = Filled.")]
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
