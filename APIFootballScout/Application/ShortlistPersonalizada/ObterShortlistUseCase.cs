using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public class ObterShortlistUseCase
    {
        private readonly IShortlistRepository _shortlistRepository;

        public ObterShortlistUseCase(IShortlistRepository shortlistRepository)
        {
            _shortlistRepository = shortlistRepository;
        }

        public async Task<ShortlistResult> ObterShortlist(
            ObterShortlistRequest request, CancellationToken cancellationToken)
        {
            var shortlist = await _shortlistRepository.ObterPorIdAsync(request.ShortlistId, request.OlheiroId ,cancellationToken) ?? throw new RecursoNaoEncontradoException("shortlist.nao_encontrada", "shortlist nao encontrada");
            return shortlist.ParaResult();
        }
    }
}
