using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SharedStuff;

var kernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("minicpm-v", new Uri("http://localhost:11434"))
    .Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();

chatHistory.AddSystemMessage(
    """
    You are tasked with reading temperature from a picture. 
    This is temperature in Celsius. 
    The number is a whole number, no decimals.
    Only respond which number is displayed.
    Give only the number in your response.
    """
);
var fileBytes = await File.ReadAllBytesAsync($"./00010-28.jpg");

var initialQuestion = DemoUtil.SimulateTyping("Just return the number without any symbols");

chatHistory.AddUserMessage([
    new TextContent(initialQuestion),
    new ImageContent(fileBytes, "image/jpeg")
]);

var chatResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
Console.WriteLine("LLM: " + chatResult.Content);