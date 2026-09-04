using APIFootballScout.Domain.Repository;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;

namespace APIFootballScout.Tests.Shortlists
{
    internal sealed class InMemoryShortlistRepository : IShortlistRepository
    {
        private readonly List<Shortlist> _shortlists = [];

        public IReadOnlyList<Shortlist> Todas => _shortlists;
        public int Atualizacoes { get; private set; }

        public Task AdicionarAsync(Shortlist shortlist, CancellationToken cancellationToken = default)
        {
            _shortlists.Add(shortlist);
            return Task.CompletedTask;
        }

        public Task<Shortlist?> ObterPorIdAsync(
            Guid shortlistId, Guid olheiroId, CancellationToken cancellationToken = default)
            => Task.FromResult(_shortlists.Find(s => s.Id == shortlistId && s.OlheiroId == olheiroId));

        public Task<IReadOnlyList<Shortlist>> ListarPorOlheiroAsync(
            Guid olheiroId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Shortlist>>(
                [.. _shortlists.Where(s => s.OlheiroId == olheiroId)]);

        public Task AtualizarAsync(Shortlist shortlist, CancellationToken cancellationToken = default)
        {
            Atualizacoes++;

            var indice = _shortlists.FindIndex(s => s.Id == shortlist.Id);

            if (indice != -1)
                _shortlists[indice] = shortlist;

            return Task.CompletedTask;
        }
    }
}
