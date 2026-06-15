using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkyloftGame.Enemy
{
    /// <summary>
    /// Sahnedeki canlı düşmanların merkezi kaydı.
    ///
    /// Neden statik servis?
    ///   - Spawner'ın canlı düşman sayısını O(1) okuması (büyük dalga optimizasyonu).
    ///   - Oyuncunun en yakın hedefi FindObjectsOfType taraması olmadan bulması.
    ///   - Skor sisteminin tek bir olaya abone olması (tek kill kanalı).
    ///
    /// Düşmanlar pool'dan alınıp verildikçe Register/Unregister çağırır.
    /// Editör domain reload'da statik durum sıfırlanır; ayrıca Clear() ile manuel
    /// sıfırlama da mümkündür.
    /// </summary>
    public static class EnemyRegistry
    {
        private static readonly HashSet<Enemy> Alive = new();

        public static int AliveCount => Alive.Count;

        /// <summary>Bir düşman öldüğünde (pool'a dönmeden önce) yayınlanır.</summary>
        public static event Action<Enemy> Killed;

        public static void Register(Enemy enemy)   => Alive.Add(enemy);
        public static void Unregister(Enemy enemy) => Alive.Remove(enemy);

        public static void NotifyKilled(Enemy enemy) => Killed?.Invoke(enemy);

        /// <summary>Canlı düşmanların anlık kopyasını döner (iterasyon sırasında güvenli iade için).</summary>
        public static List<Enemy> Snapshot() => new(Alive);

        public static void Clear() => Alive.Clear();

        /// <summary>Verilen konuma en yakın canlı düşmanı, isteğe bağlı menzil içinde bulur.</summary>
        public static Enemy FindNearest(Vector3 position, float maxRange = Mathf.Infinity)
        {
            Enemy nearest = null;
            float bestSqr = maxRange * maxRange;

            foreach (var enemy in Alive)
            {
                if (enemy == null || enemy.IsDead) continue;

                float sqr = (enemy.transform.position - position).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    nearest = enemy;
                }
            }

            return nearest;
        }
    }
}
