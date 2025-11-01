var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama")
    .WithDataVolume("ollama-models")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects._05b_MCPServer>("MCPServer");

var redis = builder.AddRedisStack("Redis")
    .WithDataVolume("redis-rag-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRedisCommander();

var embeddingModel = ollama.AddModel("mxbai-embed-large");

var ragPrepare = builder.AddProject<Projects._07_Rag>("RagPrepare")
    .WaitFor(redis)
    .WaitFor(embeddingModel)
    .WithArgs("--prepare")
    .WithEnvironment(i =>
    {
        i.EnvironmentVariables.Add("Ollama_Url", ollama.GetEndpoint("http"));
        i.EnvironmentVariables.Add("Redis", redis.Resource.ConnectionStringExpression);
    });

builder.AddProject<Projects.WebUI>("WebUI")
    .WithReference(ollama)
    .WaitFor(ollama.AddModel("gemma3:4b"))
    .WaitFor(ollama.AddModel("minicpm-v"))
    .WaitFor(ollama.AddModel("llama3.2:3b"))
    .WaitFor(ollama.AddModel("mistral:7b"))
    .WaitFor(ollama.AddModel("deepseek-r1:1.5b"))
    .WaitFor(embeddingModel)
    .WaitFor(redis)
    .WithReference(redis)
    // Enable the line below to have the webUI wait for the RAG preparation to complete
    //.WaitForCompletion(ragPrepare) 
    .WithEnvironment(i =>
    {
        i.EnvironmentVariables.Add("Ollama_Url", ollama.GetEndpoint("http"));
    });

builder.Build().Run();