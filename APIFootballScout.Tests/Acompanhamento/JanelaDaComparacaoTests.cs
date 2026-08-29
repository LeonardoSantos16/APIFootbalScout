using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Tests.Acompanhamento
{
    public class JanelaDaComparacaoTests
    {
        [Fact]
        public void A_janela_declara_o_intervalo_decorrido_entre_as_duas_pontas()
        {
            var janela = new JanelaDaComparacao(
                de: new DateTime(2024, 1, 10),
                ate: new DateTime(2024, 3, 10));

            Assert.Equal(TimeSpan.FromDays(60), janela.Duracao());
        }

        [Fact]
        public void O_intervalo_tem_resolucao_menor_que_um_dia()
        {
            var janela = new JanelaDaComparacao(
                de: new DateTime(2024, 1, 10, 10, 0, 0),
                ate: new DateTime(2024, 1, 10, 15, 30, 0));

            Assert.Equal(TimeSpan.FromMinutes(330), janela.Duracao());
        }

        [Fact]
        public void Janela_invertida_e_recusada()
        {
            var erro = Assert.Throws<ValorInvalidoException>(
                () => new JanelaDaComparacao(
                    de: new DateTime(2024, 3, 10),
                    ate: new DateTime(2024, 1, 10)));

            Assert.Equal("janela_da_comparacao.intervalo_invalido", erro.Codigo);
        }

        [Fact]
        public void Janela_de_duracao_zero_e_recusada()
        {
            var instante = new DateTime(2024, 1, 10, 9, 0, 0);

            Assert.Throws<ValorInvalidoException>(
                () => new JanelaDaComparacao(de: instante, ate: instante));
        }
    }
}
