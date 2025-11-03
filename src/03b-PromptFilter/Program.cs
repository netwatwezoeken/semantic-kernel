using _03b_PromptFilter;
using SharedStuff;
using Plumbing;

var mr = new MessageRelay();
var demo = new _03bPromptFilter(mr, new OllamaConfig());
await demo.Start();
var cw = new ConsoleUi(mr);
await cw.Run(demo.DemoQuestion);