using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;


namespace ProjectServer
{
    public class SharedDrawingManager
    {// this class will be incharge of managing the drawing

        /// <summary>
        /// property that allows this class to have access to the session hashtable 
        /// </summary>
        private TcpServer tcpServer;

        /// <summary>
        /// constructor that receives the TcpServer object
        /// </summary>
        /// <param name="server"></param>
        public SharedDrawingManager(TcpServer server)
        {
            this.tcpServer = server;
        }
        /// <summary>
        /// this function will be called when a new user joines the shared drawing board and he requests the board.
        /// it will go over all the connected clients in the session hashtable and send 1 user that opened the drawingBoard to send him the current drawing board
        /// </summary>
        /// <param name="newClient"></param>
        public void RequestedFullDrawingState(TcpClientSession newClient)
        {
            foreach (DictionaryEntry entry in tcpServer.Sessions)
            {
                TcpClientSession clientSession = (TcpClientSession)entry.Value;
                if (clientSession != newClient && clientSession.openedDrawing)
                {
                    clientSession.SendMessage("SendFullDrawingState", newClient._ClientNick);
                    break; // We only need to request from one client
                }
            }
        }
        /// <summary>
        /// this function will be called after the "RequestedFullDrawingState" function was called,
        /// and the client it sent the request already sent back the board.
        /// it will send the client with the received ip the board state
        /// </summary>
        /// <param name="imageData"></param>
        /// <param name="recipientIP"></param>
        public void SendFullDrawingState(string imageData, string clientNick)
        {
            foreach (DictionaryEntry entry in tcpServer.Sessions)
            {
                TcpClientSession clientSession = (TcpClientSession)entry.Value;
                if (clientSession._ClientNick == clientNick)
                {
                    clientSession.SendMessage("FullDrawingState", imageData);
                    
                }
            }
        }


    }
}
