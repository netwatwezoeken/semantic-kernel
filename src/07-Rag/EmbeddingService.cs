using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Redis;
using OllamaSharp;
using StackExchange.Redis;

namespace _07_Rag;

public class EmbeddingService(ConnectionMultiplexer connectionMux)
{
    public async Task Prepare(){
        var embeddingGenerationService = CreateTextEmbeddingGenerationService();
                
        var collection = await GetCollection();
        
        using var reader = new StreamReader("./issues.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var userStories = csv.GetRecords<UserStory>().ToList();
        
        foreach (var userStory in userStories)
        {
            Console.WriteLine(userStory.title);
            var vectorStory = new EmbeddedUserStory(userStory)
            {
                Vector = (await embeddingGenerationService.GenerateAsync(
                    userStory.title + userStory.description)).Vector
            };
            
            await collection.UpsertAsync(vectorStory);
        }
    }

    private static IEmbeddingGenerator<string, Embedding<float>> CreateTextEmbeddingGenerationService() =>
        new OllamaApiClient(new Uri("http://localhost:11434")
            , "mxbai-embed-large");
    
    private async Task<VectorStoreCollection<string, EmbeddedUserStory>> GetCollection()
    {
        var memoryStore = GetStore();
        var collection = memoryStore.GetCollection<string, EmbeddedUserStory>("stories");

        await collection.EnsureCollectionExistsAsync();
        return collection;
    }
    
    public VectorStore GetStore()
    {
        var database = connectionMux.GetDatabase();
        return new RedisVectorStore(database, new RedisVectorStoreOptions()
        {
            StorageType = RedisStorageType.HashSet
        });
    }
}