using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Tests.Shortlists
{
    public class FluxoDaAdicaoDeAlvoUseCaseTests
    {
        [Fact]
        public async Task O_alvo_adicionado_e_persistido()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCriada();

            // Act
            await contexto.Adicionar().AdicionarAlvo(
                contexto.PedidoDeAdicao(shortlistId, jogadorId: 1001, prioridade: 1),
                CancellationToken.None);

            // Assert
            var alvo = Assert.Single(contexto.Achar(shortlistId).Alvos);
            Assert.Equal(1001, alvo.JogadorId);
            Assert.Equal(1, alvo.Prioridade.Valor);
            Assert.Equal(ShortlistTestContext.Euros(5), alvo.CustoEstimado);
            Assert.Equal(1, contexto.Shortlists.Atualizacoes);
        }

        [Fact]
        public async Task O_result_traz_a_lista_com_o_novo_alvo()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001);

            // Act
            var result = await contexto.Adicionar().AdicionarAlvo(
                contexto.PedidoDeAdicao(shortlistId, jogadorId: 1002, prioridade: 2,
                    custoEstimado: ShortlistTestContext.Euros(12)),
                CancellationToken.None);

            // Assert
            Assert.Equal(shortlistId, result.ShortlistId);
            Assert.Equal([(1001, 1), (1002, 2)],
                result.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade)));
            Assert.Equal(ShortlistTestContext.Euros(17), result.CustoTotal);
        }

        [Fact]
        public async Task A_insercao_desloca_os_alvos_seguintes()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002, 1003);

            // Act
            await contexto.Adicionar().AdicionarAlvo(
                contexto.PedidoDeAdicao(shortlistId, jogadorId: 2001, prioridade: 2),
                CancellationToken.None);

            // Assert
            Assert.Equal([(1001, 1), (2001, 2), (1002, 3), (1003, 4)],
                ShortlistTestContext.Ordem(contexto.Achar(shortlistId)));
        }

        [Fact]
        public async Task O_alvo_alem_do_limite_da_lista_e_recusado()
        {
            // Arrange
            var contexto = new ShortlistTestContext { LimiteDeAlvos = 2 };
            var shortlistId = await contexto.ShortlistCom(1001, 1002);

            // Act
            var erro = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => contexto.Adicionar().AdicionarAlvo(
                    contexto.PedidoDeAdicao(shortlistId, jogadorId: 2001, prioridade: 3),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.limite_de_alvos_atingido", erro.Codigo);
        }

        [Fact]
        public async Task O_jogador_repetido_e_recusado()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001);

            // Act
            var erro = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => contexto.Adicionar().AdicionarAlvo(
                    contexto.PedidoDeAdicao(shortlistId, jogadorId: 1001, prioridade: 2),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.jogador_ja_na_lista", erro.Codigo);
        }

        [Fact]
        public async Task O_custo_em_outra_moeda_e_recusado()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001);

            // Act
            var erro = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => contexto.Adicionar().AdicionarAlvo(
                    contexto.PedidoDeAdicao(shortlistId, jogadorId: 1002, prioridade: 2,
                        custoEstimado: ShortlistTestContext.Libras(5)),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.moeda_divergente", erro.Codigo);
        }

        [Fact]
        public async Task A_prioridade_fora_da_ordem_e_recusada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);

            // Act
            var erro = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => contexto.Adicionar().AdicionarAlvo(
                    contexto.PedidoDeAdicao(shortlistId, jogadorId: 2001, prioridade: 5),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.prioridade_fora_da_ordem", erro.Codigo);
        }

        [Fact]
        public async Task A_recusa_nao_persiste_a_lista()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001);
            var atualizacoesAntes = contexto.Shortlists.Atualizacoes;

            // Act
            await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => contexto.Adicionar().AdicionarAlvo(
                    contexto.PedidoDeAdicao(shortlistId, jogadorId: 1001, prioridade: 2),
                    CancellationToken.None));

            // Assert
            Assert.Equal(atualizacoesAntes, contexto.Shortlists.Atualizacoes);
            Assert.Equal([(1001, 1)], ShortlistTestContext.Ordem(contexto.Achar(shortlistId)));
        }

        [Fact]
        public async Task A_shortlist_inexistente_nao_e_encontrada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            await contexto.ShortlistCriada();

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => contexto.Adicionar().AdicionarAlvo(
                    contexto.PedidoDeAdicao(Guid.NewGuid()), CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.nao_encontrada", erro.Codigo);
        }

        [Fact]
        public async Task A_shortlist_de_outro_olheiro_nao_e_encontrada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCriada();

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => contexto.Adicionar().AdicionarAlvo(
                    contexto.PedidoDeAdicao(shortlistId, olheiroId: Guid.NewGuid()),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.nao_encontrada", erro.Codigo);
            Assert.Empty(contexto.Achar(shortlistId).Alvos);
        }
    }
}
