using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Examples.BasicChat;

public class BasicChat : IExample 
{
    public async Task<string> Execute()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
            .Build();

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var result = await chat.GetChatMessageContentsAsync("Which band invented metal? Just give the band name, no explanation.");

        return result.First().Content ?? "";
    }
}