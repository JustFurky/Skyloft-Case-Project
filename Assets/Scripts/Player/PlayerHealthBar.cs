using PrimeTween;
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

        [Tooltip("Duration, target colors and billboard behaviour.")]
        [SerializeField] private HealthBarData _data;

        private Transform _cameraTransform;
        private Tween     _fillTween;
        private Tween     _colorTween;

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
            _fillTween.Stop();
            _colorTween.Stop();
        }

        private void LateUpdate()
        {
            if (_data != null && _data.faceCamera && _cameraTransform != null)
                transform.forward = _cameraTransform.forward;
        }

        private void Refresh(float current, float max)
        {
            if (_fill == null) return;

            float ratio    = max > 0f ? current / max : 0f;
            float duration = _data != null ? _data.fillDuration : 0.2f;

            _fillTween.Stop();
            _fillTween = Tween.UIFillAmount(_fill, ratio, duration);

            if (_data != null)
            {
                Color color = Color.Lerp(_data.lowHealthColor, _data.fullHealthColor, ratio);
                _colorTween.Stop();
                _colorTween = Tween.Color(_fill, color, duration);
            }
        }
    }
}
