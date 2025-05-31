using _05a_MCPClient;
using SharedStuff;
using Plumbing;

var mr = new MessageRelay();
var demo = new _05MCPClient(mr);
await demo.Start();
var cw = new ConsoleUi(mr);
await cw.Run(demo.DemoQuestion);