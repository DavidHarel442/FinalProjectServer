using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    public class MessageHandler
    {
        /// <summary>
        /// property to use for checking login and register
        /// </summary>
        private LoginAndRegister loginAndRegister;
        /// <summary>
        /// property to help managing the messages
        /// </summary>
        private TcpClientSession clientSession;
        public MessageHandler(TcpClientSession session)
        {
            clientSession = session;
            loginAndRegister = new LoginAndRegister(clientSession.communicationProtocol,clientSession);
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
                case "Login":
                    string username = message.Arguments.Split('\t')[0];
                    string password = message.Arguments.Split('\t')[1];
                    loginAndRegister.CheckLogin(username, password);
                    break;
                case "Register":
                    loginAndRegister.Register(message.Arguments);
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
                string command = message.Split('\n')[0];
                if (command == "USERNAME")
                {
                    string encodedUsername = message.Split('\n')[1].TrimEnd('\r');
                    
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
        public void Login(string username,string password)
        {

        }
    }
}
