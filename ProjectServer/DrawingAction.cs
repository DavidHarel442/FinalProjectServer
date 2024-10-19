using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ProjectServer
{
    public class DrawingAction
    {// this class will be incharge of an action to the drawing
        public string Type { get; set; } // e.g., "DrawLine", "Erase", "Clear", "Fill"
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public Color Color { get; set; }
        public float Size { get; set; }

        public string Serialize()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static DrawingAction Deserialize(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DrawingAction>(json);
        }
    }
}
