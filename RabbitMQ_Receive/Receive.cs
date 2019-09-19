using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ_Receive
{
    class Receive
    {
        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.QueueDeclare(queue: "hello",
                                     durable: false,
                                   exclusive: false,
                                  autoDelete: false,
                                   arguments: null);

                    model.BasicQos(prefetchSize: 0,
                                  prefetchCount: 1,
                                         global: false);

                    var consumer = new EventingBasicConsumer(model);

                    consumer.Received += (mo, ea) =>
                    {
                        var body = ea.Body;    
                        var message = Encoding.UTF8.GetString(body);

                        Console.WriteLine($"Received {message}");
                        Thread.Sleep(new Random().Next(5000));

                        model.BasicAck(deliveryTag: ea.DeliveryTag,
                                          multiple: false);        //Подтверждение сообщения вручную
                    };

                    model.BasicConsume(queue: "hello",
                                     autoAck: false,
                                    consumer: consumer);


                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
