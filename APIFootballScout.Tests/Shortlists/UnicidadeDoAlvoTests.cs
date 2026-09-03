using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Tests.Shortlists
{
    // R7.2 - nao ha jogador repetido na mesma lista
    public class UnicidadeDoAlvoTests
    {
        private const int Teto = 3;
        private static readonly LimiteDeAlvos Limite = new(Teto);

        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static Shortlist NovaShortlist()
            => Shortlist.Criar(olheiroId: Guid.NewGuid(), nome: "Laterais esquerdos 2026", limite: Limite);

        [Fact]
        public void O_mesmo_jogador_nao_entra_duas_vezes()
        {
            // Arrange
            var shortlist = NovaShortlist();
            shortlist.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(5));

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(5)));

            // Assert
            Assert.Equal("shortlist.jogador_ja_na_lista", erro.Codigo);
        }

        [Fact]
        public void O_custo_diferente_nao_faz_do_jogador_um_alvo_novo()
        {
            // Arrange
            var shortlist = NovaShortlist();
            shortlist.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(5));

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(12)));

            // Assert
            Assert.Equal("shortlist.jogador_ja_na_lista", erro.Codigo);
        }

        [Fact]
        public void A_recusa_deixa_a_lista_intacta()
        {
                        // Arrange
            var shortlist = NovaShortlist();
            shortlist.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(5));

            // Act
            Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(12)));

            // Assert
            var alvo = Assert.Single(shortlist.Alvos);
            Assert.Equal(Euros(5), alvo.CustoEstimado);
        }

        [Fact]
        public void Jogadores_distintos_convivem_na_mesma_lista()
        {
            // Arrange
            var shortlist = NovaShortlist();

            // Act
            shortlist.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(5));
            shortlist.AdicionarAlvo(jogadorId: 1002, new Prioridade(2), Euros(5));

            // Assert
            Assert.Equal([1001, 1002], shortlist.Alvos.Select(alvo => alvo.JogadorId));
        }

        [Fact]
        public void O_mesmo_jogador_entra_em_listas_diferentes()
        {
            // Arrange
            var olheiroId = Guid.NewGuid();
            var laterais = Shortlist.Criar(olheiroId, "Laterais esquerdos 2026", Limite);
            var emergencia = Shortlist.Criar(olheiroId, "Emergencia janela de inverno", Limite);

            // Act
            laterais.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(5));
            emergencia.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(5));

            // Assert
            Assert.Equal(1001, Assert.Single(laterais.Alvos).JogadorId);
            Assert.Equal(1001, Assert.Single(emergencia.Alvos).JogadorId);
        }

        [Fact]
        public void Em_lista_cheia_a_repeticao_e_recusada_como_repeticao()
        {
            // Arrange
            var shortlist = NovaShortlist();
            for (var posicao = 1; posicao <= Teto; posicao++)
                shortlist.AdicionarAlvo(jogadorId: 1000 + posicao, new Prioridade(posicao), Euros(5));

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(5)));

            // Assert
            Assert.Equal("shortlist.jogador_ja_na_lista", erro.Codigo);
        }     
    }
}
