using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;

namespace RequestRabbitMQ
{

    class MessagePublisher
    {
        private const string UserName = "guest";
        private const string Password = "guest";
        private const string HostName = "localhost";

        public void SendMessage(string message)
        {
            var connectionFactory = new ConnectionFactory()
            {
                UserName = UserName,
                Password = Password,
                HostName = HostName
            };

            var connection = connectionFactory.CreateConnection();
            var model = connection.CreateModel();

            //Creating Exchange
            model.ExchangeDeclare("demoExchange", ExchangeType.Direct, true, true);
            //Console.WriteLine("Creating Exchange");

            //Creating Queue
            model.QueueDeclare("demoQueue", true, false, false, null);
            //Console.WriteLine("Creating Queue");

            //Creating Binding
            model.QueueBind("demoQueue", "demoExchange", "directexchange_key");
            //Console.WriteLine("Creating Binding");


            var properties = model.CreateBasicProperties();
            properties.Persistent = false;

            var messagebuffer = Encoding.Unicode.GetBytes(message);

            model.BasicPublish("demoExchange", "directexchange_key", properties, messagebuffer);
        }
    }




    class Program
    {
        static void Main()
        {
            MessagePublisher publisher = new MessagePublisher();

            for (int i = 0; i < 1000; i++)
            {
                publisher.SendMessage($"Send {i}");
                Console.WriteLine($"Send {i}");
                Thread.Sleep(1000);
            }


            Console.ReadLine();
        }
    }
}
