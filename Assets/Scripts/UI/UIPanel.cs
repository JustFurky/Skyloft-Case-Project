using PrimeTween;
using UnityEngine;

namespace SkyloftGame.UI
{
    /// <summary>
    /// Tüm UI panellerinin ortak temeli. CanvasGroup üzerinden PrimeTween ile
    /// göster/gizle (fade + scale) ve etkileşim kontrolü sağlar. Alt sınıflar
    /// yalnızca içeriklerini doldurmaya odaklanır (DRY + SRP).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = 0.25f;
        [SerializeField] private bool  _scaleOnShow  = true;

        protected CanvasGroup CanvasGroup { get; private set; }
        private Sequence _animation;
        private bool _shown;

        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();

            // Başlangıçta gizle — AMA Show() çağrısının tetiklediği SetActive(true)
            // içinde Awake çalışırsa (panel sahnede pasif başladıysa) paneli yeniden
            // kapatma; bu yüzden _shown bayrağına bakılır.
            if (!_shown) ApplyHidden();
        }

        public void Show()
        {
            _shown = true;
            gameObject.SetActive(true);   // pasif başlamış panelde Awake'i tetikler; o da _shown'a bakıp gizlemez
            SetInteractable(true);
            OnBeforeShow();

            _animation.Stop();            // varsa önceki animasyonu kes (dead sequence'te güvenli no-op)
            CanvasGroup.alpha = 0f;

            _animation = Sequence.Create()
                .Group(Tween.Alpha(CanvasGroup, 1f, _fadeDuration));

            if (_scaleOnShow)
                _animation.Group(Tween.Scale(transform, Vector3.one * 0.85f, Vector3.one,
                                             _fadeDuration, Ease.OutBack));
        }

        public void Hide()
        {
            _shown = false;
            if (!gameObject.activeInHierarchy) return;
            SetInteractable(false);

            _animation.Stop();
            _animation = Sequence.Create()
                .Group(Tween.Alpha(CanvasGroup, 0f, _fadeDuration))
                .ChainCallback(() => gameObject.SetActive(false));
        }

        /// <summary>Panel görünür olmadan hemen önce; içerik doldurma noktası.</summary>
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
