using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public class RepriorizarAlvoUseCase
    {
        private readonly IShortlistRepository _shortlistRepository;

        public RepriorizarAlvoUseCase(IShortlistRepository shortlistRepository)
        {
            _shortlistRepository = shortlistRepository;
        }

        public Task<ShortlistResult> RepriorizarAlvo(
            RepriorizarAlvoRequest request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}
