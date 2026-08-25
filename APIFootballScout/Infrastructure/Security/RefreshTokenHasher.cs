using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace APIFootballScout.Infrastructure.Security
{
    public static class RefreshTokenHasher
    {
        public static string Hash(string refreshToken) =>
            Base64UrlEncoder.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}
