namespace APIFootballScout.Infrastructure.Security
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public string Key { get; init; } = string.Empty;
        public int AccessTokenMinutes { get; init; } = 15;
        public int RefreshTokenDays { get; init; } = 7;
    }

}
