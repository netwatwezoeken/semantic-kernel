using _04b_FunctionFilter;
using SharedStuff;
using Plumbing;

var mr = new MessageRelay();
var demo = new _04bFunctionFilter(mr, new OllamaConfig());
await demo.Start();
var cw = new ConsoleUi(mr);
await cw.Run(demo.DemoQuestion);