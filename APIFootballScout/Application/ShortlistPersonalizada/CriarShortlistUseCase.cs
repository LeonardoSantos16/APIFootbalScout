using APIFootballScout.Application.Configuration;
using APIFootballScout.Domain.Repository;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;
using Microsoft.Extensions.Options;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public class CriarShortlistUseCase
    {
        private readonly IShortlistRepository _shortlistRepository;
        private readonly LimiteDeAlvos _limiteDeAlvos;

        public CriarShortlistUseCase(
            IShortlistRepository shortlistRepository, IOptions<ScoutConfig> politica)
        {
            _shortlistRepository = shortlistRepository;
            _limiteDeAlvos = new LimiteDeAlvos(politica.Value.LimiteDeAlvosDaShortlist);
        }

        public async Task<ShortlistResult> CriarShortlist(
            CriarShortlistRequest request, CancellationToken cancellationToken)
        {
            var shortlist = Shortlist.Criar(request.OlheiroId,
               request.Nome, _limiteDeAlvos);

            await _shortlistRepository.AdicionarAsync(shortlist, cancellationToken);

            return shortlist.ParaResult();
        }
    }
}
