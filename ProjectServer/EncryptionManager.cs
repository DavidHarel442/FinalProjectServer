using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    internal class EncryptionManager
    {// this class manages the communication encryption between server and client
        private RSAEncryption rsaEncryption;
        private AESEncryption aesEncryption;

        public EncryptionManager()
        {
            rsaEncryption = new RSAEncryption();
            aesEncryption = new AESEncryption();
        }

        public string GetPublicKey()
        {
            return rsaEncryption.GetPublicKey();
        }

        public void SetClientPublicKey(string publicKey)
        {
            rsaEncryption.ImportPublicKey(publicKey);
        }

        public string GetEncryptedAesKey()
        {
            byte[] aesKey = aesEncryption.GetKey();
            byte[] encryptedAesKey = rsaEncryption.Encrypt(aesKey);
            return Convert.ToBase64String(encryptedAesKey);
        }

        public void SetAesKey(string encryptedAesKey)
        {
            byte[] encryptedAesKeyBytes = Convert.FromBase64String(encryptedAesKey);
            byte[] decryptedAesKey = rsaEncryption.Decrypt(encryptedAesKeyBytes);
            aesEncryption.SetKey(decryptedAesKey);
        }

        public string EncryptMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] encryptedBytes = aesEncryption.Encrypt(messageBytes);
            return Convert.ToBase64String(encryptedBytes);
        }

        public string DecryptMessage(string encryptedMessage)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedMessage);
            byte[] decryptedBytes = aesEncryption.Decrypt(encryptedBytes);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
