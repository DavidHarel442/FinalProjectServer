using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    public class TcpServer
    {

        public SharedDrawingManager DrawingManager { get; private set; }
        // Store list of all clients connecting to the server
        // the list is static so all memebers of the chat will be able to obtain list
        // of current connected client
        public Hashtable Sessions = new Hashtable();
        /// <summary>
        /// this property allows the server to communicate with the client
        /// </summary>
        public TcpCommunicationProtocol communicationProtocol = new TcpCommunicationProtocol();

        /// <summary>
        /// two static properties. protects againts a dos attack with 'IPDosManager'
        /// </summary>
        public DosProtection IPDosManager = new DosProtection();
        /// <summary>
        /// this property 'portNo' contains the value of the port the server sits on
        /// </summary>
        const int portNo = 5000;
        /// <summary>
        /// this property 'ipAddress' is the IPAddress the server sits on. local host
        /// </summary>
        private const string ipAddress = "127.0.0.1";

        public void Listen()
        {
            DrawingManager = new SharedDrawingManager(this);
            IPAddress localAdd = IPAddress.Parse(ipAddress);
            TcpListener listener = new TcpListener(localAdd, portNo);
            Console.WriteLine("Simple TCP Server");
            Console.WriteLine($"Listening on ip {ipAddress} port: {portNo}");
            Console.WriteLine("Server is ready.");
            listener.Start();

            while (true)
            {
                TcpClient tcpClient = listener.AcceptTcpClient();
                Console.WriteLine($"New socket: {tcpClient.Client.RemoteEndPoint}");
                IPAddress ip = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;
                if (IPDosManager.ShouldAllowToContinueSession(ip))
                {
                    TcpClientSession userSession = new TcpClientSession(tcpClient, communicationProtocol);
                    Sessions.Add(userSession.GetClientIP, userSession);
                }
            }
        }

        public void RemoveClientSession(string clientIP)
        {
            if (Sessions.ContainsKey(clientIP))
            {
                Sessions.Remove(clientIP);
                Console.WriteLine($"Removed client session: {clientIP}");
            }
        }

        public bool SomeoneAlreadyConnected(string username)
        {
            foreach (DictionaryEntry c in Sessions)
            {
                TcpClientSession client = (TcpClientSession)(c.Value);
                if (client._ClientNick == username)
                {
                    return true;
                }
            }
            return false;
        }
        public void BroadCast(string command,string message,bool isDrawing)
        {
            foreach(DictionaryEntry c in Sessions)
            {
                TcpClientSession client = (TcpClientSession)(c.Value);
                if (isDrawing)
                {
                    if (client.openedDrawing)
                    {
                        client.SendMessage(command, message);
                    }
                }
                else
                {
                    client.SendMessage(command, message);
                }
            }
        }
    }
}
