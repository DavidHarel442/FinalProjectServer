using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    internal class DosProtection
    {
        // this class's goal is to protect the server from a Dos Attack. he is saving a list of all the clients. a list of type "EndPoint"

        /// <summary>
        /// this property 'IPSList' contains the list of all the ips the server communicated with
        /// </summary>
        public List<EndPoint> IPSList = new List<EndPoint>();


        /// <summary>
        /// this function check if the recieved ip exists in the list "IPSList"
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool IsIPExistInList(IPAddress ip)
        {
            for (int i = 0; i < IPSList.Count; i++)
            {
                if (IPSList.ElementAt(i).ip.Equals(ip))
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
            for (int i = 0; i < IPSList.Count; i++)
            {
                if (IPSList.ElementAt(i).ip.Equals(ip))
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
        public bool ShouldAllowToContinueSession(IPAddress ip)
        {
            if (IPSList.Count != 0)
            {
                for (int i = 0; i < IPSList.Count; i++)
                {
                    if (IPSList.ElementAt(i).TimeStamps.Count > 0)
                    {
                        TimeSpan LastRequestMadeForEachClient = DateTime.Now - IPSList.ElementAt(i).TimeStamps.ElementAt(IPSList.ElementAt(i).TimeStamps.Count - 1);
                        if (LastRequestMadeForEachClient.TotalHours >= 1)
                        {
                            IPSList.Remove(IPSList.ElementAt(i));
                        }
                    }
                }
            }
            if (IsIPExistInList(ip))
            {
                DateTime CurrentTime = DateTime.Now;
                int index = GetIndextOfIPLocation(ip);
                IPSList.ElementAt(index).TimeStamps.AddLast(CurrentTime);
                if (ShouldGetBlocked(ip))
                {
                    IPSList.ElementAt(index).isBlocked = true;
                    IPSList.ElementAt(index).BlockedTimeSince = DateTime.Now;
                    IPSList.ElementAt(index).TimeStamps.Clear();
                    return false;
                }
                else if (IPSList.ElementAt(index).isBlocked)
                {
                    TimeSpan timeSpan = DateTime.Now - IPSList.ElementAt(index).BlockedTimeSince;
                    if (timeSpan.TotalMinutes > 30)
                    {
                        IPSList.ElementAt(index).isBlocked = false;
                        IPSList.ElementAt(index).TimeStamps.AddLast(CurrentTime);
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            else
            {
                IPAddress Ip = ip;
                EndPoint ToInsert = new EndPoint(DateTime.Now, ip);
                IPSList.Add(ToInsert);
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
            EndPoint IPClient = IPSList.ElementAt(index);
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
        /// this fucntion recieves an ip address as a string and returns the ClientSession in the hashtable with that ip. Assumption: the ip exists in the hashtable
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public TcpClientSession GetClientByIP(string ip)
        {
            foreach (DictionaryEntry c in ServerManager.tcpServer.Sessions)
            {
                TcpClientSession client = ((TcpClientSession)(c.Value));
                if (client.GetClientIP == ip)
                {
                    return client;
                }
            }
            return null;
        }
    }
}

