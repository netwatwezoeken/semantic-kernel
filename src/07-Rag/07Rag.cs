using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.Redis;
using OllamaSharp;
using Plumbing;
using StackExchange.Redis;
using ChatMessage = Plumbing.ChatMessage;

namespace _07_Rag;

public class _07Rag : IDemo
{
    public string Name => "07 Rag";
    public string[] Models => ["mxbai-embed-large"];
    public string? DemoQuestion => "onboarding in an Android app that allows users to create a new account. They must verify their email address";
    public string Instruction => "Type a short userstory to get an estimate based on reference data";

    private void CreateKernel()
    {
        var openAiKey = "";
        
        _kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
            .Build();

        var promptyTemplate = File.ReadAllText($"./07-estimator.prompty");
        
        _function = _kernel.CreateFunctionFromPrompty(promptyTemplate);
    }
    
    public async Task Start()
    {
        _relay.OnMessageAsync += HandleChatMessage;
        
        await _relay.HandleMessageAsync(
            new ChatMessage
            {
                From = "instructor",
                Message = "Type a short user story to get an estimate based on reference data"
            });
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
    private readonly VectorStore _store;
    private Kernel _kernel;
    private KernelFunction _function;

    private async Task HandleChatMessage(ChatMessage message)
    {
        if (message.From != "user" && message.From != "instructor") return;
        
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerationService = 
            new OllamaApiClient(new Uri("http://localhost:11434"),"mxbai-embed-large");

        VectorStoreCollection<string, EmbeddedUserStory> collection = await GetCollection();

        var queryEmbedding = await embeddingGenerationService.GenerateAsync(message.Message);
       
        var searchResultItems = await collection.SearchAsync(
            queryEmbedding,
            options: new VectorSearchOptions<EmbeddedUserStory>
            {
                VectorProperty = story => story.Vector
            },
            top: 5).ToListAsync();

        var referenceStories = JsonSerializer.Serialize(searchResultItems.Select(v => 
            new { Key = v.Record.Key, 
                Title = v.Record.Title, 
                Description = v.Record.Description, 
                StoryPoints = v.Record.StoryPoints,
                Score = v.Score
            })
        );
        
        var arguments = new KernelArguments(new OllamaPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            Temperature = 0
        });

        arguments.Add("reference_stories_in_json_format", referenceStories);
        arguments.Add("story_to_estimate", message.Message);

        var result = await _kernel.InvokeAsync(_function, arguments);
        
        await _relay.HandleMessageAsync(
            new ChatMessage
            {
                From = "assistant",
                Message = result.ToString()
            });
    }
    
    private async Task<VectorStoreCollection<string, EmbeddedUserStory>> GetCollection()
    {
        var collection = _store.GetCollection<string, EmbeddedUserStory>("stories");

        await collection.EnsureCollectionExistsAsync();
        return collection;
    }
    
    public _07Rag(MessageRelay relay, IConnectionMultiplexer connectionMux)
    {
        var database = connectionMux.GetDatabase();
        _store = new RedisVectorStore(database, new RedisVectorStoreOptions()
        {
            StorageType = RedisStorageType.HashSet
        });
        CreateKernel();
        _relay = relay;
    }
}