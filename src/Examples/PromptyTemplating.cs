using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace Examples;

public class PromptyTemplating : IExample 
{
    public async Task<string> Execute()
    {
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
        arguments.Add("question", "Which band invented metal? Just give the band name, no explanation.");

        var chatResult = await kernel.InvokeAsync(function, arguments);
        Console.WriteLine(chatResult);
        return  "";
    }
}