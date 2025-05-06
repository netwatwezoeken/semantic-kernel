using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SharedStuff;

var kernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
    .Build();

var chat = kernel.GetRequiredService<IChatCompletionService>();

var initialQuestion =DemoUtil.SimulateTyping("Which band invented metal? Just give the band name, no explanation.");
var initialResult = await chat.GetChatMessageContentsAsync(initialQuestion ?? "");
Console.WriteLine("LLM: " + initialResult.First().Content ?? "");

while (true)
{
    Console.Write("You: ");
    var question = Console.ReadLine();
    var result = await chat.GetChatMessageContentsAsync(question ?? "");
    Console.WriteLine("LLM: " + result.First().Content ?? "");
}