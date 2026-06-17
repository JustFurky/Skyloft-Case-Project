using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace SkyloftGame.Data
{
    public sealed class EncryptedJsonDataService : IDataService
    {
        private const string PrefsKey       = "gd_v1";
        private const string CorruptBackupKey = "gd_v1_corrupt";
        private const int    IvSize         = 16;

        private readonly byte[] _key;

        public EncryptedJsonDataService()
        {
            string seed = SystemInfo.deviceUniqueIdentifier + "SKY_SALT_42";
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(seed));

            _key = new byte[32];
            Array.Copy(hash, 0, _key, 0, 32);
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

            string encrypted = PlayerPrefs.GetString(PrefsKey);
            try
            {
                string json = Decrypt(encrypted);
                return JsonUtility.FromJson<GameData>(json) ?? GameData.Default();
            }
            catch (Exception e)
            {
                PlayerPrefs.SetString(CorruptBackupKey, encrypted);
                PlayerPrefs.Save();
                Debug.LogError($"[DataService] Failed to decrypt save, reverted to default. " +
                               $"Corrupt data backed up under '{CorruptBackupKey}'. ({e.Message})");
                return GameData.Default();
            }
        }

        public void Delete()
        {
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.DeleteKey(CorruptBackupKey);
            PlayerPrefs.Save();
        }

        private string Encrypt(string plainText)
        {
            using var aes = CreateAes();
            aes.GenerateIV();
            using var enc = aes.CreateEncryptor();

            byte[] input  = Encoding.UTF8.GetBytes(plainText);
            byte[] cipher = enc.TransformFinalBlock(input, 0, input.Length);

            byte[] output = new byte[IvSize + cipher.Length];
            Buffer.BlockCopy(aes.IV, 0, output, 0,      IvSize);
            Buffer.BlockCopy(cipher, 0, output, IvSize, cipher.Length);
            return Convert.ToBase64String(output);
        }

        private string Decrypt(string cipherText)
        {
            byte[] raw = Convert.FromBase64String(cipherText);
            if (raw.Length <= IvSize)
                throw new CryptographicException("Encrypted data is incomplete (missing IV).");

            byte[] iv = new byte[IvSize];
            Buffer.BlockCopy(raw, 0, iv, 0, IvSize);

            using var aes = CreateAes();
            aes.IV = iv;
            using var dec = aes.CreateDecryptor();

            byte[] output = dec.TransformFinalBlock(raw, IvSize, raw.Length - IvSize);
            return Encoding.UTF8.GetString(output);
        }

        private Aes CreateAes()
        {
            var aes = Aes.Create();
            aes.Key     = _key;
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }
    }
}
