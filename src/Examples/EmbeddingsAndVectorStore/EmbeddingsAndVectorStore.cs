using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Redis;
using Microsoft.SemanticKernel.Embeddings;
using OllamaSharp;
using StackExchange.Redis;

namespace Examples.EmbeddingsAndVectorStore;

public class EmbeddingsAndVectorStore : IExample
{
    public async Task<string> Execute()
    {
        ITextEmbeddingGenerationService textEmbeddingGenerationService = CreateTextEmbeddingGenerationService();

        IVectorStoreRecordCollection<string, EmbeddedUserStory> collection = await GetCollection();

        var queryEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(
            "onboarding in an Android app that allows users to create a new account. The must verify their email address");
        
        var searchResultItems = await collection.SearchEmbeddingAsync(
            queryEmbedding,
            options: new VectorSearchOptions<EmbeddedUserStory>
            {
                VectorProperty = story => story.Vector
            },
            top: 5).ToListAsync();

        foreach (var result in searchResultItems)
        {
            Console.WriteLine($"Match on {result.Record.Title}  --- {result.Record.Description}");
        }
        
        return "";
    }

    public async Task Prepare(){
        var textEmbeddingGenerationService = CreateTextEmbeddingGenerationService();
                
        var collection = await GetCollection();
        
        using var reader = new StreamReader("./EmbeddingsAndVectorStore/issues.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var userStories = csv.GetRecords<UserStory>().ToList();
        
        foreach (var userStory in userStories)
        {
            Console.WriteLine(userStory.title);
            var vectorStory = new EmbeddedUserStory(userStory);
            vectorStory.Vector =
                await textEmbeddingGenerationService.GenerateEmbeddingAsync(userStory.title + userStory.description);
            await collection.UpsertAsync(vectorStory);
        }
    }
    
    private static ITextEmbeddingGenerationService CreateTextEmbeddingGenerationService() =>
        new OllamaApiClient(new Uri("http://localhost:11434")
                , "mxbai-embed-large")
            .AsTextEmbeddingGenerationService();
    
    private async Task<IVectorStoreRecordCollection<string, EmbeddedUserStory>> GetCollection()
    {
        var memoryStore = GetStore();
        var collection = memoryStore.GetCollection<string, EmbeddedUserStory>("stories");

        await collection.CreateCollectionIfNotExistsAsync();
        return collection;
    }
    
    public IVectorStore GetStore()
    {
        var rconfig = new ConfigurationOptions
        {
            EndPoints =
            {
                "localhost:6379"
            },
            AbortOnConnectFail = false,
            ConnectRetry = 10,
            ReconnectRetryPolicy = new ExponentialRetry(5000),
            ClientName = "ApiClient"
        };
            
        var multiplexer = ConnectionMultiplexer.Connect(rconfig);
        var database = multiplexer.GetDatabase();
        return new RedisVectorStore(database, new RedisVectorStoreOptions()
        {
            StorageType = RedisStorageType.HashSet
        });
    }
}