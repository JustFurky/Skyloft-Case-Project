using UnityEngine;

namespace SkyloftGame.Pool
{
    /// <summary>
    /// Pool'a alınabilen her prefab'a eklenen hafif köprü bileşeni.
    ///
    /// Görevleri:
    ///   1. Hangi pool'a ait olduğunu bilmek (key).
    ///   2. Nesnenin kendini pool'a geri iade etmesini sağlamak (Release).
    ///   3. Pool'dan bağımsız kodun (örn. mermi hasarı) nesneyi doğrudan
    ///      devre dışı bırakmak yerine güvenli biçimde pool'a iade etmesine
    ///      olanak vermek.
    ///
    /// Kullanım:
    ///   GetComponent<PooledObject>().Release();
    /// </summary>
    [DisallowMultipleComponent]
    public class PooledObject : MonoBehaviour
    {
        // ObjectPooler tarafından, havuzdan ilk çıkarılma anında set edilir.
        public string PoolKey { get; internal set; }

        // Aynı karede iki tetikleme (ör. mermi iki düşmana değme) ya da isabet +
        // lifetime'ın çakışmasında çift iadeyi engeller. Spawn'da (OnEnable) sıfırlanır.
        private bool _released;

        private void OnEnable() => _released = false;

        /// <summary>
        /// Nesneyi kendi pool'una güvenli şekilde iade eder (idempotent — çift iade güvenli).
        /// ObjectPooler bulunamazsa (örn. sahne geçişi) nesneyi yok eder.
        /// </summary>
        public void Release()
        {
            if (_released) return;
            _released = true;
            CancelInvoke(nameof(Release));

            if (ObjectPooler.Instance != null)
                ObjectPooler.Instance.Release(PoolKey, gameObject);
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// Belirtilen saniye sonra pool'a otomatik iade eder.
        /// Mermi ve geçici efektler için kullanışlıdır.
        /// </summary>
        public void ReleaseAfter(float seconds)
        {
            CancelInvoke(nameof(Release));
            Invoke(nameof(Release), seconds);
        }

        private void OnDisable()
        {
            // Bekleyen zamanlı iade varsa iptal et (sahne temizliği vb.)
            CancelInvoke(nameof(Release));
        }
    }
}
