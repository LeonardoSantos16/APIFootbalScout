using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Aggregate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Tests.Relatorios
{
    public class FluxoDaConcorrenciaDoRelatorioUseCaseTests
    {
        [Fact]
        public async Task O_rascunho_aberto_comeca_na_versao_1()
        {
            // Arrange
            var ctx = new RelatorioTestContext();

            // Act
            var result = await ctx.AbrirRascunho().AbrirRascunho(
                ctx.PedidoDeAbertura(), CancellationToken.None);

            // Assert
            Assert.Equal(1, result.Version);
        }

        [Fact]
        public async Task A_correcao_nasce_na_versao_1_sem_alterar_a_versao_do_original()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var originalId = await ctx.RelatorioFinalizado();

            // Act
            var correcao = await ctx.Corrigir().CorrigirRelatorio(
                ctx.PedidoDeCorrecao(originalId), CancellationToken.None);

            // Assert
            Assert.Equal(1, correcao.Version);
            Assert.Equal(3, ctx.Relatorios.VersaoDe(originalId));
        }

        [Fact]
        public async Task A_consulta_traz_a_versao_atual()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var relatorioId = await ctx.RascunhoConcluido();

            // Act
            var result = await ctx.Obter().ObterRelatorio(
                ctx.PedidoDeConsulta(relatorioId), CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Version);
        }

        [Fact]
        public async Task A_edicao_com_a_versao_atual_avanca_a_versao()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var aberto = await ctx.AbrirRascunho().AbrirRascunho(
                ctx.PedidoDeAbertura(), CancellationToken.None);

            // Act
            var result = await ctx.EditarRascunho().EditarRascunho(
                ctx.PedidoDeEdicao(aberto.RelatorioId, nota: 7m, versao: 1),
                CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Version);
            Assert.Equal(2, ctx.Relatorios.VersaoDe(aberto.RelatorioId));
        }

        [Fact]
        public async Task A_finalizacao_com_a_versao_atual_avanca_a_versao()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var relatorioId = await ctx.RascunhoConcluido();

            // Act
            var result = await ctx.Finalizar().FinalizarRelatorio(
                ctx.PedidoDeFinalizacao(relatorioId, versao: 2), CancellationToken.None);

            // Assert
            Assert.Equal(3, result.Version);
        }

        [Fact]
        public async Task A_edicao_com_versao_desatualizada_e_recusada()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var relatorioId = await ctx.RascunhoConcluido();

            // Act
            var erro = await Assert.ThrowsAsync<ConflitoDeConcorrenciaException>(
                () => ctx.EditarRascunho().EditarRascunho(
                    ctx.PedidoDeEdicao(relatorioId, texto: "Outro texto.", versao: 1),
                    CancellationToken.None));

            // Assert
            Assert.Equal("relatorio.alterado_por_outra_requisicao", erro.Codigo);
        }

        [Fact]
        public async Task A_finalizacao_com_versao_desatualizada_e_recusada()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var relatorioId = await ctx.RascunhoConcluido();

            // Act
            var erro = await Assert.ThrowsAsync<ConflitoDeConcorrenciaException>(
                () => ctx.Finalizar().FinalizarRelatorio(
                    ctx.PedidoDeFinalizacao(relatorioId, versao: 1), CancellationToken.None));

            // Assert
            Assert.Equal("relatorio.alterado_por_outra_requisicao", erro.Codigo);
        }

        [Fact]
        public async Task A_recusa_por_versao_nao_persiste_nada()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var relatorioId = await ctx.RascunhoConcluido();
            var atualizacoesAntes = ctx.Relatorios.Atualizacoes;

            // Act
            await Assert.ThrowsAsync<ConflitoDeConcorrenciaException>(
                () => ctx.Finalizar().FinalizarRelatorio(
                    ctx.PedidoDeFinalizacao(relatorioId, versao: 1), CancellationToken.None));

            // Assert
            var persistido = ctx.Unico();
            Assert.Equal(StatusRelatorio.Rascunho, persistido.Status);
            Assert.Null(persistido.FinalizadoEm);
            Assert.Equal(2, ctx.Relatorios.VersaoDe(relatorioId));
            Assert.Equal(atualizacoesAntes, ctx.Relatorios.Atualizacoes);
        }

        [Fact]
        public async Task A_segunda_de_duas_edicoes_sobre_a_mesma_versao_e_recusada()
        {
            // Arrange
            var ctx = new RelatorioTestContext();
            var relatorioId = await ctx.RascunhoConcluido();
            var versaoLidaPorAmbas = ctx.Relatorios.VersaoDe(relatorioId);
            await ctx.EditarRascunho().EditarRascunho(
                ctx.PedidoDeEdicao(relatorioId, nota: 9m, versao: versaoLidaPorAmbas),
                CancellationToken.None);

            // Act
            await Assert.ThrowsAsync<ConflitoDeConcorrenciaException>(
                () => ctx.EditarRascunho().EditarRascunho(
                    ctx.PedidoDeEdicao(relatorioId, texto: "Outro texto.", versao: versaoLidaPorAmbas),
                    CancellationToken.None));

            // Assert
            var persistido = ctx.Unico();
            Assert.Equal(new Nota(9m), persistido.Nota);
            Assert.Equal(RelatorioTestContext.TextoInicial, persistido.Texto);
        }
    }
}
