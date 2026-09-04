using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public class ListarShortlistsDoOlheiroUseCase
    {
        private readonly IShortlistRepository _shortlistRepository;

        public ListarShortlistsDoOlheiroUseCase(IShortlistRepository shortlistRepository)
        {
            _shortlistRepository = shortlistRepository;
        }

        public async Task<IReadOnlyList<ShortlistResult>> ListarShortlists(
            ListarShortlistsDoOlheiroRequest request, CancellationToken cancellationToken)
        {
            var shortlists = await _shortlistRepository.ListarPorOlheiroAsync(request.OlheiroId, cancellationToken) ?? throw new RecursoNaoEncontradoException("shortlist.nao_encontrada", "shortlist nao encontrada");
            var shortlistOrdenada = shortlists.OrderBy(shortlist => shortlist.Nome).ToList();
            return shortlistOrdenada.ParaResult();

        }
    }
}
