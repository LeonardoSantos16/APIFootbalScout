using APIFootballScout.Domain.Aggregate;

namespace APIFootballScout.Domain.Repository
{
    public interface IDossieRepository
    {
        Task<bool> VerificarAcompanhamentoJogador(int jogadorId, CancellationToken cancellationToken = default);
        Task<bool> ValidarLimiteAcompanhamentoJogador(CancellationToken cancellationToken = default);
        Task AtualizarAsync(Dossie dossie, CancellationToken cancellationToken = default);
        Task<Dossie?> ObterPorIdAsync(int jogadorId, CancellationToken cancellationToken = default);
        Task AdicionarAsync(Dossie dossie, CancellationToken cancellationToken = default);
    }
}
