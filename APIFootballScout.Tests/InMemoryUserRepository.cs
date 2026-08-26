using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Persistence.Repositories;

namespace APIFootballScout.Tests
{
    public sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, UserDocument> _users = [];

        public IReadOnlyCollection<UserDocument> Todos => _users.Values;

        public Task AdicionarAsync(UserDocument user, CancellationToken cancellationToken = default)
        {
            if (_users.Values.Any(u => u.Email == user.Email))
            {
                throw new ConflitoDeDominioException(
                    "usuario.email_ja_cadastrado",
                    "There is already an account registered with this e-mail.");
            }

            _users[user.Id] = user;
            return Task.CompletedTask;
        }

        public Task<UserDocument?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.GetValueOrDefault(id));

        public Task<UserDocument?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.Values.FirstOrDefault(u => u.Email == email));

        public Task AtualizarPerfilAsync(Guid id, string name, CancellationToken cancellationToken = default)
        {
            if (_users.TryGetValue(id, out var user))
                user.Name = name;

            return Task.CompletedTask;
        }

        public Task AtualizarSenhaAsync(Guid id, string passwordHash, string securityStamp, CancellationToken cancellationToken = default)
        {
            if (_users.TryGetValue(id, out var user))
            {
                user.PasswordHash = passwordHash;
                user.SecurityStamp = securityStamp;
            }

            return Task.CompletedTask;
        }

        public Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _users.Remove(id);
            return Task.CompletedTask;
        }
    }
}
