using _02_ChatHistory;
using SharedStuff;

using Plumbing;

var mr = new MessageRelay();
var demo = new _02ChatHistory(mr);
await demo.Start();
var cw = new ConsoleUi(mr);
await cw.Run(demo.DemoQuestion!);