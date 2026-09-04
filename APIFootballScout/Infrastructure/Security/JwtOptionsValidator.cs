using Microsoft.Extensions.Options;
using System.Text;

namespace APIFootballScout.Infrastructure.Security
{
    public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
    {
        private const int MinimumKeyBytes = 32;

        public ValidateOptionsResult Validate(string? name, JwtOptions options)
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(options.Key))
                erros.Add("JwtOptions.Key é obrigatória.");
            else if (Encoding.UTF8.GetByteCount(options.Key) < MinimumKeyBytes)
                erros.Add($"JwtOptions.Key precisa ter ao menos {MinimumKeyBytes} bytes (256 bits) para HMAC-SHA256.");

            if (string.IsNullOrWhiteSpace(options.Issuer))
                erros.Add("JwtOptions.Issuer é obrigatório.");

            if (string.IsNullOrWhiteSpace(options.Audience))
                erros.Add("JwtOptions.Audience é obrigatória.");

            if (options.AccessTokenMinutes <= 0)
                erros.Add("JwtOptions.AccessTokenMinutes precisa ser maior que zero.");

            if (options.RefreshTokenDays <= 0)
                erros.Add("JwtOptions.RefreshTokenDays precisa ser maior que zero.");

            return erros.Count > 0
                ? ValidateOptionsResult.Fail(erros)
                : ValidateOptionsResult.Success;
        }
    }
}
