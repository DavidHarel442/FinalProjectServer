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
        /// <summary>
        /// List of all client endpoints the server has communicated with
        /// </summary>
        private readonly List<EndPoint> ipsList = new List<EndPoint>();

        /// <summary>
        /// Maximum number of connections allowed per client per minute
        /// </summary>
        private const int MaxConnectionsPerMinute = 1000;

        /// <summary>
        /// Maximum number of requests allowed per client per minute
        /// </summary>
        private const int MaxRequestsPerMinute = 200;

        /// <summary>
        /// Duration in minutes for how long a client remains blocked
        /// </summary>
        private const int BlockDurationMinutes = 30;

        /// <summary>
        /// Duration in hours after which inactive clients are removed from tracking
        /// </summary>
        private const int InactivityTimeoutHours = 1;

        /// <summary>
        /// Checks if the received IP exists in the tracked clients list
        /// </summary>
        /// <param name="ip">The IP address to check</param>
        /// <returns>True if the IP exists in the list, otherwise false</returns>
        public bool IsIPExistInList(IPAddress ip)
        {
            if (ip == null)
                throw new ArgumentNullException(nameof(ip));

            return ipsList.Any(endpoint => endpoint.Ip.Equals(ip));
        }

        /// <summary>
        /// Returns the index of the received IP in the list
        /// </summary>
        /// <param name="ip">The IP address to find</param>
        /// <returns>The index of the IP in the list, or -1 if not found</returns>
        private int GetIndexOfIPLocation(IPAddress ip)
        {
            if (ip == null)
                throw new ArgumentNullException(nameof(ip));

            for (int i = 0; i < ipsList.Count; i++)
            {
                if (ipsList[i].Ip.Equals(ip))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Determines if the server should allow the client to continue its session
        /// </summary>
        /// <param name="ip">The client's IP address</param>
        /// <param name="startingSession">True if this is a new connection, false if it's an ongoing session request</param>
        /// <returns>True if the client should be allowed to continue, false if blocked</returns>
        public bool ShouldAllowToContinueSession(IPAddress ip, bool startingSession)
        {
            if (ip == null)
                throw new ArgumentNullException(nameof(ip));

            // Remove inactive clients
            CleanupInactiveClients();

            if (IsIPExistInList(ip))
            {
                int index = GetIndexOfIPLocation(ip);
                EndPoint endpoint = ipsList[index];
                DateTime currentTime = DateTime.Now;

                // Check if client is blocked
                if (endpoint.IsBlocked)
                {
                    TimeSpan blockedDuration = currentTime - endpoint.BlockedTimeSince;
                    if (blockedDuration.TotalMinutes > BlockDurationMinutes)
                    {
                        // Unblock client after block duration
                        endpoint.IsBlocked = false;
                        endpoint.TimeStamps.Clear();
                        endpoint.ConnectionAttempts.Clear();
                    }
                    else
                    {
                        return false; // Still blocked
                    }
                }

                // Handle request based on type
                if (startingSession)
                {
                    // Track new connection attempt
                    endpoint.ConnectionAttempts.AddLast(currentTime);

                    // Clean old connection attempts
                    CleanupOldTimestamps(endpoint.ConnectionAttempts);

                    if (endpoint.ConnectionAttempts.Count > MaxConnectionsPerMinute)
                    {
                        BlockEndpoint(endpoint);
                        return false;
                    }
                }
                else
                {
                    // Track new request
                    endpoint.TimeStamps.AddLast(currentTime);

                    // Clean old request timestamps
                    CleanupOldTimestamps(endpoint.TimeStamps);

                    if (endpoint.TimeStamps.Count > MaxRequestsPerMinute)
                    {
                        BlockEndpoint(endpoint);
                        return false;
                    }
                }
            }
            else
            {
                // Create new endpoint tracking for this IP
                EndPoint newEndpoint = new EndPoint(DateTime.Now, ip);
                if (startingSession)
                {
                    newEndpoint.ConnectionAttempts.AddLast(DateTime.Now);
                }
                else
                {
                    newEndpoint.TimeStamps.AddLast(DateTime.Now);
                }
                ipsList.Add(newEndpoint);
            }

            return true;
        }

        /// <summary>
        /// Removes timestamps older than one minute from a timestamp collection
        /// </summary>
        /// <param name="timestamps">The collection of timestamps to clean</param>
        private void CleanupOldTimestamps(LinkedList<DateTime> timestamps)
        {
            if (timestamps.Count == 0)
                return;

            DateTime oneMinuteAgo = DateTime.Now.AddMinutes(-1);

            // Remove timestamps older than one minute
            while (timestamps.Count > 0 && timestamps.First.Value < oneMinuteAgo)
            {
                timestamps.RemoveFirst();
            }
        }

        /// <summary>
        /// Blocks an endpoint by setting the blocked flag and clearing its tracking data
        /// </summary>
        /// <param name="endpoint">The endpoint to block</param>
        private void BlockEndpoint(EndPoint endpoint)
        {
            endpoint.IsBlocked = true;
            endpoint.BlockedTimeSince = DateTime.Now;
            endpoint.TimeStamps.Clear();
            endpoint.ConnectionAttempts.Clear();
        }

        /// <summary>
        /// Removes clients that have been inactive for more than the specified timeout
        /// </summary>
        private void CleanupInactiveClients()
        {
            if (ipsList.Count == 0)
                return;

            DateTime cutoffTime = DateTime.Now.AddHours(-InactivityTimeoutHours);

            // Use a temporary list to avoid collection modification during enumeration
            List<EndPoint> inactiveEndpoints = new List<EndPoint>();

            foreach (EndPoint endpoint in ipsList)
            {
                DateTime lastActivity = GetLastActivity(endpoint);
                if (lastActivity < cutoffTime)
                {
                    inactiveEndpoints.Add(endpoint);
                }
            }

            // Remove all inactive endpoints
            foreach (EndPoint endpoint in inactiveEndpoints)
            {
                ipsList.Remove(endpoint);
            }
        }

        /// <summary>
        /// Gets the timestamp of the most recent activity for an endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint to check</param>
        /// <returns>The timestamp of the most recent activity</returns>
        private DateTime GetLastActivity(EndPoint endpoint)
        {
            DateTime lastTimeStamp = endpoint.TimeStamps.Count > 0 ?
                endpoint.TimeStamps.Last.Value :
                DateTime.MinValue;

            DateTime lastConnectionAttempt = endpoint.ConnectionAttempts.Count > 0 ?
                endpoint.ConnectionAttempts.Last.Value :
                DateTime.MinValue;

            return lastTimeStamp > lastConnectionAttempt ? lastTimeStamp : lastConnectionAttempt;
        }
    }
}

