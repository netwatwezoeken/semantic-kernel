using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Plumbing;

namespace _06_AgentFramework;

public class _06AgentFramework : IDemo
{
    public string Name => "06 AgentFramework";
    public string[] Models => ["deepseek-r1:1.5b", "llama3.2:3b"];
    public string? DemoQuestion => "An eco-friendly stainless steel water bottle that keeps drinks cold for 24 hour";
    private const int ResultTimeoutInSeconds = 3000;
    
    private void CreateKernel()
    {
        var openAiKey = "";
        
        var kernel = Kernel.CreateBuilder()
            //.AddOpenAIChatCompletion(modelId: "gpt-4o-mini", apiKey: openAiKey)
            .AddOllamaChatCompletion("deepseek-r1:1.5b", new Uri("http://localhost:11434"))
            .Build();

        var kernel2 = Kernel.CreateBuilder()
            //.AddOpenAIChatCompletion(modelId: "gpt-4o-mini", apiKey: openAiKey)
            .AddOllamaChatCompletion("llama3.2:3b", new Uri("http://localhost:11434"))
            .Build();
        
        ChatCompletionAgent analystAgent =
            new () {
                Name = "analyst",
                Instructions =
                    """
                    You are a marketing analyst. Given a product description, identify:
                    - Key features
                    - Target audience
                    - Unique selling points
                    """,
                Description = "A agent that extracts key concepts from a product description.",
                Kernel = kernel2.Clone()
            };
        ChatCompletionAgent writerAgent =
            new () {
                Name = "copywriter",
                Instructions = 
                    """
                    You are a marketing copywriter. Given a block of text describing features, audience, and USPs,
                    compose a compelling marketing copy (like a newsletter section) that highlights these points.
                    Output should be short (around 150 words), output just the copy as a single text block.
                    """,
                Description =  "An agent that writes a marketing copy based on the extracted concepts.",
                Kernel = kernel.Clone()
            };
        ChatCompletionAgent editorAgent =
            new()
            {
                Name = "editor",
                Instructions =
                    """
                    You are an editor. Given the draft copy, correct grammar, improve clarity, ensure consistent tone,
                    give format, make it shorter and make it polished. Output the final improved copy as a single text block.
                    """,
                Description = "An agent that formats and proofreads the marketing copy.",
                Kernel = kernel.Clone()
            };
        
        _orchestration =
            new CustomSequentialThing<string, string>(analystAgent, writerAgent, editorAgent)
            {
                ResponseCallback = ResponseCallback,
                LoggerFactory = LoggerFactory.Create(builder =>
                {
                    builder
                        .AddConsole()
                        .SetMinimumLevel(LogLevel.Information);
                }),
                ResultTransform = ResultTransform
            };
        
         _runtime = new InProcessRuntime();
    }

    private ValueTask<string> ResultTransform(IList<ChatMessageContent> result, CancellationToken cancellationToken)
    {
        return new ValueTask<string>(result.Last().Content ?? "");
    }

    private async ValueTask ResponseCallback(ChatMessageContent response)
    {
        Console.WriteLine($"\n# INTERMEDIATE: {response.Content}");
        await _relay.HandleMessageAsync(
            new ChatMessage
            {
                From = response.AuthorName ?? response.Role.Label,
                Message = response.Content ?? ""
            });
    }
    
    public async Task Start()
    {
        _relay.OnMessageAsync += HandleChatMessage;
    }

    public async Task Stop()
    {
        try
        {
            await _runtime.StopAsync();
        }
        catch { }

        _relay.OnMessageAsync -= HandleChatMessage;
    }

    private readonly MessageRelay _relay;
    private IChatCompletionService _chat;
    private AgentOrchestration<string, string> _orchestration;
    private InProcessRuntime _runtime;

    private async Task HandleChatMessage(ChatMessage message)
    {
        _runtime = new InProcessRuntime();
        await _runtime.StartAsync();
        if (message.From != "user") return;
        var result = await _orchestration.InvokeAsync(message.Message, _runtime);
        var text = await result.GetValueAsync(TimeSpan.FromSeconds(ResultTimeoutInSeconds));
        Console.WriteLine($"\n#END  RESULT: {text}");
        await _runtime.RunUntilIdleAsync();
    }
    
    public _06AgentFramework(MessageRelay relay)
    {
        CreateKernel();

        _relay = relay;
    }
}