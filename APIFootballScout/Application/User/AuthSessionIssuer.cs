using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Persistence.Repositories;
using APIFootballScout.Infrastructure.Security;

namespace APIFootballScout.Application.User
{
    public sealed class AuthSessionIssuer(
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        TimeProvider timeProvider)
    {
        public Task<AuthResult> EmitirAsync(UserDocument user, CancellationToken cancellationToken = default)
            => GravarAsync(user, GerarTokens(user), cancellationToken);

        public TokenResult GerarTokens(UserDocument user)
            => tokenService.GenerateTokens(new TokenSubject(
                user.Id,
                user.Email,
                user.SecurityStamp,
                user.Roles));

        public async Task<AuthResult> GravarAsync(UserDocument user, TokenResult tokens, CancellationToken cancellationToken = default)
        {
            await refreshTokenRepository.AdicionarAsync(new RefreshTokenDocument
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = RefreshTokenHasher.Hash(tokens.RefreshToken),
                ExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc,
                CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime
            }, cancellationToken);

            return new AuthResult(
                user.Id,
                user.Name,
                user.Email,
                tokens.AccessToken,
                tokens.AccessTokenExpiresAtUtc,
                tokens.RefreshToken,
                tokens.RefreshTokenExpiresAtUtc);
        }
    }
}