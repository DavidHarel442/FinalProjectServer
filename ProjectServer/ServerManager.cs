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
    public class ServerManager
    {//the class that runs when you start the run. By pressing 'Start'. this class is incharge of the ServerManager. it basically managers the servers

        public static TcpServer tcpServer;
        /// <summary>
        /// the main. function that runs when the server runs, By pressing on "Start" or F5
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            tcpServer = new TcpServer();


            // Start TCP server
            tcpServer.Listen();
        }


       
    }
}

