using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Plumbing;

namespace _04_Functions;

public class _04Functions : IDemo
{
    public string Name => "04 Functions";
    public string[] Models => ["llama3.1:8b"];
    public string? DemoQuestion => "Play the title song of the second album of the band that invented metal.";
    
    private void CreateKernel()
    {
        var kernelBuilder = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("llama3.1:8b", new Uri("http://localhost:11434"));
            
        kernelBuilder.Plugins.AddFromType<MusicPlayerPlugin>("PlaySong");
        
        _kernel  = kernelBuilder.Build();

        var promptyTemplate = File.ReadAllText($"./04-music-assistant.prompty");

        _function = _kernel.CreateFunctionFromPrompty(promptyTemplate);
    }
    
    public Task Start()
    {
        _relay.OnMessageAsync += HandleChatMessage;
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        _relay.OnMessageAsync -= HandleChatMessage;
        return Task.CompletedTask;
    }

    private readonly MessageRelay _relay;
    private Kernel _kernel;
    private KernelFunction _function;

    private async Task HandleChatMessage(ChatMessage message)
    {
        if (message.From != "user") return;
        
        var arguments = new KernelArguments(new OllamaPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            Temperature = 0.8f
        });

        arguments.Add("question", message.Message);

        var chatResult = await _kernel.InvokeAsync(_function, arguments);
        
        await _relay.HandleMessageAsync(
            new ChatMessage
            {
                From = "assistant",
                Message = chatResult.ToString()
            });
    }
    
    public _04Functions(MessageRelay relay)
    {
        CreateKernel();

        _relay = relay;
    }
}