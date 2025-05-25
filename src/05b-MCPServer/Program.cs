using _05b_MCPServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using ModelContextProtocol.Server;

HashSet<IMcpServer> subscriptions = [];

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<PlaySongTool>();
var app = builder.Build();

app.MapMcp();

app.Run("http://localhost:3001");