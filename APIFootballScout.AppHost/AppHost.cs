var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.addRedis("cache");

builder.AddProject<Projects.APIFootballScout>("apifootballscout").withReference(cache);

builder.Build().Run();
