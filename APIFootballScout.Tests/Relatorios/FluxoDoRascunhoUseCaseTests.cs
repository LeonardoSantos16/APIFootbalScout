using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Tests.Relatorios
{
    /// <summary>
    /// F5 - o fluxo do rascunho: abrir, editar e finalizar.
    /// As invariantes sao provadas no dominio (ver RelatorioTests). Aqui se prova
    /// o que elas causam na fronteira da aplicacao: o que foi persistido, o que
    /// nao foi, e qual codigo de erro atravessou.
    /// </summary>
    public class FluxoDoRascunhoUseCaseTests
    {
        [Fact]
        public async Task O_rascunho_percorre_abertura_edicao_e_finalizacao()
        {
            // Arrange
            var ctx = new RelatorioTestContext();

            // Act
            var aberto = await ctx.AbrirRascunho().AbrirRascunho(
                ctx.PedidoDeAbertura(), CancellationToken.None);

            await ctx.EditarRascunho().EditarRascunho(
                ctx.PedidoDeEdicao(
                    aberto.RelatorioId,
                    texto: "Reavaliado apos o classico.",
                    nota: 8.5m,
                    pontosPositivos: ["Leitura de jogo"],
                    pontosNegativos: ["Fragilidade defensiva"],
                    parecer: Parecer.Contratar),
                CancellationToken.None);

            ctx.Time.Advance(TimeSpan.FromHours(2));

            var finalizado = await ctx.Finalizar().FinalizarRelatorio(
                ctx.PedidoDeFinalizacao(aberto.RelatorioId), CancellationToken.None);

            // Assert - um unico relatorio atravessou o fluxo inteiro.
            var persistido = ctx.Unico();
            Assert.Equal(aberto.RelatorioId, persistido.Id);
            Assert.Equal(ctx.OlheiroId, persistido.OlheiroId);
            Assert.Equal(RelatorioTestContext.JogadorId, persistido.JogadorId);

            Assert.Equal(StatusRelatorio.Rascunho, aberto.Status);
            Assert.Null(aberto.FinalizadoEm);

            Assert.Equal(StatusRelatorio.Finalizado, persistido.Status);
            Assert.Equal("Reavaliado apos o classico.", persistido.Texto);
            Assert.Equal(new Nota(8.5m), persistido.Nota);
            Assert.Equal(["Leitura de jogo"], persistido.PontosPositivos);
            Assert.Equal(["Fragilidade defensiva"], persistido.PontosNegativos);
            Assert.Equal(Parecer.Contratar, persistido.Parecer);

            // A data da observacao nao se confunde com a da finalizacao (R5.5).
            Assert.Equal(RelatorioTestContext.ObservadoEm, persistido.ObservadoEm);
            Assert.Equal(ctx.Agora, persistido.FinalizadoEm);
            Assert.Equal(ctx.Agora, finalizado.FinalizadoEm);
            Assert.Equal(StatusRelatorio.Finalizado, finalizado.Status);
        }

        [Fact]
        public async Task A_finalizacao_sem_conteudo_minimo_nao_persiste()
        {
            // Arrange - o minimo e politica de configuracao, nao do dominio.
            var ctx = new RelatorioTestContext { MinimoDeContras = 2 };

            var relatorioId = await ctx.RascunhoConcluido();

            // Act
            var erro = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => ctx.Finalizar().FinalizarRelatorio(
                    ctx.PedidoDeFinalizacao(relatorioId), CancellationToken.None));

            // Assert
            Assert.Equal("relatorio.conteudo_minimo_nao_atendido", erro.Codigo);
            Assert.Equal(StatusRelatorio.Rascunho, ctx.Unico().Status);
            Assert.Null(ctx.Unico().FinalizadoEm);
        }

        [Fact]
        public async Task A_finalizacao_sem_nota_ou_parecer_nao_persiste()
        {
            // Arrange - rascunho aberto e nunca concluido.
            var ctx = new RelatorioTestContext();

            var aberto = await ctx.AbrirRascunho().AbrirRascunho(
                ctx.PedidoDeAbertura(), CancellationToken.None);

            // Act
            var erro = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => ctx.Finalizar().FinalizarRelatorio(
                    ctx.PedidoDeFinalizacao(aberto.RelatorioId), CancellationToken.None));

            // Assert
            Assert.Equal("relatorio.conclusao_ausente", erro.Codigo);
            Assert.Equal(StatusRelatorio.Rascunho, ctx.Unico().Status);
        }

        [Fact]
        public async Task A_edicao_de_relatorio_finalizado_nao_persiste()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var relatorioId = await ctx.RelatorioFinalizado();
            var atualizacoesAteAqui = ctx.Relatorios.Atualizacoes;

            // Act
            var erro = await Assert.ThrowsAsync<ConflitoDeDominioException>(
                () => ctx.EditarRascunho().EditarRascunho(
                    ctx.PedidoDeEdicao(relatorioId, texto: "outro texto", nota: 2m),
                    CancellationToken.None));

            // Assert
            Assert.Equal("relatorio.ja_finalizado", erro.Codigo);
            Assert.Equal(atualizacoesAteAqui, ctx.Relatorios.Atualizacoes);

            var persistido = ctx.Unico();
            Assert.Equal(RelatorioTestContext.TextoInicial, persistido.Texto);
            Assert.Equal(new Nota(8.5m), persistido.Nota);
        }

        [Fact]
        public async Task A_finalizacao_de_relatorio_ja_finalizado_e_recusada()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var relatorioId = await ctx.RelatorioFinalizado();
            var finalizadoEm = ctx.Unico().FinalizadoEm;

            ctx.Time.Advance(TimeSpan.FromDays(1));

            // Act
            var erro = await Assert.ThrowsAsync<ConflitoDeDominioException>(
                () => ctx.Finalizar().FinalizarRelatorio(
                    ctx.PedidoDeFinalizacao(relatorioId), CancellationToken.None));

            // Assert - a data da primeira finalizacao nao e reescrita.
            Assert.Equal("relatorio.ja_finalizado", erro.Codigo);
            Assert.Equal(finalizadoEm, ctx.Unico().FinalizadoEm);
        }

        [Fact]
        public async Task O_olheiro_nao_alcanca_o_relatorio_de_outro()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var relatorioId = await ctx.RascunhoConcluido();
            var intruso = Guid.NewGuid();

            // Act
            var naEdicao = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => ctx.EditarRascunho().EditarRascunho(
                    ctx.PedidoDeEdicao(relatorioId, olheiroId: intruso, texto: "outro texto"),
                    CancellationToken.None));

            var naFinalizacao = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => ctx.Finalizar().FinalizarRelatorio(
                    ctx.PedidoDeFinalizacao(relatorioId, olheiroId: intruso), CancellationToken.None));

            // Assert - o relatorio de outro olheiro e indistinguivel de inexistente.
            Assert.Equal("relatorio.nao_encontrado", naEdicao.Codigo);
            Assert.Equal("relatorio.nao_encontrado", naFinalizacao.Codigo);

            var persistido = ctx.Unico();
            Assert.Equal(RelatorioTestContext.TextoInicial, persistido.Texto);
            Assert.Equal(StatusRelatorio.Rascunho, persistido.Status);
        }

        [Fact]
        public async Task Relatorio_inexistente_recusa()
        {
            // Arrange
            var ctx = new RelatorioTestContext();

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => ctx.EditarRascunho().EditarRascunho(
                    ctx.PedidoDeEdicao(Guid.NewGuid(), texto: "outro texto"),
                    CancellationToken.None));

            // Assert
            Assert.Equal("relatorio.nao_encontrado", erro.Codigo);
            Assert.Empty(ctx.Relatorios.Todos);
        }
    }
}
