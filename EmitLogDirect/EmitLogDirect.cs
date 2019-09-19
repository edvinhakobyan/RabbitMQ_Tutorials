using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading;
using System.IO;

namespace EmitLogDirect
{
    class EmitLogDirect
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.ExchangeDeclare(exchange: "direct_logs",
                                              type: "direct");

                    for (int i = 0; i < int.MaxValue; i++)
                    {
                        var message = i % 2 == 0 ? "info" : i % 3 == 0 ? "warning" : "error";
                        var body = Encoding.Unicode.GetBytes(message);

                        model.BasicPublish(exchange: "direct_logs",
                                             routingKey: message,
                                             basicProperties: null,
                                             body: body);

                        Console.WriteLine($" [x] Sent {message}");
                        Thread.Sleep(1000);
                    }
                }
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
