using APIFootballScout;
using APIFootballScout.Application;
using APIFootballScout.Application.Acompanhamento;
using APIFootballScout.Application.Configuration;
using APIFootballScout.Application.RelatorioScouting;
using APIFootballScout.Application.User;
using APIFootballScout.Domain.Acompanhamento.Services;
using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.Repository;
using APIFootballScout.Infrastructure.Context;
using APIFootballScout.Infrastructure.External;
using APIFootballScout.Infrastructure.Persistence.Repositories;
using APIFootballScout.Infrastructure.Security;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.Acl;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MongoDB.Driver;
using Refit;
using System.Text;


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

builder.Services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateOnStart();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>, IHostEnvironment>((bearer, jwtOptions, environment) =>
    {
        var jwt = jwtOptions.Value;

        bearer.MapInboundClaims = false;
        bearer.RequireHttpsMetadata = !environment.IsDevelopment();
        bearer.SaveToken = true;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = TokenService.RoleClaimType
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<IPasswordHasher>(new BCryptPasswordHasher(builder.Configuration));
builder.Services.AddScoped<IUserRepository, UserRepositoryMongo>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepositoryMongo>();
builder.Services.AddScoped<AuthSessionIssuer>();
builder.Services.AddScoped<SignUpUserUseCase>();
builder.Services.AddScoped<SignInUserUseCase>();
builder.Services.AddScoped<RefreshTokenUseCase>();
builder.Services.AddScoped<SignOutUserUseCase>();
builder.Services.AddScoped<ChangePasswordUseCase>();
builder.Services.AddScoped<DeleteUserUseCase>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<AbrirRascunhoRelatorioUseCase>();
builder.Services.AddScoped<EditarRascunhoRelatorioUseCase>();
builder.Services.AddScoped<FinalizarRelatorioUseCase>();
builder.Services.AddScoped<CorrigirRelatorioUseCase>();
builder.Services.AddScoped<ObterRelatorioUseCase>();
builder.Services.AddScoped<ListarRelatoriosDoJogadorUseCase>();


builder.Services.AddScoped<ISofascorePlayerReader, SofascorePlayerReader>();
builder.Services.AddScoped<ISofascoreTournamentReader, SofascoreTournamentReader>();

builder.Services.AddScoped(sp => new AbrirAcompanhamentoUseCase(
    sp.GetRequiredService<IAcompanhamentoRepository>(),
    sp.GetRequiredService<IOptions<ScoutConfig>>().Value.LimiteObservacoesJogadores,
    sp.GetRequiredService<IAcompanhamentoService>(),
    sp.GetRequiredService<ICatalogoDeJogador>()
));

builder.Services.AddScoped(sp => new ConsultarMudancaAcompanhamentoUseCase(
    sp.GetRequiredService<IAcompanhamentoRepository>(),
    sp.GetRequiredService<ICatalogoDeJogador>(),
    new AferidorDeMudanca(
        sp.GetRequiredService<ScoutSpecificationFactory>().MudancaRelevante(),
        new LeiturasComparaveisSpecification())
    ));

builder.Services.AddScoped<IRelatorioRepository, RelatorioRepositoryMongo>();
builder.Services.AddScoped<IAcompanhamentoRepository, AcompanhamentoRepositoryMongo>();
builder.Services.AddScoped<IAcompanhamentoService, AcompanhamentoService>();
builder.Services.AddScoped<ICatalogoDeJogador, FonteDeDadosSofascore>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Informe apenas o access token; o prefixo Bearer e adicionado automaticamente.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
    });
});

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
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandlerGlobal>();
var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
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
