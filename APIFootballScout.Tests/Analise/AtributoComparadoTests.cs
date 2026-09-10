using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Tests.Analise
{
    public class AtributoComparadoTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        private static ResultadoDeAtributo Valor(decimal valor)
            => new AtributoCalculado(valor, new Minutagem(2400, Brasileirao2024));

        [Fact]
        public void O_atributo_comparado_indexa_os_valores_pela_identidade_do_jogador()
        {
            // Act
            var atributo = new AtributoComparado(
                TipoDeAtributo.Gols,
                new Dictionary<int, ResultadoDeAtributo>
                {
                    [13812] = Valor(0.45m),
                    [8256] = Valor(0.62m)
                });

            // Assert
            Assert.Equal([8256, 13812], atributo.Valores.Keys.OrderBy(id => id));
        }

        [Fact]
        public void Um_jogador_so_nao_forma_atributo_comparado()
        {
            // Act
            var erro = Assert.Throws<ValorInvalidoException>(
                () => new AtributoComparado(
                    TipoDeAtributo.Gols,
                    new Dictionary<int, ResultadoDeAtributo> { [13812] = Valor(0.45m) }));

            // Assert
            Assert.Equal("atributo_comparado.par_incompleto", erro.Codigo);
        }

        [Fact]
        public void Tres_jogadores_nao_formam_atributo_comparado()
        {
            // Act
            var erro = Assert.Throws<ValorInvalidoException>(
                () => new AtributoComparado(
                    TipoDeAtributo.Gols,
                    new Dictionary<int, ResultadoDeAtributo>
                    {
                        [13812] = Valor(0.45m),
                        [8256] = Valor(0.62m),
                        [4711] = Valor(0.30m)
                    }));

            // Assert
            Assert.Equal("atributo_comparado.par_incompleto", erro.Codigo);
        }
    }
}
