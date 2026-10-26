using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Tests.Shortlists
{
    public class FluxoDaConcorrenciaDaShortlistUseCaseTests
    {
        [Fact]
        public async Task A_shortlist_criada_comeca_na_versao_1()
        {
            // Arrange
            var contexto = new ShortlistTestContext();

            // Act
            var result = await contexto.Criar().CriarShortlist(
                contexto.PedidoDeCriacao(), CancellationToken.None);

            // Assert
            Assert.Equal(1, result.Version);
        }

        [Fact]
        public async Task A_consulta_traz_a_versao_atual()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);

            // Act
            var result = await contexto.Obter().ObterShortlist(
                contexto.PedidoDeConsulta(shortlistId), CancellationToken.None);

            // Assert
            Assert.Equal(3, result.Version);
        }

        [Fact]
        public async Task A_escrita_com_a_versao_atual_avanca_a_versao()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001);

            // Act
            var result = await contexto.Adicionar().AdicionarAlvo(
                contexto.PedidoDeAdicao(shortlistId, jogadorId: 1002, prioridade: 2, versao: 2),
                CancellationToken.None);

            // Assert
            Assert.Equal(3, result.Version);
            Assert.Equal(3, contexto.Shortlists.VersaoDe(shortlistId));
        }

        [Fact]
        public async Task A_adicao_com_versao_desatualizada_e_recusada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);

            // Act
            var erro = await Assert.ThrowsAsync<ConflitoDeConcorrenciaException>(
                () => contexto.Adicionar().AdicionarAlvo(
                    contexto.PedidoDeAdicao(shortlistId, jogadorId: 2001, prioridade: 3, versao: 2),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.alterada_por_outra_requisicao", erro.Codigo);
        }

        [Fact]
        public async Task A_remocao_com_versao_desatualizada_e_recusada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);

            // Act
            var erro = await Assert.ThrowsAsync<ConflitoDeConcorrenciaException>(
                () => contexto.Remover().RemoverAlvo(
                    contexto.PedidoDeRemocao(shortlistId, jogadorId: 1001, versao: 2),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.alterada_por_outra_requisicao", erro.Codigo);
        }

        [Fact]
        public async Task A_repriorizacao_com_versao_desatualizada_e_recusada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);

            // Act
            var erro = await Assert.ThrowsAsync<ConflitoDeConcorrenciaException>(
                () => contexto.Repriorizar().RepriorizarAlvo(
                    contexto.PedidoDeRepriorizacao(shortlistId, jogadorId: 1002, prioridade: 1, versao: 2),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.alterada_por_outra_requisicao", erro.Codigo);
        }

        [Fact]
        public async Task A_recusa_por_versao_nao_persiste_nada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);
            var atualizacoesAntes = contexto.Shortlists.Atualizacoes;

            // Act
            await Assert.ThrowsAsync<ConflitoDeConcorrenciaException>(
                () => contexto.Repriorizar().RepriorizarAlvo(
                    contexto.PedidoDeRepriorizacao(shortlistId, jogadorId: 1002, prioridade: 1, versao: 2),
                    CancellationToken.None));

            // Assert
            Assert.Equal([(1001, 1), (1002, 2)],
                ShortlistTestContext.Ordem(contexto.Achar(shortlistId)));
            Assert.Equal(3, contexto.Shortlists.VersaoDe(shortlistId));
            Assert.Equal(atualizacoesAntes, contexto.Shortlists.Atualizacoes);
        }

        [Fact]
        public async Task A_segunda_de_duas_escritas_sobre_a_mesma_versao_nao_apaga_a_primeira()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);
            var versaoLidaPorAmbas = contexto.Shortlists.VersaoDe(shortlistId);
            await contexto.Adicionar().AdicionarAlvo(
                contexto.PedidoDeAdicao(shortlistId, jogadorId: 2001, prioridade: 3, versao: versaoLidaPorAmbas),
                CancellationToken.None);

            // Act
            await Assert.ThrowsAsync<ConflitoDeConcorrenciaException>(
                () => contexto.Remover().RemoverAlvo(
                    contexto.PedidoDeRemocao(shortlistId, jogadorId: 1001, versao: versaoLidaPorAmbas),
                    CancellationToken.None));

            // Assert
            Assert.Equal([(1001, 1), (1002, 2), (2001, 3)],
                ShortlistTestContext.Ordem(contexto.Achar(shortlistId)));
        }
    }
}