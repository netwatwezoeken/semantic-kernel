using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Plumbing;

namespace _01_Basic;

public class _01Basic : IDemo
{
    public string Name => "01 Basic";
    public string[] Models => ["gemma3:4b"];
    public string? DemoQuestion => "Which band invented metal? Just give the band name, no explanation.";
    
    private void CreateKernel()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
            .Build();

        _chat = kernel.GetRequiredService<IChatCompletionService>();
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
    private IChatCompletionService _chat;

    private async Task HandleChatMessage(ChatMessage message)
    {
        if (message.From != "user" || _chat == null) return;
            
        var response = await _chat.GetChatMessageContentsAsync(message.Message);
        await _relay.HandleMessageAsync(
            new ChatMessage
            {
                From = "assistant",
                Message = response.First().Content ?? ""
            });
    }
    
    public _01Basic(MessageRelay relay)
    {
        CreateKernel();
        _relay = relay;
    }
}