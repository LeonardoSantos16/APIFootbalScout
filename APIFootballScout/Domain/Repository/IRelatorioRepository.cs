using APIFootballScout.Domain.RelatorioScouting.Agreggate;

namespace APIFootballScout.Domain.Repository
{
    public interface IRelatorioRepository
    {
        Task AdicionarAsync(Relatorio relatorio, CancellationToken cancellationToken = default);
        Task<Relatorio?> ObterPorIdAsync(Guid relatorioId, Guid olheiroId, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Relatorio relatorio, CancellationToken cancellationToken = default);

        // O olheiro faz parte do filtro, nao de uma verificacao posterior: nao ha
        // visao consolidada entre olheiros, e o relatorio alheio nao chega a ser lido.
        Task<IReadOnlyList<Relatorio>> ListarPorJogadorAsync(
            int jogadorId, Guid olheiroId, CancellationToken cancellationToken = default);
    }
}
