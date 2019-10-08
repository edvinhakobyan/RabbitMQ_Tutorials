using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace MQRabbit
{
    /// <summary>
    /// Class MqManager.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class RabbitMQManager : IDisposable
    {
        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        /// <value>The channel.</value>
        protected IModel Channel { get; set; }

        /// <summary>
        /// Gets or sets the connection.
        /// </summary>
        /// <value>The connection.</value>
        protected IConnection Connection { get; set; }

        /// <summary>
        /// Gets or sets the connection factory.
        /// </summary>
        /// <value>The connection factory.</value>
        private ConnectionFactory ConnectionFactory { get; set; }

        private Dictionary<string, object> QueueArgs;
        private readonly int _xMaxLength = int.Parse(ConfigurationManager.AppSettings["x-max-length"]);
        private readonly int _xMessageTtl = int.Parse(ConfigurationManager.AppSettings["x-message-ttl"]);
        private readonly int _xExpires = int.Parse(ConfigurationManager.AppSettings["x-expires"]);

        private RabbitMQManager()
        {
            if (QueueArgs is null)
            {
                QueueArgs = new Dictionary<string, object>()
                {
                    {"x-max-length", _xMaxLength},                  //"How many (ready) messages a queue can contain before it starts to drop them from its head"
                    {"x-message-ttl", _xMessageTtl /*20 seconds*/}, //Time - to - live "set timespan to a queue which will discard if queue reaches its lifespan"
                    {"x-expires", _xExpires /*30 minutes*/}         //Auto expire "controls for how long a queue can be unused before it is automatically deleted"
                };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQManager"/> class.
        /// </summary>
        /// <param name="mqInitModel">The mq initialize model.</param>
        public RabbitMQManager(ConnectionModel mqInitModel) : this()
        {
            ConnectionFactory = new ConnectionFactory
            {
                HostName = mqInitModel.HostName,
                //Port = mqInitModel.Port,
                UserName = mqInitModel.UserName,
                Password = mqInitModel.Password,
                AutomaticRecoveryEnabled = mqInitModel.AutomaticRecoveryEnabled, //true,
                RequestedHeartbeat = mqInitModel.RequestedHeartbeat, //30,
                NetworkRecoveryInterval = mqInitModel.NetworkRecoveryInterval //TimeSpan.FromSeconds(10) // default is 5
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMQManager"/> class.
        /// </summary>
        /// <param name="rabbitHost">The rabbit host.</param>
        /// <param name="rabbitPort">The rabbit port.</param>
        /// <param name="rabbitUserName">Name of the rabbit user.</param>
        /// <param name="rabbitPassword">The rabbit password.</param>
        public RabbitMQManager(string rabbitHost, int rabbitPort, string rabbitUserName, string rabbitPassword) : this()
        {
            ConnectionFactory = new ConnectionFactory
            {
                HostName = rabbitHost,
                Port = rabbitPort,
                UserName = rabbitUserName,
                Password = rabbitPassword,
                AutomaticRecoveryEnabled = true,
                RequestedHeartbeat = 30,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10) // default is 5
            };
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool Connect()
        {
            try
            {
                if (Connection == null || !Connection.IsOpen)
                {
                    Connection = ConnectionFactory.CreateConnection();
                    Channel = Connection.CreateModel();
                }
                else if (!Channel.IsOpen)
                {
                    Channel = Connection.CreateModel();
                }
                return true;
            }
            catch (BrokerUnreachableException e)
            {
                return false;
            }
            catch (System.IO.EndOfStreamException e)
            {
                return false;
            }
        }

        /// <summary>
        /// Handles the BasicNacks event of the Channel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="BasicNackEventArgs"/> instance containing the event data.</param>
        private void Channel_BasicNacks(object sender, BasicNackEventArgs e)
        {

        }

        /// <summary>
        /// Handles the BasicAcks event of the Channel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="BasicAckEventArgs"/> instance containing the event data.</param>
        private void Channel_BasicAcks(object sender, BasicAckEventArgs e)
        {

        }

        /// <summary>
        /// Produces the specified exchange name.
        /// </summary>
        /// <param name="exchangeName">Name of the exchange.</param>
        /// <param name="message">The message.</param>
        /// <param name="bindingKey">The binding key.</param>
        /// <exception cref="System.Exception">Connection failed!</exception>
        public void Produce(string exchangeName, 
                           string exchangeType, 
                           string bindingKey, 
                           string message)
        {
            if (Connect())
            {
                if (!string.IsNullOrEmpty(exchangeName))
                    Channel.ExchangeDeclare(exchangeName, exchangeType, false);

                var properties = Channel.CreateBasicProperties();
                properties.DeliveryMode = 2;
                properties.ContentType = "application/json";

                var binaryMessage = Encoding.Unicode.GetBytes(message);

                lock (Channel)
                {
                    Channel.BasicPublish(exchange: exchangeName,
                                       routingKey: bindingKey,
                                  basicProperties: properties,
                                             body: binaryMessage);
                }
        }
            else
            {
                throw new Exception("Connection failed!");
            }
        }

        /// <summary>
        /// Consumes the specified exchange name.
        /// </summary>
        /// <param name="exchangeName">Name of the exchange.</param>
        /// <param name="messageHandler">The message handler.</param>
        /// <param name="bindingKeys">The binding keys.</param>
        /// <exception cref="System.Exception">Connection failed!</exception>
        public void Consume(string exchangeName, 
                            string exchangeType,
                            string queueName,
                            Action<object, string> messageHandler,
                            params string[] bindingKeys)
        {
            if (Connect())
            {
                Channel.ContinuationTimeout = TimeSpan.FromSeconds(20);

                Channel.ExchangeDeclare(exchangeName, exchangeType);

                queueName = queueName.Length > 255 ? queueName.Substring(0, 254) : queueName;

                Channel.QueueDeclare(queue: queueName,
                                   durable: false,
                                 exclusive: false,
                                autoDelete: true,
                                 arguments: QueueArgs);

                /*Note: - prefetchCount
                  In order to defeat that we can set the prefetch count with the value of 1. This tells RabbitMQ not to give
                  more than one message to a worker at a time. Or, in other words, don't dispatch a new message to a
                  worker until it has processed and acknowledged the previous one. Instead, it will dispatch it to the next
                  worker that is not still busy.*/
                //Channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                foreach (var bindingKey in bindingKeys)
                {
                    Channel.QueueBind(queue: queueName,
                                   exchange: exchangeName,
                                 routingKey: bindingKey);
                }

                Consumer consumer = new Consumer(Channel);

                Channel.BasicConsume(queue: queueName,
                                   autoAck: true,
                                  consumer: consumer);
            }
            else
            {
                throw new Exception("Connection failed!");
            }
        }


        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            if (Connection != null && Connection.IsOpen)
            {
                Connection.Close();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Connection?.Close();
            Channel?.Dispose();
            Connection?.Dispose();
        }

        /// <summary>
        /// Resets the chunnel dispatcher.
        /// </summary>
        public void ResetChunnelDispatcher()
        {

        }
    }

    public class Consumer : DefaultBasicConsumer
    {
        private readonly IModel _model;

        public Consumer(IModel model)
        {
            _model = model;
        }

        public override void HandleBasicDeliver(string consumerTag,
                                                ulong deliveryTag,
                                                bool redelivered,
                                                string exchange,
                                                string routingKey,
                                                IBasicProperties properties,
                                                byte[] body)
        {
            string mesage = Encoding.Unicode.GetString(body);
            //_model.BasicAck(deliveryTag, false); //hastatum a vor stacel a bayc problem ka
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Consumer Receive {mesage}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}