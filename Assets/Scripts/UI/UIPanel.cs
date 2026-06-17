using PrimeTween;
using UnityEngine;

namespace SkyloftGame.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = 0.25f;
        [SerializeField] private bool  _scaleOnShow  = true;

        [Tooltip("When enabled, the animation runs independently of Time.timeScale. " +
                 "Required for panels that must be visible while timeScale=0, such as the pause menu.")]
        [SerializeField] private bool  _useUnscaledTime = false;

        protected CanvasGroup CanvasGroup { get; private set; }
        private Sequence _animation;
        private bool _shown;

        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();

            if (!_shown) ApplyHidden();
        }

        public void Show()
        {
            _shown = true;
            gameObject.SetActive(true);
            SetInteractable(true);
            OnBeforeShow();

            _animation.Stop();
            CanvasGroup.alpha = 0f;

            _animation = Sequence.Create(useUnscaledTime: _useUnscaledTime)
                .Group(Tween.Alpha(CanvasGroup, 1f, _fadeDuration, useUnscaledTime: _useUnscaledTime));

            if (_scaleOnShow)
                _animation.Group(Tween.Scale(transform, Vector3.one * 0.85f, Vector3.one,
                                             _fadeDuration, Ease.OutBack, useUnscaledTime: _useUnscaledTime));
        }

        public void Hide()
        {
            _shown = false;
            if (!gameObject.activeInHierarchy) return;
            SetInteractable(false);

            _animation.Stop();
            _animation = Sequence.Create(useUnscaledTime: _useUnscaledTime)
                .Group(Tween.Alpha(CanvasGroup, 0f, _fadeDuration, useUnscaledTime: _useUnscaledTime))
                .ChainCallback(() => gameObject.SetActive(false));
        }

        protected virtual void OnBeforeShow() { }

        private void SetInteractable(bool value)
        {
            CanvasGroup.interactable   = value;
            CanvasGroup.blocksRaycasts = value;
        }

        private void ApplyHidden()
        {
            CanvasGroup.alpha = 0f;
            SetInteractable(false);
            gameObject.SetActive(false);
        }
    }
}
