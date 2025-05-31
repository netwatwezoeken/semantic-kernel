using _04_Functions;
using SharedStuff;
using Plumbing;

var mr = new MessageRelay();
var demo = new _04Functions(mr);
await demo.Start();
var cw = new ConsoleUi(mr);
await cw.Run(demo.DemoQuestion);