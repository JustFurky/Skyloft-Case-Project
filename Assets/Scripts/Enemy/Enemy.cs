using System;
using PrimeTween;
using UnityEngine;
using SkyloftGame.Pool;
using SkyloftGame.Core;

namespace SkyloftGame.Enemy
{
    [RequireComponent(typeof(EnemyAI))]
    [RequireComponent(typeof(PooledObject))]
    public class Enemy : MonoBehaviour, IPoolable, IDamageable
    {
        [Header("Data")]
        [SerializeField] private EnemyData _data;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer _bodyRenderer;


        public event Action<float, float> OnDamaged;
        public event Action<Enemy>        OnDied;

        public float     CurrentHp { get; private set; }
        public bool      IsDead    { get; private set; }
        public EnemyData Data      => _data;

        private EnemyAI      _ai;
        private PooledObject _pooledObject;
        private MaterialPropertyBlock _mpb;

        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorPropertyId     = Shader.PropertyToID("_Color");

        private Color _baseColor = Color.white;
        private Tween _hitFlashTween;

        private void Awake()
        {
            _ai           = GetComponent<EnemyAI>();
            _pooledObject = GetComponent<PooledObject>();
            _mpb          = new MaterialPropertyBlock();
            _baseColor    = ReadMaterialBaseColor();
        }

        private Color ReadMaterialBaseColor()
        {
            if (_bodyRenderer == null) return Color.white;
            var mat = _bodyRenderer.sharedMaterial;
            if (mat == null) return Color.white;
            if (mat.HasProperty(BaseColorPropertyId)) return mat.GetColor(BaseColorPropertyId);
            if (mat.HasProperty(ColorPropertyId))     return mat.GetColor(ColorPropertyId);
            return Color.white;
        }

        public void OnSpawn()
        {
            if (_data == null)
            {
                Debug.LogError($"[Enemy] EnemyData is not assigned: {name}", this);
                _pooledObject.Release();
                return;
            }

            IsDead    = false;
            CurrentHp = _data.maxHp;

            _ai.Init(_data, PlayerLocator.Current);
            _ai.StartAI();

            SetRendererColor(_baseColor);
            EnemyRegistry.Register(this);

            _data.spawnVfx.Play(transform);
        }

        public void OnDespawn()
        {
            EnemyRegistry.Unregister(this);
            _ai.StopAI();
            _hitFlashTween.Stop();
            IsDead    = true;
            OnDamaged = null;
            OnDied    = null;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            float effective = Mathf.Max(0f, amount * (1f - _data.armor));
            CurrentHp -= effective;

            OnDamaged?.Invoke(effective, CurrentHp);

            if (_bodyRenderer != null)
                StartHitFlash();

            if (CurrentHp <= 0f)
                Die();
        }

        public void SetTarget(Transform newTarget) => _ai.SetTarget(newTarget);

        public void ReturnToPool() => _pooledObject.Release();

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            EnemyRegistry.NotifyKilled(this);
            _data.deathVfx.Play(transform);
            OnDied?.Invoke(this);
            _pooledObject.Release();
        }

        private void StartHitFlash()
        {
            _hitFlashTween.Stop();         // stop a running flash (re-triggered by a new hit)
            SetRendererColor(_baseColor);  // return to normal first

            _hitFlashTween = Tween.Custom(this, 1f, 0f, _data.hitFlashDuration,   // then replay
                (Enemy e, float t) => e.SetRendererColor(
                    Color.Lerp(e._baseColor, e._data.hitFlashColor, t)));
        }

        private void SetRendererColor(Color color)
        {
            _bodyRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColorPropertyId, color);
            _mpb.SetColor(ColorPropertyId, color);
            _bodyRenderer.SetPropertyBlock(_mpb);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_data == null || IsDead) return;
            float ratio  = CurrentHp / _data.maxHp;
            Vector3 pos  = transform.position + Vector3.up * 2.5f;
            Gizmos.color = Color.Lerp(Color.red, Color.green, ratio);
            Gizmos.DrawLine(pos - Vector3.right * 0.5f, pos + Vector3.right * (ratio - 0.5f));
        }
#endif
    }
}
