using APIFootballScout.Infrastructure.Security;

namespace APIFootballScout.Tests.Auth
{
    public class ChangePasswordUseCaseTests
    {
        private const string SenhaAtual = "senha-atual-123";
        private const string SenhaNova = "senha-nova-456";

        [Fact]
        public async Task Execute_troca_a_senha_e_a_antiga_para_de_valer()
        {
            // Arrange
            var ctx = new AuthTestContext();
            var (usuario, _) = await ctx.SeedUsuarioAsync(senha: SenhaAtual);
            var hashAntigo = usuario.PasswordHash;

            // Act
            await ctx.NovoChangePassword().Execute(usuario.Id, SenhaAtual, SenhaNova);

            // Assert
            var persistido = (await ctx.Users.ObterPorIdAsync(usuario.Id))!;

            Assert.False(ctx.Hasher.Verify(SenhaAtual, persistido.PasswordHash));
            Assert.True(ctx.Hasher.Verify(SenhaNova, persistido.PasswordHash));
            Assert.NotEqual(hashAntigo, persistido.PasswordHash);
        }

        [Fact]
        public async Task Execute_revoga_todas_as_sessoes_emite_uma_nova_e_regenera_o_stamp()
        {
            // Arrange
            var ctx = new AuthTestContext();
            var (usuario, sessaoAtual) = await ctx.SeedUsuarioAsync(senha: SenhaAtual);
            var outroDispositivo = await ctx.Issuer.EmitirAsync(usuario);
            var stampAntigo = usuario.SecurityStamp;

            ctx.Time.Advance(TimeSpan.FromMinutes(5));

            // Act
            var novaSessao = await ctx.NovoChangePassword().Execute(usuario.Id, SenhaAtual, SenhaNova);

            // Assert
            Assert.Equal(ctx.Agora, ctx.Tokens.PorToken(sessaoAtual.RefreshToken)!.RevokedAtUtc);
            Assert.Equal(ctx.Agora, ctx.Tokens.PorToken(outroDispositivo.RefreshToken)!.RevokedAtUtc);

            var ativo = Assert.Single(ctx.Tokens.Ativos);
            Assert.Equal(RefreshTokenHasher.Hash(novaSessao.RefreshToken), ativo.TokenHash);

            var persistido = (await ctx.Users.ObterPorIdAsync(usuario.Id))!;
            Assert.NotEqual(stampAntigo, persistido.SecurityStamp);
            Assert.Equal(persistido.SecurityStamp, ctx.TokenService.LastSubject!.SecurityStamp);
        }
    }
}
