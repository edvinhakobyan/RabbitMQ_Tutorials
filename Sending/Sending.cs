using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading;
using System.IO;

namespace Sending
{
    class Send
    {
        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);


                for (int i = 0; i < int.MaxValue; i++)
                {
                    var message = $"{Path.GetRandomFileName()} {i}";
                    var body = Encoding.Unicode.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "hello",
                                         basicProperties: null,
                                         body: body);

                    Console.WriteLine($"Sent {message}");
                    Thread.Sleep(1000);
                }
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
