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
     //this class is responsible for the encryption of the data using symetric encryption AES
        /// <summary>
        /// The encryption key.
        /// </summary>
        private byte[] Key;
        /// <summary>
        /// The size of the encryption key in bits.
        /// </summary>
        private const int KeySize = 256;
        /// <summary>
        /// The block size for AES encryption in bits.
        /// </summary>
        private const int BlockSize = 128;
        /// <summary>
        /// Initializes a new instance of the AESEncryption class with a randomly generated key.
        /// </summary>
        public AESEncryption()
        {
            GenerateKey();
        }
        /// <summary>
        /// Initializes a new instance of the AESEncryption class with a provided key.
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="ArgumentException"></exception>
        public AESEncryption(byte[] key)
        {
            if (key.Length * 8 != KeySize)
                throw new ArgumentException($"Key size must be {KeySize} bits.");
            Key = key;
        }
        /// <summary>
        /// Generates a new random encryption key.
        /// </summary>
        public void GenerateKey()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.GenerateKey();
                Key = aes.Key;
            }
        }
        /// <summary>
        /// Retrieves the current encryption key.
        /// </summary>
        /// <returns></returns>
        public byte[] GetKey()
        {
            return Key;
        }
        /// <summary>
        ///  Sets a new encryption key.
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public void SetKey(byte[] key)
        {
            if (key.Length * 8 != KeySize)
                throw new ArgumentException($"Key size must be {KeySize} bits.");
            Key = key;
        }
        /// <summary>
        /// Encrypts the provided plaintext using AES encryption.
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Decrypts the provided ciphertext using AES decryption.
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
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


    }
}
