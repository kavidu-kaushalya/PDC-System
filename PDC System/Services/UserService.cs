using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using PDC_System.Models;
using System.Collections.Generic;

namespace PDC_System.Services
{
    public static class UserService
    {
        private static string file = "Savers/users.dat";

        // 🔐 Secret key (change this & keep safe)
        private static readonly string secretKey = "PDC_SYSTEM_2025_SECURE_KEY";

        public static List<User> Load()
        {
            if (!File.Exists(file))
                return new List<User>();

            byte[] encryptedData = File.ReadAllBytes(file);
            string json = Decrypt(encryptedData);

            return JsonConvert.DeserializeObject<List<User>>(json)
                   ?? new List<User>();
        }

        public static void Save(List<User> users)
        {
            Directory.CreateDirectory("Savers");

            string json = JsonConvert.SerializeObject(
                users, Newtonsoft.Json.Formatting.Indented);

            byte[] encryptedData = Encrypt(json);
            File.WriteAllBytes(file, encryptedData);
        }

        // 🔐 PASSWORD HASH (already good – unchanged)
        public static string Hash(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(
                sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        // ================= ENCRYPT / DECRYPT =================

        private static byte[] Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = GetKey();
            aes.IV = new byte[16]; // fixed IV (OK for local file)

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var sw = new StreamWriter(cs);

            sw.Write(plainText);
            sw.Close();

            return ms.ToArray();
        }

        private static string Decrypt(byte[] cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = GetKey();
            aes.IV = new byte[16];

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipherText);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        private static byte[] GetKey()
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(secretKey));
        }
    }
}
