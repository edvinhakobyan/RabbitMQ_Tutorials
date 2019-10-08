using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Redis
{
    public interface IRedisCallbackBL
    {
        void KeyEventHandler(string eventType, string value, int? databaseId);
    }

    public class RedisCoreBL : IRedisCoreBL
    {
        private RedisConfig _configuration;
​
        private ConnectionMultiplexer _redisConnection;
​
        private IDatabase _db;
​
        //private ISubscriber _subscriber;
​
        protected IDatabase db
        {
            get { return _db; }
            private set
            {
                _db = value;
            }
        }
​
        private TimeSpan _timeout;
​
        public TimeSpan Timeout
        {
            get { return _timeout; }
            private set { _timeout = value; }
        }
​
        #region Constructors
​
        public RedisCoreBL(ConnectionMultiplexer redisConnection, RedisConfig config)
        {
            _redisConnection = redisConnection;
            _configuration = config;
​
            _db = _redisConnection.GetDatabase(_configuration.DatabaseId);
            Timeout = TimeSpan.FromMinutes(_configuration.Timeout);
        }
​
        #endregion
​
        #region Subsciption
​
        public void Subscribe(string subscriptionType, List<string> channelParams, IRedisCallbackBL bl, int? databaseId = null)
        {
            string dbId = databaseId.HasValue ? databaseId.Value.ToString() : "*";
            var subscriber = _redisConnection.GetSubscriber();
            subscriber.Subscribe($"__{subscriptionType}@{dbId}__:*", (channel, value) =>
            {
                foreach (string param in channelParams)
                {
                    Regex regex = new Regex($"__{subscriptionType}@(\\d)__:{param}");
                    var match = regex.Match(channel);
                    if (!match.Success)
                        break;
​
                    int.TryParse(match.Groups[1].Value, out int sourceDatabaseId);
                    try
                    {
                        if ((string)channel == $"__{subscriptionType}@{sourceDatabaseId}__:{param}")
                            bl.KeyEventHandler(param, value, sourceDatabaseId);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });
        }​
        #endregion
    }
}
