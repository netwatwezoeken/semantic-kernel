using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using Plumbing;

namespace _05a_MCPClient;

public class _05MCPClient : AbstractDemo
{
    public _05MCPClient(MessageRelay relay) : base(relay)
    {
        Name = "05 MCPClient";
        DemoQuestion = "Play the title song of the second album of the band that invented metal.";
        Instruction = "Ask a for a song to be played";
        
        _kernelBuilder = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("mistral:7b", new Uri("http://localhost:11434"));
    }

    protected override async Task BeforeStart()
    {
        var transport = new SseClientTransport(new SseClientTransportOptions
        {
            Endpoint = new Uri("http://localhost:3001"),
            UseStreamableHttp = true,
        });
        var mcpClient = await McpClientFactory.CreateAsync(transport);
        var tools = await mcpClient.ListToolsAsync();

        _settings = new OllamaPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            Temperature = 0
        };

        _kernel = _kernelBuilder.Build();
        _kernel.Plugins.AddFromFunctions("McpTools", tools.Select(f => f.AsKernelFunction()));
        _chat = _kernel.GetRequiredService<IChatCompletionService>();
        _chatHistory = new ChatHistory();
    }

    protected override async Task<string> OnHandleUserMessage(ChatMessage message)
    {
        if(_chatHistory is null || _chat is null) return "";
        _chatHistory.AddUserMessage([
            new TextContent(message.Message)
        ]);
        var result = await _chat.GetChatMessageContentsAsync(_chatHistory, _settings, _kernel);
        var messageContent = result.First().Content ?? "";
        _chatHistory.AddAssistantMessage(messageContent);
        return messageContent;
    }
    
    private Kernel? _kernel;
    private IChatCompletionService? _chat;
    private readonly IKernelBuilder _kernelBuilder;
    private ChatHistory? _chatHistory;
    private OllamaPromptExecutionSettings? _settings;
}