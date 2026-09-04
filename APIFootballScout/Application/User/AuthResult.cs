namespace APIFootballScout.Application.User
{
    public sealed record AuthResult(
        Guid UserId,
        string Name,
        string Email,
        string AccessToken,
        DateTime AccessTokenExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc);
}
