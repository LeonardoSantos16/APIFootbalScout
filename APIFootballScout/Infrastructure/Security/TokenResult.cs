namespace APIFootballScout.Infrastructure.Security
{
    public sealed record TokenResult(
        string AccessToken,
        DateTime AccessTokenExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc);
}
