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

        public Task<ShortlistResult> ObterShortlist(
            ObterShortlistRequest request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}
