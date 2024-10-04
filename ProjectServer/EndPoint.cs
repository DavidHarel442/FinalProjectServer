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
        /// this property 'TimeStamps' contains a list of the time of all the request made by a certain ip 
        /// </summary>
        public LinkedList<DateTime> TimeStamps = new LinkedList<DateTime>();
        /// <summary>
        /// this property 'ip' contains the IPAddress of the object.
        /// </summary>
        public IPAddress ip;
        /// <summary>
        /// this property 'isBlocked' contains a value Whether the ip is blocked or not. if it is blocked it is blocked for 30 min
        /// </summary>
        public bool isBlocked = false;
        /// <summary>
        /// this  property contains the time that the user got blocked
        /// </summary>
        public DateTime BlockedTimeSince = new DateTime();

        /// <summary>
        /// this function is a constructor for the object EndPoint.
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="IP"></param>
        public EndPoint(DateTime timeStamp, IPAddress IP)
        {
            TimeStamps.AddLast(timeStamp);
            ip = IP;
        }
    }
}
