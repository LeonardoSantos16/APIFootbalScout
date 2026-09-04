using APIFootballScout.Infrastructure.Persistence.Documents;

namespace APIFootballScout.Infrastructure.Persistence.Repositories
{
    public interface IUserRepository
    {
        Task AdicionarAsync(UserDocument user, CancellationToken cancellationToken = default);
        Task<UserDocument?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserDocument?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AtualizarPerfilAsync(Guid id, string name, CancellationToken cancellationToken = default);
        Task AtualizarSenhaAsync(Guid id, string passwordHash, string securityStamp, CancellationToken cancellationToken = default);
        Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
