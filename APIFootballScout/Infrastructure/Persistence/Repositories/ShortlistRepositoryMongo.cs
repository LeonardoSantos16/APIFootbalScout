using APIFootballScout.Domain.Repository;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
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
            await _colecaoShortlist.InsertOneAsync(shortlist.MapToEntity(), options: null, cancellationToken);
        }

        public async Task AtualizarAsync(Shortlist shortlist, CancellationToken cancellationToken = default)
        {
            var filter = Builders<ShortlistDocument>.Filter.Eq(s => s.Id, shortlist.Id) & Builders<ShortlistDocument>.Filter.Eq(s => s.OlheiroId, shortlist.OlheiroId);
            await _colecaoShortlist.ReplaceOneAsync(filter, shortlist.MapToEntity(), new ReplaceOptions { IsUpsert= false}, cancellationToken);
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
