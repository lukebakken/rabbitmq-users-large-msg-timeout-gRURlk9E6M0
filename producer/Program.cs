using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

TimeSpan fiveSeconds = TimeSpan.FromSeconds(5);
AutoResetEvent latch = new AutoResetEvent(false);

void CancelHandler(object? sender, ConsoleCancelEventArgs e)
{
    Console.WriteLine("CTRL-C pressed, exiting!");
    e.Cancel = true;
    latch.Set();
}

void WaitAndMaybeExit(TimeSpan wait)
{
    if (latch.WaitOne(wait))
    {
        Console.WriteLine("PRODUCER EXITING");
        Environment.Exit(0);
    }
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

string? inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
if (false == String.IsNullOrWhiteSpace(inContainer))
{
    Console.WriteLine($"PRODUCER: waiting 5 seconds to try initial connection to {hostName}:{port}");
    WaitAndMaybeExit(fiveSeconds);
}

var factory = new ConnectionFactory()
{
    HostName = hostName,
    Port = port
};

TimeSpan latchWaitSpan = TimeSpan.FromSeconds(1);
bool connected = false;

IConnection? connection = null;

while (!connected)
{
    try
    {
        connection = factory.CreateConnection();
        connected = true;
    }
    catch (BrokerUnreachableException)
    {
        connected = false;
        Console.WriteLine($"PRODUCER: waiting 5 seconds to re-try connection!");
        WaitAndMaybeExit(fiveSeconds);
    }
}

byte[] buffer = new byte[1024 * 1024 * 100];
Random rnd = new Random();

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
            Console.Error.WriteLine($"PRODUCER: connection.CallbackException: {cea}");
        };

        connection.ConnectionBlocked += (s, ea) =>
        {
            var cbea = (ConnectionBlockedEventArgs)ea;
            Console.Error.WriteLine($"PRODUCER: connection.ConnectionBlocked: {cbea}");
        };

        connection.ConnectionUnblocked += (s, ea) =>
        {
            Console.Error.WriteLine($"PRODUCER: connection.ConnectionUnblocked: {ea}");
        };

        connection.ConnectionShutdown += (s, ea) =>
        {
            var sdea = (ShutdownEventArgs)ea;
            Console.Error.WriteLine($"PRODUCER: connection.ConnectionShutdown: {sdea}");
        };

        using (var channel = connection.CreateModel())
        {
            channel.ConfirmSelect();

            channel.CallbackException += (s, ea) =>
            {
                var cea = (CallbackExceptionEventArgs)ea;
                Console.Error.WriteLine($"PRODUCER: channel.CallbackException: {cea}");
            };

            channel.ModelShutdown += (s, ea) =>
            {
                var sdea = (ShutdownEventArgs)ea;
                Console.Error.WriteLine($"PRODUCER: channel.ModelShutdown: {sdea}");
            };

            channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            while (true)
            {
                rnd.NextBytes(buffer);
                string now = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ffffff");
                channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: buffer);
                channel.WaitForConfirmsOrDie();
                Console.WriteLine($"PRODUCER sent large message at {now}, exiting!");
                break;
                /*
                if (latch.WaitOne(latchWaitSpan))
                {
                    Console.WriteLine("PRODUCER EXITING");
                    break;
                }
                */
            }
        }
    }
}
