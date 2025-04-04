using System;

namespace ProjectServer
{
    /// <summary>
    /// Metadata class to store information about saved drawings
    /// </summary>
    public class DrawingMetadata
    {
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastModified { get; set; }
    }
}
