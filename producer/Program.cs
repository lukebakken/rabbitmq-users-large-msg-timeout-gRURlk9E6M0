using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

using System.Text;

Console.WriteLine("PRODUCER: waiting 5 seconds to try initial connection");
Thread.Sleep(TimeSpan.FromSeconds(5));

var factory = new ConnectionFactory()
{
    HostName = "rabbitmq",
    Port = 5672
};

bool connected = false;

IConnection? connection = null;

while(!connected)
{
    try
    {
        connection = factory.CreateConnection();
        connected = true;
    }
    catch (BrokerUnreachableException)
    {
        connected = false;
        Console.WriteLine("PRODUCER: waiting 5 seconds to re-try connection!");
        Thread.Sleep(TimeSpan.FromSeconds(5));
    }
}

using (connection)
{
    if (connection == null)
    {
        Console.Error.WriteLine("PRODUCER: unexpected null connection");
    }
    else
    {
        connection.CallbackException += (s, ea) =>
        {
            var cea = (CallbackExceptionEventArgs)ea;
            Console.Error.WriteLine("PRODUCER: connection.CallbackException: {cea}");
        };

        connection.ConnectionBlocked += (s, ea) =>
        {
            var cbea = (ConnectionBlockedEventArgs)ea;
            Console.Error.WriteLine("PRODUCER: connection.ConnectionBlocked: {cbea}");
        };

        connection.ConnectionUnblocked += (s, ea) =>
        {
            Console.Error.WriteLine("PRODUCER: connection.ConnectionUnblocked: {ea}");
        };

        connection.ConnectionShutdown += (s, ea) =>
        {
            var sdea = (ShutdownEventArgs)ea;
            Console.Error.WriteLine("PRODUCER: connection.ConnectionShutdown: {sdea}");
        };

        using (var channel = connection.CreateModel())
        {
            channel.CallbackException += (s, ea) =>
            {
                var cea = (CallbackExceptionEventArgs)ea;
                Console.Error.WriteLine("PRODUCER: channel.CallbackException: {cea}");
            };

            channel.ModelShutdown += (s, ea) =>
            {
                var sdea = (ShutdownEventArgs)ea;
                Console.Error.WriteLine("PRODUCER: channel.ModelShutdown: {sdea}");
            };

            channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            while (true)
            {
                string message = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ffffff");
                var body = Encoding.ASCII.GetBytes(message);
                channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
                Console.WriteLine($"PRODUCER sent {message}");
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
