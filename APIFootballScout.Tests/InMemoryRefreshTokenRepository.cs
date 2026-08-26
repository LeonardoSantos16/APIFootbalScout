using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Persistence.Repositories;
using APIFootballScout.Infrastructure.Security;

namespace APIFootballScout.Tests
{
    public sealed class InMemoryRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly Dictionary<Guid, RefreshTokenDocument> _tokens = [];
        public IReadOnlyCollection<RefreshTokenDocument> Todos => _tokens.Values;

        public IEnumerable<RefreshTokenDocument> Ativos =>
            _tokens.Values.Where(t => t.RevokedAtUtc is null);

        public RefreshTokenDocument? PorToken(string refreshToken) =>
            _tokens.Values.FirstOrDefault(t => t.TokenHash == RefreshTokenHasher.Hash(refreshToken));

        public Task AdicionarAsync(RefreshTokenDocument token, CancellationToken cancellationToken = default)
        {
            _tokens[token.Id] = token;
            return Task.CompletedTask;
        }

        public Task<RefreshTokenDocument?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult(_tokens.Values.FirstOrDefault(t => t.TokenHash == tokenHash));

        public Task RevogarAsync(Guid id, DateTime revogadoEmUtc, string? substituidoPorHash, CancellationToken cancellationToken = default)
        {
            if (_tokens.TryGetValue(id, out var token) && token.RevokedAtUtc is null)
            {
                token.RevokedAtUtc = revogadoEmUtc;
                token.ReplacedByHash = substituidoPorHash;
            }

            return Task.CompletedTask;
        }

        public Task RevogarTodosDoUsuarioAsync(Guid userId, DateTime revogadoEmUtc, CancellationToken cancellationToken = default)
        {
            foreach (var token in _tokens.Values.Where(t => t.UserId == userId && t.RevokedAtUtc is null))
                token.RevokedAtUtc = revogadoEmUtc;

            return Task.CompletedTask;
        }

        public Task RemoverTodosDoUsuarioAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            foreach (var id in _tokens.Values.Where(t => t.UserId == userId).Select(t => t.Id).ToList())
                _tokens.Remove(id);

            return Task.CompletedTask;
        }
    }
}
