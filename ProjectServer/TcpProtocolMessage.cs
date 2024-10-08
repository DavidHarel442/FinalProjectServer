using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    public class TcpProtocolMessage
    {//this class contains the fields for the messages received through the Tcp Connection
        private string command;
        private string username;
        private string arguments;

        public string Command { get => command; set => command = value; }
        public string Username { get => username; set => username = value; }
        public string Arguments { get => arguments; set => arguments = value; }

    }
}
