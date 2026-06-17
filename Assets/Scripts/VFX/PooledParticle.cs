using UnityEngine;
using SkyloftGame.Pool;

namespace SkyloftGame.VFX
{
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
