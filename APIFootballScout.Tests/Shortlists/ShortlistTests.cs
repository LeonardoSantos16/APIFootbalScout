using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Tests.Shortlists
{
    public class ShortlistTests
    {
        private const int Teto = 3;
        private static readonly LimiteDeAlvos Limite = new(Teto);

        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static Shortlist NovaShortlist()
            => Shortlist.Criar(olheiroId: Guid.NewGuid(), nome: "Laterais esquerdos 2026", limite: Limite);

        private static Shortlist ShortlistCheia()
        {
            var shortlist = NovaShortlist();
            for (var posicao = 1; posicao <= Teto; posicao++)
                shortlist.AdicionarAlvo(jogadorId: 1000 + posicao, new Prioridade(posicao), Euros(5));

            return shortlist;
        }

        [Fact]
        public void Alvo_e_aceito_enquanto_ha_vaga()
        {
            // Arrange
            var shortlist = NovaShortlist();

            // Act
            shortlist.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(5));

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
            Assert.Equal(Teto, shortlist.Alvos.Count);
        }

        [Fact]
        public void Alvo_alem_do_limite_e_recusado()
        {
            // Arrange
            var shortlist = ShortlistCheia();

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 2001, new Prioridade(4), Euros(5)));

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
                () => shortlist.AdicionarAlvo(jogadorId: 2001, new Prioridade(4), Euros(5)));

            // Assert
            Assert.Equal(alvosAntes, shortlist.Alvos);
        }

        [Fact]
        public void O_teto_de_uma_lista_nao_limita_outra()
        {
            // Arrange
            var olheiroId = Guid.NewGuid();
            var apertada = Shortlist.Criar(olheiroId, "Emergencia janela de inverno", new LimiteDeAlvos(1));
            var folgada = Shortlist.Criar(olheiroId, "Laterais esquerdos 2026", new LimiteDeAlvos(25));

            // Act
            apertada.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(5));
            folgada.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(5));
            folgada.AdicionarAlvo(jogadorId: 1002, new Prioridade(2), Euros(5));

            // Assert
            Assert.Throws<RegraDeNegocioException>(
                () => apertada.AdicionarAlvo(jogadorId: 1002, new Prioridade(2), Euros(5)));
            Assert.Equal(2, folgada.Alvos.Count);
        }

        [Fact]
        public void O_limite_acompanha_a_lista_restaurada()
        {
            // Arrange
            var shortlist = Shortlist.Restaurar(
                id: Guid.NewGuid(),
                olheiroId: Guid.NewGuid(),
                nome: "Laterais esquerdos 2026",
                limite: new LimiteDeAlvos(2),
                alvos:
                [
                    new Alvo(1001, new Prioridade(1), Euros(5)),
                    new Alvo(1002, new Prioridade(2), Euros(5))
                ]);

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 2001, new Prioridade(3), Euros(5)));

            // Assert
            Assert.Equal("shortlist.limite_de_alvos_atingido", erro.Codigo);
        }
    }
}
