using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;
using APIFootballScout.Infrastructure.Persistence.Mappers;

namespace APIFootballScout.Tests.Shortlists
{
    public class ShortlistMapperTests
    {
        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static Shortlist ShortlistCom(int limite, params int[] jogadores)
        {
            var shortlist = Shortlist.Criar(
                olheiroId: Guid.NewGuid(),
                nome: "Laterais esquerdos 2026",
                limite: new LimiteDeAlvos(limite));

            for (var posicao = 1; posicao <= jogadores.Length; posicao++)
                shortlist.AdicionarAlvo(
                    jogadores[posicao - 1], new Prioridade(posicao), Euros(posicao));

            return shortlist;
        }

        private static Shortlist RoundTrip(Shortlist shortlist)
            => ShortlistMapper.MapToDomain(ShortlistMapper.MapToEntity(shortlist));

        private static (int Jogador, int Prioridade)[] Ordem(Shortlist shortlist)
            => [.. shortlist.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade.Valor))];

        [Fact]
        public void A_shortlist_vazia_faz_round_trip_pelo_documento()
        {
            // Arrange
            var original = ShortlistCom(limite: 25);

            // Act
            var restaurada = RoundTrip(original);

            // Assert
            Assert.Equal(original.Id, restaurada.Id);
            Assert.Equal(original.OlheiroId, restaurada.OlheiroId);
            Assert.Equal(original.Nome, restaurada.Nome);
            Assert.Equal(25, restaurada.Limite.Valor);
            Assert.Empty(restaurada.Alvos);
            Assert.Null(restaurada.CustoTotal);
        }

        [Fact]
        public void A_shortlist_com_alvos_faz_round_trip_pelo_documento()
        {
            // Arrange
            var original = ShortlistCom(25, 1001, 1002, 1003);

            // Act
            var restaurada = RoundTrip(original);

            // Assert
            Assert.Equal([(1001, 1), (1002, 2), (1003, 3)], Ordem(restaurada));
            Assert.Equal(Euros(2), restaurada.Alvos.Single(a => a.JogadorId == 1002).CustoEstimado);
            Assert.Equal(Euros(6), restaurada.CustoTotal);
        }

        [Fact]
        public void O_limite_sobrevive_ao_round_trip()
        {
            // Arrange
            var restaurada = RoundTrip(ShortlistCom(limite: 2, 1001, 1002));

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => restaurada.AdicionarAlvo(2001, new Prioridade(3), Euros(9)));

            // Assert
            Assert.Equal("shortlist.limite_de_alvos_atingido", erro.Codigo);
            Assert.Equal(2, restaurada.Limite.Valor);
        }

        [Fact]
        public void A_shortlist_restaurada_continua_editavel()
        {
            // Arrange
            var restaurada = RoundTrip(ShortlistCom(25, 1001, 1002, 1003));

            // Act
            restaurada.RemoverAlvo(1001);
            restaurada.AdicionarAlvo(2001, new Prioridade(1), Euros(9));

            // Assert
            Assert.Equal([(2001, 1), (1002, 2), (1003, 3)], Ordem(restaurada));
        }

        [Fact]
        public void A_moeda_da_lista_sobrevive_ao_round_trip()
        {
            // Arrange
            var restaurada = RoundTrip(ShortlistCom(25, 1001));

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => restaurada.AdicionarAlvo(2001, new Prioridade(2), new Dinheiro(900_000_000, "GBP")));

            // Assert
            Assert.Equal("shortlist.moeda_divergente", erro.Codigo);
        }

        [Fact]
        public void O_documento_guarda_a_shortlist_e_o_limite()
        {
            // Arrange
            var shortlist = ShortlistCom(limite: 10, 1001);

            // Act
            var documento = ShortlistMapper.MapToEntity(shortlist);

            // Assert
            Assert.Equal(shortlist.Id, documento.Id);
            Assert.Equal(shortlist.OlheiroId, documento.OlheiroId);
            Assert.Equal("Laterais esquerdos 2026", documento.Nome);
            Assert.Equal(10, documento.LimiteDeAlvos);
        }

        [Fact]
        public void O_documento_guarda_os_alvos_na_ordem_com_custo_em_centavos()
        {
            // Arrange
            var shortlist = ShortlistCom(25, 1001, 1002);

            // Act
            var documento = ShortlistMapper.MapToEntity(shortlist);

            // Assert
            Assert.Equal([(1001, 1), (1002, 2)],
                documento.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade)));
            Assert.Equal(100_000_000, documento.Alvos[0].CustoEmCentavos);
            Assert.Equal("EUR", documento.Alvos[0].Moeda);
        }

        [Fact]
        public void A_ordem_dos_alvos_no_documento_segue_a_repriorizacao()
        {
            // Arrange
            var shortlist = ShortlistCom(25, 1001, 1002, 1003);
            shortlist.AtualizarPrioridade(1003, new Prioridade(1));

            // Act
            var documento = ShortlistMapper.MapToEntity(shortlist);

            // Assert
            Assert.Equal([(1003, 1), (1001, 2), (1002, 3)],
                documento.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade)));
        }
    }
}
