using System;
using System.Linq;
using RabbitMQ.Client;
using System.Text;
using System.IO;
using System.Threading;

namespace EmitLogTopic
{
    class EmitLogTopic
    {
        public static void Main(string[] a)
        {
            string exchangeName = "My_exchange_EmitLogTopic";

            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.ExchangeDeclare(exchange: exchangeName,
                                              type: "topic");

                    while (true)
                    {
                        var arg = GetKeyValueString();

                        var args = arg.Split(" ");

                        if (args[0] == "") break;

                        var routingKey = args[0]; // "info", "warning", "error"

                        var message = (args.Length > 1) ? string.Join(" ", args.Skip(1).ToArray()) : "Empty Mesage";

                        var body = Encoding.Unicode.GetBytes(message);

                        model.BasicPublish(exchange: exchangeName,
                                         routingKey: routingKey,
                                    basicProperties: null,
                                               body: body);

                        Console.WriteLine($" [x] Sent '{routingKey}':'{message}'");
                        Thread.Sleep(50);
                    }
                }
            }

        }

        private static string GetKeyValueString()
        {
            var rand = new Random();
            var r = rand.Next(4);
            var l = rand.Next(20);
            var m = rand.Next(100);

            if (r == 0)
                return $"{RandomString(l)}.orange.{RandomString(l)} {RandomString(m)}";
            if (r == 1)
                return $"{RandomString(l)}.{RandomString(l)}.rabbit {RandomString(m)}";
            if (r == 2)
                return $"lazy.{RandomString(l)}.{RandomString(l)} {RandomString(m)}";
            else
                return $"{RandomString(l)}.{RandomString(l)}.{RandomString(l)} {RandomString(m)}";
        }


        private static string RandomString(int length)
        {

            StringBuilder str = new StringBuilder();
            for (int i = 0; i < length; i++)
                str.Append((char)new Random().Next(65,91));
            return str.ToString();
        }


    }
}
