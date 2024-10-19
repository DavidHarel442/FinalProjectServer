using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace ProjectServer
{
    public class SharedDrawingManager
    {// this class will be incharge of managing the drawing

        private List<DrawingAction> drawingActions = new List<DrawingAction>();

        private TcpServer tcpServer;


        public SharedDrawingManager(TcpServer server)
        {
            this.tcpServer = server;
        }


        public void AddAction(DrawingAction action)
        {
            drawingActions.Add(action);
            BroadcastAction(action);
        }

        private void BroadcastAction(DrawingAction action)
        {
            foreach (DictionaryEntry entry in tcpServer.Sessions)
            {
                TcpClientSession clientSession = (TcpClientSession)entry.Value;
                clientSession.SendMessage("DrawingUpdate", action.Serialize());
            }
        }

        public void RequestFullDrawingState(TcpClientSession newClient)
        {
            foreach (DictionaryEntry entry in tcpServer.Sessions)
            {
                TcpClientSession clientSession = (TcpClientSession)entry.Value;
                if (clientSession != newClient && clientSession.openedDrawing)
                {
                    clientSession.SendMessage("SendFullDrawingState", newClient.GetClientIP);
                    break; // We only need to request from one client
                }
            }
        }

        public void SendFullDrawingState(string imageData, string recipientIP)
        {
            TcpClientSession recipient = tcpServer.Sessions[recipientIP] as TcpClientSession;
            if (recipient != null)
            {
                recipient.SendMessage("FullDrawingState", imageData);
            }
        }


    }
}
