using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading;
using System.IO;

namespace EmitLog
{
    class EmitLog
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.ExchangeDeclare(exchange: "logs", 
                                              type: "fanout");

                    for (int i = 0; i < int.MaxValue; i++)
                    {
                        var message = $"{Path.GetRandomFileName()} {i}";
                        var body = Encoding.Unicode.GetBytes(message);

                        model.BasicPublish(exchange: "logs",
                                         routingKey: "",
                                    basicProperties: null,
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
