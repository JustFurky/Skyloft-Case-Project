using UnityEngine;

namespace SkyloftGame.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect          _lastSafeArea;
        private Vector2Int    _lastScreen;

        private void Awake() => _rect = GetComponent<RectTransform>();

        private void OnEnable() => Apply();

        private void Update()
        {
            if (Screen.safeArea != _lastSafeArea ||
                Screen.width != _lastScreen.x || Screen.height != _lastScreen.y)
                Apply();
        }

        private void Apply()
        {
            if (_rect == null) return;

            int w = Screen.width;
            int h = Screen.height;
            if (w <= 0 || h <= 0) return;

            Rect safe = Screen.safeArea;
            _lastSafeArea = safe;
            _lastScreen   = new Vector2Int(w, h);

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= w;  anchorMin.y /= h;
            anchorMax.x /= w;  anchorMax.y /= h;

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
