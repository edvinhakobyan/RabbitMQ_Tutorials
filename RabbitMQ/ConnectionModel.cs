using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace MQRabbit
{
    public class ConnectionModel
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool AutomaticRecoveryEnabled { get; set; } = true;
        public ushort RequestedHeartbeat { get; set; } = 30;
        public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(10);
    }
}
