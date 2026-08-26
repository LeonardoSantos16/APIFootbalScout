using APIFootballScout.Application.User;
using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace APIFootballScout.Tests.Auth
{
    internal sealed class AuthTestContext
    {
        public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 8, 25, 12, 0, 0, TimeSpan.Zero));
        public InMemoryUserRepository Users { get; } = new();
        public InMemoryRefreshTokenRepository Tokens { get; } = new();
        public FakeTokenService TokenService { get; }
        public IPasswordHasher Hasher { get; }
        public AuthSessionIssuer Issuer { get; }

        public AuthTestContext(string pepper = "pepper-de-teste")
        {
            TokenService = new FakeTokenService(Time);
            Hasher = NovoHasher(pepper);
            Issuer = new AuthSessionIssuer(TokenService, Tokens, Time);
        }

        public static IPasswordHasher NovoHasher(string pepper) =>
            new BCryptPasswordHasher(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Password:Pepper"] = pepper })
                .Build());

        public DateTime Agora => Time.GetUtcNow().UtcDateTime;

        public SignUpUserUseCase NovoSignUp() => new(Users, Hasher, Issuer, Time);

        public SignOutUserUseCase NovoSignOut() => new(Tokens, Time);

        public ChangePasswordUseCase NovoChangePassword() => new(Users, Tokens, Hasher, Issuer, Time);

        public RefreshTokenUseCase NovoRefresh() =>
            new(Users, Tokens, Issuer, Time, NullLogger<RefreshTokenUseCase>.Instance);

        public async Task<(UserDocument Usuario, AuthResult Sessao)> SeedUsuarioAsync(
            string email = "leo@mail.com",
            string senha = "senha-atual-123",
            string nome = "Leo")
        {
            var usuario = new UserDocument
            {
                Id = Guid.NewGuid(),
                Email = email,
                Name = nome,
                PasswordHash = Hasher.Hash(senha),
                SecurityStamp = Guid.NewGuid().ToString("N"),
                Roles = ["olheiro"],
                CreatedAtUtc = Agora
            };

            await Users.AdicionarAsync(usuario);
            var sessao = await Issuer.EmitirAsync(usuario);

            return (usuario, sessao);
        }
    }
}
