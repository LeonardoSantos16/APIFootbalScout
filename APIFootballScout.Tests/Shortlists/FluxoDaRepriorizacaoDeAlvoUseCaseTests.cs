using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Tests.Shortlists
{
    public class FluxoDaRepriorizacaoDeAlvoUseCaseTests
    {
        [Fact]
        public async Task A_repriorizacao_persiste_a_nova_ordem()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002, 1003);
            var atualizacoesAntes = contexto.Shortlists.Atualizacoes;

            // Act
            await contexto.Repriorizar().RepriorizarAlvo(
                contexto.PedidoDeRepriorizacao(shortlistId, jogadorId: 1001, prioridade: 3),
                CancellationToken.None);

            // Assert
            Assert.Equal([(1002, 1), (1003, 2), (1001, 3)],
                ShortlistTestContext.Ordem(contexto.Achar(shortlistId)));
            Assert.Equal(atualizacoesAntes + 1, contexto.Shortlists.Atualizacoes);
        }

        [Fact]
        public async Task Subir_um_alvo_rebaixa_quem_estava_no_caminho()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002, 1003);

            // Act
            await contexto.Repriorizar().RepriorizarAlvo(
                contexto.PedidoDeRepriorizacao(shortlistId, jogadorId: 1003, prioridade: 1),
                CancellationToken.None);

            // Assert
            Assert.Equal([(1003, 1), (1001, 2), (1002, 3)],
                ShortlistTestContext.Ordem(contexto.Achar(shortlistId)));
        }

        [Fact]
        public async Task O_result_traz_a_nova_ordem()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002, 1003);

            // Act
            var result = await contexto.Repriorizar().RepriorizarAlvo(
                contexto.PedidoDeRepriorizacao(shortlistId, jogadorId: 1002, prioridade: 1),
                CancellationToken.None);

            // Assert
            Assert.Equal(shortlistId, result.ShortlistId);
            Assert.Equal([(1002, 1), (1001, 2), (1003, 3)],
                result.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade)));
            Assert.Equal(ShortlistTestContext.Euros(15), result.CustoTotal);
        }

        [Fact]
        public async Task A_repriorizacao_nao_altera_a_quantidade_nem_o_custo_do_alvo()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCriada();
            await contexto.Adicionar().AdicionarAlvo(
                contexto.PedidoDeAdicao(shortlistId, jogadorId: 1001, prioridade: 1,
                    custoEstimado: ShortlistTestContext.Euros(4)),
                CancellationToken.None);
            await contexto.Adicionar().AdicionarAlvo(
                contexto.PedidoDeAdicao(shortlistId, jogadorId: 1002, prioridade: 2,
                    custoEstimado: ShortlistTestContext.Euros(9)),
                CancellationToken.None);

            // Act
            var result = await contexto.Repriorizar().RepriorizarAlvo(
                contexto.PedidoDeRepriorizacao(shortlistId, jogadorId: 1002, prioridade: 1),
                CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Alvos.Count);
            Assert.Equal(ShortlistTestContext.Euros(9),
                result.Alvos.Single(alvo => alvo.JogadorId == 1002).CustoEstimado);
            Assert.Equal(ShortlistTestContext.Euros(13), result.CustoTotal);
        }

        [Fact]
        public async Task A_posicao_alem_da_ultima_e_recusada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002, 1003);

            // Act
            var erro = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => contexto.Repriorizar().RepriorizarAlvo(
                    contexto.PedidoDeRepriorizacao(shortlistId, jogadorId: 1001, prioridade: 4),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.prioridade_fora_da_ordem", erro.Codigo);
        }

        [Fact]
        public async Task A_prioridade_nao_positiva_e_recusada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);

            // Act
            var erro = await Assert.ThrowsAsync<ValorInvalidoException>(
                () => contexto.Repriorizar().RepriorizarAlvo(
                    contexto.PedidoDeRepriorizacao(shortlistId, jogadorId: 1001, prioridade: 0),
                    CancellationToken.None));

            // Assert
            Assert.Equal("prioridade.nao_positiva", erro.Codigo);
        }

        [Fact]
        public async Task A_recusa_nao_persiste_a_lista()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002, 1003);
            var atualizacoesAntes = contexto.Shortlists.Atualizacoes;

            // Act
            await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => contexto.Repriorizar().RepriorizarAlvo(
                    contexto.PedidoDeRepriorizacao(shortlistId, jogadorId: 1001, prioridade: 4),
                    CancellationToken.None));

            // Assert
            Assert.Equal(atualizacoesAntes, contexto.Shortlists.Atualizacoes);
            Assert.Equal([(1001, 1), (1002, 2), (1003, 3)],
                ShortlistTestContext.Ordem(contexto.Achar(shortlistId)));
        }

        [Fact]
        public async Task O_jogador_ausente_nao_e_encontrado()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => contexto.Repriorizar().RepriorizarAlvo(
                    contexto.PedidoDeRepriorizacao(shortlistId, jogadorId: 9999, prioridade: 1),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.alvo_nao_encontrado", erro.Codigo);
            Assert.Equal([(1001, 1), (1002, 2)],
                ShortlistTestContext.Ordem(contexto.Achar(shortlistId)));
        }

        [Fact]
        public async Task A_shortlist_inexistente_nao_e_encontrada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            await contexto.ShortlistCom(1001);

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => contexto.Repriorizar().RepriorizarAlvo(
                    contexto.PedidoDeRepriorizacao(Guid.NewGuid(), jogadorId: 1001, prioridade: 1),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.nao_encontrada", erro.Codigo);
        }

        [Fact]
        public async Task A_shortlist_de_outro_olheiro_nao_e_encontrada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => contexto.Repriorizar().RepriorizarAlvo(
                    contexto.PedidoDeRepriorizacao(
                        shortlistId, jogadorId: 1002, prioridade: 1, olheiroId: Guid.NewGuid()),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.nao_encontrada", erro.Codigo);
            Assert.Equal([(1001, 1), (1002, 2)],
                ShortlistTestContext.Ordem(contexto.Achar(shortlistId)));
        }
    }
}
