using System;

namespace SkyloftGame.Gameplay
{
    public interface ICountdownTimer
    {
        float Remaining { get; }
        float Duration  { get; }
        bool  IsRunning { get; }

        event Action<float> OnTick;

        event Action OnElapsed;

        void Begin(float duration);
        void Stop();
        void Tick(float deltaTime);
    }
}
