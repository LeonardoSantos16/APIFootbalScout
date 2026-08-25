using System.Security.Cryptography;
using System.Text;

namespace APIFootballScout.Infrastructure.Security
{
    public sealed class BCryptPasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;
        private readonly byte[] _pepper;

        public BCryptPasswordHasher(IConfiguration configuration)
        {
            var pepperString = configuration["Password:Pepper"] ?? throw new InvalidOperationException("Password:Pepper is not configured.");
            _pepper = Encoding.UTF8.GetBytes(pepperString);
        }

        private string Pepper(string password) =>
            Convert.ToBase64String(HMACSHA256.HashData(_pepper, Encoding.UTF8.GetBytes(password)));

        public string Hash(string password) =>
            BCrypt.Net.BCrypt.HashPassword(Pepper(password), BCrypt.Net.BCrypt.GenerateSalt(WorkFactor));

        public bool Verify(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(Pepper(password), passwordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }
    }
}
