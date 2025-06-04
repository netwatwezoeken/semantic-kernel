using System.Globalization;
using System.Text.Json;
using CsvHelper;
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

public class _07RagPrepare
{
    public _07RagPrepare(IConnectionMultiplexer connectionMux)
    {
        var database = connectionMux.GetDatabase();
        _store = new RedisVectorStore(database, new RedisVectorStoreOptions()
        {
            StorageType = RedisStorageType.HashSet
        });
    }
    
    public async Task Prepare(){
        var embeddingGenerationService = CreateTextEmbeddingGenerationService();
                
        var collection = await GetCollection();
        
        using var reader = new StreamReader("./issues.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var userStories = csv.GetRecords<UserStory>().ToList();
        
        foreach (var userStory in userStories)
        {
            Console.WriteLine(userStory.title);
            var embedding = (await embeddingGenerationService.GenerateAsync(
                userStory.title + userStory.description));
            
            var vectorStory = new EmbeddedUserStory(userStory)
            {
                Vector = embedding.Vector
            };
            
            await collection.UpsertAsync(vectorStory);
        }
    }
    
    private readonly VectorStore _store;

    private static IEmbeddingGenerator<string, Embedding<float>> CreateTextEmbeddingGenerationService() =>
        new OllamaApiClient(new Uri("http://localhost:11434")
            , "mxbai-embed-large");
    
    private async Task<VectorStoreCollection<string, EmbeddedUserStory>> GetCollection()
    {
        var collection = _store.GetCollection<string, EmbeddedUserStory>("stories");

        await collection.EnsureCollectionExistsAsync();
        return collection;
    }
}