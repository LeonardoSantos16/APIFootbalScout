using APIFootballScout.Domain.Analise.Specifications;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Tests.Analise
{
    public class PosicoesCompativeisSpecificationTests
    {
        private static PosicoesCompativeisSpecification Compativeis(
            params (Posicao Posicao, Posicao[] Com)[] declaracoes)
            => new(declaracoes.ToDictionary(
                declaracao => declaracao.Posicao,
                declaracao => (IReadOnlyCollection<Posicao>)declaracao.Com));

        [Theory]
        [InlineData(Posicao.Goleiro)]
        [InlineData(Posicao.Defesa)]
        [InlineData(Posicao.MeioCampo)]
        [InlineData(Posicao.Ataque)]
        public void Posicao_igual_e_compativel_sem_precisar_do_mapa(Posicao posicao)
        {
            // Arrange
            var compativeis = Compativeis();

            // Act
            var compativel = compativeis.IsSatisfiedBy((posicao, posicao));

            // Assert
            Assert.True(compativel);
        }

        [Fact]
        public void O_mapa_declara_compatibilidade_alem_da_identidade()
        {
            // Arrange
            var compativeis = Compativeis((Posicao.Ataque, [Posicao.MeioCampo]));

            // Act
            var compativel = compativeis.IsSatisfiedBy((Posicao.Ataque, Posicao.MeioCampo));

            // Assert
            Assert.True(compativel);
        }

        [Fact]
        public void A_relacao_vale_nos_dois_sentidos_mesmo_declarada_em_um_so()
        {
            // Arrange — a config declara ataque compativel com meio-campo, e nada diz
            var compativeis = Compativeis((Posicao.Ataque, [Posicao.MeioCampo]));

            // Act
            var compativel = compativeis.IsSatisfiedBy((Posicao.MeioCampo, Posicao.Ataque));

            // Assert
            Assert.True(compativel);
        }

        [Fact]
        public void Posicoes_que_o_mapa_nao_declara_nao_sao_compativeis()
        {
            // Arrange
            var compativeis = Compativeis((Posicao.Ataque, [Posicao.MeioCampo]));

            // Act & Assert
            Assert.False(compativeis.IsSatisfiedBy((Posicao.Goleiro, Posicao.Ataque)));
            Assert.False(compativeis.IsSatisfiedBy((Posicao.Defesa, Posicao.Ataque)));
        }

        [Fact]
        public void O_mapa_e_politica_e_nao_relacao_fixa_do_modelo()
        {
            // Arrange
            var par = (Posicao.Defesa, Posicao.MeioCampo);

            // Act & Assert
            Assert.False(Compativeis().IsSatisfiedBy(par));
            Assert.True(Compativeis((Posicao.Defesa, [Posicao.MeioCampo])).IsSatisfiedBy(par));
        }
    }
}
