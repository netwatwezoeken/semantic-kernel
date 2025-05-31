using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Plumbing;

namespace _04_Functions;

public class _04Functions : AbstractDemo
{
    public _04Functions(MessageRelay relay) : base(relay)
    {
        Name = "04 Functions";
        DemoQuestion = "Play the title song of the second album of the band that invented metal.";
        Instruction = "Ask a for a song to be played";
        
        var kernelBuilder = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("mistral:7b", new Uri("http://localhost:11434"));
            
        kernelBuilder.Plugins.AddFromType<MusicPlayerPlugin>("PlaySong");
        
        _kernel  = kernelBuilder.Build();
        _chat = _kernel.GetRequiredService<IChatCompletionService>();
    }

    protected override async Task<string> OnHandleUserMessage(ChatMessage message)
    {
        var arguments = new OllamaPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            Temperature = 0.8f
        };
        
        var result = await _chat.GetChatMessageContentsAsync(message.Message, arguments, _kernel);
        return result[0].Content ?? "";
    }
    
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
}