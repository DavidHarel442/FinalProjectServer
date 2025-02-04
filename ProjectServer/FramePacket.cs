using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    internal class FramePacket
    {
        public byte[] FrameData { get; set; }
        public int DrawingMode { get; set; }
        public Color PenColor { get; set; }
        public float PenSize { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(DrawingMode);
                writer.Write(PenColor.ToArgb());
                writer.Write(PenSize);
                writer.Write(FrameData);
                return ms.ToArray();
            }
        }

        public static FramePacket Deserialize(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                return new FramePacket
                {
                    DrawingMode = reader.ReadInt32(),
                    PenColor = Color.FromArgb(reader.ReadInt32()),
                    PenSize = reader.ReadSingle(),
                    FrameData = reader.ReadBytes((int)(ms.Length - ms.Position))
                };
            }
        }
    }
}
