using APIFootballScout.Application.Configuration;
using APIFootballScout.Domain.Repository;
using Microsoft.Extensions.Options;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public class CriarShortlistUseCase
    {
        private readonly IShortlistRepository _shortlistRepository;
        private readonly ScoutConfig _politica;

        public CriarShortlistUseCase(
            IShortlistRepository shortlistRepository, IOptions<ScoutConfig> politica)
        {
            _shortlistRepository = shortlistRepository;
            _politica = politica.Value;
        }

        public Task<ShortlistResult> CriarShortlist(
            CriarShortlistRequest request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}
