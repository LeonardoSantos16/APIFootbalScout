
using APIFootballScout.Domain.Acompanhamento.Aggregate;

namespace APIFootballScout.Domain.Repository
{
    public interface IAcompanhamentoRepository
    {
        Task<bool> VerificarAcompanhamentoJogador(Guid olheiroId, int jogadorId, CancellationToken cancellationToken = default);
        Task<int> ContarDossiesAtivosAsync(Guid olheiroId, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Dossie dossie, CancellationToken cancellationToken = default);
        Task<Dossie?> ObterPorIdAsync(Guid olheiroId, int jogadorId, CancellationToken cancellationToken = default);
        Task AdicionarAsync(Dossie dossie, CancellationToken cancellationToken = default);
        Task RemoverTodosDoOlheiroAsync(Guid olheiroId, CancellationToken cancellationToken = default);
    }
}
