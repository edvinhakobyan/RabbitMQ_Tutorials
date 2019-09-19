using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ReceiveLogs
{
    class ReceiveLogs
    {
        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.ExchangeDeclare(exchange: "logs", 
                                              type: "fanout");

                    var queueName = model.QueueDeclare().QueueName;

                    //A binding is a relationship between an exchange and a queue
                    model.QueueBind(queue: queueName,
                                 exchange: "logs",
                               routingKey: ""); //binding key. 

                    Console.WriteLine(" [*] Waiting for logs.");

                    var consumer = new EventingBasicConsumer(model);
                    consumer.Received += (mo, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.Unicode.GetString(body);

                        Console.WriteLine($" [x] {message}");
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
