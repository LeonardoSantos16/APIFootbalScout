using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Tests.Shortlists
{
    // R7.4 - a remocao de um alvo nao deixa lacuna na ordem. Quem estava depois do
    // removido sobe uma posicao; a ordem relativa entre os que ficam nao muda.
    public class RemocaoDeAlvoTests
    {
        private static readonly LimiteDeAlvos Limite = new(25);

        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static Shortlist ShortlistCom(params int[] jogadores)
        {
            var shortlist = Shortlist.Criar(olheiroId: Guid.NewGuid(), nome: "Laterais esquerdos 2026", limite: Limite);
            for (var posicao = 1; posicao <= jogadores.Length; posicao++)
                shortlist.AdicionarAlvo(jogadores[posicao - 1], new Prioridade(posicao), Euros(5));

            return shortlist;
        }

        private static (int Jogador, int Prioridade)[] Ordem(Shortlist shortlist)
            => [.. shortlist.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade.Valor))];

        [Fact]
        public void A_remocao_do_meio_promove_os_subsequentes()
        {
            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003);

            // Act
            shortlist.RemoverAlvo(jogadorId: 1002);

            // Assert
            Assert.Equal([(1001, 1), (1003, 2)], Ordem(shortlist));
        }

        [Fact]
        public void A_remocao_do_topo_promove_todos()
        {
            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003);

            // Act
            shortlist.RemoverAlvo(jogadorId: 1001);

            // Assert
            Assert.Equal([(1002, 1), (1003, 2)], Ordem(shortlist));
        }

        [Fact]
        public void A_remocao_do_ultimo_nao_mexe_em_ninguem()
        {
            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003);

            // Act
            shortlist.RemoverAlvo(jogadorId: 1003);

            // Assert
            Assert.Equal([(1001, 1), (1002, 2)], Ordem(shortlist));
        }

        [Fact]
        public void A_remocao_do_unico_alvo_esvazia_a_lista()
        {
            // Lista vazia e estado valido: a shortlist existe antes de ter alvos e
            // continua existindo depois de perder o ultimo.

            // Arrange
            var shortlist = ShortlistCom(1001);

            // Act
            shortlist.RemoverAlvo(jogadorId: 1001);

            // Assert
            Assert.Empty(shortlist.Alvos);
        }

        [Fact]
        public void Remover_jogador_fora_da_lista_e_recusado()
        {
            // Arrange
            var shortlist = ShortlistCom(1001, 1002);

            // Act
            var erro = Assert.Throws<RecursoNaoEncontradoException>(
                () => shortlist.RemoverAlvo(jogadorId: 9999));

            // Assert
            Assert.Equal("shortlist.alvo_nao_encontrado", erro.Codigo);
            Assert.Equal([(1001, 1), (1002, 2)], Ordem(shortlist));
        }

        [Fact]
        public void Depois_de_varias_remocoes_as_prioridades_sao_exatamente_um_ate_n()
        {
            // A invariante do conjunto: nem lacuna nem empate, qualquer que seja a
            // sequencia de remocoes.

            // Arrange
            var shortlist = ShortlistCom(1001, 1002, 1003, 1004, 1005);

            // Act
            shortlist.RemoverAlvo(jogadorId: 1002);
            shortlist.RemoverAlvo(jogadorId: 1005);
            shortlist.RemoverAlvo(jogadorId: 1001);

            // Assert
            Assert.Equal([(1003, 1), (1004, 2)], Ordem(shortlist));
        }

        [Fact]
        public void A_remocao_libera_vaga()
        {
            // R7.4 encontrando R7.1: a lista cheia volta a aceitar alvo depois que um
            // sai, porque o limite conta alvos, nao insercoes ja feitas.

            // Arrange
            var shortlist = Shortlist.Criar(Guid.NewGuid(), "Laterais esquerdos 2026", new LimiteDeAlvos(3));
            shortlist.AdicionarAlvo(1001, new Prioridade(1), Euros(5));
            shortlist.AdicionarAlvo(1002, new Prioridade(2), Euros(5));
            shortlist.AdicionarAlvo(1003, new Prioridade(3), Euros(5));

            // Act
            shortlist.RemoverAlvo(jogadorId: 1002);
            shortlist.AdicionarAlvo(2001, new Prioridade(3), Euros(5));

            // Assert
            Assert.Equal([(1001, 1), (1003, 2), (2001, 3)], Ordem(shortlist));
        }

        [Fact]
        public void O_jogador_removido_pode_voltar_a_lista()
        {
            // R7.2 julga o estado atual, nao o historico: sair da lista devolve o
            // jogador a condicao de alvo elegivel.

            // Arrange
            var shortlist = ShortlistCom(1001, 1002);

            // Act
            shortlist.RemoverAlvo(jogadorId: 1001);
            shortlist.AdicionarAlvo(1001, new Prioridade(2), Euros(5));

            // Assert
            Assert.Equal([(1002, 1), (1001, 2)], Ordem(shortlist));
        }
    }
}
