using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading;
using System.IO;

namespace NewTask
{
    class NewTask
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "task_queue",
                                         durable: true,
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
                                             routingKey: "task_queue",
                                             basicProperties: properties,
                                             body: body);

                        Console.WriteLine("Sent {0}", message);
                        Thread.Sleep(1000);
                    }
                }

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
