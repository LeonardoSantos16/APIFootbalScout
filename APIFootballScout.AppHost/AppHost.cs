var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("mongodb")
    .AddDatabase("scoutdb");

var cache = builder.AddRedis("cache");
var sofaKey = builder.AddParameter("SofaScoreKey", secret: true);

builder.AddProject<Projects.APIFootballScout>("apifootballscout")
    .WithReference(mongodb)
    .WithReference(cache)
    .WithEnvironment("SofaScore__ApiKey", sofaKey);

builder.Build().Run();