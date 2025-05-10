using _04_Functions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using SharedStuff;

var kernelBuilder = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("llama3.1:8b", new Uri("http://localhost:11434"));
            
kernelBuilder.Plugins.AddFromType<MusicPlayerPlugin>("PlaySong");
        
var kernel = kernelBuilder.Build();

kernel.Plugins.AddFromType<MusicPlayerPlugin>("PlaySong");
var promptyTemplate = await File.ReadAllTextAsync($"./music-assistant.prompty");
        
var function = kernel.CreateFunctionFromPrompty(promptyTemplate);

var arguments = new KernelArguments(new OllamaPromptExecutionSettings()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
    Temperature = 0
});

var initialQuestion = DemoUtil.SimulateTyping("Play the best song of the band that invented metal.");
arguments.Add("question", initialQuestion);

var chatResult = await kernel.InvokeAsync(function, arguments);
