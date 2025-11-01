public static class RedisStackResourceBuilderExtensions
{
    public static IResourceBuilder<RedisStackResource> AddRedisStack(
        this IDistributedApplicationBuilder builder,
        string name,
        int? tcpPort = null)
    {
        var resource = new RedisStackResource(name);

        return builder.AddResource(resource)
            .WithImage("redis/redis-stack-server")
            .WithImageRegistry("docker.io")
            .WithImageTag("latest")
            .WithEndpoint(
                targetPort: 6379,
                port: tcpPort,
                name: RedisStackResource.EndpointName)
            .WithEnvironment("REDIS_ARGS", "--save 60 1 --bind 0.0.0.0 --maxmemory 512mb --protected-mode no");
    }
}
