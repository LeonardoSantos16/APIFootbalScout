using APIFootballScout.Application.User;
using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Tests.Auth
{
    public class SignUpUserUseCaseTests
    {
        [Fact]
        public async Task Execute_com_email_duplicado_lanca_conflito()
        {
            var ctx = new AuthTestContext();
            await ctx.SeedUsuarioAsync(email: "leo@mail.com");

            var excecao = await Assert.ThrowsAsync<ConflitoDeDominioException>(
                () => ctx.NovoSignUp().Execute(
                    new SignUpUserRequest("Outro Leo", "  LEO@Mail.com ", "outra-senha-123")));

            // Assert
            Assert.Equal("usuario.email_ja_cadastrado", excecao.Codigo);

            Assert.Single(ctx.Users.Todos);
        }

        [Fact]
        public async Task Execute_normaliza_o_email_e_apara_o_nome()
        {
            var ctx = new AuthTestContext();

            // Act
            var sessao = await ctx.NovoSignUp().Execute(
                new SignUpUserRequest("  Leo  ", "  LEO@Mail.com ", "senha123"));

            // Assert
            var salvo = Assert.Single(ctx.Users.Todos);
            Assert.Equal("leo@mail.com", salvo.Email);
            Assert.Equal("Leo", salvo.Name);
            Assert.Equal("leo@mail.com", sessao.Email);

            Assert.NotEqual("senha123", salvo.PasswordHash);
            Assert.True(ctx.Hasher.Verify("senha123", salvo.PasswordHash));

            Assert.Single(ctx.Tokens.Ativos);
        }
    }
}
