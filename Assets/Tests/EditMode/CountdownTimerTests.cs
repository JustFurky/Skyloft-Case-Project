using NUnit.Framework;
using SkyloftGame.Gameplay;

namespace SkyloftGame.Tests
{
    /// <summary>
    /// CountdownTimer saf C#'tır (Unity döngüsünden bağımsız), bu yüzden doğrudan
    /// EditMode'da test edilebilir. Geri sayım, durdurma ve süre-doldu olayını doğrular.
    /// </summary>
    public class CountdownTimerTests
    {
        [Test]
        public void Begin_SetsRemainingAndRunning()
        {
            var timer = new CountdownTimer();
            timer.Begin(10f);

            Assert.IsTrue(timer.IsRunning);
            Assert.AreEqual(10f, timer.Remaining, 1e-4);
            Assert.AreEqual(10f, timer.Duration, 1e-4);
        }

        [Test]
        public void Tick_DecreasesRemaining()
        {
            var timer = new CountdownTimer();
            timer.Begin(10f);

            timer.Tick(3f);

            Assert.AreEqual(7f, timer.Remaining, 1e-4);
            Assert.IsTrue(timer.IsRunning);
        }

        [Test]
        public void Tick_PastDuration_FiresElapsedOnceAndStops()
        {
            var timer    = new CountdownTimer();
            int elapsed  = 0;
            timer.OnElapsed += () => elapsed++;
            timer.Begin(5f);

            timer.Tick(5f);            // tam süre
            timer.Tick(1f);            // bittikten sonra ek tick

            Assert.AreEqual(0f, timer.Remaining, 1e-4);
            Assert.IsFalse(timer.IsRunning);
            Assert.AreEqual(1, elapsed, "OnElapsed yalnızca bir kez tetiklenmeli.");
        }

        [Test]
        public void Stop_PreventsFurtherTicking()
        {
            var timer = new CountdownTimer();
            timer.Begin(10f);

            timer.Stop();
            timer.Tick(5f);

            Assert.IsFalse(timer.IsRunning);
            Assert.AreEqual(10f, timer.Remaining, 1e-4, "Durdurulmuş sayaç ilerlememeli.");
        }

        [Test]
        public void OnTick_ReportsRemaining()
        {
            var timer = new CountdownTimer();
            float lastReported = -1f;
            timer.OnTick += r => lastReported = r;

            timer.Begin(8f);          // başlangıçta da yayınlar
            Assert.AreEqual(8f, lastReported, 1e-4);

            timer.Tick(2f);
            Assert.AreEqual(6f, lastReported, 1e-4);
        }
    }
}
