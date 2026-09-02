using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.Specifications;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Tests.Shortlists
{
    // R7.3 - a prioridade e uma ordem total, sem empates. O olheiro escolhe a posicao
    // e a lista se reacomoda: inserir no meio empurra os subsequentes, e nunca ha duas
    // vezes a mesma posicao nem posicao vazia entre a primeira e a ultima.
    public class OrdemDosAlvosTests
    {
        private static readonly ShortlistComVagaSpecification ComVaga = new(25);

        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static Shortlist NovaShortlist()
            => Shortlist.Criar(olheiroId: Guid.NewGuid(), nome: "Laterais esquerdos 2026");

        private static Shortlist ShortlistCom(params int[] jogadores)
        {
            var shortlist = NovaShortlist();
            for (var posicao = 1; posicao <= jogadores.Length; posicao++)
                shortlist.AdicionarAlvo(jogadores[posicao - 1], new Prioridade(posicao), Euros(5), ComVaga);

            return shortlist;
        }

        private static (int Jogador, int Prioridade)[] Ordem(Shortlist shortlist)
            => [.. shortlist.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade.Valor))];

        [Fact]
        public void O_primeiro_alvo_ocupa_a_posicao_um()
        {
            // Arrange
            var shortlist = NovaShortlist();

            // Act
            shortlist.AdicionarAlvo(jogadorId: 1001, new Prioridade(1), Euros(5), ComVaga);

            // Assert
            Assert.Equal([(1001, 1)], Ordem(shortlist));
        }

        [Fact]
        public void A_insercao_no_fim_nao_mexe_em_ninguem()
        {
            // Arrange
            var shortlist = ShortlistCom(1001, 1002);

            // Act
            shortlist.AdicionarAlvo(jogadorId: 1003, new Prioridade(3), Euros(5), ComVaga);

            // Assert
            Assert.Equal([(1001, 1), (1002, 2), (1003, 3)], Ordem(shortlist));
        }

        [Fact]
        public void A_insercao_no_meio_desloca_os_subsequentes()
        {
            // Exemplo trabalhado: com 1001@1, 1002@2 e 1003@3, o alvo 2001 entrando na
            // posicao 2 empurra 1002 para 3 e 1003 para 4. Quem esta antes nao se move.

            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003);

            // Act
            shortlist.AdicionarAlvo(jogadorId: 2001, new Prioridade(2), Euros(5), ComVaga);

            // Assert
            Assert.Equal([(1001, 1), (2001, 2), (1002, 3), (1003, 4)], Ordem(shortlist));
        }

        [Fact]
        public void A_insercao_no_topo_desloca_todos()
        {
            // Arrange
            var shortlist = ShortlistCom(1001, 1002);

            // Act
            shortlist.AdicionarAlvo(jogadorId: 2001, new Prioridade(1), Euros(5), ComVaga);

            // Assert
            Assert.Equal([(2001, 1), (1001, 2), (1002, 3)], Ordem(shortlist));
        }

        [Fact]
        public void A_prioridade_alem_da_proxima_posicao_e_recusada()
        {
            // Com dois alvos, so ha tres posicoes possiveis: 1, 2 e 3. Aceitar 4 abriria
            // uma lacuna, e a ordem deixaria de ser contigua.

            // Arrange
            var shortlist = ShortlistCom(1001, 1002);

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 2001, new Prioridade(4), Euros(5), ComVaga));

            // Assert
            Assert.Equal("shortlist.prioridade_fora_da_ordem", erro.Codigo);
        }

        [Fact]
        public void A_lista_vazia_so_aceita_a_posicao_um()
        {
            // Arrange
            var shortlist = NovaShortlist();

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 1001, new Prioridade(2), Euros(5), ComVaga));

            // Assert
            Assert.Equal("shortlist.prioridade_fora_da_ordem", erro.Codigo);
        }

        [Fact]
        public void Depois_de_varias_insercoes_as_prioridades_sao_exatamente_um_ate_n()
        {
            // A invariante do conjunto, nao de uma insercao: sem empate e sem lacuna,
            // seja qual for a sequencia de posicoes escolhidas pelo olheiro.

            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003);

            // Act
            shortlist.AdicionarAlvo(jogadorId: 2001, new Prioridade(2), Euros(5), ComVaga);
            shortlist.AdicionarAlvo(jogadorId: 2002, new Prioridade(1), Euros(5), ComVaga);
            shortlist.AdicionarAlvo(jogadorId: 2003, new Prioridade(6), Euros(5), ComVaga);

            // Assert
            Assert.Equal([1, 2, 3, 4, 5, 6], shortlist.Alvos.Select(alvo => alvo.Prioridade.Valor).Order());
        }

        [Fact]
        public void A_recusa_da_prioridade_deixa_a_ordem_intacta()
        {
            // Arrange
            var shortlist = ShortlistCom(1001, 1002);

            // Act
            Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AdicionarAlvo(jogadorId: 2001, new Prioridade(9), Euros(5), ComVaga));

            // Assert
            Assert.Equal([(1001, 1), (1002, 2)], Ordem(shortlist));
        }
    }
}
