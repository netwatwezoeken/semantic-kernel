using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.Redis;
using OllamaSharp;
using Plumbing;
using StackExchange.Redis;
using ChatMessage = Plumbing.ChatMessage;

namespace _07_Rag;

public class _07Rag : AbstractDemo
{
    public _07Rag(MessageRelay relay, IConnectionMultiplexer multiplexer) : base(relay)
    {
        Name = "07 Rag";
        DemoQuestion = "Onboarding in an Android app that allows users to create a new account. They must verify their email address";
        Instruction = "Type a short userstory to get an estimate based on reference data";
        
        // The estimation LLM
        _kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
            .Build();
        var promptyTemplate = File.ReadAllText($"./07-estimator.prompty");
        _function = _kernel.CreateFunctionFromPrompty(promptyTemplate);
        
        // Emmbedding LLM
        _embeddingGenerationService = new OllamaApiClient(new Uri("http://localhost:11434"),"mxbai-embed-large");
        
        // Vector store
        var database = multiplexer.GetDatabase();
        _store = new RedisVectorStore(database, new RedisVectorStoreOptions()
        {
            StorageType = RedisStorageType.HashSet
        });
    }
    
    protected override async Task<string> OnHandleUserMessage(ChatMessage message)
    {
        VectorStoreCollection<string, EmbeddedUserStory> collection = await GetCollection();

        var queryEmbedding = await _embeddingGenerationService.GenerateAsync(message.Message);
       
        var searchResultItems = await collection.SearchAsync(
            queryEmbedding,
            options: new VectorSearchOptions<EmbeddedUserStory>
            {
                VectorProperty = story => story.Vector
            },
            top: 5).ToListAsync();
        
        var referenceStories = StoriesToJson(searchResultItems);
        
        var arguments = new KernelArguments(new OllamaPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            Temperature = 0
        });

        arguments.Add("reference_stories_in_json_format", referenceStories);
        arguments.Add("story_to_estimate", message.Message);

        var result = await _kernel.InvokeAsync(_function, arguments);
        return result.ToString();
    }

    private readonly VectorStore _store;
    private readonly Kernel _kernel;
    private readonly KernelFunction _function;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerationService;
    
    private async Task<VectorStoreCollection<string, EmbeddedUserStory>> GetCollection()
    {
        var collection = _store.GetCollection<string, EmbeddedUserStory>("stories");

        await collection.EnsureCollectionExistsAsync();
        return collection;
    }

    private static string StoriesToJson(List<VectorSearchResult<EmbeddedUserStory>> searchResultItems)
    {
        return JsonSerializer.Serialize(searchResultItems.Select(v => 
            new { Key = v.Record.Key, 
                Title = v.Record.Title, 
                Description = v.Record.Description, 
                StoryPoints = v.Record.StoryPoints,
                Score = v.Score
            })
        );
    }
}