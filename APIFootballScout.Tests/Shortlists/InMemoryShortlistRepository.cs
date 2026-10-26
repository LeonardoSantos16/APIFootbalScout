using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.Repository;
using APIFootballScout.Domain.ShortlistPersonalizada.Aggregate;
using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Persistence.Mappers;

namespace APIFootballScout.Tests.Shortlists
{
    internal sealed class InMemoryShortlistRepository : IShortlistRepository
    {
        private readonly List<ShortlistDocument> _documentos = [];

        public IReadOnlyList<Shortlist> Todas => [.. _documentos.Select(d => d.MapToDomain())];
        public int Atualizacoes { get; private set; }

        public long VersaoDe(Guid shortlistId)
            => _documentos.Find(d => d.Id == shortlistId)?.Version ?? 0;

        public Task AdicionarAsync(Shortlist shortlist, CancellationToken cancellationToken = default)
        {
            shortlist.AtualizarVersao(1);
            _documentos.Add(shortlist.MapToEntity());
            return Task.CompletedTask;
        }

        public Task<Shortlist?> ObterPorIdAsync(
            Guid shortlistId, Guid olheiroId, CancellationToken cancellationToken = default)
            => Task.FromResult(
                _documentos.Find(d => d.Id == shortlistId && d.OlheiroId == olheiroId)?.MapToDomain());

        public Task<IReadOnlyList<Shortlist>> ListarPorOlheiroAsync(
            Guid olheiroId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Shortlist>>(
                [.. _documentos.Where(d => d.OlheiroId == olheiroId).Select(d => d.MapToDomain())]);

        public Task AtualizarAsync(
            Shortlist shortlist, long expectedVersion, CancellationToken cancellationToken = default)
        {
            var indice = _documentos.FindIndex(d =>
                d.Id == shortlist.Id && d.OlheiroId == shortlist.OlheiroId && d.Version == expectedVersion);

            if (indice == -1)
                throw new ConflitoDeConcorrenciaException(
                    "shortlist.alterada_por_outra_requisicao",
                    "A shortlist foi alterada por outra requisicao.");

            shortlist.AtualizarVersao(expectedVersion + 1);
            _documentos[indice] = shortlist.MapToEntity();
            Atualizacoes++;

            return Task.CompletedTask;
        }
    }
}
