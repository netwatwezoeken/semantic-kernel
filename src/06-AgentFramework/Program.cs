using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;

namespace _06_AgentFramework;

public static class Program
{
    public const int ResultTimeoutInSeconds = 30;
    public static async Task Main()
    {
        Console.WriteLine("Creating kernel...");

        var kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("deepseek-r1:1.5b", new Uri("http://localhost:11434"))
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
        OrchestrationMonitor monitor = new();
        // Define the orchestration
        SequentialOrchestration orchestration =
            new(analystAgent, writerAgent, editorAgent)
            {
                ResponseCallback = monitor.ResponseCallback,
                LoggerFactory = LoggerFactory.Create(builder =>
                {
                    builder
                        .AddConsole()
                        .SetMinimumLevel(LogLevel.Information);
                })
            };

        // Start the runtime
        InProcessRuntime runtime = new();
        await runtime.StartAsync();

        // Run the orchestration
        string input = "An eco-friendly stainless steel water bottle that keeps drinks cold for 24 hours";
        Console.WriteLine($"\n# INPUT: {input}\n");
        OrchestrationResult<string> result = await orchestration.InvokeAsync(input, runtime);
        string text = await result.GetValueAsync(TimeSpan.FromSeconds(ResultTimeoutInSeconds));
        Console.WriteLine($"\n# RESULT: {text}");

        await runtime.RunUntilIdleAsync();

        Console.WriteLine("\n\nORCHESTRATION HISTORY");
        foreach (ChatMessageContent message in monitor.History)
        {
            WriteAgentChatMessage(message);
            //Console.WriteLine(message);
        }
    }

    public static void WriteAgentChatMessage(ChatMessageContent message)
    {
        // Include ChatMessageContent.AuthorName in output, if present.
        string authorExpression = message.Role == AuthorRole.User ? string.Empty : $" - {message.AuthorName ?? "*"}";
        // Include TextContent (via ChatMessageContent.Content), if present.
        string contentExpression = string.IsNullOrWhiteSpace(message.Content) ? string.Empty : message.Content;
        bool isCode = false;
        string codeMarker = isCode ? "\n  [CODE]\n" : " ";
        Console.WriteLine($"\n# {message.Role}{authorExpression}:{codeMarker}{contentExpression}");

        // Provide visibility for inner content (that isn't TextContent).
        foreach (KernelContent item in message.Items)
        {
            if (item is AnnotationContent annotation)
            {
                if (annotation.Kind == AnnotationKind.UrlCitation)
                {
                    Console.WriteLine($"  [{item.GetType().Name}] {annotation.Label}: {annotation.ReferenceId} - {annotation.Title}");
                }
                else
                {
                    Console.WriteLine($"  [{item.GetType().Name}] {annotation.Label}: File #{annotation.ReferenceId}");
                }
            }
            else if (item is FileReferenceContent fileReference)
            {
                Console.WriteLine($"  [{item.GetType().Name}] File #{fileReference.FileId}");
            }
            else if (item is ImageContent image)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {image.Uri?.ToString() ?? image.DataUri ?? $"{image.Data?.Length} bytes"}");
            }
            else if (item is FunctionCallContent functionCall)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {functionCall.Id}");
            }
            else if (item is FunctionResultContent functionResult)
            {
                Console.WriteLine($"  [{item.GetType().Name}] {functionResult.CallId} - {functionResult.Result ?? "*"}");
            }
        }
    }
}

public class ConsoleThing : ILoggerProvider
{
    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new LoggerThing();
    }
}

public class LoggerThing : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine(formatter(state, exception));
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        //throw new NotImplementedException();
        return null;
    }
}

public sealed class OrchestrationMonitor
{
    public ChatHistory History { get; } = [];

    public ValueTask ResponseCallback(ChatMessageContent response)
    {
        this.History.Add(response);
        Console.WriteLine(response.AuthorName);
        //Console.WriteLine(response.Role);
        Console.WriteLine(response.Content[..Math.Min(50, response.Content.Length)]);
        return ValueTask.CompletedTask;
    }
}