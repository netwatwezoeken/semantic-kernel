using AppHost;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var runOllamaInDocker = builder.Configuration.GetValue<bool>("RunOllamaInDocker");

IResourceBuilder<OllamaResource>? ollama = null;
IResourceBuilder<Resource> gemma;
IResourceBuilder<Resource> mini;
IResourceBuilder<Resource> llama;
IResourceBuilder<Resource> mistral;
IResourceBuilder<Resource> deepseek;
IResourceBuilder<Resource> embed;

if (runOllamaInDocker) {
    ollama = builder.AddOllama("ollama")
        // Pick your poison
        //.WithGPUSupport(OllamaGpuVendor.AMD)
        //.WithGPUSupport(OllamaGpuVendor.Nvidia)
        .WithDataVolume("ollama-models")
        .WithLifetime(ContainerLifetime.Persistent);
    gemma = ollama.AddModel("gemma3:4b");
    mini = ollama.AddModel("minicpm-v");
    llama = ollama.AddModel("llama3.2:3b");
    mistral = ollama.AddModel("mistral:7b");
    deepseek = ollama.AddModel("deepseek-r1:1.5b");
    embed = ollama.AddModel("mxbai-embed-large");
} else 
{
    var group = builder.AddGroup("models");
    gemma = builder.AddExecutable("gemma3-4b", "ollama", ".", "pull", "gemma3:4b").InGroup(group);
    mini = builder.AddExecutable("minicpm-v", "ollama", ".", "pull", "minicpm-v").InGroup(group);
    llama = builder.AddExecutable("llama3-2-3b", "ollama", ".", "pull", "llama3.2:3b").InGroup(group);
    mistral = builder.AddExecutable("mistral-7b", "ollama", ".", "pull", "mistral:7b").InGroup(group);
    deepseek = builder.AddExecutable("deepseek-r1-1-5b", "ollama", ".", "pull", "deepseek-r1:1.5b").InGroup(group);
    embed = builder.AddExecutable("mxbai-embed-large", "ollama", ".", "pull", "mxbai-embed-large").InGroup(group);
}

builder.AddProject<Projects._05b_MCPServer>("MCPServer");

var redis = builder.AddRedisStack("Redis")
    .WithDataVolume("redis-rag-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRedisCommander();

var ragPrepare = builder.AddProject<Projects._07_Rag>("RagPrepare")
    .WaitFor(redis)
    .WaitFor(embed)
    .WithArgs("--prepare")
    .WithEnvironment(i =>
    {
        i.EnvironmentVariables.Add("Ollama_Url", OllamaEndpoint());
        i.EnvironmentVariables.Add("Redis", redis.Resource.ConnectionStringExpression);
    });

var mainProject = builder.AddProject<Projects.WebUI>("WebUI");

if (runOllamaInDocker)
{
    mainProject.WithReference(ollama)
        .WaitFor(gemma)
        .WaitFor(mini)
        .WaitFor(llama)
        .WaitFor(mistral)
        .WaitFor(deepseek)
        .WaitFor(embed);
} else {
    mainProject.WaitForCompletion(gemma)
        .WaitForCompletion(mini)
        .WaitForCompletion(llama)
        .WaitForCompletion(mistral)
        .WaitForCompletion(deepseek)
        .WaitForCompletion(embed);
}

mainProject.WaitFor(redis)
    .WithReference(redis)
    // Enable the line below to have the webUI wait for the RAG preparation to complete
    //.WaitForCompletion(ragPrepare) 
    .WithEnvironment(i =>
    {
        i.EnvironmentVariables.Add("Ollama_Url", OllamaEndpoint());
    });

builder.Build().Run();
return;

string OllamaEndpoint()
{
    return ollama?.GetEndpoint("http").Url ?? "http://localhost:11434";
}