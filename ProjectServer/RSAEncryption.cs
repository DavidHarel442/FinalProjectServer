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
     // this class is responsible for the RSA encyption proccess. 

        /// <summary>
        /// property responsible for the RSA encryption and decryption
        /// </summary>
        private RSACryptoServiceProvider rsa;
        /// <summary>
        /// Initializes a new instance of the RSAEncryption class with the specified key size.
        /// key size is the size of the RSA key in bits
        /// </summary>
        /// <param name="keySize"></param>
        public RSAEncryption(int keySize = 2048)
        {
            rsa = new RSACryptoServiceProvider(keySize);
        }

        /// <summary>
        ///  Imports a public key from its XML string representation.
        /// </summary>
        /// <param name="publicKeyString"></param>
        public void ImportPublicKey(string publicKeyString)
        {
            rsa.FromXmlString(publicKeyString);
        }
        /// <summary>
        /// Encrypts data using the imported public key with OAEP padding.
        /// </summary>
        /// <param name="dataToEncrypt"></param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] dataToEncrypt)
        {
            return rsa.Encrypt(dataToEncrypt, true); // true for OAEP padding
        }

    }
}
