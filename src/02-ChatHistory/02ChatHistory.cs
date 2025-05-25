using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Plumbing;

namespace _02_ChatHistory;

public class _02ChatHistory : IDemo
{
    public string Name => "02 ChatHistory";
    public string[] Models => ["gemma3:4b"];
    public string? DemoQuestion => "Which band invented metal?";
    
    private void CreateKernel()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
            .Build();
        _chat = kernel.GetRequiredService<IChatCompletionService>();
    }
    
    public Task Start()
    {
        _chatHistory = new ChatHistory();
        _chatHistory.AddSystemMessage(
            """
            You are a helpful metal music expert.
            Your task is to provide the best music advice and facts about metal music for the user.
            Only metal music of course. Hell yeah! 🤘
            Never give an explanation unless explicitly asked.
            """
        );
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
    private ChatHistory _chatHistory;

    private async Task HandleChatMessage(ChatMessage message)
    {
        if (message.From != "user" || _chat == null) return;
        
        _chatHistory.AddUserMessage([
            new TextContent(message.Message)
        ]);

        var response = await _chat.GetChatMessageContentsAsync(_chatHistory);
        var messageContent = response.First().Content;
        _chatHistory.AddAssistantMessage(messageContent);
        await _relay.HandleMessageAsync(
            new ChatMessage
            {
                From = "assistant",
                Message = response.First().Content ?? ""
            });
    }
    
    public _02ChatHistory(MessageRelay relay)
    {
        CreateKernel();

        _relay = relay;
    }
}