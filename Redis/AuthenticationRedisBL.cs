using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis
{
    public class AuthenticationRedisBL : RedisCoreBL, IAuthenicationRedisBL
    {
        public AuthenticationRedisBL(ConnectionMultiplexer redisConnection, RedisConfig config) : base(redisConnection, config)
        {

        }
​​
        public async Task<string> RefreshToken(string token, Session session = null)
        {
            RedisValue value = await db.StringGetAsync(token);
            if (value.IsNull)
                throw new Exception();
​
            if (session is null)
                session = JsonConvert.DeserializeObject<Session>(value);
            await db.KeyExpireAsync(token, Timeout);
​
            string tokenKey = $"{session.UserId}_{session.UserType}";
            await db.KeyExpireAsync(tokenKey, Timeout);
            return token;
        }
​
        public async Task<Session> GetClientSession(string token)
        {
            RedisValue value = await db.StringGetAsync(token);
            if (value.IsNull)
                return null;//throw CreateException(Constants.Errors.WrongClientToken);

            var session = JsonConvert.DeserializeObject<Session>(value);
            await db.KeyExpireAsync(token, Timeout);
​
            string tokenKey = $"{session.UserId}_{session.UserType}";
            await db.KeyExpireAsync(tokenKey, Timeout);
​
            return session;
        }
​
        public async Task AddClientSession(string token, Session session, bool ignoreIfExists = false)
        {
            RedisValue value = await db.StringGetAsync(token);
            if (!value.IsNull)
            {
                if (ignoreIfExists)
                    return;
                throw new Exception();
            }
            var data = JsonConvert.SerializeObject(session, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
​
            await db.StringSetAsync(token, data, Timeout);
​
            string tokenKey = $"{session.UserId}_{session.UserType}";
            await db.SetAddAsync(tokenKey, token);
            await db.KeyExpireAsync(tokenKey, Timeout);
        }
​
        public async Task<List<string>> GetClientTokens(string clientlogin, int partnerId)
        {
            string tokenKey = $"{clientlogin}_{partnerId}";
            RedisValue[] values = await db.SetMembersAsync(tokenKey);
            if (!values.Any())
                return null;//throw CreateException(Constants.Errors.WrongClientToken);
​
            await db.KeyExpireAsync(tokenKey, Timeout);
            return values.Select(x => x.ToString()).ToList();
        }
​
        public async Task RemoveClientSession(string token)
        {
            RedisValue value = await db.StringGetAsync(token);
            if (value.IsNull) return;
​
            var session = JsonConvert.DeserializeObject<Session>(value);
            await db.KeyDeleteAsync(token);
            await db.SetRemoveAsync($"{session.UserId}_{session.UserType}", token);
            await db.KeyExpireAsync($"{session.UserId}_{session.UserType}", Timeout);
        }
​
        public async Task RemoveClientSession(string clientLogin, int partnerId)
        {
            var token = $"{clientLogin}_{partnerId}";
​
            RedisValue[] values = await db.SetMembersAsync(token);
            if (!values.Any()) return;
​
            await db.KeyDeleteAsync(token);
            foreach (var value in values)
            {
                await db.KeyDeleteAsync(value.ToString());
            }
        }
    }
}
