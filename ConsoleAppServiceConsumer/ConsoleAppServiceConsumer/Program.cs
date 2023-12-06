
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


using System.Configuration;

using Newtonsoft.Json;

using MimeKit;
using MailKit.Net.Smtp;

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
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                
               
                channel.QueueDeclare(queue: "MyQueue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);

//                {
//                    "name": "dxg",
//  "family": "fdgdf",
//  "patronymic": "dfgdfdfg",
//  "phone": "string",
//  "email": "testov.23@internet.ru"
//}


                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(message);

                    var dist = JSONToDictionary(message);
                    var s = dist["Email"];

                    using var emailMessage = new MimeMessage();

                    emailMessage.From.Add(new MailboxAddress("", ConfigurationManager.AppSettings["email"]));
                    emailMessage.To.Add(new MailboxAddress("", s.ToString()));
                    emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    {
                        Text = "<button type='submit' style='background-color:red;'>Ok</button>"
                    };
                    using (var client = new SmtpClient())
                    {
                        client.Connect("smtp.mail.ru",587);
                        client.Authenticate(ConfigurationManager.AppSettings["email"], ConfigurationManager.AppSettings["key"]);
                        
                        client.Send(emailMessage);
                        client.Disconnect(true);
                    };
                };

                channel.BasicConsume(queue: "MyQueue",
                                 autoAck: true,
                             consumer: consumer);
                
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();

            }
        }
        static private Dictionary<string, object> JSONToDictionary(string jsonString)
        {
            var jObj = JsonConvert.DeserializeObject(jsonString);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jObj.ToString());

            return dict;
        }

    }
}
