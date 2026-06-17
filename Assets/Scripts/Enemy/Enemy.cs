using System;
using System.Collections;
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

        [Tooltip("Particle effect pool played on spawn (optional).")]
        [SerializeField] private PoolId _spawnVfx;

        [Tooltip("Particle effect pool played on death (optional).")]
        [SerializeField] private PoolId _deathVfx;

        public event Action<float, float> OnDamaged;
        public event Action<Enemy>        OnDied;

        public float     CurrentHp { get; private set; }
        public bool      IsDead    { get; private set; }
        public EnemyData Data      => _data;

        private EnemyAI      _ai;
        private PooledObject _pooledObject;
        private MaterialPropertyBlock _mpb;

        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly WaitForSeconds HitFlashDuration = new(0.08f);

        private void Awake()
        {
            _ai           = GetComponent<EnemyAI>();
            _pooledObject = GetComponent<PooledObject>();
            _mpb          = new MaterialPropertyBlock();
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

            SetRendererColor(Color.white);
            EnemyRegistry.Register(this);

            SpawnVfx(_spawnVfx);
        }

        public void OnDespawn()
        {
            EnemyRegistry.Unregister(this);
            _ai.StopAI();
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
                StartCoroutine(HitFlashRoutine());

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
            SpawnDeathVfx();
            OnDied?.Invoke(this);
            _pooledObject.Release();
        }

        private void SpawnDeathVfx() => SpawnVfx(_deathVfx);

        private void SpawnVfx(PoolId vfx)
        {
            if (vfx == null || ObjectPooler.Instance == null) return;
            ObjectPooler.Instance.Get(vfx, transform.position, Quaternion.identity);
        }

        private IEnumerator HitFlashRoutine()
        {
            SetRendererColor(Color.red);
            yield return HitFlashDuration;
            SetRendererColor(Color.white);
        }

        private void SetRendererColor(Color color)
        {
            _bodyRenderer.GetPropertyBlock(_mpb);
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
