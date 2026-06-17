using System;
using UnityEngine;
using SkyloftGame.Level;

namespace SkyloftGame.Gameplay
{
    public interface IEnemySpawner
    {
        int AliveCount { get; }

        bool IsCleared { get; }

        event Action<int> WaveCountdownTick;

        event Action WaveStarted;

        void BeginLevel(LevelData level, Transform target);

        void StopAndClear();
    }
}
