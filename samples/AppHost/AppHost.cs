var builder = DistributedApplication.CreateBuilder(args);

//builder.

builder.AddProject<Projects.DotNetRestAPI>("dotnetrestapi");

builder.Build().Run();
