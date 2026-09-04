using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Tests.Shortlists
{
    public class CustoDaShortlistTests
    {
        private static readonly LimiteDeAlvos Limite = new(25);

        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static Dinheiro Libras(long milhoes) => new(milhoes * 1_000_000_00, "GBP");

        private static Shortlist NovaShortlist()
            => Shortlist.Criar(olheiroId: Guid.NewGuid(), nome: "Laterais esquerdos 2026", limite: Limite);

        private static (int Jogador, int Prioridade)[] Ordem(Shortlist shortlist)
            => [.. shortlist.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade.Valor))];

        [Fact]
        public void A_lista_vazia_nao_tem_custo_total()
        {
            // Act
            var shortlist = NovaShortlist();

            // Assert
            Assert.Null(shortlist.CustoTotal);
        }

        [Fact]
        public void O_custo_total_de_um_alvo_e_o_proprio_custo()
        {
            // Arrange
            var shortlist = NovaShortlist();

            // Act
            shortlist.AdicionarAlvo(1001, new Prioridade(1), Euros(5));

            // Assert
            Assert.Equal(Euros(5), shortlist.CustoTotal);
        }

        [Fact]
        public void O_custo_total_soma_os_alvos()
        {
            // Arrange
            var shortlist = NovaShortlist();

            // Act
            shortlist.AdicionarAlvo(1001, new Prioridade(1), Euros(12));
            shortlist.AdicionarAlvo(1002, new Prioridade(2), Euros(30));
            shortlist.AdicionarAlvo(1003, new Prioridade(3), Euros(8));

            // Assert
            Assert.Equal(Euros(50), shortlist.CustoTotal);
        }

        [Fact]
        public void O_alvo_em_moeda_divergente_e_recusado()
        {
            // Arrange
            var shortlist = NovaShortlist();
            shortlist.AdicionarAlvo(1001, new Prioridade(1), Euros(5));

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(1002, new Prioridade(2), Libras(5)));

            // Assert
            Assert.Equal("shortlist.moeda_divergente", erro.Codigo);
        }

        [Fact]
        public void A_recusa_da_moeda_deixa_a_lista_intacta()
        {
            // Arrange
            var shortlist = NovaShortlist();
            shortlist.AdicionarAlvo(1001, new Prioridade(1), Euros(5));

            // Act
            Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(1002, new Prioridade(1), Libras(5)));

            // Assert
            Assert.Equal([(1001, 1)], Ordem(shortlist));
            Assert.Equal(Euros(5), shortlist.CustoTotal);
        }

        [Fact]
        public void A_primeira_insercao_define_a_moeda_da_lista()
        {
            // Arrange
            var shortlist = NovaShortlist();
            shortlist.AdicionarAlvo(1001, new Prioridade(1), Libras(5));

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(1002, new Prioridade(2), Euros(5)));

            // Assert
            Assert.Equal("shortlist.moeda_divergente", erro.Codigo);
            Assert.Equal(Libras(5), shortlist.CustoTotal);
        }

        [Fact]
        public void A_lista_esvaziada_aceita_uma_moeda_nova()
        {
            // Arrange
            var shortlist = NovaShortlist();
            shortlist.AdicionarAlvo(1001, new Prioridade(1), Euros(5));

            // Act
            shortlist.RemoverAlvo(1001);
            shortlist.AdicionarAlvo(1002, new Prioridade(1), Libras(7));

            // Assert
            Assert.Equal(Libras(7), shortlist.CustoTotal);
        }

        [Fact]
        public void O_custo_total_acompanha_a_remocao()
        {
            // Arrange
            var shortlist = NovaShortlist();
            shortlist.AdicionarAlvo(1001, new Prioridade(1), Euros(12));
            shortlist.AdicionarAlvo(1002, new Prioridade(2), Euros(30));

            // Act
            shortlist.RemoverAlvo(1002);

            // Assert
            Assert.Equal(Euros(12), shortlist.CustoTotal);
        }

        [Fact]
        public void A_repriorizacao_nao_altera_o_custo_total()
        {
            // Arrange
            var shortlist = NovaShortlist();
            shortlist.AdicionarAlvo(1001, new Prioridade(1), Euros(12));
            shortlist.AdicionarAlvo(1002, new Prioridade(2), Euros(30));

            // Act
            shortlist.AtualizarPrioridade(1001, new Prioridade(2));

            // Assert
            Assert.Equal(Euros(42), shortlist.CustoTotal);
        }
    }
}
