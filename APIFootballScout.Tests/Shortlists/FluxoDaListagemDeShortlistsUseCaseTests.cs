namespace APIFootballScout.Tests.Shortlists
{
    public class FluxoDaListagemDeShortlistsUseCaseTests
    {
        [Fact]
        public async Task A_listagem_traz_as_shortlists_do_olheiro()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var primeira = await contexto.ShortlistCriada(nome: "Atacantes 2026");
            var segunda = await contexto.ShortlistCriada(nome: "Meias 2026");

            // Act
            var result = await contexto.Listar().ListarShortlists(
                contexto.PedidoDeListagem(), CancellationToken.None);

            // Assert
            Assert.Equal([primeira, segunda],
                result.Select(shortlist => shortlist.ShortlistId));
        }

        [Fact]
        public async Task A_listagem_vem_ordenada_por_nome()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            await contexto.ShortlistCriada(nome: "Zagueiros 2026");
            await contexto.ShortlistCriada(nome: "Atacantes 2026");
            await contexto.ShortlistCriada(nome: "Meias 2026");

            // Act
            var result = await contexto.Listar().ListarShortlists(
                contexto.PedidoDeListagem(), CancellationToken.None);

            // Assert
            Assert.Equal(["Atacantes 2026", "Meias 2026", "Zagueiros 2026"],
                result.Select(shortlist => shortlist.Nome));
        }

        [Fact]
        public async Task As_shortlists_de_outro_olheiro_ficam_de_fora()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var propria = await contexto.ShortlistCriada(nome: "Atacantes 2026");
            await contexto.ShortlistCriada(olheiroId: Guid.NewGuid(), nome: "Meias 2026");

            // Act
            var result = await contexto.Listar().ListarShortlists(
                contexto.PedidoDeListagem(), CancellationToken.None);

            // Assert
            var unica = Assert.Single(result);
            Assert.Equal(propria, unica.ShortlistId);
            Assert.Equal(contexto.OlheiroId, unica.OlheiroId);
        }

        [Fact]
        public async Task O_olheiro_sem_shortlists_recebe_lista_vazia()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            await contexto.ShortlistCom(1001);

            // Act
            var result = await contexto.Listar().ListarShortlists(
                contexto.PedidoDeListagem(olheiroId: Guid.NewGuid()), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Cada_shortlist_traz_seus_alvos_e_seu_custo()
        {
            // Arrange
            var contexto = new ShortlistTestContext();
            var comAlvos = await contexto.ShortlistCom(1001, 1002);
            var vazia = await contexto.ShortlistCriada(nome: "Zagueiros 2026");

            // Act
            var result = await contexto.Listar().ListarShortlists(
                contexto.PedidoDeListagem(), CancellationToken.None);

            // Assert
            var listaComAlvos = result.Single(shortlist => shortlist.ShortlistId == comAlvos);
            Assert.Equal([(1001, 1), (1002, 2)],
                listaComAlvos.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade)));
            Assert.Equal(ShortlistTestContext.Euros(10), listaComAlvos.CustoTotal);

            var listaVazia = result.Single(shortlist => shortlist.ShortlistId == vazia);
            Assert.Empty(listaVazia.Alvos);
            Assert.Null(listaVazia.CustoTotal);
        }
    }
}
