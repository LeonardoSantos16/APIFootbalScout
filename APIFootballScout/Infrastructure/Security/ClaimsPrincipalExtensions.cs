using System.Security.Claims;
using APIFootballScout.Domain.Base.Exceptions;
using Microsoft.IdentityModel.JsonWebTokens;

namespace APIFootballScout.Infrastructure.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid ObterUserId(this ClaimsPrincipal principal)
        {
            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(sub, out var userId)
                ? userId
                : throw new NaoAutenticadoException(
                    "usuario.token_sem_identificacao",
                    "The token does not carry a valid user identifier.");
        }

        public static string ObterEmail(this ClaimsPrincipal principal) =>
            principal.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;

        public static IReadOnlyCollection<string> ObterRoles(this ClaimsPrincipal principal) =>
            principal.FindAll(TokenService.RoleClaimType).Select(c => c.Value).ToArray();
    }
}
