using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

using System.Text;

AutoResetEvent latch = new AutoResetEvent(false);

void CancelHandler(object? sender, ConsoleCancelEventArgs e)
{
    Console.WriteLine("CTRL-C pressed, exiting!");
    e.Cancel = true;
    latch.Set();
}

Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);

string hostName = "rabbitmq";
ushort port = 5672;

string? hostNameStr = Environment.GetEnvironmentVariable("RABBITMQ_NODENAME");
if (false == String.IsNullOrWhiteSpace(hostNameStr))
{
    hostName = hostNameStr;
}

string? nodePortStr = Environment.GetEnvironmentVariable("RABBITMQ_NODE_PORT");
if (false == String.IsNullOrWhiteSpace(nodePortStr))
{
    port = ushort.Parse(nodePortStr);
}

Console.WriteLine($"CONSUMER: waiting 5 seconds to try initial connection to {hostName}:{port}");
Thread.Sleep(TimeSpan.FromSeconds(5));

var factory = new ConnectionFactory()
{
    HostName = hostName,
    Port = port
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
        Console.WriteLine("CONSUMER: waiting 5 seconds to re-try connection!");
        Thread.Sleep(TimeSpan.FromSeconds(5));
    }
}

int message_count = 0;
const ushort prefetch_count = 30;

using (connection)
{
    if (connection == null)
    {
        Console.Error.WriteLine("CONSUMER: unexpected null connection");
    }
    else
    {
        connection.CallbackException += (s, ea) =>
        {
            var cea = (CallbackExceptionEventArgs)ea;
            Console.Error.WriteLine($"CONSUMER: connection.CallbackException: {cea}");
        };

        connection.ConnectionBlocked += (s, ea) =>
        {
            var cbea = (ConnectionBlockedEventArgs)ea;
            Console.Error.WriteLine($"CONSUMER: connection.ConnectionBlocked: {cbea}");
        };

        connection.ConnectionUnblocked += (s, ea) =>
        {
            Console.Error.WriteLine($"CONSUMER: connection.ConnectionUnblocked: {ea}");
        };

        connection.ConnectionShutdown += (s, ea) =>
        {
            var sdea = (ShutdownEventArgs)ea;
            Console.Error.WriteLine($"CONSUMER: connection.ConnectionShutdown: {sdea}");
        };

        using (var channel = connection.CreateModel())
        {
            channel.CallbackException += (s, ea) =>
            {
                var cea = (CallbackExceptionEventArgs)ea;
                Console.Error.WriteLine($"CONSUMER: channel.CallbackException: {cea}");
            };

            channel.ModelShutdown += (s, ea) =>
            {
                var sdea = (ShutdownEventArgs)ea;
                Console.Error.WriteLine($"CONSUMER: channel.ModelShutdown: {sdea}");
            };

            channel.BasicQos(0, prefetch_count, false);

            channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            Console.WriteLine("CONSUMER: waiting for messages...");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                DateTime received = DateTime.Now;
                var body = ea.Body.ToArray();
                string sentText = Encoding.ASCII.GetString(body);
                DateTime sent = DateTime.ParseExact(sentText, "MM/dd/yyyy HH:mm:ss.ffffff", null);
                TimeSpan delay = received - sent;
                string receivedText = received.ToString("MM/dd/yyyy HH:mm:ss.ffffff");
                Console.WriteLine($"CONSUMER received at {receivedText}, sent at {sentText} - count: {message_count++}, delay: {delay}");
                if ((message_count % prefetch_count) == 0)
                {
                    Console.WriteLine($"CONSUMER SENDING BATCH ACK pfc: {prefetch_count} mc: {message_count}");
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: true);
                }
            };

            channel.BasicConsume(queue: "hello", autoAck: false, consumer: consumer);

            latch.WaitOne();
        }
    }
}
