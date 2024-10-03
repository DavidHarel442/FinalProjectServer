using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    public class RSAEncryption
    {// taken from claude. 
        private RSACryptoServiceProvider rsa;

        public RSAEncryption(int keySize = 2048)
        {
            rsa = new RSACryptoServiceProvider(keySize);
        }

        public string GetPublicKey()
        {
            return rsa.ToXmlString(false);
        }

        public void ImportPublicKey(string publicKeyString)
        {
            rsa.FromXmlString(publicKeyString);
        }

        public byte[] Encrypt(byte[] dataToEncrypt)
        {
            return rsa.Encrypt(dataToEncrypt, true); // true for OAEP padding
        }

        public byte[] Decrypt(byte[] dataToDecrypt)
        {
            return rsa.Decrypt(dataToDecrypt, true); // true for OAEP padding
        }
    }
}
