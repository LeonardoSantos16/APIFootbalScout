using APIFootballScout.Infrastructure.Persistence.Documents;

using MongoDB.Driver;

namespace APIFootballScout.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepositoryMongo : IRefreshTokenRepository
    {
        private readonly IMongoCollection<RefreshTokenDocument> _colecaoTokens;

        public RefreshTokenRepositoryMongo(IMongoClient mongoClient)
        {
            _colecaoTokens = HelperObterColecao.ObterColecao<RefreshTokenDocument>(mongoClient, "refresh_tokens");
        }

        public static Task GarantirIndicesTtlParaTokensVencidosAsync(IMongoClient mongoClient, CancellationToken cancellationToken = default)
        {
            var colecao = HelperObterColecao.ObterColecao<RefreshTokenDocument>(mongoClient, "refresh_tokens");

            var opcoes = new CreateIndexOptions { Name = "ttl_expires_at_utc", ExpireAfter = TimeSpan.FromDays(7) };

            return colecao.Indexes.CreateOneAsync(
                new CreateIndexModel<RefreshTokenDocument>(Builders<RefreshTokenDocument>.IndexKeys.Ascending(t => t.ExpiresAtUtc), opcoes),
                cancellationToken: cancellationToken);
        }

        public static Task GarantirIndicesAsync(IMongoClient mongoClient, CancellationToken cancellationToken = default)
        {
            var colecao = HelperObterColecao.ObterColecao<RefreshTokenDocument>(mongoClient, "refresh_tokens");

            var opcoes = new CreateIndexOptions { Name = "uniq_token_hash", Unique = true };

            return colecao.Indexes.CreateOneAsync(
                new CreateIndexModel<RefreshTokenDocument>(Builders<RefreshTokenDocument>.IndexKeys.Ascending(t => t.TokenHash), opcoes),
                cancellationToken: cancellationToken);
        }

        public Task AdicionarAsync(RefreshTokenDocument token, CancellationToken cancellationToken = default)
            => _colecaoTokens.InsertOneAsync(token, options: null, cancellationToken);

        public Task<RefreshTokenDocument?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken = default)
            => _colecaoTokens.Find(t => t.TokenHash == tokenHash).FirstOrDefaultAsync(cancellationToken)!;

        public async Task<bool> RevogarAsync(Guid id, DateTime revogadoEmUtc, string? substituidoPorHash, CancellationToken cancellationToken = default)
        {
            var resultado = await _colecaoTokens.UpdateOneAsync(
                t => t.Id == id && t.RevokedAtUtc == null,
                Builders<RefreshTokenDocument>.Update
                    .Set(t => t.RevokedAtUtc, revogadoEmUtc)
                    .Set(t => t.ReplacedByHash, substituidoPorHash),
                options: null,
                cancellationToken);

            return resultado.ModifiedCount == 1;
        }

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