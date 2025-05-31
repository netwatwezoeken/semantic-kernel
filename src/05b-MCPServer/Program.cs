using _05b_MCPServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using ModelContextProtocol.Server;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

HashSet<IMcpServer> subscriptions = [];

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<PlaySongTool>();

ResourceBuilder resource = ResourceBuilder.CreateDefault().AddService("mcp-server");

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("*").AddHttpClientInstrumentation().SetResourceBuilder(resource))
    .WithMetrics(b => b.AddMeter("*").AddHttpClientInstrumentation().SetResourceBuilder(resource))
    .WithLogging(b => b.SetResourceBuilder(resource))
    .UseOtlpExporter();

var app = builder.Build();

app.MapMcp();

app.Run("http://localhost:3001");