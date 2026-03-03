var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("mongodb")
    .AddDatabase("scoutdb");

var cache = builder.AddRedis("cache");

builder.AddProject<Projects.APIFootballScout>("apifootballscout")
    .WithReference(mongodb)
    .WithReference(cache);

builder.Build().Run();