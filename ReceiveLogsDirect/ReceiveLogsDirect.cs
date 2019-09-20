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
                    var exchangeName = "My_exchange_name";

                    var queueName = model.QueueDeclare().QueueName;

                    //Sarqum em Exchange!!! Anun@ -> My_Exchange, tip@ -> direct
                    model.ExchangeDeclare(exchange: exchangeName,
                                              type: "direct");


                    Console.Error.WriteLine("Usage: {0} [info] [warning] [error]");


                    foreach (var routingKey in args)
                    {
                        model.QueueBind(queue: queueName,
                                     exchange: exchangeName,
                                   routingKey: routingKey);
                    }

                    Console.WriteLine(" [*] Waiting for messages...");

                    var consumer = new EventingBasicConsumer(model);

                    consumer.Received += (mo, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.Unicode.GetString(body);

                        var routingKey = ea.RoutingKey;
                        Console.WriteLine(" [x] Received '{0}':'{1}'", routingKey, message);
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
