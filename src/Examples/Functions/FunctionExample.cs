using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace Examples.Functions;

public class FunctionExample : IExample 
{
    public async Task<string> Execute()
    {
        var kernelBuilder = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("llama3.1:8b", new Uri("http://localhost:11434"));
            
        kernelBuilder.Plugins.AddFromType<MusicPlayerPlugin>("PlaySong");
        
        var kernel = kernelBuilder.Build();;
        var promptyTemplate = await File.ReadAllTextAsync($"./Functions/music-assistant.prompty");
        
        var function = kernel.CreateFunctionFromPrompty(promptyTemplate);
        var arguments = new KernelArguments(new OllamaPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            Temperature = 0
        });
        arguments.Add("question", "Play the best song of the band that invented metal.");

        var chatResult = await kernel.InvokeAsync(function, arguments);
        return "";
    }
}