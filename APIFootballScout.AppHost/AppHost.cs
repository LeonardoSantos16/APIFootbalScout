var builder = DistributedApplication.CreateBuilder(args);

var mongoServer = builder.AddMongoDB("mongodb");
var mongodb = mongoServer.AddDatabase("scoutdb");

var cache = builder.AddRedis("cache");
var sofaKey = builder.AddParameter("SofaScoreKey", secret: true);
var jwtKey = builder.AddParameter("JwtSigningKey", secret: true);

builder.AddProject<Projects.APIFootballScout>("apifootballscout")
    .WithReference(mongoServer)
    .WithReference(mongodb)
    .WithReference(cache)
    .WaitFor(mongodb)
    .WaitFor(cache)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("SofaScore__ApiKey", sofaKey);

builder.Build().Run();
