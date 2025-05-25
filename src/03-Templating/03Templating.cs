using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Plumbing;

namespace _03_Templating;

public class _03Templating : IDemo
{
    public string Name => "03 Templating";
    public string[] Models => ["gemma3:4b"];
    public string? DemoQuestion => "Which band invented metal?";
    
    private void CreateKernel()
    {
        _kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
            .Build();

        var promptyTemplate = File.ReadAllText($"./music-assistant.prompty");
        
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
            Temperature = 0
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
    
    public _03Templating(MessageRelay relay)
    {
        CreateKernel();

        _relay = relay;
    }
}