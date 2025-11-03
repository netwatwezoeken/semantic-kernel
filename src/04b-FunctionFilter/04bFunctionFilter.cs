using _04_Functions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Plumbing;

namespace _04b_FunctionFilter;

public class _04bFunctionFilter : AbstractDemo
{
    public _04bFunctionFilter(MessageRelay relay, OllamaConfig ollamaConfig) : base(relay)
    {
        Name = "04b FunctionFilter";
        DemoQuestion = "Play a song by Nickelback.";
        Instruction = "Ask a for a song to be played";
        
        var kernelBuilder = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("llama3.2:3b", ollamaConfig.Uri);
            
        kernelBuilder.Plugins.AddFromType<MusicPlayerPlugin>("PlaySong");
        
        _kernel  = kernelBuilder.Build();
        _kernel.FunctionInvocationFilters.Add(new NickelbackFilter());
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