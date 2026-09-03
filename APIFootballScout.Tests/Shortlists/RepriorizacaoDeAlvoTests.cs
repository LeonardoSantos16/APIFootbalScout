using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.Specifications;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Tests.Shortlists
{
    // R7.7 - a repriorizacao nao cria posicao nova nem deixa lacuna. Mover um alvo e
    // remove-lo e inseri-lo na mesma operacao, entao valem os deslocamentos de R7.3 e
    // R7.4; a faixa e que muda: quem ja esta na lista disputa de 1 a n, nao a n+1.
    public class RepriorizacaoDeAlvoTests
    {
        private static readonly ShortlistComVagaSpecification ComVaga = new(25);

        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static Shortlist ShortlistCom(params int[] jogadores)
        {
            var shortlist = Shortlist.Criar(olheiroId: Guid.NewGuid(), nome: "Laterais esquerdos 2026");
            for (var posicao = 1; posicao <= jogadores.Length; posicao++)
                shortlist.AdicionarAlvo(jogadores[posicao - 1], new Prioridade(posicao), Euros(posicao), ComVaga);

            return shortlist;
        }

        private static (int Jogador, int Prioridade)[] Ordem(Shortlist shortlist)
            => [.. shortlist.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade.Valor))];

        [Fact]
        public void Descer_um_alvo_promove_quem_estava_no_caminho()
        {
            // 1001 sai da posicao 1 para a 3; 1002 e 1003 sobem uma posicao cada.

            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003);

            // Act
            shortlist.AtualizarPrioridade(jogadorId: 1001, new Prioridade(3));

            // Assert
            Assert.Equal([(1002, 1), (1003, 2), (1001, 3)], Ordem(shortlist));
        }

        [Fact]
        public void Subir_um_alvo_rebaixa_quem_estava_no_caminho()
        {
            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003);

            // Act
            shortlist.AtualizarPrioridade(jogadorId: 1003, new Prioridade(1));

            // Assert
            Assert.Equal([(1003, 1), (1001, 2), (1002, 3)], Ordem(shortlist));
        }

        [Fact]
        public void Mover_para_a_propria_posicao_nao_muda_nada()
        {
            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003);

            // Act
            shortlist.AtualizarPrioridade(jogadorId: 1002, new Prioridade(2));

            // Assert
            Assert.Equal([(1001, 1), (1002, 2), (1003, 3)], Ordem(shortlist));
        }

        [Fact]
        public void A_repriorizacao_nao_altera_a_quantidade_de_alvos()
        {
            // Mover nao e adicionar: o alvo sai e volta na mesma operacao.

            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003);

            // Act
            shortlist.AtualizarPrioridade(jogadorId: 1001, new Prioridade(2));

            // Assert
            Assert.Equal(3, shortlist.Alvos.Count);
            Assert.Equal([1001, 1002, 1003], shortlist.Alvos.Select(alvo => alvo.JogadorId).Order());
        }

        [Fact]
        public void A_repriorizacao_preserva_o_custo_estimado()
        {
            // So a posicao muda. O custo e reconstruido junto do alvo, entao precisa
            // sobreviver a viagem.

            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003);

            // Act
            shortlist.AtualizarPrioridade(jogadorId: 1003, new Prioridade(1));

            // Assert
            var alvo = shortlist.Alvos.Single(a => a.JogadorId == 1003);
            Assert.Equal(Euros(3), alvo.CustoEstimado);
        }

        [Fact]
        public void A_posicao_alem_da_ultima_e_recusada()
        {
            // Com tres alvos as posicoes sao 1, 2 e 3. A 4 so existe para quem esta
            // entrando na lista, nao para quem ja esta nela.

            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003);

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AtualizarPrioridade(jogadorId: 1001, new Prioridade(4)));

            // Assert
            Assert.Equal("shortlist.prioridade_fora_da_ordem", erro.Codigo);
        }

        [Fact]
        public void A_recusa_da_posicao_deixa_a_lista_intacta()
        {
            // A recusa nao pode ter removido o alvo pelo caminho: nem perde o jogador
            // nem abre lacuna na ordem.

            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003);

            // Act
            Assert.Throws<RegraDeNegocioException>(
                () => shortlist.AtualizarPrioridade(jogadorId: 1001, new Prioridade(4)));

            // Assert
            Assert.Equal([(1001, 1), (1002, 2), (1003, 3)], Ordem(shortlist));
        }

        [Fact]
        public void Repriorizar_jogador_fora_da_lista_e_recusado()
        {
            // Arrange
            var shortlist = ShortlistCom(1001, 1002);

            // Act
            var erro = Assert.Throws<RecursoNaoEncontradoException>(
                () => shortlist.AtualizarPrioridade(jogadorId: 9999, new Prioridade(1)));

            // Assert
            Assert.Equal("shortlist.alvo_nao_encontrado", erro.Codigo);
            Assert.Equal([(1001, 1), (1002, 2)], Ordem(shortlist));
        }
    }
}
