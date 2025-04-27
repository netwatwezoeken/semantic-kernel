using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Examples.ChatWithHistory;

public class ChatWithSystemPrompt : IExample 
{
    public async Task<string> Execute()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
            .Build();

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(
            """
            You are a helpful metal music expert.
            Your task is to provide the best music advice and facts about metal music for the user.
            Only metal music of course. Hell yeah! 🤘
            """
        );
        chatHistory.AddUserMessage([
            new TextContent("Which band invented metal? Just give the band name, no explanation.")
        ]);
        var chatResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        
        return chatResult.Content ?? "";
    }
}