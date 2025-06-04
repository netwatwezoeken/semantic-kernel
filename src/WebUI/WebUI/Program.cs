using _01_Basic;
using _02_ChatHistory;
using _03_Templating;
using _04_Functions;
using _05a_MCPClient;
using _06_AgentFramework;
using _07_Rag;
using Microsoft.AspNetCore.Mvc;
using MudBlazor.Services;
using Plumbing;
using StackExchange.Redis;
using WebUI;
using WebUI.Components;

var builder = WebApplication.CreateBuilder(args);
// Add MudBlazor services
builder.Services.AddMudServices();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

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

builder.Services.AddTransient<IDemo, _01Basic>();
builder.Services.AddTransient<IDemo, _02ChatHistory>();
builder.Services.AddTransient<IDemo, _03Templating>();
builder.Services.AddTransient<IDemo, _04Functions>();
builder.Services.AddTransient<IDemo, _05MCPClient>();
builder.Services.AddTransient<IDemo, _06AgentFramework>();
builder.Services.AddTransient<IDemo, _07Rag>();
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
builder.Services.AddSingleton<_07RagPrepare>();
builder.Services.AddSingleton<_07Rag>()
    .AddSingleton<IDemo, _07Rag>(p => p.GetRequiredService<_07Rag>());
builder.Services.AddSingleton<DemoSelector>();
builder.Services.AddSingleton<MessageRelay>();
builder.Services.AddSignalR();

builder.ConfigureOpenTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();
app.MapHub<ChatHub>("/chathub");
app.MapGet("/prepare07", ([FromServices] _07RagPrepare rag) => rag.Prepare());
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(WebUI.Client._Imports).Assembly);

app.Run();