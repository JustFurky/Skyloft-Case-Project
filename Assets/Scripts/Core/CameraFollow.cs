using UnityEngine;

namespace SkyloftGame.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("Target to follow. If empty, PlayerLocator.Current is used.")]
        [SerializeField] private Transform _target;

        [Tooltip("World-space offset from the target (e.g. (0, 14, -8) for a top-down angle).")]
        [SerializeField] private Vector3 _offset = new(0f, 14f, -8f);

        [Tooltip("Position follow smoothness (seconds). 0 = snap instantly.")]
        [Min(0f)] [SerializeField] private float _smoothTime = 0.15f;

        [Tooltip("When on, the camera always looks at the target; when off, it keeps a fixed orientation.")]
        [SerializeField] private bool _lookAtTarget = true;

        [Tooltip("Upward offset of the look point from the target (to look at the body instead of the feet).")]
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
