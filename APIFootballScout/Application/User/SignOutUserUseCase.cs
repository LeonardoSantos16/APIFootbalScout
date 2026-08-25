using APIFootballScout.Infrastructure.Persistence.Repositories;
using APIFootballScout.Infrastructure.Security;

namespace APIFootballScout.Application.User
{
    public class SignOutUserUseCase(
        IRefreshTokenRepository refreshTokenRepository,
        TimeProvider timeProvider)
    {
        public async Task Execute(Guid userId, string refreshToken, CancellationToken cancellationToken = default)
        {
            var armazenado = await refreshTokenRepository.ObterPorHashAsync(
                RefreshTokenHasher.Hash(refreshToken), cancellationToken);

            if (armazenado is null || armazenado.UserId != userId || armazenado.RevokedAtUtc is not null)
                return;

            await refreshTokenRepository.RevogarAsync(
                armazenado.Id,
                timeProvider.GetUtcNow().UtcDateTime,
                substituidoPorHash: null,
                cancellationToken);
        }

        public Task ExecuteTodasSessoes(Guid userId, CancellationToken cancellationToken = default)
            => refreshTokenRepository.RevogarTodosDoUsuarioAsync(
                userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
    }
}
