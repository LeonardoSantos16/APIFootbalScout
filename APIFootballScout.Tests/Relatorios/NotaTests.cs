using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Tests.Relatorios
{
    // R5.4 - a nota respeita uma faixa de valores valida: [0, 10] fechada, com no
    // maximo uma casa decimal. A faixa e do tipo, nao do agregado: nota fora dela
    // nao chega a existir, entao nenhum caminho de atribuicao escapa da checagem.
    public class NotaTests
    {
        public static TheoryData<decimal> NotasValidas => new() { 0m, 10m, 7m, 8.5m, 0.1m, 9.9m };

        public static TheoryData<decimal> NotasForaDaFaixa => new() { -0.1m, -1m, 10.1m, 11m, 100m };

        public static TheoryData<decimal> NotasPrecisasDemais => new() { 8.55m, 7.123m, 0.01m, 9.999m };

        [Theory]
        [MemberData(nameof(NotasValidas))]
        public void A_nota_dentro_da_faixa_e_aceita(decimal valor)
        {
            // Act
            var nota = new Nota(valor);

            // Assert
            Assert.Equal(valor, nota.Valor);
        }

        [Theory]
        [MemberData(nameof(NotasForaDaFaixa))]
        public void A_nota_fora_da_faixa_e_recusada(decimal valor)
        {
            // Act
            var erro = Assert.Throws<ValorInvalidoException>(() => new Nota(valor));

            // Assert
            Assert.Equal("nota.fora_da_faixa", erro.Codigo);
        }

        [Fact]
        public void Os_extremos_da_faixa_sao_notas_validas()
        {
            // A faixa e fechada: 0 e 10 valem. Zero e a nota mais baixa que o olheiro
            // pode dar, nao a ausencia de nota - essa e representada por null no rascunho.

            // Act & Assert
            Assert.Equal(0m, new Nota(0m).Valor);
            Assert.Equal(10m, new Nota(10m).Valor);
        }

        [Theory]
        [MemberData(nameof(NotasPrecisasDemais))]
        public void A_nota_com_mais_de_uma_casa_decimal_e_recusada(decimal valor)
        {
            // Act
            var erro = Assert.Throws<ValorInvalidoException>(() => new Nota(valor));

            // Assert
            Assert.Equal("nota.precisao_invalida", erro.Codigo);
        }

        [Fact]
        public void A_precisao_e_julgada_pelo_valor_nao_pela_representacao()
        {
            // 8.50m e 8.5m sao o mesmo numero escrito com escalas diferentes. A regra
            // fala da granularidade da nota, entao a escala do decimal nao pode decidir
            // se ela e valida.

            // Act
            var nota = new Nota(8.50m);

            // Assert
            Assert.Equal(new Nota(8.5m), nota);
        }

        [Fact]
        public void Duas_notas_de_mesmo_valor_sao_iguais()
        {
            // Value object: identidade e o valor, nao a instancia.

            // Act & Assert
            Assert.Equal(new Nota(7.5m), new Nota(7.5m));
            Assert.NotEqual(new Nota(7.5m), new Nota(7.6m));
        }
    }
}
