using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    public class DosProtection
    {
        // this class's goal is to protect the server from a Dos Attack. he is saving a list of all the clients. a list of type "EndPoint"

        /// <summary>
        /// this property 'IPSList' contains the list of all the ips the server communicated with
        /// </summary>
        public List<EndPoint> ipsList = new List<EndPoint>();
        private const int maxConnectionsPerMinute = 20;  // New constant for connection limit

        /// <summary>
        /// this function check if the recieved ip exists in the list "IPSList"
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool IsIPExistInList(IPAddress ip)
        {
            for (int i = 0; i < ipsList.Count; i++)
            {
                if (ipsList.ElementAt(i).ip.Equals(ip))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// this function returns the index of the recieved ip. Assumption: the ip exists in the list
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public int GetIndextOfIPLocation(IPAddress ip)
        {
            for (int i = 0; i < ipsList.Count; i++)
            {
                if (ipsList.ElementAt(i).ip.Equals(ip))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// this if the main function of this class. it is the handler. it checks if the server should allow the certain user to send requests. at first it checks every ip in the list if it made a request in the last hour, if not it deletes them from the list.
        /// after that it check how much requests the certain ip made in the last min if it is over 100 it blocks him for 30 min. 
        /// it also deletes timeStamps from over a minute since the call of the function.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool ShouldAllowToContinueSession(IPAddress ip,bool startingSession)
        {
            if (ipsList.Count != 0)
            {
                for (int i = 0; i < ipsList.Count; i++)
                {
                    if (ipsList.ElementAt(i).TimeStamps.Count > 0)
                    {
                        TimeSpan LastRequestMadeForEachClient = DateTime.Now - ipsList.ElementAt(i).TimeStamps.ElementAt(ipsList.ElementAt(i).TimeStamps.Count - 1);
                        if (LastRequestMadeForEachClient.TotalHours >= 1)
                        {
                            ipsList.Remove(ipsList.ElementAt(i));
                        }
                    }
                }
            }

            if (IsIPExistInList(ip))
            {
                DateTime CurrentTime = DateTime.Now;
                int index = GetIndextOfIPLocation(ip);
                var endpoint = ipsList.ElementAt(index);

                // Handle blocked status check
                if (endpoint.isBlocked)
                {
                    TimeSpan timeSpan = DateTime.Now - endpoint.BlockedTimeSince;
                    if (timeSpan.TotalMinutes > 30)
                    {
                        endpoint.isBlocked = false;
                        endpoint.TimeStamps.Clear();
                        endpoint.connectionAttempts.Clear();
                    }
                    else
                    {
                        return false;  // Still blocked
                    }
                }

                // Handle the request based on type
                if (startingSession)
                {
                    endpoint.connectionAttempts.AddLast(CurrentTime);
                    if (ShouldGetBlockedDueToConnections(ip))
                    {
                        endpoint.isBlocked = true;
                        endpoint.BlockedTimeSince = DateTime.Now;
                        endpoint.TimeStamps.Clear();
                        endpoint.connectionAttempts.Clear();
                        return false;
                    }
                }
                else
                {
                    endpoint.TimeStamps.AddLast(CurrentTime);
                    if (ShouldGetBlocked(ip))
                    {
                        endpoint.isBlocked = true;
                        endpoint.BlockedTimeSince = DateTime.Now;
                        endpoint.TimeStamps.Clear();
                        endpoint.connectionAttempts.Clear();
                        return false;
                    }
                }
            }
            else
            {
                EndPoint ToInsert = new EndPoint(DateTime.Now, ip);
                if (startingSession)
                {
                    ToInsert.connectionAttempts.AddLast(DateTime.Now);
                }
                ipsList.Add(ToInsert);
            }

            return true;
        }

        /// <summary>
        /// this function is called in the handler "ShouldAllowToContinueSession"
        /// and checks if the certain ip made over 100 requests in the last min if he did the function returns true if not it returns false.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool ShouldGetBlocked(IPAddress ip)
        {
            int index = GetIndextOfIPLocation(ip);
            EndPoint IPClient = ipsList.ElementAt(index);
            for (int i = 0; i < IPClient.TimeStamps.Count; i++)
            {
                if ((DateTime.Now.Minute - IPClient.TimeStamps.ElementAt(i).Minute) > 1 || DateTime.Now.Hour != IPClient.TimeStamps.ElementAt(i).Hour || DateTime.Now.Day != IPClient.TimeStamps.ElementAt(i).Day)
                {
                    IPClient.TimeStamps.Remove(IPClient.TimeStamps.ElementAt(i));
                }
                else
                {
                    break;
                }
            }
            if (IPClient.TimeStamps.Count > 200)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// this function is called in the handler "ShouldAllowToContinueSession"
        /// and checks if the certain ip made over 'maxConnectionsPerMinute' connections in the last min if he did the function returns true if not it returns false.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool ShouldGetBlockedDueToConnections(IPAddress ip)
        {
            int index = GetIndextOfIPLocation(ip);
            EndPoint IPClient = ipsList.ElementAt(index);
            for (int i = 0; i < IPClient.connectionAttempts.Count; i++)
            {
                if ((DateTime.Now.Minute - IPClient.TimeStamps.ElementAt(i).Minute) > 1 || DateTime.Now.Hour != IPClient.TimeStamps.ElementAt(i).Hour || DateTime.Now.Day != IPClient.TimeStamps.ElementAt(i).Day)
                {
                    IPClient.connectionAttempts.Remove(IPClient.TimeStamps.ElementAt(i));
                }
                else
                {
                    break;
                }
            }
            return IPClient.connectionAttempts.Count > maxConnectionsPerMinute;
        }
    }
}

