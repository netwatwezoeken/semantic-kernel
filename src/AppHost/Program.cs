var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects._05b_MCPServer>("MCPServer");
builder.AddProject<Projects.WebUI>("WebUI");

builder.Build().Run();