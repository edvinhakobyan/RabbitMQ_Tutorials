using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;

namespace Worker
{
    class Worker
    {
        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.QueueDeclare(queue: "task_queue",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    model.BasicQos(prefetchSize: 0,
                                    prefetchCount: 1,
                                           global: false);

                    Console.WriteLine(" [*] Waiting for messages.");

                    var consumer = new EventingBasicConsumer(model);

                    consumer.Received += (mo, ea) =>
                    {
                        int sleep = new Random().Next(1000);
                        var body = ea.Body;
                        var message = Encoding.Unicode.GetString(body);

                        Console.WriteLine($" [x] Received {message}\t|Consume work {sleep}ms|");

                        Thread.Sleep(sleep);

                        model.BasicAck(deliveryTag: ea.DeliveryTag,
                                            multiple: false);        //Подтверждение сообщения вручную
                    };
                    model.BasicConsume(queue: "task_queue",
                                         autoAck: false,
                                         consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
