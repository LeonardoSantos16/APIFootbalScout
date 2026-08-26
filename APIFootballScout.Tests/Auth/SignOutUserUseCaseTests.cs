
namespace APIFootballScout.Tests.Auth
{
    public class SignOutUserUseCaseTests
    {
        [Fact]
        public async Task Execute_revoga_o_token_apresentado()
        {
            // Arrange
            var ctx = new AuthTestContext();
            var (usuario, sessao) = await ctx.SeedUsuarioAsync();

            // Act
            await ctx.NovoSignOut().Execute(usuario.Id, sessao.RefreshToken);

            // Assert
            var token = ctx.Tokens.PorToken(sessao.RefreshToken)!;
            Assert.Equal(ctx.Agora, token.RevokedAtUtc);
            Assert.Empty(ctx.Tokens.Ativos);

            // logout nao e rotacao: sem substituto
            Assert.Null(token.ReplacedByHash);
        }

        [Fact]
        public async Task Execute_com_token_de_outro_usuario_nao_revoga_nada()
        {
            // Arrange
            var ctx = new AuthTestContext();
            var (_, sessao) = await ctx.SeedUsuarioAsync();
            var intruso = Guid.NewGuid();

            // Act: token valido, mas apresentado por quem nao e o dono
            await ctx.NovoSignOut().Execute(intruso, sessao.RefreshToken);

            // Assert
            Assert.Null(ctx.Tokens.PorToken(sessao.RefreshToken)!.RevokedAtUtc);
            Assert.Single(ctx.Tokens.Ativos);
        }

        [Fact]
        public async Task Execute_com_token_inexistente_nao_lanca()
        {
            // Arrange
            var ctx = new AuthTestContext();
            var (usuario, _) = await ctx.SeedUsuarioAsync();

            // Act: sai em silencio de proposito, para nao revelar se o token existe
            await ctx.NovoSignOut().Execute(usuario.Id, "rt-que-nunca-existiu");

            // Assert
            Assert.Single(ctx.Tokens.Ativos);
        }

        [Fact]
        public async Task Execute_em_token_ja_revogado_preserva_a_data_original()
        {
            // Arrange
            var ctx = new AuthTestContext();
            var (usuario, sessao) = await ctx.SeedUsuarioAsync();
            var signOut = ctx.NovoSignOut();
            await signOut.Execute(usuario.Id, sessao.RefreshToken);
            var revogadoEm = ctx.Agora;

            ctx.Time.Advance(TimeSpan.FromMinutes(30));

            // Act
            await signOut.Execute(usuario.Id, sessao.RefreshToken);

            // Assert
            Assert.Equal(revogadoEm, ctx.Tokens.PorToken(sessao.RefreshToken)!.RevokedAtUtc);
        }

        [Fact]
        public async Task ExecuteTodasSessoes_revoga_todos_os_tokens_do_usuario()
        {
            // Arrange
            var ctx = new AuthTestContext();
            var (usuario, primeira) = await ctx.SeedUsuarioAsync();
            var segunda = await ctx.Issuer.EmitirAsync(usuario); 

            // Act
            await ctx.NovoSignOut().ExecuteTodasSessoes(usuario.Id);

            // Assert
            Assert.Empty(ctx.Tokens.Ativos);
            Assert.Equal(ctx.Agora, ctx.Tokens.PorToken(primeira.RefreshToken)!.RevokedAtUtc);
            Assert.Equal(ctx.Agora, ctx.Tokens.PorToken(segunda.RefreshToken)!.RevokedAtUtc);
        }
    }
}
