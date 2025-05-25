using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Plumbing;

namespace _06_AgentFramework;

public class _06AgentFramework : IDemo
{
    public string Name => "06 AgentFramework";
    public string[] Models => ["deepseek-r1:1.5b"];
    public string? DemoQuestion => "An eco-friendly stainless steel water bottle that keeps drinks cold for 24 hour";
    private const int ResultTimeoutInSeconds = 3000;
    
    private void CreateKernel()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("llama3.1", new Uri("http://localhost:11434"))
            .Build();

        ChatCompletionAgent analystAgent =
            new () {
                Name = "Analyst",
                Instructions =
                """
                You are a marketing analyst. Given a product description, identify:
                - Key features
                - Target audience
                - Unique selling points
                """,
                Description = "A agent that extracts key concepts from a product description.",
                Kernel = kernel.Clone()
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
                    give format and make it polished. Output the final improved copy as a single text block.
                    """,
                Description = "An agent that formats and proofreads the marketing copy.",
                Kernel = kernel.Clone()
            };

        // Create a monitor to capturing agent responses (via ResponseCallback)
        // to display at the end of this sample. (optional)
        // NOTE: Create your own callback to capture responses in your application or service.
        //OrchestrationMonitor monitor = new();
        // Define the orchestration
        _orchestration =
            new(analystAgent, writerAgent, editorAgent)
            {
                ResponseCallback = ResponseCallback,
                LoggerFactory = LoggerFactory.Create(builder =>
                {
                    builder
                        .AddConsole()
                        .SetMinimumLevel(LogLevel.Information);
                })
            };
         _runtime = new InProcessRuntime();
    }
    
    private async ValueTask ResponseCallback(ChatMessageContent response)
    {
        Console.WriteLine($"\n# INTERMEDIATE: {response.Content}");
        await _relay.HandleMessageAsync(
            new ChatMessage
            {
                From = response.AuthorName ?? "assistant",
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
    private SequentialOrchestration _orchestration;
    private InProcessRuntime _runtime;

    private async Task HandleChatMessage(ChatMessage message)
    {
        _runtime = new();
        await _runtime.StartAsync();
        if (message.From != "user") return;
        Console.WriteLine($"\n# INPUT: {message.Message}\n");
        OrchestrationResult<string> result = await _orchestration.InvokeAsync(message.Message, _runtime);
        string text = await result.GetValueAsync(TimeSpan.FromSeconds(ResultTimeoutInSeconds));
        Console.WriteLine($"\n# RESULT: {text}");
        await _runtime.RunUntilIdleAsync();
    }
    
    public _06AgentFramework(MessageRelay relay)
    {
        CreateKernel();

        _relay = relay;
    }
}