using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectServer
{
    internal class TcpClientSession
    {// this class is in charge of managing each client's session

        
        // this class contains details and info about each client
        /// <summary>
        /// this property '_client' is an object which represents the client tcp
        /// </summary>
        public TcpClient _client;
        /// <summary>
        /// this property 'clientIP' contains the ipAddress of the client
        /// </summary>
        private string clientIP;
        /// <summary>
        /// this property '_ClientNick' contains the name of the client
        /// </summary>
        public string _ClientNick;
        // used for sending and reciving data
        public byte[] data;
        // the nickname being sent
        public bool ReceiveNick = true;
        /// <summary>
        /// this property allows the server to communicate with the client
        /// </summary>
        public TcpCommunicationProtocol communicationProtocol = null;
        /// <summary>
        /// getters and setters for the clientIP
        /// </summary>
        public string GetClientIP { get => clientIP; set => clientIP = value; }
        private bool isInitialConnectionComplete = false;
        private bool isAwaitingUsername = false;

        /// <summary>
        /// When the client gets connected to the server the server will create an instance of the ClientSession and pass the TcpClient
        /// </summary>
        /// <param name="client"></param>
        public TcpClientSession(TcpClient client, TcpCommunicationProtocol communicationProtocol)
        {
            _client = client;
            this.communicationProtocol = communicationProtocol;
            GetClientIP = client.Client.RemoteEndPoint.ToString();
            data = new byte[_client.ReceiveBufferSize];

            _client.GetStream().BeginRead(data, 0, System.Convert.ToInt32(_client.ReceiveBufferSize), ReceiveMessage, null);
        }

        public void SendMessage(string command, string arguments)
        {
            try
            {
                NetworkStream ns;
                lock (_client.GetStream())
                {
                    ns = _client.GetStream();
                }

                string message;
                if (!isInitialConnectionComplete)
                {
                    message = $"{command}\n{arguments}\r";
                }
                else
                {
                    message = communicationProtocol.ToProtocol(command, _ClientNick, arguments);
                }

                byte[] bytesToSend = Encoding.UTF8.GetBytes(message);
                ns.Write(bytesToSend, 0, bytesToSend.Length);
                ns.Flush();
            }
            catch (Exception)
            {
                ServerManager.tcpServer.RemoveClientSession(GetClientIP);
            }
        }

        /// <summary>
        /// reciev and handel incomming streem 
        /// Asynchrom
        /// </summary>
        /// <param name="ar">IAsyncResult Interface</param>
        public void ReceiveMessage(IAsyncResult ar)
        {
            int bytesRead;
            try
            {
                lock (_client.GetStream())
                {
                    bytesRead = _client.GetStream().EndRead(ar);
                }

                if (bytesRead < 1)
                {
                    Console.WriteLine($"Client disconnected: {GetClientIP}");
                    ServerManager.tcpServer.RemoveClientSession(GetClientIP);
                    return;
                }

                string messageReceived = Encoding.UTF8.GetString(data, 0, bytesRead);
                Console.WriteLine($"Received raw message: {messageReceived}");

                if (!isInitialConnectionComplete)
                {
                    if (isAwaitingUsername)
                    {
                        HandleUsernameMessage(messageReceived);
                    }
                    else
                    {
                        HandleInitialConnection(messageReceived);
                    }
                }
                else
                {
                    List<TcpCommunicationProtocol> messages = communicationProtocol.FromProtocol(messageReceived);
                    foreach (TcpCommunicationProtocol message in messages)
                    {
                        HandleMessage(message);
                    }
                }

                _client.GetStream().BeginRead(data, 0, System.Convert.ToInt32(_client.ReceiveBufferSize), ReceiveMessage, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReceiveMessage: {ex.Message}");
                ServerManager.tcpServer.RemoveClientSession(GetClientIP);
            }
        }

        private void HandleInitialConnection(string initialMessage)
        {
            string[] parts = initialMessage.Split('\n');
            if (parts.Length >= 2 && parts[0] == "INIT")
            {
                string clientPublicKey = parts[1].TrimEnd('\r');
                communicationProtocol.SetClientPublicKey(clientPublicKey);
                string encryptedAesKey = communicationProtocol.GetEncryptedAesKey();
                SendMessage("AES_KEY", encryptedAesKey);
                isAwaitingUsername = true;
            }
            else
            {
                SendMessage("ERROR", "InvalidInitialMessage");
                ServerManager.tcpServer.RemoveClientSession(GetClientIP);
            }
        }
        private void HandleUsernameMessage(string encryptedMessage)
        {
            try
            {
                encryptedMessage = encryptedMessage.TrimEnd('\r');

                string decryptedMessage = communicationProtocol.DecryptMessage(encryptedMessage);
                string[] parts = decryptedMessage.Split('\n');
                if (parts.Length >= 2 && parts[0] == "USERNAME")
                {
                    string encodedUsername = parts[1];
                    string decodedUsername = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsername));

                    if (ServerManager.tcpServer.SomeoneAlreadyConnected(decodedUsername))
                    {
                        SendMessage("ERROR", "SomeoneAlreadyConnected");
                    }
                    else
                    {
                        _ClientNick = decodedUsername;
                        isInitialConnectionComplete = true;
                        isAwaitingUsername = false;
                        SendMessage("OK", "UsernameAccepted");
                    }
                }
                else
                {
                    SendMessage("ERROR", "InvalidUsernameMessage");
                }
            }
            catch (Exception)
            {
                SendMessage("ERROR", "InvalidUsernameMessage");
            }
        }

        /// <summary>
        /// handles message. 
        /// </summary>
        /// <param name="protocol"></param>
        private void HandleMessage(TcpCommunicationProtocol protocol)
        {
            Console.WriteLine($"Handling message: Command={protocol.Command}, Username={protocol.Username}, Arguments={protocol.Arguments}");
            switch (protocol.Command)
            {
                case "WhatsUp":
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// send message to all the clients that are stored in the allclients hashtable
        /// </summary>
        /// <param name="message"></param>
        //public static void Broadcast(string message)
        //{
        //    foreach (DictionaryEntry c in ServerManager.tcpServer.Sessions)
        //    {

        //        ((TcpClientSession)(c.Value)).SendMessage(message);

        //    }

        //}
    }
}
