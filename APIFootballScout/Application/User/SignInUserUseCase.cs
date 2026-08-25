using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Infrastructure.Persistence.Repositories;
using APIFootballScout.Infrastructure.Security;

namespace APIFootballScout.Application.User
{
    public class SignInUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        AuthSessionIssuer sessionIssuer)
    {
        public async Task<AuthResult> Execute(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await userRepository.ObterPorEmailAsync(
                SignUpUserUseCase.NormalizarEmail(email), cancellationToken);

            if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
            {
                throw new NaoAutenticadoException(
                    "usuario.credenciais_invalidas",
                    "Invalid e-mail or password.");
            }

            return await sessionIssuer.EmitirAsync(user, cancellationToken);
        }
    }
}
