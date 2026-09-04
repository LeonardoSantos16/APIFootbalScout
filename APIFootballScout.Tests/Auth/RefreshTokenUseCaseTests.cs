using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Infrastructure.Security;

namespace APIFootballScout.Tests.Auth
{
    public class RefreshTokenUseCaseTests
    {
        [Fact]
        public async Task Execute_rotaciona_e_revoga_o_anterior_apontando_o_substituto()
        {
            // Arrange
            var ctx = new AuthTestContext();
            var (_, sessao) = await ctx.SeedUsuarioAsync();
            var anterior = ctx.Tokens.PorToken(sessao.RefreshToken)!;

            ctx.Time.Advance(TimeSpan.FromMinutes(5));

            // Act
            var novaSessao = await ctx.NovoRefresh().Execute(sessao.RefreshToken);

            // Assert
            Assert.NotEqual(sessao.RefreshToken, novaSessao.RefreshToken);

            Assert.Equal(ctx.Agora, anterior.RevokedAtUtc);

            Assert.Equal(RefreshTokenHasher.Hash(novaSessao.RefreshToken), anterior.ReplacedByHash);

            var ativo = Assert.Single(ctx.Tokens.Ativos);
            Assert.Equal(RefreshTokenHasher.Hash(novaSessao.RefreshToken), ativo.TokenHash);
        }

        [Fact]
        public async Task Execute_com_token_ja_rotacionado_lanca_e_derruba_todas_as_sessoes()
        {
            // Arrange
            var ctx = new AuthTestContext();
            var (usuario, sessaoInicial) = await ctx.SeedUsuarioAsync();

            var outroDispositivo = await ctx.Issuer.EmitirAsync(usuario);

            var refresh = ctx.NovoRefresh();
            var sessaoRotacionada = await refresh.Execute(sessaoInicial.RefreshToken);

            ctx.Time.Advance(TimeSpan.FromMinutes(10));

            var excecao = await Assert.ThrowsAsync<NaoAutenticadoException>(
                () => refresh.Execute(sessaoInicial.RefreshToken));

            // Assert
            Assert.Equal("usuario.refresh_token_invalido", excecao.Codigo);

            // a cascata derruba tudo: a cadeia rotacionada E a sessao do outro dispositivo
            Assert.Empty(ctx.Tokens.Ativos);
            Assert.Equal(ctx.Agora, ctx.Tokens.PorToken(sessaoRotacionada.RefreshToken)!.RevokedAtUtc);
            Assert.Equal(ctx.Agora, ctx.Tokens.PorToken(outroDispositivo.RefreshToken)!.RevokedAtUtc);

            // o rastro da rotacao original sobrevive a cascata
            var reutilizado = ctx.Tokens.PorToken(sessaoInicial.RefreshToken)!;
            Assert.Equal(RefreshTokenHasher.Hash(sessaoRotacionada.RefreshToken), reutilizado.ReplacedByHash);
            Assert.NotEqual(ctx.Agora, reutilizado.RevokedAtUtc);
        }

        [Fact]
        public async Task Execute_com_token_expirado_lanca_sem_derrubar_as_outras_sessoes()
        {
            // Arrange
            var ctx = new AuthTestContext();
            var (usuario, sessao) = await ctx.SeedUsuarioAsync();
            var outroDispositivo = await ctx.Issuer.EmitirAsync(usuario);

            // RefreshTokenDays = 7
            ctx.Time.Advance(TimeSpan.FromDays(8));

            // Act
            var excecao = await Assert.ThrowsAsync<NaoAutenticadoException>(
                () => ctx.NovoRefresh().Execute(sessao.RefreshToken));

            // Assert
            Assert.Equal("usuario.refresh_token_invalido", excecao.Codigo);

            // expirar nao e sinal de vazamento: nada e revogado, nem o proprio token
            Assert.Null(ctx.Tokens.PorToken(sessao.RefreshToken)!.RevokedAtUtc);
            Assert.Null(ctx.Tokens.PorToken(outroDispositivo.RefreshToken)!.RevokedAtUtc);
            Assert.Equal(2, ctx.Tokens.Ativos.Count());
        }
    }
}
