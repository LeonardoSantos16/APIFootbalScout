using APIFootballScout.Infrastructure.Security;
using Microsoft.Extensions.Configuration;

namespace APIFootballScout.Tests.Security
{
    public class BCryptPasswordHasherTests
    {
        private static IPasswordHasher ComPepper(string pepper) =>
            new BCryptPasswordHasher(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Password:Pepper"] = pepper
            }).Build());

        [Fact]
        public void Hash_e_verify_password_com_pepper()
        {
            var hasher = ComPepper("pepper-test");

            Assert.True(hasher.Verify("senha123", hasher.Hash("senha123")));
        }

        [Fact]
        public void Verify_falha_quando_a_senha_esta_errada()
        {
            var hasher = ComPepper("pepper-test");

            Assert.False(hasher.Verify("senha-errada", hasher.Hash("senha123")));
        }

        [Fact]
        public void Hash_gerado_com_outro_pepper_nao_valida()
        {
            var hashDoOutroServidor = ComPepper("pepper-do-servidor-A").Hash("senha123");

            Assert.False(ComPepper("pepper-do-servidor-B").Verify("senha123", hashDoOutroServidor));
        }

        [Fact]
        public void Hash_da_mesma_senha_gera_valores_diferentes()
        {
            var hasher = ComPepper("pepper-test");

            Assert.NotEqual(hasher.Hash("senha123"), hasher.Hash("senha123"));
        }

        [Fact]
        public void Verify_devolve_false_quando_o_hash_e_invalido()
        {
            var hasher = ComPepper("pepper-test");

            Assert.False(hasher.Verify("senha123", "isto-nao-e-um-hash-bcrypt"));
        }
    }
}
