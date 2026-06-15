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
        [Header("Veri")]
        [SerializeField] private EnemyData _data;

        [Header("Görsel Geri Bildirim")]
        [SerializeField] private Renderer _bodyRenderer;

        [Tooltip("Ölüm anında oynatılacak partikül efekti pool anahtarı (opsiyonel).")]
        [SerializeField] private string _deathVfxKey = "DeathVfx";

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
                Debug.LogError($"[Enemy] EnemyData atanmamış: {name}", this);
                _pooledObject.Release();
                return;
            }

            IsDead    = false;
            CurrentHp = _data.maxHp;

            // Hedefi her spawn'da FindWithTag ile aramak yerine merkezi locator'dan al.
            _ai.Init(_data, PlayerLocator.Current);
            _ai.StartAI();

            SetRendererColor(Color.white);
            EnemyRegistry.Register(this);
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

        /// <summary>Düşmanı (öldürmeden) güvenli biçimde pool'a iade eder — seviye temizliği için.</summary>
        public void ReturnToPool() => _pooledObject.Release();

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            // Skor sistemi tek kill kanalı olarak buna abonedir (DataManager bağımlılığı yok).
            EnemyRegistry.NotifyKilled(this);
            SpawnDeathVfx();
            OnDied?.Invoke(this);
            _pooledObject.Release();
        }

        private void SpawnDeathVfx()
        {
            if (string.IsNullOrEmpty(_deathVfxKey) || ObjectPooler.Instance == null) return;
            ObjectPooler.Instance.Get(_deathVfxKey, transform.position, Quaternion.identity);
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
