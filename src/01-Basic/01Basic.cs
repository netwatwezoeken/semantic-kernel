using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Plumbing;

namespace _01_Basic;

public class _01Basic : AbstractDemo
{
    public _01Basic(MessageRelay relay) : base(relay)
    {
        _chat = CreateChat();
        Name = "01 Basic";
        DemoQuestion = "Which band invented metal? Just give the band name, no explanation.";
        Instruction = "Type your question.";
    }
    
    private static IChatCompletionService CreateChat()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
            .Build();

        return kernel.GetRequiredService<IChatCompletionService>();
    }

    protected override async Task<string> OnHandleUserMessage(ChatMessage message)
    {   
        var response = await _chat.GetChatMessageContentsAsync(message.Message);
        return response[0].Content ?? "";
    }
    
    private readonly IChatCompletionService _chat;
}