using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Redis
{
    public static class RedisBLFactory
    {
        private static readonly ConnectionMultiplexer Connection;
        private static readonly RedisConfig Config;
​
        static RedisBLFactory()
        {
            Config = (RedisConfig)ConfigurationManager.GetSection("redisConfig");
            if (Config == null)
                return;

            EndPoint endPoint = null;
            if (IPAddress.TryParse(Config.Host, out IPAddress ipAddress))
            {
                endPoint = new IPEndPoint(ipAddress, Config.Port);
            }
            else
            {
                endPoint = new DnsEndPoint(Config.Host, Config.Port);
            }
​
            ConfigurationOptions connectionConfig = new ConfigurationOptions
            {
                EndPoints = { endPoint },
                Password = Config.Password,
                ClientName = "AGP",
                AbortOnConnectFail = false
            };
            Connection = ConnectionMultiplexer.Connect(connectionConfig);
        }
​
        public static IAuthenicationRedisBL CreateAuthenicationRedisBl()
        {
            return new AuthenticationRedisBL(Connection, Config);
        }
        public static IArbitrageRedisBL CreateArbitrageRedisBL()
        {
            return new ArbitrageRedisBL();
        }
​
        public static IUserRedisBL CreateUserRedisBl()
        {
            return new Referance.UserRedisBL(Connection, Config);
        }
        public static ISwarmNotificationRedisBL CreateSwarmNotificationRedisBL()
        {
            return new Referance.SwarmNotificationRedisBL(Connection, Config);
        }
​
        public static IRedisCoreBL CreateRedisCoreBL()
        {
            return new RedisCoreBL(Connection, Config);
        }
​
        public static IRequestTrackingRedisBL CreateRequestTrackingRedis()
        {
            return new RequestTrackingRedisBL(Connection, Config);
        }
​
        public static ILoginTrackingRedisBL CreateLoginTrackingRedisBL()
        {
            return new LoginTrackingRedisBL(Connection, Config);
        }
​
        public static IGenericRedisBL CreateGenericRedisBL()
        {
            return new GenericRedisBL(Connection, Config);
        }
​
        public static void Init()
        {
        }
​
        public static void CloseConnection()
        {
            Dispose(true);
        }
​
        private static void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Connection != null)
                {
                    Connection.Dispose();
                }
            }
        }
    }
}
