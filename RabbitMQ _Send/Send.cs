using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RabbitMQ_Send
{
    class Send
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true; //Теперь нам нужно пометить наши сообщения как постоянные

                    for (int i = 0; i < int.MaxValue; i++)
                    {
                        var message = $"{Path.GetRandomFileName()} {i}";
                        var body = Encoding.Unicode.GetBytes(message);

                        channel.BasicPublish(exchange: "",
                                           routingKey: "hello",
                                      basicProperties: properties,
                                                 body: body);

                        Console.WriteLine($"Sent {message}");
                        Thread.Sleep(1000);
                    }
                }
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
