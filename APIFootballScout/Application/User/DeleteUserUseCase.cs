using APIFootballScout.Domain.Repository;
using APIFootballScout.Infrastructure.Persistence.Repositories;

namespace APIFootballScout.Application.User
{
    public class DeleteUserUseCase(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAcompanhamentoRepository acompanhamentoRepository)
    {
        public async Task Execute(Guid userId, CancellationToken cancellationToken = default)
        {
            await acompanhamentoRepository.RemoverTodosDoOlheiroAsync(userId, cancellationToken);
            await refreshTokenRepository.RemoverTodosDoUsuarioAsync(userId, cancellationToken);
            await userRepository.RemoverAsync(userId, cancellationToken);
        }
    }
}
