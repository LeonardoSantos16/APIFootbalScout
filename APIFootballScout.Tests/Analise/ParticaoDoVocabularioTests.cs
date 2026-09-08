using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Tests.Analise
{
    /// <summary>
    /// R9.4 — um enum unico de atributo tornou exprimivel o que o modelo nao admite:
    /// acumulavel com tipo derivado e derivado com tipo acumulavel. A guarda recompoe
    /// a particao que a unificacao do vocabulario deixou de garantir pelo tipo.
    /// </summary>
    public class ParticaoDoVocabularioTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        [Theory]
        [InlineData(TipoDeAtributo.Rating)]
        [InlineData(TipoDeAtributo.PrecisaoDePasse)]
        public void Tipo_derivado_nao_e_estatistica_acumulavel(TipoDeAtributo tipo)
        {
            // Act
            var erro = Assert.Throws<ValorInvalidoException>(
                () => new EstatisticaAcumulavel(tipo, 12, new Minutagem(2400, Brasileirao2024)));

            // Assert
            Assert.Equal("estatistica_acumulavel.tipo_nao_acumulavel", erro.Codigo);
        }

        [Theory]
        [InlineData(TipoDeAtributo.Gols)]
        [InlineData(TipoDeAtributo.Assistencias)]
        [InlineData(TipoDeAtributo.PassesDecisivos)]
        [InlineData(TipoDeAtributo.Desarmes)]
        [InlineData(TipoDeAtributo.Interceptacoes)]
        public void Tipo_acumulavel_nao_e_valor_derivado(TipoDeAtributo tipo)
        {
            // Act
            var erro = Assert.Throws<ValorInvalidoException>(
                () => new ValorDerivado(tipo, 7.42m, new Minutagem(2400, Brasileirao2024)));

            // Assert
            Assert.Equal("valor_derivado.tipo_nao_derivado", erro.Codigo);
        }
    }
}
