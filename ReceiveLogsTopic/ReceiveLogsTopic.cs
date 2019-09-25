using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ReceiveLogsTopic
{
    class ReceiveLogsTopic
    {
        public static void Main(string[] args)
        {
            string exchangeName = "My_exchange_EmitLogTopic";

            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.ExchangeDeclare(exchange: exchangeName, 
                                                type: "topic");

                    var queueName = model.QueueDeclare().QueueName;

                    if (args.Length < 1)
                    {
                        Console.Error.WriteLine("Usage: {0} [binding_key...]",
                                                Environment.GetCommandLineArgs()[0]);
                        Console.WriteLine(" Press [enter] to exit.");
                        Console.ReadLine();
                        Environment.ExitCode = 1;
                        return;
                    }

                    foreach (var bindingKey in args)
                    {
                        model.QueueBind(queue: queueName,
                                       exchange: exchangeName,
                                     routingKey: bindingKey);
                    }

                    Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

                    Consumer consumer = new Consumer(model);


                    //var consumer = new EventingBasicConsumer(channel);

                    //consumer.Received += (model, ea) =>
                    //{
                    //    var body = ea.Body;
                    //    var message = Encoding.Unicode.GetString(body);
                    //    var routingKey = ea.RoutingKey;
                    //    Console.WriteLine(" [x] Received '{0}':'{1}'", routingKey, message);
                    //};

                    model.BasicConsume(queue: queueName,
                                       autoAck: true,
                                      consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }


    public class Consumer : DefaultBasicConsumer
    {
        private readonly IModel _model;

        public Consumer(IModel model)
        {
            _model = model;
        }

        public override void HandleBasicDeliver(string consumerTag, 
                                                ulong deliveryTag,
                                                bool redelivered, 
                                                string exchange, 
                                                string routingKey, 
                                                IBasicProperties properties,
                                                byte[] body)
        {
            string mesage = Encoding.Unicode.GetString(body);
            _model.BasicAck(deliveryTag, false); //hastatum a vor stacel a
            Console.WriteLine(mesage);
        }
    }

}
