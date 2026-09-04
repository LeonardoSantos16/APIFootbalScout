using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.Repository;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public class RepriorizarAlvoUseCase
    {
        private readonly IShortlistRepository _shortlistRepository;

        public RepriorizarAlvoUseCase(IShortlistRepository shortlistRepository)
        {
            _shortlistRepository = shortlistRepository;
        }

        public async Task<ShortlistResult> RepriorizarAlvo(
            RepriorizarAlvoRequest request, CancellationToken cancellationToken)
        {
            var shortlist = await _shortlistRepository.ObterPorIdAsync(request.ShortlistId, request.OlheiroId, cancellationToken) ?? throw new RecursoNaoEncontradoException("shortlist.nao_encontrada", "shortlist nao encontrada");
            var alvo = shortlist.Alvos.FirstOrDefault(a => a.JogadorId == request.JogadorId) ?? throw new RecursoNaoEncontradoException("shortlist.alvo_nao_encontrado", "alvo nao encontrado");

            shortlist.AtualizarPrioridade(request.JogadorId, new Prioridade(request.Prioridade));

            await _shortlistRepository.AtualizarAsync(shortlist, cancellationToken);
            return shortlist.ParaResult();
        }
    }
}
