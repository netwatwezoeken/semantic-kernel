using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using SharedStuff;

var kernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
    .Build();

var promptyTemplate = await File.ReadAllTextAsync($"./music-assistant.prompty");
        
var function = kernel.CreateFunctionFromPrompty(promptyTemplate);
var arguments = new KernelArguments(new OllamaPromptExecutionSettings()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
    Temperature = 0
});

var initialQuestion = DemoUtil.SimulateTyping("Which band invented metal?");
arguments.Add("question", initialQuestion);

var chatResult = await kernel.InvokeAsync(function, arguments);
Console.WriteLine("LLM: " + chatResult);