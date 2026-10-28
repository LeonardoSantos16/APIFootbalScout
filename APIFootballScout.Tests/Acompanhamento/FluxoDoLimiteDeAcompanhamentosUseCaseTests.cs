using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Tests.Acompanhamento
{
    public class FluxoDoLimiteDeAcompanhamentosUseCaseTests
    {
        [Fact]
        public async Task Aberturas_simultaneas_nao_ultrapassam_o_limite()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext { Limite = 1 };
            ctx.SeedPerfil();
            var catalogoLiberado = new TaskCompletionSource();
            ctx.Catalogo.Bloqueio = catalogoLiberado.Task;

            // Act
            var primeira = ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(jogadorId: 42), CancellationToken.None);
            var segunda = ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(jogadorId: 43), CancellationToken.None);
            catalogoLiberado.SetResult();

            // Assert
            await primeira;
            var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() => segunda);
            Assert.Equal("acompanhamento.limite_atingido", excecao.Codigo);

            Assert.Single(ctx.Dossies.Todos);
            Assert.Equal(1, ctx.Dossies.VagasOcupadas(ctx.OlheiroId));
        }

        [Fact]
        public async Task Abertura_ocupa_uma_vaga_do_olheiro()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext();
            ctx.SeedPerfil();

            // Act
            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            // Assert
            Assert.Equal(1, ctx.Dossies.VagasOcupadas(ctx.OlheiroId));
        }

        [Fact]
        public async Task Perfil_nao_encontrado_devolve_a_vaga_reservada()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext { Limite = 1 };
            ctx.Catalogo.Perfil = null;

            // Act
            await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None));

            // Assert
            Assert.Equal(0, ctx.Dossies.VagasOcupadas(ctx.OlheiroId));
        }

        [Fact]
        public async Task Perfil_sem_informacoes_minimas_devolve_a_vaga_reservada()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext { Limite = 1 };
            ctx.Catalogo.Perfil = ctx.PerfilValido() with { Clube = null };

            // Act
            await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None));

            // Assert
            Assert.Equal(0, ctx.Dossies.VagasOcupadas(ctx.OlheiroId));
        }

        [Fact]
        public async Task Falha_ao_gravar_o_dossie_devolve_a_vaga_reservada()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext { Limite = 1 };
            ctx.SeedPerfil();
            ctx.Dossies.FalhaNaInsercao = new InvalidOperationException("falha ao gravar");

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None));

            // Assert
            Assert.Equal(0, ctx.Dossies.VagasOcupadas(ctx.OlheiroId));
        }

        [Fact]
        public async Task Vaga_devolvida_permite_nova_abertura_no_limite()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext { Limite = 1 };
            ctx.Catalogo.Perfil = null;
            await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None));
            ctx.SeedPerfil();

            // Act
            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            // Assert
            Assert.Single(ctx.Dossies.Todos);
            Assert.Equal(1, ctx.Dossies.VagasOcupadas(ctx.OlheiroId));
        }
    }
}