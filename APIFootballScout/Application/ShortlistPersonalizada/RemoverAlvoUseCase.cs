using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public class RemoverAlvoUseCase
    {
        private readonly IShortlistRepository _shortlistRepository;

        public RemoverAlvoUseCase(IShortlistRepository shortlistRepository)
        {
            _shortlistRepository = shortlistRepository;
        }

        public async Task<ShortlistResult> RemoverAlvo(
            RemoverAlvoRequest request, CancellationToken cancellationToken)
        {
            var shortlist = await _shortlistRepository.ObterPorIdAsync(
                request.ShortlistId, request.OlheiroId, cancellationToken);

            if (shortlist is null)
                throw new RecursoNaoEncontradoException(
                    "shortlist.nao_encontrada",
                    "A shortlist não foi encontrada.");

            shortlist.RemoverAlvo(request.JogadorId);

            await _shortlistRepository.AtualizarAsync(shortlist, cancellationToken);

            return shortlist.ParaResult();
        }
    }
}
