using NUnit.Framework;
using UnityEngine;
using SkyloftGame.Data;

namespace SkyloftGame.Tests
{
    /// <summary>
    /// Şifreli kayıt servisinin yuvarlak-yolculuğunu (Save → Load) ve rastgele-IV
    /// davranışını doğrular. Test, kendi PlayerPrefs anahtarını sonunda temizler.
    /// </summary>
    public class EncryptedJsonDataServiceTests
    {
        [TearDown]
        public void Cleanup() => new EncryptedJsonDataService().Delete();

        [Test]
        public void SaveThenLoad_RoundTripsValues()
        {
            var service = new EncryptedJsonDataService();
            var data    = new GameData { totalEnemiesKilled = 123, highestUnlockedLevel = 2 };

            service.Save(data);
            GameData loaded = new EncryptedJsonDataService().Load();   // taze servis = gerçek senaryo

            Assert.AreEqual(123, loaded.totalEnemiesKilled);
            Assert.AreEqual(2,   loaded.highestUnlockedLevel);
        }

        [Test]
        public void Load_WithoutSavedData_ReturnsDefault()
        {
            new EncryptedJsonDataService().Delete();

            GameData loaded = new EncryptedJsonDataService().Load();

            Assert.AreEqual(0, loaded.totalEnemiesKilled);
            Assert.AreEqual(0, loaded.highestUnlockedLevel);
        }

        [Test]
        public void Encrypt_UsesRandomIv_DifferentCipherForSamePlaintext()
        {
            const string Key = "gd_v1";
            var service = new EncryptedJsonDataService();
            var data    = new GameData { totalEnemiesKilled = 7, highestUnlockedLevel = 1 };

            service.Save(data);
            string first = PlayerPrefs.GetString(Key);

            service.Save(data);
            string second = PlayerPrefs.GetString(Key);

            // Rastgele IV sayesinde aynı veri farklı şifreli metin üretmeli...
            Assert.AreNotEqual(first, second, "Her kayıt farklı IV ile şifrelenmeli.");

            // ...ama her ikisi de doğru çözülebilmeli.
            GameData loaded = new EncryptedJsonDataService().Load();
            Assert.AreEqual(7, loaded.totalEnemiesKilled);
        }
    }
}
