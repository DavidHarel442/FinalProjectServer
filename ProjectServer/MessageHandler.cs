using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    public class MessageHandler
    {
        private TcpClientSession clientSession;
        public MessageHandler(TcpClientSession session)
        {
            clientSession = session;
        }
        /// <summary>
        /// handles message. 
        /// </summary>
        /// <param name="message"></param>
        public void HandleMessage(TcpCommunicationProtocol message)
        {
            Console.WriteLine($"Handling message: Command={message.Command}, Username={message.Username}, Arguments={message.Arguments}");
            switch (message.Command)
            {
                case "...":
                    
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// handles the acceptence of username after encryption
        /// </summary>
        /// <param name="encryptedMessage"></param>
        public void HandleUsernameMessage(string message)
        {
            try
            {
                TcpCommunicationProtocol usernameMessage = clientSession.communicationProtocol.FromProtocol(message)[0];
                if (usernameMessage.Command == "USERNAME")
                {
                    string encodedUsername = usernameMessage.Arguments;
                    
                        string decodedUsername = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsername));
                        Console.WriteLine($"Received username: {decodedUsername}");

                        if (ServerManager.tcpServer.SomeoneAlreadyConnected(decodedUsername))
                        {
                        clientSession.SendMessage("ERROR", "SomeoneAlreadyConnected");
                        }
                        else
                        {
                        clientSession._ClientNick = decodedUsername;
                        clientSession.isInitialConnectionComplete = true;
                            clientSession.SendMessage("UsernameAccepted", "UsernameAccepted");
                        }
                }
                else
                {
                    clientSession.SendMessage("ERROR", "ExpectedUsernameMessage");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling username message: {ex.Message}");
                clientSession.SendMessage("ERROR", "InvalidUsernameMessage");
            }
        }
    }
}
