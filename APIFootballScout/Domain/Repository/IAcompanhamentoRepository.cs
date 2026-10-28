
using APIFootballScout.Domain.Acompanhamento.Aggregate;

namespace APIFootballScout.Domain.Repository
{
    public interface IAcompanhamentoRepository
    {
        Task<bool> VerificarAcompanhamentoJogador(Guid olheiroId, int jogadorId, CancellationToken cancellationToken = default);
        Task<bool> ReservarVagaDeAcompanhamentoAsync(Guid olheiroId, int limite, CancellationToken cancellationToken = default);
        Task LiberarVagaDeAcompanhamentoAsync(Guid olheiroId, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Dossie dossie, CancellationToken cancellationToken = default);
        Task<List<Dossie>> ObterPorIdAsync(Guid olheiroId, int jogadorId, CancellationToken cancellationToken = default);
        Task AdicionarAsync(Dossie dossie, CancellationToken cancellationToken = default);
        Task RemoverTodosDoOlheiroAsync(Guid olheiroId, CancellationToken cancellationToken = default);
    }
}