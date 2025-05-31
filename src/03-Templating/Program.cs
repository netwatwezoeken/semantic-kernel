using SharedStuff;
using _03_Templating;
using Plumbing;

var mr = new MessageRelay();
var demo = new _03Templating(mr);
await demo.Start();
var cw = new ConsoleUi(mr);
await cw.Run(demo.DemoQuestion);