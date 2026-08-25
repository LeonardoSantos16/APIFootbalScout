using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Persistence.Repositories;
using APIFootballScout.Infrastructure.Security;

namespace APIFootballScout.Application.User
{
    public class SignUpUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        AuthSessionIssuer sessionIssuer,
        TimeProvider timeProvider)
    {
        private const string RolePadrao = "olheiro";

        public async Task<AuthResult> Execute(SignUpUserRequest request, CancellationToken cancellationToken = default)
        {
            var email = NormalizarEmail(request.Email);

            if (await userRepository.ObterPorEmailAsync(email, cancellationToken) is not null)
            {
                throw new ConflitoDeDominioException(
                    "usuario.email_ja_cadastrado",
                    "There is already an account registered with this e-mail.");
            }

            var user = new UserDocument
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Email = email,
                PasswordHash = passwordHasher.Hash(request.Password),
                SecurityStamp = Guid.NewGuid().ToString("N"),
                Roles = [RolePadrao],
                CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime
            };

            await userRepository.AdicionarAsync(user, cancellationToken);

            return await sessionIssuer.EmitirAsync(user, cancellationToken);
        }

        internal static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
    }
}
