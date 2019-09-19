using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ReceiveLogsDirect
{
    class ReceiveLogsDirect
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

                    var queueName = model.QueueDeclare().QueueName;


                    foreach (var severity in new string[] { "info", "warning", "error"})
                    {
                        model.QueueBind(queue: queueName,
                                          exchange: "direct_logs",
                                          routingKey: severity);
                    }

                    Console.WriteLine(" [*] Waiting for messages.");

                    var consumer = new EventingBasicConsumer(model);

                    consumer.Received += (mo, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.Unicode.GetString(body);

                        var routingKey = ea.RoutingKey;
                        Console.WriteLine(" [x] Received '{0}':'{1}'",
                                          routingKey, message);
                    };
                    model.BasicConsume(queue: queueName,
                                         autoAck: true,
                                         consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
