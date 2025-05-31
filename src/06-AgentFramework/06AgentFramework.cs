using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Plumbing;

namespace _06_AgentFramework;

public class _06AgentFramework : AbstractDemo
{
    public _06AgentFramework(MessageRelay relay) : base(relay)
    {
        Name = "06 AgentFramework";
        DemoQuestion = "An eco-friendly stainless steel water bottle that keeps drinks cold for 24 hour.";
        Instruction = "Type a short userstory to get an estimate based on reference data";
        
        var kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("deepseek-r1:1.5b", new Uri("http://localhost:11434"))
            .Build();

        var kernel2 = Kernel.CreateBuilder()
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

    protected override async Task<string> OnHandleUserMessage(ChatMessage message)
    {
        _runtime = new InProcessRuntime();
        await _runtime.StartAsync();
        var result = await _orchestration.InvokeAsync(message.Message, _runtime);
        var text = await result.GetValueAsync(TimeSpan.FromSeconds(ResultTimeoutInSeconds));
        await _runtime.RunUntilIdleAsync();
        return text;
    }
    
    private ValueTask<string> ResultTransform(IList<ChatMessageContent> result, CancellationToken cancellationToken)
    {
        return new ValueTask<string>(result.Last().Content ?? "");
    }

    private async ValueTask ResponseCallback(ChatMessageContent response)
    {
        Console.WriteLine($"\n# INTERMEDIATE: {response.Content}");
        await Relay.HandleMessageAsync(
            new ChatMessage
            {
                From = response.AuthorName ?? response.Role.Label,
                Message = response.Content ?? ""
            });
    }
    
    private AgentOrchestration<string, string> _orchestration;
    private InProcessRuntime _runtime;
    private const int ResultTimeoutInSeconds = 3000;
}