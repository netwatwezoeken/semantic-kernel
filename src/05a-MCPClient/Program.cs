using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using SharedStuff;

var kernelBuilder = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("llama3.2:3b", new Uri("http://localhost:11434"));

var transport = new SseClientTransport(new SseClientTransportOptions
{
    Endpoint = new Uri("http://localhost:3001"),
    UseStreamableHttp = true,
});
var mcpClient = await McpClientFactory.CreateAsync(transport);
var tools = await mcpClient.ListToolsAsync();

var settings = new OllamaPromptExecutionSettings()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
    Temperature = 0
};

var kernel = kernelBuilder.Build();

kernel.Plugins.AddFromFunctions("McpTools", tools.Select(f => f.AsKernelFunction()));

var chat = kernel.GetRequiredService<IChatCompletionService>();

var chatHistory = new ChatHistory();

var initialQuestion= DemoUtil.SimulateTyping("Play the title song of the second album of the band that invented metal.");

chatHistory.AddUserMessage([
    new TextContent(initialQuestion)
]);
var result = await chat.GetChatMessageContentsAsync(chatHistory, settings, kernel);
Console.WriteLine("LLM: " + result.First().Content ?? "");