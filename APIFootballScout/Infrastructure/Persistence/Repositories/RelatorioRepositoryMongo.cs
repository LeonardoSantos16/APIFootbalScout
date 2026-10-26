using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Aggregate;
using APIFootballScout.Domain.Repository;
using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Persistence.Mappers;

using MongoDB.Driver;

namespace APIFootballScout.Infrastructure.Persistence.Repositories
{
    public class RelatorioRepositoryMongo : IRelatorioRepository
    {
        private readonly IMongoCollection<RelatorioDocument> _colecaoRelatorio;

        public RelatorioRepositoryMongo(IMongoClient mongoClient)
        {
            _colecaoRelatorio = HelperObterColecao.ObterColecao<RelatorioDocument>(mongoClient, "relatorios");
        }

        public async Task AdicionarAsync(Relatorio relatorio, CancellationToken cancellationToken = default)
        {
            relatorio.AtualizarVersao(1);
            var relatorioDocument = RelatorioMapper.MapToEntity(relatorio);
            await _colecaoRelatorio.InsertOneAsync(relatorioDocument, options: null, cancellationToken);
        }

        public async Task AtualizarAsync(Relatorio relatorio, long expectedVersion, CancellationToken cancellationToken = default)
        {
            var filter = Builders<RelatorioDocument>.Filter.Eq(r => r.Id, relatorio.Id)
                & Builders<RelatorioDocument>.Filter.Eq(r => r.Version, expectedVersion);
            relatorio.AtualizarVersao(expectedVersion + 1);
            var relatorioDocument = RelatorioMapper.MapToEntity(relatorio);
            var relatorioBd = await _colecaoRelatorio.ReplaceOneAsync(filter, relatorioDocument, new ReplaceOptions { IsUpsert = false }, cancellationToken);

            if (relatorioBd.MatchedCount == 0)
            {
                throw new ConflitoDeConcorrenciaException(
                   "relatorio.alterado_por_outra_requisicao",
                   "O relatorio foi alterado por outra requisição. Recarregue e tente novamente.");
            }
        }

        public async Task<IReadOnlyList<Relatorio>> ListarPorJogadorAsync(int jogadorId, Guid olheiroId, CancellationToken cancellationToken = default)
        {
            var filters = Builders<RelatorioDocument>.Filter.Eq(r => r.JogadorId, jogadorId) & Builders<RelatorioDocument>.Filter.Eq(r => r.OlheiroId, olheiroId);
            var relatorios = await _colecaoRelatorio.Find(filters).ToListAsync(cancellationToken);
            var relatoriosDomain = relatorios.Select(RelatorioMapper.MapToDomain).ToList();

            return relatoriosDomain;
        }

        public async Task<Relatorio?> ObterPorIdAsync(Guid relatorioId, Guid olheiroId, CancellationToken cancellationToken = default)
        {
            var filters = Builders<RelatorioDocument>.Filter.Eq(r => r.Id, relatorioId) & Builders<RelatorioDocument>.Filter.Eq(r => r.OlheiroId, olheiroId);
            var relatorio = await _colecaoRelatorio.Find(filters).FirstOrDefaultAsync(cancellationToken);

            return relatorio is null ? null : RelatorioMapper.MapToDomain(relatorio);
        }
    }
}