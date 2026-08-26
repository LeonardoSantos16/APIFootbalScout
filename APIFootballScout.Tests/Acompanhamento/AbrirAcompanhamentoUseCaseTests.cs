using APIFootballScout.Domain.Acompanhamento.Aggregate;
using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Tests.Acompanhamento
{
    public class AbrirAcompanhamentoUseCaseTests
    {
        [Fact]
        public async Task Persitir_dossie_retorna_DossieId_e_AbertoEm()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext();
            var perfil = ctx.SeedPerfil();

            // Act
            var resultado = await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            // Assert
            var dossie = Assert.Single(ctx.Dossies.Todos);
            Assert.Equal(dossie.Id, resultado.DossieId);
            Assert.Equal(dossie.AbertoEm, resultado.AbertoEm);
            Assert.Equal(perfil.LidoEm, resultado.MedidaEm);

            Assert.Equal(ctx.OlheiroId, dossie.OlheiroId);
            Assert.Equal(AcompanhamentoTestContext.JogadorId, dossie.JogadorId);
            Assert.Equal(StatusDossie.Ativo, dossie.Status);
            Assert.Null(dossie.EncerradoEm);

            Assert.Equal(AcompanhamentoTestContext.RecortePadrao, ctx.Catalogo.UltimoRecorte);
        }

        [Fact]
        public async Task Jogador_ja_acompanhado()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext();
            ctx.SeedPerfil();
            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            // Act
            var excecao = await Assert.ThrowsAsync<ConflitoDeDominioException>(
              () => ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None));

            // Assert
            Assert.Equal("acompanhamento.jogador_ja_acompanhado", excecao.Codigo);

            Assert.Single(ctx.Dossies.Todos);
        }

        [Fact]
        public async Task Limite_de_acompanhamentos_atingido_recusa_abertura()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext { Limite = 2 };
            ctx.SeedPerfil();

            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(jogadorId: 42), CancellationToken.None);
            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(jogadorId: 43), CancellationToken.None);

            // Act
            var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(jogadorId: 44), CancellationToken.None));

            // Assert
            Assert.Equal("acompanhamento.limite_atingido", excecao.Codigo);

            Assert.Equal(2, ctx.Dossies.Todos.Count);
        }

        [Theory]
        [InlineData("", "F", "Santos")]
        [InlineData("   ", "F", "Santos")]
        [InlineData("Neymar", null, "Santos")]
        [InlineData("Neymar", "   ", "Santos")]
        [InlineData("Neymar", "F", null)]
        [InlineData("Neymar", "F", "   ")]
        public async Task Perfil_sem_informacoes_minimas_recusa_abertura(string nome, string? posicao, string? clube)
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext();
            ctx.Catalogo.Perfil = ctx.PerfilValido() with { Nome = nome, Posicao = posicao, Clube = clube };

            // Act
            var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None));

            // Assert
            Assert.Equal("jogador.informacoes_insuficientes", excecao.Codigo);

            Assert.Empty(ctx.Dossies.Todos);
        }

        [Fact]
        public async Task Reacompanhar_apos_encerrar_cria_nova_linha_de_base_e_preserva_a_anterior()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext();
            var perfilAntigo = ctx.SeedPerfil(clube: "Santos");

            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            var primeiro = Assert.Single(ctx.Dossies.Todos);
            primeiro.Encerrar(primeiro.AbertoEm.AddDays(30));

            var perfilAtual = ctx.SeedPerfil(clube: "Al-Hilal", lidoEm: ctx.Agora);

            // Act
            var resultado = await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            // Assert
            Assert.Equal(2, ctx.Dossies.Todos.Count);

            var encerrado = Assert.Single(ctx.Dossies.Todos, d => d.Status is StatusDossie.Encerrado);
            Assert.Equal(primeiro.Id, encerrado.Id);
            Assert.Equal(perfilAntigo.Clube, encerrado.LinhaDeBase.Clube);
            Assert.Equal(perfilAntigo.LidoEm, encerrado.LinhaDeBase.MedidaEm);

            var novo = Assert.Single(ctx.Dossies.Todos, d => d.Status is StatusDossie.Ativo);
            Assert.Equal(resultado.DossieId, novo.Id);
            Assert.NotEqual(primeiro.Id, novo.Id);
            Assert.Equal(perfilAtual.Clube, novo.LinhaDeBase.Clube);
            Assert.Equal(perfilAtual.LidoEm, novo.LinhaDeBase.MedidaEm);
            Assert.Null(novo.EncerradoEm);
        }

        [Fact]
        public async Task Perfil_nao_encontrado_recusa_abertura()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext();
            ctx.Catalogo.Perfil = null;

            // Act
            var excecao = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None));

            // Assert
            Assert.Equal("jogador.perfil_nao_encontrado", excecao.Codigo);

            Assert.Empty(ctx.Dossies.Todos);
        }

        [Fact]
        public async Task Dois_olheiros_acompanham_o_mesmo_jogador_com_dossies_independentes()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext();
            var outroOlheiro = Guid.NewGuid();

            var perfilDoPrimeiro = ctx.SeedPerfil(clube: "Santos");
            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            var perfilDoSegundo = ctx.SeedPerfil(clube: "Al-Hilal", lidoEm: ctx.Agora);

            // Act
            await ctx.AbrirDossie().AbrirAcompanhamento(
                ctx.Pedido(olheiroId: outroOlheiro), CancellationToken.None);

            // Assert
            Assert.Equal(2, ctx.Dossies.Todos.Count);

            var doPrimeiro = Assert.Single(ctx.Dossies.Todos, d => d.OlheiroId == ctx.OlheiroId);
            var doSegundo = Assert.Single(ctx.Dossies.Todos, d => d.OlheiroId == outroOlheiro);

            Assert.Equal(AcompanhamentoTestContext.JogadorId, doPrimeiro.JogadorId);
            Assert.Equal(AcompanhamentoTestContext.JogadorId, doSegundo.JogadorId);
            Assert.NotEqual(doPrimeiro.Id, doSegundo.Id);

            // Cada dossie tem sua propria linha de base.
            Assert.Equal(perfilDoPrimeiro.Clube, doPrimeiro.LinhaDeBase.Clube);
            Assert.Equal(perfilDoPrimeiro.LidoEm, doPrimeiro.LinhaDeBase.MedidaEm);
            Assert.Equal(perfilDoSegundo.Clube, doSegundo.LinhaDeBase.Clube);
            Assert.Equal(perfilDoSegundo.LidoEm, doSegundo.LinhaDeBase.MedidaEm);
        }

        [Fact]
        public async Task Limite_de_acompanhamentos_e_contado_por_olheiro()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext { Limite = 1 };
            ctx.SeedPerfil();

            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            // Act — o segundo olheiro nao consome a cota do primeiro.
            await ctx.AbrirDossie().AbrirAcompanhamento(
                ctx.Pedido(olheiroId: Guid.NewGuid()), CancellationToken.None);

            // Assert
            Assert.Equal(2, ctx.Dossies.Todos.Count);
        }
    }
}
