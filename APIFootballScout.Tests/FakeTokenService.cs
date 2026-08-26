using APIFootballScout.Infrastructure.Security;
using System.Security.Claims;

namespace APIFootballScout.Tests
{
    internal class FakeTokenService(TimeProvider time) : ITokenService
    {
        private int _cont;
        public int AccessTokenMinutes { get; set; } = 15;
        public int RefreshTokenDays { get; set; } = 7;
        public TokenSubject? LastSubject { get; private set; }
        public List<TokenResult> Emitidos { get; } = [];
        public string GenerateRefreshToken() => $"rt-{++_cont}";


        public TokenResult GenerateTokens(TokenSubject usuario)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            var agora = time.GetUtcNow().UtcDateTime;

            var resultado = new TokenResult(
                $"at-{_cont + 1}",              
                agora.AddMinutes(AccessTokenMinutes),
                GenerateRefreshToken(),   
                agora.AddDays(RefreshTokenDays));

            LastSubject = usuario;
            Emitidos.Add(resultado);

            return resultado;
        }

        public Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token, CancellationToken ct = default)
        => throw new NotSupportedException("Não usado nos testes de Application/User.");
    }
}
