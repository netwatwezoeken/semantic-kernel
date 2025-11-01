public sealed class RedisStackResource(string name) : RedisResource(name), IResourceWithConnectionString
{
    internal const string EndpointName = "tcp";
    
    private EndpointReference? _tcpReference;

    public EndpointReference Endpoint =>
        _tcpReference ??= new(this, EndpointName);
    
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{Endpoint.Property(EndpointProperty.HostAndPort)}"
        );
}
