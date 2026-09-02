using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.Specifications;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Tests.Shortlists
{
    // R7.2 - nao ha jogador repetido na mesma lista. Invariante pura: nao tem valor
    // de politica a ajustar, entao mora inteira no agregado, sem specification.
    // O jogador e a identidade do alvo; o custo estimado nao participa dela.
    public class UnicidadeDoAlvoTests
    {
        private const int Limite = 3;
        private static readonly ShortlistComVagaSpecification ComVaga = new(Limite);

        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static Shortlist NovaShortlist()
            => Shortlist.Criar(olheiroId: Guid.NewGuid(), nome: "Laterais esquerdos 2026");

        [Fact]
        public void O_mesmo_jogador_nao_entra_duas_vezes()
        {
            // Arrange
            var shortlist = NovaShortlist();
            shortlist.AdicionarAlvo(jogadorId: 1001, Euros(5), ComVaga);

            // Act
            var erro = Assert.Throws<ConflitoDeDominioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 1001, Euros(5), ComVaga));

            // Assert
            Assert.Equal("shortlist.jogador_ja_na_lista", erro.Codigo);
        }

        [Fact]
        public void O_custo_diferente_nao_faz_do_jogador_um_alvo_novo()
        {
            // A identidade do alvo e o jogador. Se o custo participasse dela, o mesmo
            // jogador entraria duas vezes so por ter sido reavaliado - que e exatamente
            // a repeticao que R7.2 proibe.

            // Arrange
            var shortlist = NovaShortlist();
            shortlist.AdicionarAlvo(jogadorId: 1001, Euros(5), ComVaga);

            // Act
            var erro = Assert.Throws<ConflitoDeDominioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 1001, Euros(12), ComVaga));

            // Assert
            Assert.Equal("shortlist.jogador_ja_na_lista", erro.Codigo);
        }

        [Fact]
        public void A_recusa_deixa_a_lista_intacta()
        {
            // Nem escreve o alvo repetido nem altera o que ja estava la: o custo antigo
            // permanece, porque a insercao recusada nao e uma atualizacao disfarcada.

            // Arrange
            var shortlist = NovaShortlist();
            shortlist.AdicionarAlvo(jogadorId: 1001, Euros(5), ComVaga);

            // Act
            Assert.Throws<ConflitoDeDominioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 1001, Euros(12), ComVaga));

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
            shortlist.AdicionarAlvo(jogadorId: 1001, Euros(5), ComVaga);
            shortlist.AdicionarAlvo(jogadorId: 1002, Euros(5), ComVaga);

            // Assert
            Assert.Equal([1001, 1002], shortlist.Alvos.Select(alvo => alvo.JogadorId));
        }

        [Fact]
        public void O_mesmo_jogador_entra_em_listas_diferentes()
        {
            // A unicidade e por lista, nao por olheiro: o mesmo alvo pode figurar na
            // lista de laterais e na de emergencia.

            // Arrange
            var olheiroId = Guid.NewGuid();
            var laterais = Shortlist.Criar(olheiroId, "Laterais esquerdos 2026");
            var emergencia = Shortlist.Criar(olheiroId, "Emergencia janela de inverno");

            // Act
            laterais.AdicionarAlvo(jogadorId: 1001, Euros(5), ComVaga);
            emergencia.AdicionarAlvo(jogadorId: 1001, Euros(5), ComVaga);

            // Assert
            Assert.Equal(1001, Assert.Single(laterais.Alvos).JogadorId);
            Assert.Equal(1001, Assert.Single(emergencia.Alvos).JogadorId);
        }

        [Fact]
        public void Em_lista_cheia_a_repeticao_e_recusada_como_repeticao()
        {
            // R7.2 e julgada antes de R7.1. "Ja esta na lista" vale independentemente de
            // haver vaga, e abrir espaco nao tornaria a insercao legitima - dizer
            // "lista cheia" mandaria o olheiro remover um alvo a toa.

            // Arrange
            var shortlist = NovaShortlist();
            for (var posicao = 1; posicao <= Limite; posicao++)
                shortlist.AdicionarAlvo(jogadorId: 1000 + posicao, Euros(5), ComVaga);

            // Act
            var erro = Assert.Throws<ConflitoDeDominioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 1001, Euros(5), ComVaga));

            // Assert
            Assert.Equal("shortlist.jogador_ja_na_lista", erro.Codigo);
        }

        private static Alvo AlvoDe(int jogadorId, int prioridade)
            => new(jogadorId, new Prioridade(prioridade), Euros(5));

        [Fact]
        public void Restaurar_recusa_documento_com_jogador_repetido()
        {
            // A invariante vale para o agregado, nao so para a operacao que o alterou.
            // Um documento com repeticao nunca deveria ter sido escrito; se existe, e
            // corrupcao, e carregar em silencio propagaria o defeito para a leitura.

            // Act
            var erro = Assert.Throws<ConflitoDeDominioException>(() => Shortlist.Restaurar(
                id: Guid.NewGuid(),
                olheiroId: Guid.NewGuid(),
                nome: "Laterais esquerdos 2026",
                alvos: [AlvoDe(1001, prioridade: 1), AlvoDe(1001, prioridade: 2)]));

            // Assert
            Assert.Equal("shortlist.jogador_ja_na_lista", erro.Codigo);
        }

        [Fact]
        public void Restaurar_aceita_documento_sem_repeticao()
        {
            // Act
            var shortlist = Shortlist.Restaurar(
                id: Guid.NewGuid(),
                olheiroId: Guid.NewGuid(),
                nome: "Laterais esquerdos 2026",
                alvos: [AlvoDe(1001, prioridade: 1), AlvoDe(1002, prioridade: 2)]);

            // Assert
            Assert.Equal([1001, 1002], shortlist.Alvos.Select(alvo => alvo.JogadorId));
        }
    }
}
