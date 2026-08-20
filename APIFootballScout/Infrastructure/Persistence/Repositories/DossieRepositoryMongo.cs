using APIFootballScout.Domain.Aggregate;
using APIFootballScout.Domain.Repository;
using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Persistence.Mappers;

using MongoDB.Driver;

namespace APIFootballScout.Infrastructure.Persistence.Repositories
{
    public class DossieRepositoryMongo : IDossieRepository
    {
        private readonly IMongoCollection<DossieDocument> _colecaoDossie;
        public DossieRepositoryMongo(IMongoClient mongoClient)
        {
            _colecaoDossie = mongoClient.GetDatabase("scoutdb").GetCollection<DossieDocument>("dossies");
        }
        public async Task AdicionarAsync(Dossie dossie, CancellationToken cancellationToken = default)
        {
            var dossieDocument = DossieMapper.MapToEntity(dossie);
            await _colecaoDossie.InsertOneAsync(dossieDocument, options: null, cancellationToken);
        }

        public async Task AtualizarAsync(Dossie dossie, CancellationToken cancellationToken = default)
        {
            var dossieDocument = DossieMapper.MapToEntity(dossie);
            var filter = Builders<DossieDocument>.Filter.Eq(d => d.Id, dossieDocument.Id);
            await _colecaoDossie.ReplaceOneAsync(filter, dossieDocument, new ReplaceOptions { IsUpsert = false }, cancellationToken);
            return;
        }
        public Task<Dossie?> ObterPorIdAsync(Guid olheiroId, int jogadorId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<DossieDocument>.Filter.Eq(d => d.OlheiroId, olheiroId) & Builders<DossieDocument>.Filter.Eq(d => d.JogadorId, jogadorId);
            var dossieDocument = _colecaoDossie.Find(filter).FirstOrDefault(cancellationToken);
            return Task.FromResult(dossieDocument != null ? DossieMapper.MapToDomain(dossieDocument) : null);
        }

        public async Task<int> ContarDossiesAtivosAsync(Guid olheiroId, CancellationToken cancellationToken = default)
        {
            var total = await _colecaoDossie.CountDocumentsAsync(
                d => d.OlheiroId == olheiroId && d.Status == (int)StatusDossie.Ativo,
                options: null,
                cancellationToken);

            return (int)total;
        }

        public async Task<bool> VerificarAcompanhamentoJogador(Guid olheiroId, int jogadorId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<DossieDocument>.Filter.Eq(d => d.OlheiroId, olheiroId) 
                & Builders<DossieDocument>.Filter.Eq(d => d.JogadorId, jogadorId);

            var document = await _colecaoDossie.FindAsync(filter, options: null, cancellationToken);
          
            return document != null;
        }
    }
}
