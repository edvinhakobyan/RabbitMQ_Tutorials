using System;
using System.Text;
using RabbitMQ.Client;

namespace RequestRabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            string UserName = "guest";
            string Password = "guest";
            string HostName = "localhost";

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
            Console.WriteLine("Creating Exchange");

            //Creating Queue
            model.QueueDeclare("demoQueue", true, false, false, null);
            Console.WriteLine("Creating Queue");

            //Creating Binding
            model.QueueBind("demoQueue", "demoExchange", "directexchange_key");
            Console.WriteLine("Creating Binding");


            var properties = model.CreateBasicProperties();
            properties.Persistent = false;

            var message = "Direct Message";
            var messagebuffer = Encoding.Unicode.GetBytes(message);

            model.BasicPublish("demoExchange", "directexchange_key", properties, messagebuffer);
            Console.WriteLine("Message Send");


            Console.ReadLine();
        }
    }
}
