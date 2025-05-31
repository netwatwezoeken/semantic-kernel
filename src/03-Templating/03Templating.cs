using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Plumbing;

namespace _03_Templating;

public class _03Templating : AbstractDemo
{
    public _03Templating(MessageRelay relay) : base(relay)
    {
        Name = "03 Templating";
        DemoQuestion = "Which band invented metal?";
        Instruction = "Ask a question about metal music";
        
        _kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
            .Build();
        var promptyTemplate = File.ReadAllText($"./03-music-assistant.prompty");
        
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

        var result = await _kernel.InvokeAsync(_function, arguments);
        return result.ToString();
    }
    
    private readonly Kernel _kernel;
    private readonly KernelFunction _function;
}