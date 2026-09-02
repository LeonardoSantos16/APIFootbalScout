using APIFootballScout.Domain.Acompanhamento.Aggregate;
using APIFootballScout.Domain.Repository;
using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Persistence.Mappers;

using MongoDB.Driver;

namespace APIFootballScout.Infrastructure.Persistence.Repositories
{
    public class AcompanhamentoRepositoryMongo : IAcompanhamentoRepository
    {
        private readonly IMongoCollection<DossieDocument> _colecaoDossie;

        public AcompanhamentoRepositoryMongo(IMongoClient mongoClient)
        {
            _colecaoDossie = HelperObterColecao.ObterColecao<DossieDocument>(mongoClient, "dossies");
        }
        public static Task GarantirIndicesAsync(IMongoClient mongoClient, CancellationToken cancellationToken = default)
        {
            var colecao = HelperObterColecao.ObterColecao<DossieDocument>(mongoClient, "dossies");

            var chave = Builders<DossieDocument>.IndexKeys
                .Ascending(d => d.OlheiroId)
                .Ascending(d => d.JogadorId);

            var opcoes = new CreateIndexOptions<DossieDocument>
            {
                Name = "uniq_olheiro_jogador_ativo",
                Unique = true,
                PartialFilterExpression = Builders<DossieDocument>.Filter.Eq(d => d.Status, (int)StatusDossie.Ativo)
            };

            return colecao.Indexes.CreateOneAsync(
                new CreateIndexModel<DossieDocument>(chave, opcoes),
                cancellationToken: cancellationToken);
        }

        public async Task AdicionarAsync(Dossie dossie, CancellationToken cancellationToken = default)
        {
            var dossieDocument = DossieMapper.MapToEntity(dossie);
            await _colecaoDossie.InsertOneAsync(dossieDocument, options: null, cancellationToken);
        }

        public Task RemoverTodosDoOlheiroAsync(Guid olheiroId, CancellationToken cancellationToken = default)
            => _colecaoDossie.DeleteManyAsync(d => d.OlheiroId == olheiroId, cancellationToken);

        public async Task AtualizarAsync(Dossie dossie, CancellationToken cancellationToken = default)
        {
            var dossieDocument = DossieMapper.MapToEntity(dossie);
            var filter = Builders<DossieDocument>.Filter.Eq(d => d.Id, dossieDocument.Id);
            await _colecaoDossie.ReplaceOneAsync(filter, dossieDocument, new ReplaceOptions { IsUpsert = false }, cancellationToken);
        }

        public async Task<List<Dossie>> ObterPorIdAsync(Guid olheiroId, int jogadorId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<DossieDocument>.Filter.Eq(d => d.OlheiroId, olheiroId)
                & Builders<DossieDocument>.Filter.Eq(d => d.JogadorId, jogadorId);

            var documents = await _colecaoDossie
               .Find(filter)
               .ToListAsync(cancellationToken);

            return DossieMapper.MapToListDomain(documents);
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
                & Builders<DossieDocument>.Filter.Eq(d => d.JogadorId, jogadorId)
                & Builders<DossieDocument>.Filter.Eq(d => d.Status, (int)StatusDossie.Ativo);

            var total = await _colecaoDossie.CountDocumentsAsync(filter, options: null, cancellationToken);

            return total > 0;
        }
    }
}
