using System;
using System.Text;
using System.Xml.Schema;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();

channel.QueueDeclare(
    queue: "letterbox",
    durable: false,
    exclusive: false,
    autoDelete: false,
    args: null);

var consumer = new EventingBasicConsumer(channel);

consumer.Recieved += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"mesaj geldiiiii:{message}");
};

channel.BasicConsume(queue: "letterbox", autoAck: true, consumer);

Console.ReadKey();