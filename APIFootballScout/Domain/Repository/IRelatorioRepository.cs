using APIFootballScout.Domain.RelatorioScouting.Agreggate;

namespace APIFootballScout.Domain.Repository
{
    public interface IRelatorioRepository
    {
        Task AdicionarAsync(Relatorio relatorio, CancellationToken cancellationToken = default);
        Task<Relatorio?> ObterPorIdAsync(Guid relatorioId, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Relatorio relatorio, CancellationToken cancellationToken = default);
    }
}
