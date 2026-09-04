using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.Repository;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public class AdicionarAlvoUseCase
    {
        private readonly IShortlistRepository _shortlistRepository;

        public AdicionarAlvoUseCase(IShortlistRepository shortlistRepository)
        {
            _shortlistRepository = shortlistRepository;
        }

        public async Task<ShortlistResult> AdicionarAlvo(
            AdicionarAlvoRequest request, CancellationToken cancellationToken)
        {
            var shortlist = await _shortlistRepository.ObterPorIdAsync(request.ShortlistId, request.OlheiroId,
                cancellationToken);

            if (shortlist == null)
            {
                throw new RecursoNaoEncontradoException("shortlist.nao_encontrada", "shortlist não encontrada");
            }

            shortlist.AdicionarAlvo(request.JogadorId, new Prioridade(request.Prioridade), request.CustoEstimado);

            await _shortlistRepository.AtualizarAsync(shortlist, cancellationToken);

            return shortlist.ParaResult();
        }
    }
}
