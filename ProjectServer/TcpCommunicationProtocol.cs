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

        public List<TcpCommunicationProtocol> FromProtocol(string text)
        {
            List<TcpCommunicationProtocol> messages = new List<TcpCommunicationProtocol>();
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
                        // Handle unencrypted messages (e.g., initial connection messages)
                        ProcessUnencryptedMessage(potentialMessage, messages);
                    }
                }
            }
            return messages;
        }
        private void ProcessDecryptedMessage(string decryptedMessage, List<TcpCommunicationProtocol> messages)
        {
            string[] parts = decryptedMessage.Split('\n');
            if (parts.Length >= 3)
            {
                TcpCommunicationProtocol protocol = new TcpCommunicationProtocol
                {
                    Command = parts[0],
                    Username = parts[1],
                    Arguments = string.Join("\n", parts.Skip(2))
                };
                messages.Add(protocol);
            }
            else
            {
                Console.WriteLine($"Invalid decrypted message format: {decryptedMessage}");
            }
        }
        private void ProcessUnencryptedMessage(string message, List<TcpCommunicationProtocol> messages)
        {
            string[] parts = message.Split('\n');
            if (parts.Length >= 2)
            {
                TcpCommunicationProtocol protocol = new TcpCommunicationProtocol
                {
                    Command = parts[0],
                    Arguments = string.Join("\n", parts.Skip(1))
                };
                messages.Add(protocol);
            }
            else
            {
                Console.WriteLine($"Invalid unencrypted message format: {message}");
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

        public string Command { get; set; }
        public string Username { get; set; }
        public string Arguments { get; set; }
    }
}
