using System;
using System.Text;
using RabbitMQ.Client;

namespace RabbitMQConsumer
{
    class Program
    {
        private const string UserName = "guest";
        private const string Password = "guest";
        private const string HostName = "localhost";

        static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory()
            {
                UserName = UserName,
                Password = Password,
                HostName = HostName
            };

            var connection = connectionFactory.CreateConnection();
            var model = connection.CreateModel();

            model.BasicQos(0, 1, false);

            MessageReceiver messageReceiver = new MessageReceiver(model);

            model.BasicConsume("demoQueue", false, messageReceiver);

            Console.WriteLine();
        }
    }



    class MessageReceiver : DefaultBasicConsumer
    {
        private readonly IModel _model;

        public MessageReceiver(IModel model)
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
            Console.WriteLine("Consuming Message");
            Console.WriteLine($"Message received from the exchange '{exchange}'");
            Console.WriteLine($"Consumer tag: '{consumerTag}'");
            Console.WriteLine($"Delivery tag: '{deliveryTag}'");
            Console.WriteLine($"Routing key: '{routingKey}'");
            Console.WriteLine($"Message: '{Encoding.Unicode.GetString(body)}'");
            Console.WriteLine(new string('-', 30));
            _model.BasicAck(deliveryTag, false);
        }

        


    }
}
