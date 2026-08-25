using APIFootballScout.Infrastructure.Persistence.Repositories;

namespace APIFootballScout.Application.User
{
    public class DeleteUserUseCase(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        public async Task Execute(Guid userId, CancellationToken cancellationToken = default)
        {
            await userRepository.RemoverAsync(userId, cancellationToken);
            await refreshTokenRepository.RemoverTodosDoUsuarioAsync(userId, cancellationToken);
        }
    }
}
