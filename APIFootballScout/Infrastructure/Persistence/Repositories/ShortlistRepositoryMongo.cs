using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.Repository;
using APIFootballScout.Domain.ShortlistPersonalizada.Aggregate;
using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Persistence.Mappers;

using MongoDB.Driver;

namespace APIFootballScout.Infrastructure.Persistence.Repositories
{
    public class ShortlistRepositoryMongo : IShortlistRepository
    {
        private readonly IMongoCollection<ShortlistDocument> _colecaoShortlist;
        public ShortlistRepositoryMongo(IMongoClient mongoClient)
        {
            _colecaoShortlist = HelperObterColecao.ObterColecao<ShortlistDocument>(mongoClient, "shortlists");
        }
        public async Task AdicionarAsync(Shortlist shortlist, CancellationToken cancellationToken = default)
        {
            shortlist.AtualizarVersao(1);
            await _colecaoShortlist.InsertOneAsync(shortlist.MapToEntity(), options: null, cancellationToken);
        }

        public async Task AtualizarAsync(Shortlist shortlist, long expectedVersion, CancellationToken cancellationToken = default)
        {
            var newVersion = expectedVersion + 1;
            var filter = Builders<ShortlistDocument>.Filter.And(Builders<ShortlistDocument>.Filter.Eq(s => s.Id, shortlist.Id), Builders<ShortlistDocument>.Filter.Eq(s => s.OlheiroId, shortlist.OlheiroId),
                Builders<ShortlistDocument>.Filter.Eq(s => s.Version, expectedVersion));
           shortlist.AtualizarVersao(newVersion);
            var shortlistBD = await _colecaoShortlist.ReplaceOneAsync(filter, shortlist.MapToEntity(), new ReplaceOptions { IsUpsert = false }, cancellationToken);

            if (shortlistBD.MatchedCount == 0)
            {
                throw new ConflitoDeConcorrenciaException(
                   "shortlist.alterada_por_outra_requisicao",
                   "a shortlist foi alterado por outra requisição. Recarregue e tente novamente.");
            }
        }

        public async Task<IReadOnlyList<Shortlist>> ListarPorOlheiroAsync(Guid olheiroId, CancellationToken cancellationToken = default)
        {
            var shortlists = await _colecaoShortlist.Find(s => s.OlheiroId == olheiroId).ToListAsync(cancellationToken);

            return [.. shortlists.Select(s => s.MapToDomain())];
        }

        public async Task<Shortlist?> ObterPorIdAsync(Guid shortlistId, Guid olheiroId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<ShortlistDocument>.Filter.Eq(s => s.Id, shortlistId) & Builders<ShortlistDocument>.Filter.Eq(s => s.OlheiroId, olheiroId);
            var shortlist = await _colecaoShortlist.Find(filter).FirstOrDefaultAsync(cancellationToken);

            return shortlist?.MapToDomain();
        }
    }
}