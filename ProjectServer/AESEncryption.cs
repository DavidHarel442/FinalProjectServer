using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    public class AESEncryption
    {//taken from claude
        private byte[] Key;
        private const int KeySize = 256;
        private const int BlockSize = 128;

        public AESEncryption()
        {
            GenerateKey();
        }

        public AESEncryption(byte[] key)
        {
            if (key.Length * 8 != KeySize)
                throw new ArgumentException($"Key size must be {KeySize} bits.");
            Key = key;
        }

        public void GenerateKey()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.GenerateKey();
                Key = aes.Key;
            }
        }

        public byte[] GetKey()
        {
            return Key;
        }

        public void SetKey(byte[] key)
        {
            if (key.Length * 8 != KeySize)
                throw new ArgumentException($"Key size must be {KeySize} bits.");
            Key = key;
        }

        public byte[] Encrypt(byte[] plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length); // Write IV to the beginning of the stream
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(plainText, 0, plainText.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }

        public byte[] Decrypt(byte[] cipherText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] iv = new byte[BlockSize / 8];
                Array.Copy(cipherText, 0, iv, 0, iv.Length);

                using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(cipherText, iv.Length, cipherText.Length - iv.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }

        public string EncryptString(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = Encrypt(plainBytes);
            return Convert.ToBase64String(cipherBytes);
        }

        public string DecryptString(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] plainBytes = Decrypt(cipherBytes);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
