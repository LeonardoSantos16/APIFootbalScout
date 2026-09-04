using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.ShortlistPersonalizada
{
    public class ListarShortlistsDoOlheiroUseCase
    {
        private readonly IShortlistRepository _shortlistRepository;

        public ListarShortlistsDoOlheiroUseCase(IShortlistRepository shortlistRepository)
        {
            _shortlistRepository = shortlistRepository;
        }

        public Task<IReadOnlyList<ShortlistResult>> ListarShortlists(
            ListarShortlistsDoOlheiroRequest request, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}
