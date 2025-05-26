var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.WebUI>("WebUI");
builder.Build().Run();