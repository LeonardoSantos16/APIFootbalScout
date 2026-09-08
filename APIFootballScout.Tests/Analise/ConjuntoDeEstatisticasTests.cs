using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Tests.Analise
{
    public class ConjuntoDeEstatisticasTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        [Fact]
        public void Conjunto_cujas_estatisticas_saem_todas_do_mesmo_recorte_e_construivel()
        {
            // Arrange
            var minutagem = new Minutagem(2400, Brasileirao2024);

            // Act
            var conjunto = new ConjuntoDeEstatisticas(
                Brasileirao2024,
                [
                    new EstatisticaAcumulavel(TipoDeAtributo.Gols, 12, minutagem),
                    new EstatisticaAcumulavel(TipoDeAtributo.Assistencias, 7, minutagem)
                ],
                [new ValorDerivado(TipoDeAtributo.Rating, 7.42m, minutagem)]);

            // Assert
            Assert.Equal(Brasileirao2024, conjunto.Recorte);
        }

        [Theory]
        [InlineData(8, 63814, ContextoDeRecorte.Clube)]
        [InlineData(325, 77012, ContextoDeRecorte.Clube)]
        [InlineData(325, 63814, ContextoDeRecorte.Selecao)]
        public void Estatistica_de_outro_recorte_nao_entra_no_conjunto(
            int competicaoId, int temporadaId, ContextoDeRecorte contexto)
        {
            // Arrange
            var outroRecorte = new Recorte(competicaoId, temporadaId, contexto);

            // Act
            var erro = Assert.Throws<ValorInvalidoException>(
                () => new ConjuntoDeEstatisticas(
                    Brasileirao2024,
                    [
                        new EstatisticaAcumulavel(
                            TipoDeAtributo.Gols, 12, new Minutagem(2400, Brasileirao2024)),
                        new EstatisticaAcumulavel(
                            TipoDeAtributo.Desarmes, 18, new Minutagem(2400, outroRecorte))
                    ],
                    []));

            // Assert
            Assert.Equal("conjunto_de_estatisticas.recorte_divergente", erro.Codigo);
        }

        [Theory]
        [InlineData(8, 63814, ContextoDeRecorte.Clube)]
        [InlineData(325, 77012, ContextoDeRecorte.Clube)]
        [InlineData(325, 63814, ContextoDeRecorte.Selecao)]
        public void Valor_derivado_de_outro_recorte_nao_entra_no_conjunto(
            int competicaoId, int temporadaId, ContextoDeRecorte contexto)
        {
            // Arrange — o derivado passou a declarar a amostra que o sustenta, e com ela
            var outroRecorte = new Recorte(competicaoId, temporadaId, contexto);

            // Act
            var erro = Assert.Throws<ValorInvalidoException>(
                () => new ConjuntoDeEstatisticas(
                    Brasileirao2024,
                    [
                        new EstatisticaAcumulavel(
                            TipoDeAtributo.Gols, 12, new Minutagem(2400, Brasileirao2024))
                    ],
                    [
                        new ValorDerivado(
                            TipoDeAtributo.Rating, 7.42m, new Minutagem(2400, outroRecorte))
                    ]));

            // Assert
            Assert.Equal("conjunto_de_estatisticas.recorte_divergente", erro.Codigo);
        }

        [Fact]
        public void O_conjunto_declara_a_minutagem_unica_que_o_sustenta()
        {
            // Arrange
            var minutagem = new Minutagem(2400, Brasileirao2024);

            // Act
            var conjunto = new ConjuntoDeEstatisticas(
                Brasileirao2024,
                [new EstatisticaAcumulavel(TipoDeAtributo.Gols, 12, minutagem)],
                [new ValorDerivado(TipoDeAtributo.Rating, 7.42m, minutagem)]);

            // Assert
            Assert.Equal(minutagem, conjunto.Minutagem);
        }

        [Fact]
        public void Acumulaveis_de_minutagens_diferentes_nao_formam_um_conjunto()
        {
            // Arrange — o mesmo recorte nao basta: a recusa da comparacao e por lado,

            // Act
            var erro = Assert.Throws<ValorInvalidoException>(
                () => new ConjuntoDeEstatisticas(
                    Brasileirao2024,
                    [
                        new EstatisticaAcumulavel(
                            TipoDeAtributo.Gols, 12, new Minutagem(2400, Brasileirao2024)),
                        new EstatisticaAcumulavel(
                            TipoDeAtributo.Desarmes, 18, new Minutagem(1800, Brasileirao2024))
                    ],
                    []));

            // Assert
            Assert.Equal("conjunto_de_estatisticas.minutagem_divergente", erro.Codigo);
        }

        [Fact]
        public void Derivado_de_outra_minutagem_nao_entra_no_conjunto()
        {
            // Act
            var erro = Assert.Throws<ValorInvalidoException>(
                () => new ConjuntoDeEstatisticas(
                    Brasileirao2024,
                    [
                        new EstatisticaAcumulavel(
                            TipoDeAtributo.Gols, 12, new Minutagem(2400, Brasileirao2024))
                    ],
                    [
                        new ValorDerivado(
                            TipoDeAtributo.Rating, 7.42m, new Minutagem(1800, Brasileirao2024))
                    ]));

            // Assert
            Assert.Equal("conjunto_de_estatisticas.minutagem_divergente", erro.Codigo);
        }
    }
}
