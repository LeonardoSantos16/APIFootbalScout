using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Tests.Relatorios
{
    /// <summary>
    /// F5 - o fluxo da correcao (R5.3). O dominio ja prova o que a correcao herda
    /// e o que ela nao herda (ver CorrecaoDeRelatorioTests). Aqui se prova o efeito
    /// na fronteira: dois relatorios passam a coexistir no repositorio, o original
    /// permanece intacto, e a recusa nao cria nada.
    /// </summary>
    public class FluxoDaCorrecaoUseCaseTests
    {
        [Fact]
        public async Task A_correcao_cria_novo_relatorio_e_preserva_o_original()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var originalId = await ctx.RelatorioFinalizado();
            var atualizacoesAntes = ctx.Relatorios.Atualizacoes;

            ctx.Time.Advance(TimeSpan.FromDays(2));

            // Act
            var correcao = await ctx.Corrigir().CorrigirRelatorio(
                ctx.PedidoDeCorrecao(originalId), CancellationToken.None);

            // Assert - dois relatorios, nao uma substituicao.
            Assert.Equal(2, ctx.Relatorios.Todos.Count);
            Assert.NotEqual(originalId, correcao.RelatorioId);

            var nova = ctx.Achar(correcao.RelatorioId);
            Assert.Equal(originalId, nova.CorrigeRelatorioId);
            Assert.Equal(StatusRelatorio.Rascunho, nova.Status);
            Assert.Equal(RelatorioTestContext.TextoDaCorrecao, nova.Texto);
            Assert.Equal(ctx.Agora, nova.EscritoEm);

            // O original nao foi tocado: e o registro historico que a correcao cita.
            var original = ctx.Achar(originalId);
            Assert.Equal(StatusRelatorio.Finalizado, original.Status);
            Assert.Equal(RelatorioTestContext.TextoInicial, original.Texto);
            Assert.Null(original.CorrigeRelatorioId);

            // Corrigir e acrescentar, nunca reescrever o corrigido.
            Assert.Equal(atualizacoesAntes, ctx.Relatorios.Atualizacoes);
        }

        [Fact]
        public async Task A_correcao_de_rascunho_nao_persiste()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var rascunhoId = await ctx.RascunhoConcluido();

            // Act
            var erro = await Assert.ThrowsAsync<ConflitoDeDominioException>(
                () => ctx.Corrigir().CorrigirRelatorio(
                    ctx.PedidoDeCorrecao(rascunhoId), CancellationToken.None));

            // Assert
            Assert.Equal("relatorio.correcao_de_rascunho", erro.Codigo);
            Assert.Single(ctx.Relatorios.Todos);
        }

        [Fact]
        public async Task A_correcao_do_relatorio_de_outro_olheiro_nao_persiste()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var originalId = await ctx.RelatorioFinalizado();

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => ctx.Corrigir().CorrigirRelatorio(
                    ctx.PedidoDeCorrecao(originalId, olheiroId: Guid.NewGuid()),
                    CancellationToken.None));

            // Assert - ninguem corrige o relatorio alheio (R5.6).
            Assert.Equal("relatorio.nao_encontrado", erro.Codigo);
            Assert.Single(ctx.Relatorios.Todos);
        }

        [Fact]
        public async Task A_correcao_nasce_editavel_e_finalizavel()
        {
            // A correcao e um relatorio de verdade: percorre o mesmo fluxo do
            // rascunho, e o elo sobrevive a finalizacao.

            // Arrange
            var ctx = new RelatorioTestContext();
            var originalId = await ctx.RelatorioFinalizado();

            var correcao = await ctx.Corrigir().CorrigirRelatorio(
                ctx.PedidoDeCorrecao(originalId), CancellationToken.None);

            // Act
            await ctx.EditarRascunho().EditarRascunho(
                ctx.PedidoDeEdicao(
                    correcao.RelatorioId,
                    nota: 6m,
                    pontosPositivos: ["Leitura de jogo"],
                    parecer: Parecer.Monitorar),
                CancellationToken.None);

            var finalizada = await ctx.Finalizar().FinalizarRelatorio(
                ctx.PedidoDeFinalizacao(correcao.RelatorioId), CancellationToken.None);

            // Assert
            Assert.Equal(StatusRelatorio.Finalizado, finalizada.Status);
            Assert.Equal(originalId, finalizada.CorrigeRelatorioId);
            Assert.Equal(6m, finalizada.Nota);
            Assert.Equal(Parecer.Monitorar, finalizada.Parecer);

            // A conclusao do original nao contamina a da correcao.
            var original = ctx.Achar(originalId);
            Assert.Equal(new Nota(8.5m), original.Nota);
            Assert.Equal(Parecer.Contratar, original.Parecer);
        }

        [Fact]
        public async Task A_correcao_da_correcao_referencia_a_ultima()
        {
            // A cadeia e encadeada, nao achatada: cada correcao aponta para o
            // relatorio que ela corrige, e nao para a origem da cadeia.

            // Arrange
            var ctx = new RelatorioTestContext();
            var primeiroId = await ctx.RelatorioFinalizado();

            var segunda = await ctx.Corrigir().CorrigirRelatorio(
                ctx.PedidoDeCorrecao(primeiroId), CancellationToken.None);

            await ctx.ConcluirEFinalizar(segunda.RelatorioId);

            // Act
            var terceira = await ctx.Corrigir().CorrigirRelatorio(
                ctx.PedidoDeCorrecao(segunda.RelatorioId, texto: "Terceira leitura."),
                CancellationToken.None);

            // Assert
            Assert.Equal(3, ctx.Relatorios.Todos.Count);
            Assert.Equal(segunda.RelatorioId, terceira.CorrigeRelatorioId);
            Assert.Equal(primeiroId, ctx.Achar(segunda.RelatorioId).CorrigeRelatorioId);
            Assert.Null(ctx.Achar(primeiroId).CorrigeRelatorioId);
        }
    }
}
