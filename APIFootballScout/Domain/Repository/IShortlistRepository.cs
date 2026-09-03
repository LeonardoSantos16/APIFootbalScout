using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;

namespace APIFootballScout.Domain.Repository
{
    public interface IShortlistRepository
    {
        Task AdicionarAsync(Shortlist shortlist, CancellationToken cancellationToken = default);

        Task<Shortlist?> ObterPorIdAsync(
            Guid shortlistId, Guid olheiroId, CancellationToken cancellationToken = default);

        Task AtualizarAsync(Shortlist shortlist, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Shortlist>> ListarPorOlheiroAsync(
            Guid olheiroId, CancellationToken cancellationToken = default);
    }
}
