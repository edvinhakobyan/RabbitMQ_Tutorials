﻿using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading;
using System.IO;
using System.Linq;

namespace EmitLogDirect
{
    class EmitLogDirect
    {
        public static void Main(string[] a)
        {
            string exchangeName = "My_exchange_name";

            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.ExchangeDeclare(exchange: exchangeName,
                                              type: "direct");

                    while(true)
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

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static string GetKeyValueString()
        {
            int r = new Random().Next(4);

            if (r == 0)
                return "info " + Path.GetRandomFileName();
            if (r == 1)
                return "warning " + Path.GetRandomFileName();
            if (r == 2)
                return "error " + Path.GetRandomFileName();
            else
                return Path.GetRandomFileName() + Path.GetRandomFileName();
        }
    }
}
