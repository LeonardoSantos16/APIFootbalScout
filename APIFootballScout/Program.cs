using APIFootballScout.Context;
using APIFootballScout.Services.External;
using Microsoft.EntityFrameworkCore;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMongoDBClient("mongodb");
builder.AddRedisClient("cache");
// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMongoDB(builder.Configuration.GetConnectionString("scoutdb") ?? throw new InvalidOperationException(), "scoutdb"));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddRefitClient<ISofascoreClient>().ConfigureHttpClient(c =>
{
    c.BaseAddress = new Uri("rapidapi.com");
    c.DefaultRequestHeaders.Add("X-RapidAPI-Key", builder.Configuration["SofaScore:ApiKey"]);
    c.DefaultRequestHeaders.Add("X-RapidAPI-Host", "sofascore.p.rapidapi.com");
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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