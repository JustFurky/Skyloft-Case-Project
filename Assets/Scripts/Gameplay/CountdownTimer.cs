using System;

namespace SkyloftGame.Gameplay
{
    /// <summary>
    /// Çerçeveden bağımsız, test edilebilir geri sayım sayacı.
    /// Update döngüsü yoktur; sahibi her karede Tick(deltaTime) çağırır.
    /// </summary>
    public sealed class CountdownTimer : ICountdownTimer
    {
        public float Remaining { get; private set; }
        public float Duration  { get; private set; }
        public bool  IsRunning { get; private set; }

        public event Action<float> OnTick;
        public event Action         OnElapsed;

        public void Begin(float duration)
        {
            Duration  = duration;
            Remaining = duration;
            IsRunning = true;
            OnTick?.Invoke(Remaining);
        }

        public void Stop() => IsRunning = false;

        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;

            Remaining -= deltaTime;

            if (Remaining <= 0f)
            {
                Remaining = 0f;
                IsRunning = false;
                OnTick?.Invoke(0f);
                OnElapsed?.Invoke();
                return;
            }

            OnTick?.Invoke(Remaining);
        }
    }
}
