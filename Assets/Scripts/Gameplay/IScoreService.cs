using System;

namespace SkyloftGame.Gameplay
{
    public interface IScoreService
    {
        int RunKills { get; }

        int TotalKills { get; }

        event Action<int> OnRunKillsChanged;

        void ResetRun();
    }
}
