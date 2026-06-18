using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SkyloftGame.Audio;

namespace SkyloftGame.UI
{
    [RequireComponent(typeof(Button))]
    public abstract class ButtonBase : MonoBehaviour, IButton
    {
        [Header("Content")]
        [Tooltip("Catalog asset holding every button's label + icon.")]
        [SerializeField] private ButtonCatalog _catalog;
        [Tooltip("Which catalog entry this button pulls its content from.")]
        [SerializeField] private ButtonId      _id;
        [SerializeField] private Image         _icon;
        [SerializeField] private TMP_Text      _label;
        [SerializeField] private AudioCue      _clickCue = AudioCue.UIClick;

        [Header("Click Feedback")]
        [Tooltip("Scale the button punches to on click (1 = none).")]
        [SerializeField] private float _pressedScale = 0.9f;
        [SerializeField] private float _feedbackDuration = 0.06f;

        private Button _button;
        private Tween  _clickTween;

        public Sprite Icon
        {
            get => _icon != null ? _icon.sprite : null;
            set { if (_icon != null) _icon.sprite = value; }
        }

        public string Label
        {
            get => _label != null ? _label.text : null;
            set { if (_label != null) _label.text = value; }
        }

        public bool Interactable
        {
            get => _button != null && _button.interactable;
            set { if (_button != null) _button.interactable = value; }
        }

        protected virtual void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleClick);
            ApplyContent();
        }

        protected virtual void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(HandleClick);
            _clickTween.Stop();
        }

        private void ApplyContent()
        {
            if (_catalog == null || !_catalog.TryGet(_id, out var entry)) return;
            if (entry.icon != null)                 Icon  = entry.icon;
            if (!string.IsNullOrEmpty(entry.label)) Label = entry.label;
        }

        private void HandleClick()
        {
            AudioEvents.Play(_clickCue);
            PlayClickFeedback();
            OnClick();
        }

        protected virtual void PlayClickFeedback()
        {
            _clickTween.Stop();
            transform.localScale = Vector3.one;
            _clickTween = Tween.Scale(transform, _pressedScale, _feedbackDuration,
                                      Ease.OutQuad, cycles: 2, cycleMode: CycleMode.Yoyo,
                                      useUnscaledTime: true);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (_catalog == null || !_catalog.TryGet(_id, out var entry)) return;
            if (_icon != null && entry.icon != null)                  _icon.sprite = entry.icon;
            if (_label != null && !string.IsNullOrEmpty(entry.label)) _label.text  = entry.label;
        }
#endif

        protected abstract void OnClick();
    }
}
