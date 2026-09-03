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

        public Task<ShortlistResult> RemoverAlvo(
            RemoverAlvoRequest request, CancellationToken cancellationToken)
        {
        }
    }
}
