using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace SkyloftGame.Data
{
    /// <summary>
    /// AES-256 CBC ile şifrelenmiş JSON'u PlayerPrefs'e kaydeden servis.
    ///
    /// Güvenlik notu:
    ///   Anahtar ve IV her cihaz için benzersiz SystemInfo.deviceUniqueIdentifier'dan
    ///   türetilir; bu sayede aynı kayıt dosyası başka cihazda okunabilir değildir.
    ///   Yerel hileciliğe (hex editörü vb.) karşı yeterli koruma sağlar.
    ///   Sunucu taraflı doğrulama gerektiren kritik veriler için bu sınıfı
    ///   bir backend servisi ile tamamlayın.
    /// </summary>
    public sealed class EncryptedJsonDataService : IDataService
    {
        private const string PrefsKey = "gd_v1";

        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptedJsonDataService()
        {
            // Cihaza özgü 32 bayt anahtar + 16 bayt IV türet (SHA-256 / MD5)
            string seed = SystemInfo.deviceUniqueIdentifier + "SKY_SALT_42";
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(seed));

            _key = new byte[32];
            _iv  = new byte[16];
            Array.Copy(hash, 0, _key, 0, 32);
            Array.Copy(hash, 0, _iv,  0, 16);
        }

        public void Save(GameData data)
        {
            string json      = JsonUtility.ToJson(data);
            string encrypted = Encrypt(json);
            PlayerPrefs.SetString(PrefsKey, encrypted);
            PlayerPrefs.Save();
        }

        public GameData Load()
        {
            if (!PlayerPrefs.HasKey(PrefsKey))
                return GameData.Default();

            try
            {
                string encrypted = PlayerPrefs.GetString(PrefsKey);
                string json      = Decrypt(encrypted);
                return JsonUtility.FromJson<GameData>(json) ?? GameData.Default();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DataService] Veri okunamadı, varsayılana dönüldü. ({e.Message})");
                return GameData.Default();
            }
        }

        public void Delete()
        {
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
        }

        private string Encrypt(string plainText)
        {
            using var aes = CreateAes();
            using var enc = aes.CreateEncryptor();
            byte[] input  = Encoding.UTF8.GetBytes(plainText);
            byte[] output = enc.TransformFinalBlock(input, 0, input.Length);
            return Convert.ToBase64String(output);
        }

        private string Decrypt(string cipherText)
        {
            using var aes = CreateAes();
            using var dec = aes.CreateDecryptor();
            byte[] input  = Convert.FromBase64String(cipherText);
            byte[] output = dec.TransformFinalBlock(input, 0, input.Length);
            return Encoding.UTF8.GetString(output);
        }

        private Aes CreateAes()
        {
            var aes = Aes.Create();
            aes.Key     = _key;
            aes.IV      = _iv;
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }
    }
}
