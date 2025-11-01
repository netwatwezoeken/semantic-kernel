using Plumbing;
using SharedStuff;

namespace _06_AgentFramework;

public static class Program
{
    public static async Task Main()
    {
        var mr = new MessageRelay();
        var demo = new _06AgentFramework(mr, new OllamaConfig());
        await demo.Start();
        var cw = new ConsoleUi(mr);
        await cw.Run(demo.DemoQuestion);
    }
}