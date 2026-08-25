using APIFootballScout.Infrastructure.Persistence.Documents;
using MongoDB.Driver;

namespace APIFootballScout.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepositoryMongo : IRefreshTokenRepository
    {
        private readonly IMongoCollection<RefreshTokenDocument> _colecaoTokens;

        public RefreshTokenRepositoryMongo(IMongoClient mongoClient)
        {
            _colecaoTokens = ObterColecao(mongoClient);
        }

        private static IMongoCollection<RefreshTokenDocument> ObterColecao(IMongoClient mongoClient)
            => mongoClient.GetDatabase("scoutdb").GetCollection<RefreshTokenDocument>("refresh_tokens");

        public Task AdicionarAsync(RefreshTokenDocument token, CancellationToken cancellationToken = default)
            => _colecaoTokens.InsertOneAsync(token, options: null, cancellationToken);

        public Task<RefreshTokenDocument?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken = default)
            => _colecaoTokens.Find(t => t.TokenHash == tokenHash).FirstOrDefaultAsync(cancellationToken)!;

        public Task RevogarAsync(Guid id, DateTime revogadoEmUtc, string? substituidoPorHash, CancellationToken cancellationToken = default)
            => _colecaoTokens.UpdateOneAsync(
                t => t.Id == id && t.RevokedAtUtc == null,
                Builders<RefreshTokenDocument>.Update
                    .Set(t => t.RevokedAtUtc, revogadoEmUtc)
                    .Set(t => t.ReplacedByHash, substituidoPorHash),
                options: null,
                cancellationToken);

        public Task RevogarTodosDoUsuarioAsync(Guid userId, DateTime revogadoEmUtc, CancellationToken cancellationToken = default)
            => _colecaoTokens.UpdateManyAsync(
                t => t.UserId == userId && t.RevokedAtUtc == null,
                Builders<RefreshTokenDocument>.Update.Set(t => t.RevokedAtUtc, revogadoEmUtc),
                options: null,
                cancellationToken);

        public Task RemoverTodosDoUsuarioAsync(Guid userId, CancellationToken cancellationToken = default)
            => _colecaoTokens.DeleteManyAsync(t => t.UserId == userId, cancellationToken);
    }
}
