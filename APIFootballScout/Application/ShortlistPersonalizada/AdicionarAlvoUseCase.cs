using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public class AdicionarAlvoUseCase
    {
        private readonly IShortlistRepository _shortlistRepository;

        public AdicionarAlvoUseCase(IShortlistRepository shortlistRepository)
        {
            _shortlistRepository = shortlistRepository;
        }

        public Task<ShortlistResult> AdicionarAlvo(
            AdicionarAlvoRequest request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}
