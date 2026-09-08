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
                [new ValorDerivado(TipoDeAtributo.Rating, 7.42m)]);

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
    }
}
