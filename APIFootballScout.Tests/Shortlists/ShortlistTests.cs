using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.Specifications;

namespace APIFootballScout.Tests.Shortlists
{
    // R7.1 - a existencia do limite e invariante do agregado: AdicionarAlvo e o unico
    // caminho que pode estoura-lo, e por isso e nele que a recusa vive. O valor do teto
    // entra pela specification, que a aplicacao monta a partir da politica.
    public class ShortlistTests
    {
        private const int Limite = 3;
        private static readonly ShortlistComVagaSpecification ComVaga = new(Limite);

        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static Shortlist NovaShortlist()
            => Shortlist.Criar(olheiroId: Guid.NewGuid(), nome: "Laterais esquerdos 2026");

        private static Shortlist ShortlistCheia()
        {
            var shortlist = NovaShortlist();
            for (var posicao = 1; posicao <= Limite; posicao++)
                shortlist.AdicionarAlvo(jogadorId: 1000 + posicao, Euros(5), ComVaga);

            return shortlist;
        }

        [Fact]
        public void Alvo_e_aceito_enquanto_ha_vaga()
        {
            // Arrange
            var shortlist = NovaShortlist();

            // Act
            shortlist.AdicionarAlvo(jogadorId: 1001, Euros(5), ComVaga);

            // Assert
            Assert.Equal(1001, Assert.Single(shortlist.Alvos).JogadorId);
        }

        [Fact]
        public void A_lista_aceita_alvos_ate_o_limite()
        {
            // O teto e alcancavel: a recusa comeca no alvo seguinte, nao no ultimo que cabe.

            // Act
            var shortlist = ShortlistCheia();

            // Assert
            Assert.Equal(Limite, shortlist.Alvos.Count);
        }

        [Fact]
        public void Alvo_alem_do_limite_e_recusado()
        {
            // Arrange
            var shortlist = ShortlistCheia();

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 2001, Euros(5), ComVaga));

            // Assert
            Assert.Equal("shortlist.limite_de_alvos_atingido", erro.Codigo);
        }

        [Fact]
        public void A_recusa_deixa_a_lista_intacta()
        {
            // A invariante vale depois da recusa: a insercao negada nao pode ter escrito
            // nada pelo caminho. Sem isto, a excecao mascararia um agregado ja corrompido.

            // Arrange
            var shortlist = ShortlistCheia();
            var alvosAntes = shortlist.Alvos.ToArray();

            // Act
            Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 2001, Euros(5), ComVaga));

            // Assert
            Assert.Equal(alvosAntes, shortlist.Alvos);
        }

        [Fact]
        public void O_teto_de_uma_lista_nao_limita_outra()
        {
            // A politica entra por operacao, nao por tipo: duas listas do mesmo olheiro
            // podem responder a limites diferentes sem que o modelo mude.

            // Arrange
            var apertada = NovaShortlist();
            var folgada = NovaShortlist();
            apertada.AdicionarAlvo(jogadorId: 1001, Euros(5), new ShortlistComVagaSpecification(limiteDeAlvos: 1));

            // Act
            folgada.AdicionarAlvo(jogadorId: 1001, Euros(5), new ShortlistComVagaSpecification(limiteDeAlvos: 25));
            folgada.AdicionarAlvo(jogadorId: 1002, Euros(5), new ShortlistComVagaSpecification(limiteDeAlvos: 25));

            // Assert
            Assert.Throws<RegraDeNegocioException>(
                () => apertada.AdicionarAlvo(jogadorId: 1002, Euros(5), new ShortlistComVagaSpecification(limiteDeAlvos: 1)));
            Assert.Equal(2, folgada.Alvos.Count);
        }
    }
}
