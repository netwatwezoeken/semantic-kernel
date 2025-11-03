using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Plumbing;

namespace _03b_PromptFilter;

public class _03bPromptFilter : AbstractDemo
{
    public _03bPromptFilter(MessageRelay relay, OllamaConfig ollamaConfig) : base(relay)
    {
        Name = "03b PromptFilter";
        DemoQuestion = "What is Fight Club?";
        Instruction = "Ask a question";
        
        _kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("gemma3:4b", ollamaConfig.Uri)
            .Build();
        var promptyTemplate = File.ReadAllText($"./03b-generic-assistant.prompty");
        _kernel.PromptRenderFilters.Add(new FightClubFilter());
        _function = _kernel.CreateFunctionFromPrompty(promptyTemplate);
    }

    protected override async Task<string> OnHandleUserMessage(ChatMessage message)
    {
        var arguments = new KernelArguments(new OllamaPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            Temperature = 0
        });

        arguments.Add("question", message.Message);

        try
        {
            var result = await _kernel.InvokeAsync(_function, arguments);
            return result.ToString();
        }
        catch (ContentException ex)
        {
            return ex.Message;
        }
    }
    
    private readonly Kernel _kernel;
    private readonly KernelFunction _function;
}