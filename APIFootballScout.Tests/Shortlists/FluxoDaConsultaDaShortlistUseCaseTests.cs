using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Tests.Shortlists
{
    public class FluxoDaConsultaDaShortlistUseCaseTests
    {
        [Fact]
        public async Task O_result_traz_a_shortlist_com_os_alvos()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);

            // Act
            var result = await contexto.Obter().ObterShortlist(
                contexto.PedidoDeConsulta(shortlistId), CancellationToken.None);

            // Assert
            Assert.Equal(shortlistId, result.ShortlistId);
            Assert.Equal(contexto.OlheiroId, result.OlheiroId);
            Assert.Equal(ShortlistTestContext.Nome, result.Nome);
            Assert.Equal(25, result.LimiteDeAlvos);
            Assert.Equal([(1001, 1), (1002, 2)],
                result.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade)));
            Assert.Equal(ShortlistTestContext.Euros(10), result.CustoTotal);
        }

        [Fact]
        public async Task A_shortlist_vazia_vem_sem_alvos_nem_custo()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCriada();

            // Act
            var result = await contexto.Obter().ObterShortlist(
                contexto.PedidoDeConsulta(shortlistId), CancellationToken.None);

            // Assert
            Assert.Empty(result.Alvos);
            Assert.Null(result.CustoTotal);
        }

        [Fact]
        public async Task A_consulta_nao_persiste_nada()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var shortlistId = await contexto.ShortlistCom(1001, 1002);
            var atualizacoesAntes = contexto.Shortlists.Atualizacoes;

            // Act
            await contexto.Obter().ObterShortlist(
                contexto.PedidoDeConsulta(shortlistId), CancellationToken.None);

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
                () => contexto.Obter().ObterShortlist(
                    contexto.PedidoDeConsulta(Guid.NewGuid()), CancellationToken.None));

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
                () => contexto.Obter().ObterShortlist(
                    contexto.PedidoDeConsulta(shortlistId, olheiroId: Guid.NewGuid()),
                    CancellationToken.None));

            // Assert
            Assert.Equal("shortlist.nao_encontrada", erro.Codigo);
        }
    }
}
