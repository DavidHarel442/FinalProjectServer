using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    public class EncryptionManager
    {// this class manages the communication encryption between server and client
        /// <summary>
        /// property incharge of managing the rsa class that is incharge of managing the rsa encryption
        /// </summary>
        private RSAEncryption rsaEncryption;
        /// <summary>
        /// property incharge of managing the aes class that is incharge of managing the aes encryption
        /// </summary>
        private AESEncryption aesEncryption;
        /// <summary>
        /// initializes the rsaEcryption and aesEncryption objects
        /// </summary>
        public EncryptionManager()
        {
            rsaEncryption = new RSAEncryption();
            aesEncryption = new AESEncryption();
        }
        /// <summary>
        /// Sets the client's RSA public key for secure key exchange.
        /// </summary>
        /// <param name="publicKey"></param>
        public void SetClientPublicKey(string publicKey)
        {
            rsaEncryption.ImportPublicKey(publicKey);
        }
        /// <summary>
        /// Gets the AES encryption key encrypted with the client's RSA public key.
        /// </summary>
        /// <returns></returns>
        public string GetEncryptedAesKey()
        {
            byte[] aesKey = aesEncryption.GetKey();
            byte[] encryptedAesKey = rsaEncryption.Encrypt(aesKey);
            return Convert.ToBase64String(encryptedAesKey);
        }
        /// <summary>
        /// Encrypts a message using AES encryption.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string EncryptMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] encryptedBytes = aesEncryption.Encrypt(messageBytes);
            return Convert.ToBase64String(encryptedBytes);
        }
        /// <summary>
        /// Decrypts an encrypted message using AES decryption.
        /// </summary>
        /// <param name="encryptedMessage"></param>
        /// <returns></returns>
        public string DecryptMessage(string encryptedMessage)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedMessage);
            byte[] decryptedBytes = aesEncryption.Decrypt(encryptedBytes);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
