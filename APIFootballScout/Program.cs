using Aspire.StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using Refit;
using APIFootballScout.Configuration;
using APIFootballScout.Infrastructure.Context;
using APIFootballScout.Infrastructure.External;
using APIFootballScout.Application.UseCases.Interfaces;
using APIFootballScout.Application.UseCases.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMongoDBClient("mongodb");
// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMongoDB(builder.Configuration.GetConnectionString("scoutdb") ?? throw new InvalidOperationException(), "scoutdb"));

builder.AddRedisClient("cache");
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IPlayerUseCase, PlayerUseCase>();
builder.Services.AddScoped<ITournamentUseCase, TournamentUseCase>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("cache");
    options.InstanceName = "APIFootballScout_";
});

builder.Services.AddRefitClient<ISofascoreClient>().ConfigureHttpClient(c =>
{
    c.BaseAddress = new Uri("https://sofascore.p.rapidapi.com");
    c.DefaultRequestHeaders.Add("X-RapidAPI-Key", builder.Configuration["SofaScore:ApiKey"]);
    c.DefaultRequestHeaders.Add("X-RapidAPI-Host", "sofascore.p.rapidapi.com");
});

builder.Services.Configure<ScoutConfig>(builder.Configuration.GetSection("ScoutConfig"));

var app = builder.Build();


app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapControllers();

app.Run();
