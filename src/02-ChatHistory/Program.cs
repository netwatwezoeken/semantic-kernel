using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SharedStuff;

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
    Never give an explanation unless explicitly asked.
    """
);

var initialQuestion = DemoUtil.SimulateTyping("Which band invented metal?");

chatHistory.AddUserMessage([
    new TextContent(initialQuestion)
]);

var chatResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
Console.WriteLine("LLM: " + chatResult.Content);