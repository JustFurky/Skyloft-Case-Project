using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkyloftGame.Enemy
{
    public static class EnemyRegistry
    {
        private static readonly HashSet<Enemy> Alive = new();

        public static int AliveCount => Alive.Count;

        public static event Action<Enemy> Killed;

        public static void Register(Enemy enemy)   => Alive.Add(enemy);
        public static void Unregister(Enemy enemy) => Alive.Remove(enemy);

        public static void NotifyKilled(Enemy enemy) => Killed?.Invoke(enemy);

        public static List<Enemy> Snapshot() => new(Alive);

        public static void Clear() => Alive.Clear();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Alive.Clear();
            Killed = null;
        }
    }
}
