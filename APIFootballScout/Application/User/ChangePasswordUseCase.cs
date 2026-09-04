using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Infrastructure.Persistence.Repositories;
using APIFootballScout.Infrastructure.Security;

namespace APIFootballScout.Application.User
{
    public class ChangePasswordUseCase(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        AuthSessionIssuer sessionIssuer,
        TimeProvider timeProvider)
    {
        public async Task<AuthResult> Execute(
            Guid userId,
            string currentPassword,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            var user = await userRepository.ObterPorIdAsync(userId, cancellationToken)
                ?? throw new NaoAutenticadoException(
                    "usuario.nao_encontrado",
                    "The authenticated user no longer exists.");

            if (!passwordHasher.Verify(currentPassword, user.PasswordHash))
            {
                throw new ValorInvalidoException(
                    "usuario.senha_atual_invalida",
                    "The current password does not match.");
            }

            if (passwordHasher.Verify(newPassword, user.PasswordHash))
            {
                throw new RegraDeNegocioException(
                    "usuario.senha_igual_a_atual",
                    "The new password must be different from the current one.");
            }

            user.PasswordHash = passwordHasher.Hash(newPassword);
            user.SecurityStamp = Guid.NewGuid().ToString("N");

            await userRepository.AtualizarSenhaAsync(
                user.Id, user.PasswordHash, user.SecurityStamp, cancellationToken);

            await refreshTokenRepository.RevogarTodosDoUsuarioAsync(
                user.Id, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

            return await sessionIssuer.EmitirAsync(user, cancellationToken);
        }
    }
}
