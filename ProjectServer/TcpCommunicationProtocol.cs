using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProjectServer
{
    public class TcpCommunicationProtocol
    {
        private EncryptionManager encryptionManager;

        public TcpCommunicationProtocol()
        {
            encryptionManager = new EncryptionManager();
        }

        public string ToProtocol(string command, string arguments)
        {
            string message = $"{command}\n{arguments}";
            return encryptionManager.EncryptMessage(message) + "\r";
        }

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
                        ProcessDecryptedMessage(decryptedMessage, messages);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error decrypting message: {ex.Message}");
                    }
                }
            }
            return messages;
        }
        private void ProcessDecryptedMessage(string decryptedMessage, List<TcpProtocolMessage> messages)
        {
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
            else
            {
                Console.WriteLine($"Invalid decrypted message format: {decryptedMessage}");
            }
        }
        
        public void SetClientPublicKey(string publicKey)
        {
            encryptionManager.SetClientPublicKey(publicKey);
        }

        public string GetEncryptedAesKey()
        {
            return encryptionManager.GetEncryptedAesKey();
        }

        public string DecryptMessage(string encryptedMessage)
        {
            return encryptionManager.DecryptMessage(encryptedMessage.TrimEnd('\r'));
        }

        
    }
}
