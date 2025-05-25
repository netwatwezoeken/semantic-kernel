using _01_Basic;
using Plumbing;
using SharedStuff;

var mr = new MessageRelay();
var demo = new _01Basic(mr);
await demo.Start();
var cw = new ConsoleUi(mr);
await cw.Run(demo.DemoQuestion!);