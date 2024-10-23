using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProjectServer
{// class incharge of the communication protocol 
    public class TcpCommunicationProtocol
    {
        /// <summary>
        /// this property is incharge of managing the encryption, both RSA and AES
        /// </summary>
        private EncryptionManager encryptionManager;
        /// <summary>
        /// constructor, initializes encryptionManager
        /// </summary>
        public TcpCommunicationProtocol()
        {
            encryptionManager = new EncryptionManager();
        }
        /// <summary>
        /// this function receives a command and argument and transfers it to the protocol format. 
        /// puts it all in one string with the username and seperates it with '\n'.
        /// in the protocol there is command,arguments (no need to send a username)
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public string ToProtocol(string command, string arguments)
        {
            string message = $"{command}\n{arguments}";
            return encryptionManager.EncryptMessage(message) + "\r";
        }
        /// <summary>
        /// this function receives a string that is in the protocol format and transfers it back to a TcpProtocolMessage object that contains:
        /// a command a username and a argument.
        /// in case that messages got mixed up and sent together there is a '\r' at the end of each message,
        /// and if checking after '\r' there is more it creates more then one object,
        /// thats why there is a list of the 'TcpProtocolMessage'
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<TcpProtocolMessage> FromProtocol(string text)
        {
            List<TcpProtocolMessage> messages = new List<TcpProtocolMessage>();
            string[] potentialMessages = text.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string potentialMessage in potentialMessages)
            {
                if (!string.IsNullOrEmpty(potentialMessage))
                {
                    try
                    {
                        string decryptedMessage = encryptionManager.DecryptMessage(potentialMessage);
                        string[] parts = decryptedMessage.Split('\n');
                        if (parts.Length >= 3)
                        {
                            TcpProtocolMessage message = new TcpProtocolMessage
                            {
                                Command = parts[0],
                                Username = parts[1],
                                Arguments = string.Join("\n", parts.Skip(2))
                            };
                            messages.Add(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error decrypting message: {ex.Message}");
                    }
                }
            }
            return messages;
        }

        /// <summary>
        /// this function sets a clients public RSA key 
        /// </summary>
        /// <param name="publicKey"></param>
        public void SetClientPublicKey(string publicKey)
        {
            encryptionManager.SetClientPublicKey(publicKey);
        }
        /// <summary>
        /// this function returns a AES key
        /// </summary>
        /// <returns></returns>
        public string GetEncryptedAesKey()
        {
            return encryptionManager.GetEncryptedAesKey();
        }



        
    }
}
