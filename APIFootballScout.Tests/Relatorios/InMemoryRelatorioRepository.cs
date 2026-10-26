using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Aggregate;
using APIFootballScout.Domain.Repository;
using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Persistence.Mappers;

namespace APIFootballScout.Tests.Relatorios
{
    internal sealed class InMemoryRelatorioRepository : IRelatorioRepository
    {
        private readonly List<RelatorioDocument> _documentos = [];

        public IReadOnlyList<Relatorio> Todos => [.. _documentos.Select(RelatorioMapper.MapToDomain)];
        public int Atualizacoes { get; private set; }

        public long VersaoDe(Guid relatorioId)
            => _documentos.Find(d => d.Id == relatorioId)?.Version ?? 0;

        public Task AdicionarAsync(Relatorio relatorio, CancellationToken cancellationToken = default)
        {
            relatorio.AtualizarVersao(1);
            _documentos.Add(RelatorioMapper.MapToEntity(relatorio));
            return Task.CompletedTask;
        }

        public Task<Relatorio?> ObterPorIdAsync(Guid relatorioId, Guid olheiroId, CancellationToken cancellationToken = default)
        {
            var documento = _documentos.Find(d => d.Id == relatorioId && d.OlheiroId == olheiroId);

            return Task.FromResult(documento is null ? null : RelatorioMapper.MapToDomain(documento));
        }

        // Sem ordenacao: a ordem da listagem e decisao do caso de uso, e e la que
        // os testes a provam. Aqui a ordem e a de insercao, de proposito.
        public Task<IReadOnlyList<Relatorio>> ListarPorJogadorAsync(
            int jogadorId, Guid olheiroId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Relatorio>>(
                [.. _documentos
                    .Where(d => d.JogadorId == jogadorId && d.OlheiroId == olheiroId)
                    .Select(RelatorioMapper.MapToDomain)]);

        public Task AtualizarAsync(
            Relatorio relatorio, long expectedVersion, CancellationToken cancellationToken = default)
        {
            var indice = _documentos.FindIndex(d => d.Id == relatorio.Id && d.Version == expectedVersion);

            if (indice == -1)
                throw new ConflitoDeConcorrenciaException(
                    "relatorio.alterado_por_outra_requisicao",
                    "O relatorio foi alterado por outra requisicao.");

            relatorio.AtualizarVersao(expectedVersion + 1);
            _documentos[indice] = RelatorioMapper.MapToEntity(relatorio);
            Atualizacoes++;

            return Task.CompletedTask;
        }
    }
}