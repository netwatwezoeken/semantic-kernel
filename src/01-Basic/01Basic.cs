using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Plumbing;

namespace _01_Basic;

public class _01Basic : AbstractDemo
{
    public _01Basic(MessageRelay relay, OllamaConfig ollamaConfig) : base(relay)
    {
        _chat = CreateChat(ollamaConfig.Uri);
        Name = "01 Basic";
        DemoQuestion = "Which band invented metal? Just give the band name, no explanation.";
        Instruction = "Type your question.";
    }
    
    private static IChatCompletionService CreateChat(Uri ollamaUri)
    {
        var kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("gemma3:4b", ollamaUri)
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