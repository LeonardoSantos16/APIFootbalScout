using APIFootballScout.Domain.Acompanhamento.Aggregate;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.Repository;
using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Persistence.Mappers;

using MongoDB.Driver;

namespace APIFootballScout.Infrastructure.Persistence.Repositories
{
    public class AcompanhamentoRepositoryMongo : IAcompanhamentoRepository
    {
        private readonly IMongoCollection<DossieDocument> _colecaoDossie;
        private readonly IMongoCollection<ContadorDeAcompanhamentosDocument> _colecaoContadores;

        public AcompanhamentoRepositoryMongo(IMongoClient mongoClient)
        {
            _colecaoDossie = HelperObterColecao.ObterColecao<DossieDocument>(mongoClient, "dossies");
            _colecaoContadores = HelperObterColecao.ObterColecao<ContadorDeAcompanhamentosDocument>(mongoClient, "contadores_de_acompanhamentos");
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

            try
            {
                await _colecaoDossie.InsertOneAsync(dossieDocument, options: null, cancellationToken);
            }
            catch (MongoWriteException e) when (e.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new ConflitoDeDominioException(
                    "acompanhamento.jogador_ja_acompanhado",
                    "The player is already being tracked by this scout.");
            }
        }

        public async Task RemoverTodosDoOlheiroAsync(Guid olheiroId, CancellationToken cancellationToken = default)
        {
            await _colecaoDossie.DeleteManyAsync(d => d.OlheiroId == olheiroId, cancellationToken);
            await _colecaoContadores.DeleteOneAsync(c => c.OlheiroId == olheiroId, cancellationToken);
        }

        public async Task<bool> ReservarVagaDeAcompanhamentoAsync(Guid olheiroId, int limite, CancellationToken cancellationToken = default)
        {
            var contadorExiste = await _colecaoContadores.Find(c => c.OlheiroId == olheiroId).AnyAsync(cancellationToken);

            if (!contadorExiste)
            {
                var ativos = await ContarDossiesAtivosAsync(olheiroId, cancellationToken);

                await _colecaoContadores.UpdateOneAsync(
                    c => c.OlheiroId == olheiroId,
                    Builders<ContadorDeAcompanhamentosDocument>.Update.SetOnInsert(c => c.Ativos, ativos),
                    new UpdateOptions { IsUpsert = true },
                    cancellationToken);
            }

            return await OcuparVagaAsync(olheiroId, limite, cancellationToken);
        }

        public async Task LiberarVagaDeAcompanhamentoAsync(Guid olheiroId, CancellationToken cancellationToken = default)
        {
            await _colecaoContadores.UpdateOneAsync(
                c => c.OlheiroId == olheiroId && c.Ativos > 0,
                Builders<ContadorDeAcompanhamentosDocument>.Update.Inc(c => c.Ativos, -1),
                options: null,
                cancellationToken);
        }

        private async Task<bool> OcuparVagaAsync(Guid olheiroId, int limite, CancellationToken cancellationToken)
        {
            var resultado = await _colecaoContadores.UpdateOneAsync(
                c => c.OlheiroId == olheiroId && c.Ativos < limite,
                Builders<ContadorDeAcompanhamentosDocument>.Update.Inc(c => c.Ativos, 1),
                options: null,
                cancellationToken);

            return resultado.ModifiedCount == 1;
        }

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

        private async Task<int> ContarDossiesAtivosAsync(Guid olheiroId, CancellationToken cancellationToken = default)
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