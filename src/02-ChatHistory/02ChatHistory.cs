using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Plumbing;

namespace _02_ChatHistory;

public class _02ChatHistory : AbstractDemo
{
    public _02ChatHistory(MessageRelay relay) : base(relay)
    {
        Name = "02 ChatHistory";
        DemoQuestion = "Which band invented metal?";
        Instruction = "Ask a question about metal music";
        
        var kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
            .Build();
        _chat = kernel.GetRequiredService<IChatCompletionService>();
        
        _chatHistory = new ChatHistory();
        _chatHistory.AddSystemMessage(
            """
            You are a helpful metal music expert.
            Your task is to provide the best music advice and facts about metal music for the user.
            Only metal music of course. Hell yeah! 🤘
            Never give an explanation unless explicitly asked.
            """
        );
    }
    
    protected override async Task<string> OnHandleUserMessage(ChatMessage message)
    {
        _chatHistory.AddUserMessage([
            new TextContent(message.Message)
        ]);

        var result = await _chat.GetChatMessageContentsAsync(_chatHistory);
        var messageContent = result.First().Content ?? "";
        _chatHistory.AddAssistantMessage(messageContent);
        return messageContent;
    }

    private readonly IChatCompletionService _chat;
    private readonly ChatHistory _chatHistory;
}