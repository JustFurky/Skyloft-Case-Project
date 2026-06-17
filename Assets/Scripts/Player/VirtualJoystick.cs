using UnityEngine;
using UnityEngine.EventSystems;

namespace SkyloftGame.Player
{
    /// <summary>
    /// Mobil için sürükle-tabanlı sanal joystick (UI).
    ///
    /// Kullanım:
    ///   - Bir Canvas altında arka plan (background) RectTransform'a ekle.
    ///   - Handle (sap) RectTransform'unu çocuk olarak ata.
    ///   - PlayerController.MovementSource alanına bu bileşeni sürükle.
    ///
    /// "Dinamik" modda joystick, basılan ilk noktada belirir; bu mobilde en akıcı
    /// his veren yaklaşımdır.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class VirtualJoystick : MovementInputSource,
        IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;

        [Tooltip("Sapın merkezden uzaklaşabileceği maksimum piksel (background yarıçapına göre ölçeklenir).")]
        [Range(0.1f, 1f)] [SerializeField] private float _handleRange = 1f;

        [Tooltip("Bu eşik altındaki girdiler yok sayılır (ölü bölge).")]
        [Range(0f, 0.5f)] [SerializeField] private float _deadZone = 0.1f;

        [Tooltip("Açıkken joystick dokunulan yerde belirir (ve serbestken gizlenir). " +
                 "Kapalıyken sabit ve hep görünür kalır.")]
        [SerializeField] private bool _dynamic = false;

        private Vector2 _direction = Vector2.zero;
        private Canvas  _canvas;
        private Vector2 _restPosition;

        public override Vector2 Direction => _direction;

        private void Start()
        {
            _canvas       = GetComponentInParent<Canvas>();
            _restPosition = _background.anchoredPosition;
            if (_dynamic) _background.gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_dynamic)
            {
                _background.gameObject.SetActive(true);
                _background.position = eventData.position;
            }
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background, eventData.position, eventData.pressEventCamera, out Vector2 local);

            float radius = _background.sizeDelta.x * 0.5f;
            Vector2 raw  = local / radius;            // -1..1 normalize

            _direction = raw.magnitude > 1f ? raw.normalized : raw;
            if (_direction.magnitude < _deadZone) _direction = Vector2.zero;

            _handle.anchoredPosition = _direction * radius * _handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _direction = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;
            if (_dynamic) _background.gameObject.SetActive(false);
            else _background.anchoredPosition = _restPosition;
        }
    }
}
