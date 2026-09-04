using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Tests.Shortlists
{
    public class FluxoDaCriacaoUseCaseTests
    {
        [Fact]
        public async Task A_criacao_persiste_a_shortlist_do_olheiro()
        {
            // Arrange
            var contexto = new ShortlistTestContext();

            // Act
            await contexto.Criar().CriarShortlist(
                contexto.PedidoDeCriacao(), CancellationToken.None);

            // Assert
            var shortlist = contexto.Unica();
            Assert.Equal(contexto.OlheiroId, shortlist.OlheiroId);
            Assert.Equal(ShortlistTestContext.Nome, shortlist.Nome);
            Assert.Empty(shortlist.Alvos);
        }

        [Fact]
        public async Task O_result_descreve_a_lista_recem_criada()
        {
            // Arrange
            var contexto = new ShortlistTestContext { LimiteDeAlvos = 25 };

            // Act
            var result = await contexto.Criar().CriarShortlist(
                contexto.PedidoDeCriacao(), CancellationToken.None);

            // Assert
            Assert.Equal(contexto.Unica().Id, result.ShortlistId);
            Assert.Equal(contexto.OlheiroId, result.OlheiroId);
            Assert.Equal(ShortlistTestContext.Nome, result.Nome);
            Assert.Equal(25, result.LimiteDeAlvos);
            Assert.Empty(result.Alvos);
            Assert.Null(result.CustoTotal);
        }

        [Fact]
        public async Task A_lista_nasce_com_o_limite_da_politica()
        {
            // Arrange
            var contexto = new ShortlistTestContext { LimiteDeAlvos = 2 };

            // Act
            var result = await contexto.Criar().CriarShortlist(
                contexto.PedidoDeCriacao(), CancellationToken.None);

            // Assert
            var shortlist = contexto.Achar(result.ShortlistId);
            shortlist.AdicionarAlvo(1001, new Prioridade(1), ShortlistTestContext.Euros(5));
            shortlist.AdicionarAlvo(1002, new Prioridade(2), ShortlistTestContext.Euros(5));

            var erro = Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(1003, new Prioridade(3), ShortlistTestContext.Euros(5)));

            Assert.Equal("shortlist.limite_de_alvos_atingido", erro.Codigo);
        }

        [Fact]
        public async Task Trocar_a_politica_nao_mexe_nas_listas_ja_criadas()
        {
            // Arrange
            var contexto = new ShortlistTestContext { LimiteDeAlvos = 2 };
            var antiga = await contexto.Criar().CriarShortlist(
                contexto.PedidoDeCriacao(), CancellationToken.None);

            // Act
            contexto.LimiteDeAlvos = 5;
            var nova = await contexto.Criar().CriarShortlist(
                contexto.PedidoDeCriacao(nome: "Emergencia janela de inverno"), CancellationToken.None);

            // Assert
            Assert.Equal(2, contexto.Achar(antiga.ShortlistId).Limite.Valor);
            Assert.Equal(5, contexto.Achar(nova.ShortlistId).Limite.Valor);
        }

        [Fact]
        public async Task Cada_criacao_gera_uma_lista_distinta()
        {
            // Arrange
            var contexto = new ShortlistTestContext();

            // Act
            var primeira = await contexto.Criar().CriarShortlist(
                contexto.PedidoDeCriacao(), CancellationToken.None);
            var segunda = await contexto.Criar().CriarShortlist(
                contexto.PedidoDeCriacao(), CancellationToken.None);

            // Assert
            Assert.NotEqual(primeira.ShortlistId, segunda.ShortlistId);
            Assert.Equal(2, contexto.Shortlists.Todas.Count);
        }
    }
}
