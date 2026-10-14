using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Infrastructure.Persistence.Repositories;
using APIFootballScout.Infrastructure.Security;

namespace APIFootballScout.Application.User
{
    public class RefreshTokenUseCase(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        AuthSessionIssuer sessionIssuer,
        TimeProvider timeProvider,
        ILogger<RefreshTokenUseCase> logger)
    {
        public async Task<AuthResult> Execute(string refreshToken, CancellationToken cancellationToken = default)
        {
            var agora = timeProvider.GetUtcNow().UtcDateTime;
            var hash = RefreshTokenHasher.Hash(refreshToken);

            var armazenado = await refreshTokenRepository.ObterPorHashAsync(hash, cancellationToken)
                ?? throw TokenInvalido();

            if (armazenado.RevokedAtUtc is not null)
            {
                if (armazenado.ReplacedByHash is not null)
                {
                    LogReusoDeTokenRotacionado(logger, armazenado.UserId, null);

                    await refreshTokenRepository.RevogarTodosDoUsuarioAsync(armazenado.UserId, agora, cancellationToken);
                }

                throw TokenInvalido();
            }

            if (armazenado.ExpiresAtUtc <= agora)
                throw TokenInvalido();

            var user = await userRepository.ObterPorIdAsync(armazenado.UserId, cancellationToken);

            if (user is null)
            {
                await refreshTokenRepository.RevogarTodosDoUsuarioAsync(armazenado.UserId, agora, cancellationToken);
                throw TokenInvalido();
            }

            var novaSessao = await sessionIssuer.EmitirAsync(user, cancellationToken);

            await refreshTokenRepository.RevogarAsync(
                armazenado.Id,
                agora,
                RefreshTokenHasher.Hash(novaSessao.RefreshToken),
                cancellationToken);

            return novaSessao;
        }

        private static readonly Action<ILogger, Guid, Exception?> LogReusoDeTokenRotacionado =
            LoggerMessage.Define<Guid>(LogLevel.Warning, new EventId(1, nameof(LogReusoDeTokenRotacionado)),
                "Reuse of a rotated refresh token detected for user {UserId}; revoking every active session.");

        private static NaoAutenticadoException TokenInvalido() =>
            new("usuario.refresh_token_invalido", "Invalid or expired refresh token.");
    }
}