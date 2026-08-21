using APIFootballScout.Application;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.Acl;
using MongoDB.Driver;
using APIFootballScout.Application.Acompanhamento;
using APIFootballScout.Application.Configuration;
using APIFootballScout.Domain.Acompanhamento.Services;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.Repository;
using APIFootballScout.Infrastructure.Context;
using APIFootballScout.Infrastructure.External;
using APIFootballScout.Infrastructure.Persistence.Repositories;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Refit;


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

builder.Services.AddScoped<ISofascorePlayerReader, SofascorePlayerReader>();
builder.Services.AddScoped<ISofascoreTournamentReader, SofascoreTournamentReader>();

builder.Services.AddScoped(sp => new AbrirAcompanhamentoUseCase(
    sp.GetRequiredService<IAcompanhamentoRepository>(),
    sp.GetRequiredService<IOptions<ScoutConfig>>().Value.LimiteObservacoesJogadores,
    sp.GetRequiredService<IAcompanhamentoService>(),
    sp.GetRequiredService<ICatalogoDeJogador>()
));

builder.Services.AddScoped<IAcompanhamentoRepository, AcompanhamentoRepositoryMongo>();
builder.Services.AddScoped<IAcompanhamentoService, AcompanhamentoService>();
builder.Services.AddScoped<ICatalogoDeJogador, FonteDeDadosSofascore>();
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
builder.Services.AddSingleton<ScoutSpecificationFactory>();
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

    // R1.2 - indice unico parcial em (olheiro_id, jogador_id) restrito a status Ativo.
    var mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
    await AcompanhamentoRepositoryMongo.GarantirIndicesAsync(mongoClient);
}

app.MapControllers();

app.Run();
