using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Drawing;
using System.IO;

namespace ProjectServer
{
    public class UdpImageServer
    {
        private readonly UdpClient udpServer;
        private readonly int port;
        private bool isRunning;
        private readonly SharedDrawingManager drawingManager;
        private CancellationTokenSource cancellationTokenSource;
        private Dictionary<IPEndPoint, List<byte[]>> fragmentedPackets;

        public UdpImageServer(int port, SharedDrawingManager drawingManager)
        {
            this.port = port;
            this.drawingManager = drawingManager;
            udpServer = new UdpClient(port);
            isRunning = false;
            cancellationTokenSource = new CancellationTokenSource();
            fragmentedPackets = new Dictionary<IPEndPoint, List<byte[]>>();
        }

        public async Task StartListening()
        {
            isRunning = true;
            Console.WriteLine($"UDP Server started on port {port}");

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await udpServer.ReceiveAsync();
                    ProcessPacket(result.Buffer, result.RemoteEndPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving packet: {ex.Message}");
                }
            }
        }

        private void ProcessPacket(byte[] packetData, IPEndPoint endPoint)
        {
            try
            {
                var packet = FramePacket.Deserialize(packetData);

                // Create drawing action from finger position
                Point fingerPosition = DetectFingerPosition(packet.FrameData);

                var drawingAction = new DrawingAction
                {
                    Type = packet.DrawingMode == 2 ? "Erase" : "DrawLine",
                    StartPoint = fingerPosition,
                    EndPoint = fingerPosition,
                    Color = packet.PenColor,
                    Size = packet.PenSize
                };

                // Broadcast to all clients
                ServerManager.tcpServer.BroadCast("DrawingUpdate",
                    JsonConvert.SerializeObject(drawingAction), true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing packet: {ex.Message}");
            }
        }

        private Point DetectFingerPosition(byte[] frameData)
        {
            // Placeholder - implement actual finger detection
            using (var ms = new MemoryStream(frameData))
            using (var bitmap = new Bitmap(ms))
            {
                return new Point(bitmap.Width / 2, bitmap.Height / 2);
            }
        }

        public void Stop()
        {
            isRunning = false;
            cancellationTokenSource.Cancel();
            udpServer.Close();
        }
    }
}
