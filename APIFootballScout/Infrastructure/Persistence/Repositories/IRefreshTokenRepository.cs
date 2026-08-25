using APIFootballScout.Infrastructure.Persistence.Documents;

namespace APIFootballScout.Infrastructure.Persistence.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AdicionarAsync(RefreshTokenDocument token, CancellationToken cancellationToken = default);
        Task<RefreshTokenDocument?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task RevogarAsync(Guid id, DateTime revogadoEmUtc, string? substituidoPorHash, CancellationToken cancellationToken = default);
        Task RevogarTodosDoUsuarioAsync(Guid userId, DateTime revogadoEmUtc, CancellationToken cancellationToken = default);
        Task RemoverTodosDoUsuarioAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
