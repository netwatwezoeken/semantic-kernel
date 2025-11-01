using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Redis;
using OllamaSharp;
using Plumbing;
using StackExchange.Redis;

namespace _07_Rag;

public class EmbeddingService(ConnectionMultiplexer connectionMux)
{
    public async Task Prepare(OllamaConfig ollamaConfig){
        var embeddingGenerationService = CreateTextEmbeddingGenerationService(ollamaConfig);
                
        var collection = await GetCollection();
        
        using var reader = new StreamReader("./issues.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var userStories = csv.GetRecords<UserStory>().ToList();
        var database = connectionMux.GetDatabase();
        var count = (int)database.Execute("DBSIZE");

        if (count > 0)
        {
            Console.WriteLine("Data already exists");
            return;
        }
        Console.WriteLine("Preparing data");
        foreach (var userStory in userStories)
        {
            Console.WriteLine(userStory.title);
            try
            {
                var vectorStory = new EmbeddedUserStory(userStory)
                {
                    Vector = (await embeddingGenerationService.GenerateAsync(
                        userStory.title + userStory.description)).Vector
                };

                await collection.UpsertAsync(vectorStory);
            }
            catch (Exception)
            {
                Console.WriteLine($"Failed to add story '{userStory.title}'");
            }

        }
    }

    private static IEmbeddingGenerator<string, Embedding<float>> CreateTextEmbeddingGenerationService(
        OllamaConfig ollamaConfig) =>
        new OllamaApiClient(ollamaConfig.Uri
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