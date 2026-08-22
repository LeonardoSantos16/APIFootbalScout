using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace APIFootballScout.Infrastructure.Security;

public sealed class TokenService : ITokenService
{
    public const string RoleClaimType = "role";
    private const string TokenTypeClaim = "token_type";
    private const string AccessTokenType = "access";
    private const int MinimumKeyBytes = 32;
    private static readonly JsonWebTokenHandler Handler = new() { MapInboundClaims = false };

    private readonly JwtOptions _opt;
    private readonly TimeProvider _time;
    private readonly SymmetricSecurityKey _key;
    private readonly TokenValidationParameters _expiredTokenParameters;

    public TokenService(IOptions<JwtOptions> opt, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(opt);
        ArgumentNullException.ThrowIfNull(time);

        _opt = opt.Value;
        _time = time;

        if (string.IsNullOrWhiteSpace(_opt.Key) || Encoding.UTF8.GetByteCount(_opt.Key) < MinimumKeyBytes)
        {
            throw new InvalidOperationException(
                $"JwtOptions.Key precisa ter ao menos {MinimumKeyBytes} bytes (256 bits) para HMAC-SHA256.");
        }

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));

        _expiredTokenParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _opt.Issuer,
            ValidateAudience = true,
            ValidAudience = _opt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateLifetime = false,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = RoleClaimType
        };
    }

    public TokenResult GenerateTokens(TokenSubject usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        var agora = _time.GetUtcNow().UtcDateTime;
        var expira = agora.AddMinutes(_opt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Email, Require(usuario.Email, nameof(usuario.Email))),
            new("tenant_id", Require(usuario.TenantId, nameof(usuario.TenantId))),
            new("sec_stamp", Require(usuario.SecurityStamp, nameof(usuario.SecurityStamp))),
            new(TokenTypeClaim, AccessTokenType)
        };

        var roles = usuario.Roles ?? [];
        claims.AddRange(roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => new Claim(RoleClaimType, r)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _opt.Issuer,
            Audience = _opt.Audience,
            Subject = new ClaimsIdentity(claims, authenticationType: "jwt", RoleClaimType, RoleClaimType),
            IssuedAt = agora,
            NotBefore = agora,
            Expires = expira,
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
        };

        var accessToken = Handler.CreateToken(descriptor);

        return new TokenResult(
            accessToken,
            expira,
            GenerateRefreshToken(),
            agora.AddDays(_opt.RefreshTokenDays));
    }

    public string GenerateRefreshToken() =>
        Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));

    public async Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        cancellationToken.ThrowIfCancellationRequested();

        var result = await Handler.ValidateTokenAsync(token, _expiredTokenParameters)
                                  .ConfigureAwait(false);

        if (!result.IsValid || result.ClaimsIdentity is null)
            return null;

        var principal = new ClaimsPrincipal(result.ClaimsIdentity);

        
        if (principal.FindFirstValue(TokenTypeClaim) != AccessTokenType)
            return null;

        return principal;
    }

    private static string Require(string? value, string nome) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"Usuario.{nome} não pode ser nulo ou vazio ao emitir o token.")
            : value;
}