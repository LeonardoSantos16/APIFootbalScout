using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Infrastructure.Persistence.Documents;
using MongoDB.Driver;

namespace APIFootballScout.Infrastructure.Persistence.Repositories
{
    public class UserRepositoryMongo : IUserRepository
    {
        private readonly IMongoCollection<UserDocument> _colecaoUsuarios;

        public UserRepositoryMongo(IMongoClient mongoClient)
        {
            _colecaoUsuarios = HelperObterColecao.ObterColecao<UserDocument>(mongoClient, "users");
        }

        public async Task AdicionarAsync(UserDocument user, CancellationToken cancellationToken = default)
        {
            try
            {
                await _colecaoUsuarios.InsertOneAsync(user, options: null, cancellationToken);
            }
            catch (MongoWriteException e) when (e.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new ConflitoDeDominioException(
                    "usuario.email_ja_cadastrado",
                    "There is already an account registered with this e-mail.");
            }
        }

        public Task<UserDocument?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
            => _colecaoUsuarios.Find(u => u.Email == email).FirstOrDefaultAsync(cancellationToken)!;

        public Task<UserDocument?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _colecaoUsuarios.Find(u => u.Id == id).FirstOrDefaultAsync(cancellationToken)!;

        public Task AtualizarPerfilAsync(Guid id, string name, CancellationToken cancellationToken = default)
            => _colecaoUsuarios.UpdateOneAsync(
                u => u.Id == id,
                Builders<UserDocument>.Update.Set(u => u.Name, name),
                options: null,
                cancellationToken);

        public Task AtualizarSenhaAsync(Guid id, string passwordHash, string securityStamp, CancellationToken cancellationToken = default)
            => _colecaoUsuarios.UpdateOneAsync(
                u => u.Id == id,
                Builders<UserDocument>.Update
                    .Set(u => u.PasswordHash, passwordHash)
                    .Set(u => u.SecurityStamp, securityStamp),
                options: null,
                cancellationToken);

        public Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
            => _colecaoUsuarios.DeleteOneAsync(u => u.Id == id, cancellationToken);
    }
}
