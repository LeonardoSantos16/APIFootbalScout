using System.ComponentModel.DataAnnotations;
using APIFootballScout.Application.User;

namespace APIFootballScout.Contracts.Auth
{
    public sealed record SignUpRequestDto
    {
        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string Name { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; init; } = string.Empty;

        [Required]
        [StringLength(72, MinimumLength = 8)]
        public string Password { get; init; } = string.Empty;
    }

    public sealed record SignInRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;
    }

    public sealed record RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; init; } = string.Empty;
    }

    public sealed record AuthResponseDto(
        Guid UserId,
        string Name,
        string Email,
        string AccessToken,
        DateTime AccessTokenExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc)
    {
        public static AuthResponseDto De(AuthResult result) => new(
            result.UserId,
            result.Name,
            result.Email,
            result.AccessToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshToken,
            result.RefreshTokenExpiresAtUtc);
    }

    public sealed record UsuarioAutenticadoDto(Guid UserId, string Email, IReadOnlyCollection<string> Roles);
}
