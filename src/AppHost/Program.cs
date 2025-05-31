var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.WebUI>("WebUI");
builder.AddProject<Projects._05b_MCPServer>("MCPServer");
builder.Build().Run();