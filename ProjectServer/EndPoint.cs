using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    public class EndPoint
    {
        // this class is the class for each client that saves their requests

        /// <summary>
        /// The IP address of the client
        /// </summary>
        public IPAddress Ip { get; private set; }

        /// <summary>
        /// Timestamps of requests from this client
        /// </summary>
        public LinkedList<DateTime> TimeStamps { get; private set; }

        /// <summary>
        /// Timestamps of connection attempts from this client
        /// </summary>
        public LinkedList<DateTime> ConnectionAttempts { get; private set; }

        /// <summary>
        /// Indicates if this client is currently blocked
        /// </summary>
        public bool IsBlocked { get; set; }

        /// <summary>
        /// The time when this client was blocked
        /// </summary>
        public DateTime BlockedTimeSince { get; set; }

        /// <summary>
        /// Creates a new endpoint tracking instance
        /// </summary>
        /// <param name="firstSeen">When this client was first seen</param>
        /// <param name="ip">The IP address of the client</param>
        public EndPoint(DateTime firstSeen, IPAddress ip)
        {
            TimeStamps = new LinkedList<DateTime>();
            ConnectionAttempts = new LinkedList<DateTime>();
            Ip = ip;
            IsBlocked = false;
        }
    }
}
