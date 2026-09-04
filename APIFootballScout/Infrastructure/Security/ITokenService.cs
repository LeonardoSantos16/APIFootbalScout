using System.Security.Claims;

namespace APIFootballScout.Infrastructure.Security
{
    public interface ITokenService
    {
        TokenResult GenerateTokens(TokenSubject usuario);
        string GenerateRefreshToken();
        Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token, CancellationToken cancellationToken = default);
    }
}
