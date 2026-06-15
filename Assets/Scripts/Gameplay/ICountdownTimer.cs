using System;

namespace SkyloftGame.Gameplay
{
    /// <summary>
    /// Geri sayım sayacı sözleşmesi. Saf C# (MonoBehaviour değil) olacak şekilde
    /// tasarlandı; tetikleme sahibini (PlayingState) Tick ile yapar. Bu sayede
    /// duraklatma/zaman ölçeği davranışı durum makinesinin kontrolündedir.
    /// </summary>
    public interface ICountdownTimer
    {
        float Remaining { get; }
        float Duration  { get; }
        bool  IsRunning { get; }

        /// <summary>Her tick'te kalan süreyi yayınlar (UI için).</summary>
        event Action<float> OnTick;

        /// <summary>Süre sıfıra ulaştığında bir kez tetiklenir.</summary>
        event Action OnElapsed;

        void Begin(float duration);
        void Stop();
        void Tick(float deltaTime);
    }
}
