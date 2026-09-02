using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Tests.Relatorios
{
    /// <summary>
    /// F5 - o fluxo da consulta (R5.6). A leitura e escopada ao olheiro: o dossie
    /// pertence ao olheiro e nao ha visao consolidada entre olheiros. E o ponto em
    /// que R5.6 se torna observavel - dois olheiros sobre o mesmo jogador produzem
    /// dois relatorios que coexistem, e nenhuma leitura os funde.
    /// </summary>
    public class FluxoDaConsultaUseCaseTests
    {
        [Fact]
        public async Task A_consulta_por_id_devolve_o_relatorio_inteiro()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var relatorioId = await ctx.RelatorioFinalizado();

            // Act
            var lido = await ctx.Obter().ObterRelatorio(
                ctx.PedidoDeConsulta(relatorioId), CancellationToken.None);

            // Assert
            Assert.Equal(relatorioId, lido.RelatorioId);
            Assert.Equal(RelatorioTestContext.JogadorId, lido.JogadorId);
            Assert.Equal(StatusRelatorio.Finalizado, lido.Status);
            Assert.Equal(8.5m, lido.Nota);
            Assert.Equal(Parecer.Contratar, lido.Parecer);
            Assert.Equal(["Leitura de jogo"], lido.PontosPositivos);
            Assert.Equal(["Fragilidade defensiva"], lido.PontosNegativos);

            // As duas datas chegam separadas ate a leitura (R5.5).
            Assert.Equal(RelatorioTestContext.ObservadoEm, lido.ObservadoEm);
            Assert.Equal(ctx.Agora, lido.EscritoEm);
            Assert.Equal(ctx.Agora, lido.FinalizadoEm);
        }

        [Fact]
        public async Task A_consulta_do_relatorio_de_outro_olheiro_recusa()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var relatorioId = await ctx.RelatorioFinalizado();

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => ctx.Obter().ObterRelatorio(
                    ctx.PedidoDeConsulta(relatorioId, olheiroId: Guid.NewGuid()),
                    CancellationToken.None));

            // Assert - o relatorio alheio e indistinguivel do inexistente.
            Assert.Equal("relatorio.nao_encontrado", erro.Codigo);
        }

        [Fact]
        public async Task A_consulta_de_relatorio_inexistente_recusa()
        {
            // Arrange
            var ctx = new RelatorioTestContext();

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => ctx.Obter().ObterRelatorio(
                    ctx.PedidoDeConsulta(Guid.NewGuid()), CancellationToken.None));

            // Assert
            Assert.Equal("relatorio.nao_encontrado", erro.Codigo);
        }

        [Fact]
        public async Task A_listagem_traz_rascunhos_e_finalizados()
        {
            // O rascunho e do olheiro tanto quanto o finalizado: a listagem e a
            // mesa de trabalho dele, nao so o que ja fechou.

            // Arrange
            var ctx = new RelatorioTestContext();
            var finalizadoId = await ctx.RelatorioFinalizado();
            var rascunho = await ctx.AbrirRascunho().AbrirRascunho(
                ctx.PedidoDeAbertura(), CancellationToken.None);

            // Act
            var lista = await ctx.Listar().ListarRelatorios(
                ctx.PedidoDeListagem(), CancellationToken.None);

            // Assert
            Assert.Equal(2, lista.Count);
            Assert.Contains(lista, r => r.RelatorioId == finalizadoId
                                     && r.Status == StatusRelatorio.Finalizado);
            Assert.Contains(lista, r => r.RelatorioId == rascunho.RelatorioId
                                     && r.Status == StatusRelatorio.Rascunho);
        }

        [Fact]
        public async Task A_listagem_nao_funde_relatorios_de_olheiros_distintos()
        {
            // R5.6 - dois olheiros sobre o mesmo jogador. Os relatorios coexistem
            // no repositorio, e cada olheiro enxerga apenas o seu.

            // Arrange
            var ctx = new RelatorioTestContext();
            var outroOlheiro = Guid.NewGuid();

            var meuId = await ctx.RelatorioFinalizado();
            var alheioId = await ctx.RelatorioFinalizado(outroOlheiro);

            // Act
            var minha = await ctx.Listar().ListarRelatorios(
                ctx.PedidoDeListagem(), CancellationToken.None);
            var alheia = await ctx.Listar().ListarRelatorios(
                ctx.PedidoDeListagem(olheiroId: outroOlheiro), CancellationToken.None);

            // Assert - os dois existem, mas nenhuma leitura os junta.
            Assert.Equal(2, ctx.Relatorios.Todos.Count);
            Assert.Equal(meuId, Assert.Single(minha).RelatorioId);
            Assert.Equal(alheioId, Assert.Single(alheia).RelatorioId);
        }

        [Fact]
        public async Task A_listagem_so_traz_o_jogador_pedido()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var doJogador = await ctx.AbrirRascunho().AbrirRascunho(
                ctx.PedidoDeAbertura(), CancellationToken.None);

            await ctx.AbrirRascunho().AbrirRascunho(
                ctx.PedidoDeAbertura(jogadorId: 99), CancellationToken.None);

            // Act
            var lista = await ctx.Listar().ListarRelatorios(
                ctx.PedidoDeListagem(), CancellationToken.None);

            // Assert
            Assert.Equal(doJogador.RelatorioId, Assert.Single(lista).RelatorioId);
        }

        [Fact]
        public async Task A_listagem_ordena_pela_data_da_observacao()
        {
            // R5.5 existe justamente por isto: ordenar pela redacao produz ordem
            // cronologica incorreta. Aqui os dois relatorios sao escritos em uma
            // ordem e observados na ordem inversa - a listagem segue a observacao.

            // Arrange
            var ctx = new RelatorioTestContext();

            // A ordem de escrita e a inversa da esperada, de proposito: sem isso o
            // teste passaria so pela ordem de insercao do repositorio.
            var observadoAntes = await ctx.AbrirRascunho().AbrirRascunho(
                ctx.PedidoDeAbertura(observadoEm: new DateTimeOffset(2026, 8, 18, 15, 0, 0, TimeSpan.Zero)),
                CancellationToken.None);

            ctx.Time.Advance(TimeSpan.FromDays(1));

            var observadoDepois = await ctx.AbrirRascunho().AbrirRascunho(
                ctx.PedidoDeAbertura(observadoEm: new DateTimeOffset(2026, 8, 22, 15, 0, 0, TimeSpan.Zero)),
                CancellationToken.None);

            // Act
            var lista = await ctx.Listar().ListarRelatorios(
                ctx.PedidoDeListagem(), CancellationToken.None);

            // Assert - o observado mais recente vem primeiro, ainda que escrito antes.
            Assert.Equal(
                [observadoDepois.RelatorioId, observadoAntes.RelatorioId],
                lista.Select(r => r.RelatorioId));
        }

        [Fact]
        public async Task A_listagem_sem_relatorios_e_vazia_e_nao_recusa()
        {
            // Nao ha relatorio para o jogador nao e um erro: e uma resposta.

            // Arrange
            var ctx = new RelatorioTestContext();

            // Act
            var lista = await ctx.Listar().ListarRelatorios(
                ctx.PedidoDeListagem(jogadorId: 99), CancellationToken.None);

            // Assert
            Assert.Empty(lista);
        }

        [Fact]
        public async Task A_cadeia_de_correcao_aparece_inteira_na_listagem()
        {
            // A correcao nao substitui o corrigido (R5.3): os dois aparecem, cada
            // um com o seu elo. A listagem e o historico, nao o estado atual.

            // Arrange
            var ctx = new RelatorioTestContext();
            var originalId = await ctx.RelatorioFinalizado();

            var correcao = await ctx.Corrigir().CorrigirRelatorio(
                ctx.PedidoDeCorrecao(originalId), CancellationToken.None);

            // Act
            var lista = await ctx.Listar().ListarRelatorios(
                ctx.PedidoDeListagem(), CancellationToken.None);

            // Assert
            Assert.Equal(2, lista.Count);
            Assert.Equal(originalId, Assert.Single(lista, r => r.RelatorioId == correcao.RelatorioId).CorrigeRelatorioId);
            Assert.Null(Assert.Single(lista, r => r.RelatorioId == originalId).CorrigeRelatorioId);
        }
    }
}
