using UnityEngine;
using SkyloftGame.Pool;

namespace SkyloftGame.VFX
{
    /// <summary>
    /// Havuzlanan tek-atımlık parçacık efekti (isabet, ölüm patlaması vb.).
    /// Spawn'da baştan oynatılır ve süresi dolunca otomatik pool'a iade edilir.
    /// Böylece partikül efektleri de sıfır-Instantiate maliyetiyle yönetilir.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(PooledObject))]
    public class PooledParticle : MonoBehaviour, IPoolable
    {
        private ParticleSystem _ps;
        private PooledObject   _pooledObject;

        private void Awake()
        {
            _ps           = GetComponent<ParticleSystem>();
            _pooledObject = GetComponent<PooledObject>();
        }

        public void OnSpawn()
        {
            _ps.Clear();
            _ps.Play();
            _pooledObject.ReleaseAfter(_ps.main.duration + _ps.main.startLifetime.constantMax);
        }

        public void OnDespawn() => _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
