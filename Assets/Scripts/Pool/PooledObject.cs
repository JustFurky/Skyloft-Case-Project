using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SkyloftGame.Pool
{
    [DisallowMultipleComponent]
    public class PooledObject : MonoBehaviour
    {
        public PoolId PoolKey { get; internal set; }

        private bool _released;

        private CancellationTokenSource _releaseCts;

        private void OnEnable() => _released = false;

        public void Release()
        {
            if (_released) return;
            _released = true;
            CancelPendingRelease();

            if (ObjectPooler.Instance != null)
                ObjectPooler.Instance.Release(PoolKey, gameObject);
            else
                Destroy(gameObject);
        }

        public void ReleaseAfter(float seconds)
        {
            CancelPendingRelease();
            _releaseCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            DelayedReleaseAsync(seconds, _releaseCts.Token).Forget();
        }

        private async UniTaskVoid DelayedReleaseAsync(float seconds, CancellationToken token)
        {
            bool canceled = await UniTask
                .Delay(TimeSpan.FromSeconds(seconds), cancellationToken: token)
                .SuppressCancellationThrow();

            if (!canceled) Release();
        }

        private void OnDisable() => CancelPendingRelease();

        private void CancelPendingRelease()
        {
            if (_releaseCts == null) return;
            _releaseCts.Cancel();
            _releaseCts.Dispose();
            _releaseCts = null;
        }
    }
}
