using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis
{
    public interface IRedisCoreBL
    {
        /// <summary>
        /// Subscribes to redis keyspace events.
        /// </summary>
        /// <param name="subscriptionType">Type of subscribtion ( keyevent/keysapace) </param>
        /// <param name="channelParams">Type of events which are need to receive ( Expire, Del etc. ) </param>
        /// <param name="bl"></param>
        /// <param name="databaseId">Database to which need to subscribe, when value is null, it means that subscribtion will work for all databases.</param>
        void Subscribe(string subscriptionType, List<string> channelParams, IRedisCallbackBL bl, int? databaseId = null);
    }

    public interface IAuthenicationRedisBL : IRedisCoreBL
    {
        Task<string> RefreshToken(string token, Session session = null);
​
        Task<Session> GetClientSession(string token);
​
        Task AddClientSession(string token, Session session, bool ignoreIfExists = false);
​
        Task<List<string>> GetClientTokens(string clientlogin, int partnerId);
​
        Task RemoveClientSession(string token);
​
        Task RemoveClientSession(string clientLogin, int partnerId);
    }
}
