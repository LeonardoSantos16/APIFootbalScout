using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Tests.Shortlists
{
    public class FluxoDaRemocaoDeAlvoUseCaseTests
    {
        [Fact]
        public async Task A_remocao_persiste_a_lista_sem_o_alvo()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002, 1003);
            var atualizacoesAntes = contexto.Shortlists.Atualizacoes;

            // Act
            await contexto.Remover().RemoverAlvo(
                contexto.PedidoDeRemocao(shortlistId, jogadorId: 1002), CancellationToken.None);

            // Assert
            var shortlist = contexto.Achar(shortlistId);
            Assert.DoesNotContain(shortlist.Alvos, alvo => alvo.JogadorId == 1002);
            Assert.Equal(atualizacoesAntes + 1, contexto.Shortlists.Atualizacoes);
        }

        [Fact]
        public async Task A_remocao_renumera_sem_deixar_lacuna()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002, 1003);

            // Act
            await contexto.Remover().RemoverAlvo(
                contexto.PedidoDeRemocao(shortlistId, jogadorId: 1001), CancellationToken.None);

            // Assert
            Assert.Equal([(1002, 1), (1003, 2)],
                ShortlistTestContext.Ordem(contexto.Achar(shortlistId)));
        }

        [Fact]
        public async Task O_result_traz_a_lista_sem_o_alvo()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);

            // Act
            var result = await contexto.Remover().RemoverAlvo(
                contexto.PedidoDeRemocao(shortlistId, jogadorId: 1001), CancellationToken.None);

            // Assert
            Assert.Equal(shortlistId, result.ShortlistId);
            Assert.Equal([(1002, 1)],
                result.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade)));
            Assert.Equal(ShortlistTestContext.Euros(5), result.CustoTotal);
        }

        [Fact]
        public async Task A_remocao_do_ultimo_alvo_esvazia_a_lista()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001);

            // Act
            var result = await contexto.Remover().RemoverAlvo(
                contexto.PedidoDeRemocao(shortlistId, jogadorId: 1001), CancellationToken.None);

            // Assert
            Assert.Empty(result.Alvos);
            Assert.Null(result.CustoTotal);
            Assert.Empty(contexto.Achar(shortlistId).Alvos);
        }

        [Fact]
        public async Task A_remocao_libera_vaga_para_um_alvo_novo()
        {
            // Arrange
            var contexto = new ShortlistTestContext { LimiteDeAlvos = 2 };
            var shortlistId = await contexto.ShortlistCom(1001, 1002);

            // Act
            await contexto.Remover().RemoverAlvo(
                contexto.PedidoDeRemocao(shortlistId, jogadorId: 1002), CancellationToken.None);
            await contexto.Adicionar().AdicionarAlvo(
                contexto.PedidoDeAdicao(shortlistId, jogadorId: 2001, prioridade: 2),
                CancellationToken.None);

            // Assert
            Assert.Equal([(1001, 1), (2001, 2)],
                ShortlistTestContext.Ordem(contexto.Achar(shortlistId)));
        }

        [Fact]
        public async Task O_jogador_ausente_nao_e_encontrado()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001);

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => contexto.Remover().RemoverAlvo(
                    contexto.PedidoDeRemocao(shortlistId, jogadorId: 2001), CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.alvo_nao_encontrado", erro.Codigo);
        }

        [Fact]
        public async Task A_recusa_nao_persiste_a_lista()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);
            var atualizacoesAntes = contexto.Shortlists.Atualizacoes;

            // Act
            await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => contexto.Remover().RemoverAlvo(
                    contexto.PedidoDeRemocao(shortlistId, jogadorId: 2001), CancellationToken.None));

            // Assert
            Assert.Equal(atualizacoesAntes, contexto.Shortlists.Atualizacoes);
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
                () => contexto.Remover().RemoverAlvo(
                    contexto.PedidoDeRemocao(Guid.NewGuid(), jogadorId: 1001), CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.nao_encontrada", erro.Codigo);
        }

        [Fact]
        public async Task A_shortlist_de_outro_olheiro_nao_e_encontrada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001);

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => contexto.Remover().RemoverAlvo(
                    contexto.PedidoDeRemocao(shortlistId, jogadorId: 1001, olheiroId: Guid.NewGuid()),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.nao_encontrada", erro.Codigo);
            Assert.Single(contexto.Achar(shortlistId).Alvos);
        }
    }
}
