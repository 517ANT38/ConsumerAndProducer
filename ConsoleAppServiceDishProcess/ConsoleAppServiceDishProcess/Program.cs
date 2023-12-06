﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


using System.Configuration;


namespace MyProg
{
    public class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(ConfigurationManager.AppSettings["uriRabbit"])
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {


                channel.QueueDeclare(queue: "QDishes",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };
                channel.BasicConsume(queue: "QDishes",
                                 autoAck: true,
                             consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
