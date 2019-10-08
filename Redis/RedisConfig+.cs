using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis
{
    public class RedisConfig : ConfigurationSection
    {
        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get { return (string)this["host"]; }
        }
​
        [ConfigurationProperty("port", DefaultValue = 6379, IsRequired = false)]
        public int Port
        {
            get { return (int)this["port"]; }
        }
​
        [ConfigurationProperty("databaseId", DefaultValue = 0, IsRequired = false)]
        public int DatabaseId
        {
            get { return (int)this["databaseId"]; }
        }
​
        [ConfigurationProperty("timeout", IsRequired = true)]
        public int Timeout
        {
            get { return (int)this["timeout"]; }
        }
​
        [ConfigurationProperty("password", DefaultValue = "", IsRequired = false)]
        public string Password
        {
            get { return (string)this["password"]; }
        }
​
        public string Server
        {
            get { return Host + ":" + Port; }
        }
    }
}
