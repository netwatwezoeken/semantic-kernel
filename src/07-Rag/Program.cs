using _07_Rag;
using SharedStuff;
using Plumbing;
using StackExchange.Redis;

var mr = new MessageRelay();
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
            
var multiplexer = ConnectionMultiplexer.Connect(rconfig);

if (args.Contains("--prepare"))
{
    await new EmbeddingService(multiplexer).Prepare();
}
else
{
    var demo = new _07Rag(mr, multiplexer);
    var cw = new ConsoleUi(mr);
    await demo.Start();
    await cw.Run(demo.DemoQuestion);
}



