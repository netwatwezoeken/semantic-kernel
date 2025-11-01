using _07_Rag;
using SharedStuff;
using Plumbing;
using StackExchange.Redis;

var mr = new MessageRelay();

ConnectionMultiplexer? multiplexer = null;

var arguments = Environment.GetCommandLineArgs();

var prepare = false;
foreach (var t in arguments)
{
    if (t.Equals("--prepare", StringComparison.OrdinalIgnoreCase))
    {
        prepare = true;
    }
}
if (Environment.GetEnvironmentVariable("Redis") != null)
{
    multiplexer = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("Redis")!);
}

if (multiplexer == null)
{
    var rconfig = new ConfigurationOptions
    {
        EndPoints =
        {
            "localhost:6379"
        },
        AbortOnConnectFail = false,
        ConnectRetry = 10,
        ReconnectRetryPolicy = new ExponentialRetry(5000),
        ClientName = "ApiClient"
    };
    multiplexer = ConnectionMultiplexer.Connect(rconfig);
}

if (prepare)
{
    await new EmbeddingService(multiplexer).Prepare(new OllamaConfig());
}
else
{
    var demo = new _07Rag(mr, multiplexer, new OllamaConfig());
    var cw = new ConsoleUi(mr);
    await demo.Start();
    await cw.Run(demo.DemoQuestion);
}



