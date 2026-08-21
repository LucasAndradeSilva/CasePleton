var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CasePletonNews_API>("casepletonnews-api");

builder.Build().Run();
