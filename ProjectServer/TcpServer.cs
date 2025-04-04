using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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

        /// <summary>
        /// this function starts the TcpServer, it starts the listener and begins listening.
        /// </summary>
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
                if (IPDosManager.ShouldAllowToContinueSession(ip,true))
                {
                    TcpClientSession userSession = new TcpClientSession(tcpClient, communicationProtocol, IPDosManager);
                    Sessions.Add(userSession.GetClientIP, userSession);


                    try
                    {
                        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        string logMessage = $"[{timestamp}] New connection created - Client: {userSession.GetClientIP}\n";
                        File.AppendAllText("D:\\Visual Studio\\ProjectServer\\ProjectServer\\LogFile.txt", logMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error writing to log file: {ex.Message}");
                    }
                }
            }
        }
        /// <summary>
        /// this function will receive an ip and remove from the session hashtable the client with the ip
        /// </summary>
        /// <param name="clientIP"></param>
        public void RemoveClientSession(string clientIP)
        {
            if (Sessions.ContainsKey(clientIP))
            {
                Sessions.Remove(clientIP);
                Console.WriteLine($"Removed client session: {clientIP}");
            }
        }
        /// <summary>
        /// this function will be called to check if someone is trying to connect from an already connect client.
        /// it will return true if someone is trying to connect from an already connected client and false if otherwise
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
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
        /// <summary>
        /// this function receives a command and a message and if its related to drawing,
        /// it will send all the clients the message with the command and message itself.
        /// if its related to drawing it will send only someone who opened the drawing
        /// </summary>
        /// <param name="command"></param>
        /// <param name="message"></param>
        /// <param name="isDrawing"></param>
        public void BroadCast(string command,string message)
        {
            foreach(DictionaryEntry c in Sessions)
            {
                TcpClientSession client = (TcpClientSession)(c.Value);
                client.SendMessage(command, message);
            }
        }
        /// <summary>
        /// this function receives a command and a message and if its related to drawing and a username,
        /// it will send all the clients except one(the client with the username specified in the parameters)
        /// the message with the command and message itself.
        /// if its related to drawing it will send only someone who opened the drawing
        /// </summary>
        /// <param name="command"></param>
        /// <param name="message"></param>
        /// <param name="isDrawing"></param>
        /// <param name="username"></param>
        public void BroadCastExceptOne(string command, string message, bool isDrawing,string username)
        {
            foreach (DictionaryEntry c in Sessions)
            {
                TcpClientSession client = (TcpClientSession)(c.Value);
                if (client._ClientNick != username)
                {
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
}
