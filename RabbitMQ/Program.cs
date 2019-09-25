using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace MQRabbit
{
    class Program
    {
        private static void OnGetParse(string s)
        {
            Debug.WriteLine(s);
        }

        static void Main(string[] args)
        {
            var connectionModel = new ConnectionModel()
            {
                HostName = ConfigurationManager.AppSettings["RabbitMQ_Host"],
                //Port = int.Parse(ConfigurationManager.AppSettings["RabbitMQ_Port"]),
                UserName = ConfigurationManager.AppSettings["RabbitMQ_UserName"],
                Password = ConfigurationManager.AppSettings["RabbitMQ_Password"]
            };
            var manager = new RabbitMQManager(connectionModel);

            string exchangeName = ConfigurationManager.AppSettings["RabbitMQ_exchangeName"];
            string queueName = ConfigurationManager.AppSettings["RabbitMQ_queueName"];


            new Thread(() =>
            {
                while (true)
                {
                    string mesage = GetKeyValueString();
                    manager.Produce(exchangeName, ExchangeType.Topic, mesage, $"Message {mesage}");
                    Console.WriteLine($"Publisher send 'Message {mesage}'");
                    Thread.Sleep(200);
                }
            }).Start();


            manager.Consume(exchangeName: exchangeName,
                            exchangeType: ExchangeType.Topic,
                               queueName: queueName,
                          messageHandler: (o, s1) => { OnGetParse(s1); },
                             bindingKeys: new string[] { "*.orange.#" });

            Console.ReadKey();
        }

        static Random rand = new Random();
        private static string GetKeyValueString()
        {
            var r = rand.Next(4);
            var l = rand.Next(20);
            var m = rand.Next(100);

            if (r == 0)
                return $"{RandomString(l)}.orange.{RandomString(l)}";
            if (r == 1)
                return $"{RandomString(l)}.{RandomString(l)}.rabbit";
            if (r == 2)
                return $"lazy.{RandomString(l)}.{RandomString(l)}";
            else
                return $"{RandomString(l)}.{RandomString(l)}.{RandomString(l)}";
        }


        private static string RandomString(int length)
        {

            StringBuilder str = new StringBuilder();
            for (int i = 0; i < length; i++)
                str.Append((char)rand.Next(65, 91));
            return str.ToString();
        }
    }
}
