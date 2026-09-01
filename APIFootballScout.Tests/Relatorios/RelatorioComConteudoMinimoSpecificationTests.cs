using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.Specifications;

namespace APIFootballScout.Tests.Relatorios
{
    public class RelatorioComConteudoMinimoSpecificationTests
    {
        private static readonly DateTimeOffset ObservadoEm = new(2026, 8, 20, 15, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset AbertoEm = new(2026, 8, 25, 12, 0, 0, TimeSpan.Zero);

        // "Leitura de jogo" tem 15 caracteres; os demais, 5 cada. Somados, 30.
        private const string PositivoLongo = "Leitura de jogo";
        private const string PositivoCurto = "Chute";
        private const string NegativoA = "Ritmo";
        private const string NegativoB = "Duelo";
        private const int TotalDeCaracteresDosPontos = 30;

        private static Relatorio Rascunho(string[] positivos, string[] negativos, string texto = "Observado no classico.")
        {
            var relatorio = Relatorio.AbrirRascunho(
                jogadorId: 42,
                olheiroId: Guid.NewGuid(),
                texto: texto,
                observadoEm: ObservadoEm,
                agora: AbertoEm);

            foreach (var positivo in positivos)
                relatorio.AdicionarPontoPositivo(positivo);

            foreach (var negativo in negativos)
                relatorio.AdicionarPontoNegativo(negativo);

            return relatorio;
        }

        private static Relatorio RascunhoNoLimiar() => Rascunho(
            positivos: [PositivoLongo, PositivoCurto],
            negativos: [NegativoA, NegativoB]);

        [Fact]
        public void Rascunho_exatamente_no_limiar_atende_o_conteudo_minimo()
        {
            // O limiar e inclusivo: atingi-lo basta. Este teste e o que trava o >=
            // contra uma troca acidental por >.

            // Arrange
            var especificacao = new RelatorioComConteudoMinimoSpecification(
                minimoDePros: 2, minimoDeContras: 2, minimoDeCaracteres: TotalDeCaracteresDosPontos);

            // Act
            var atende = especificacao.IsSatisfiedBy(RascunhoNoLimiar());

            // Assert
            Assert.True(atende);
        }

        [Fact]
        public void Um_ponto_positivo_a_menos_nao_atende()
        {
            // Arrange
            var especificacao = new RelatorioComConteudoMinimoSpecification(
                minimoDePros: 2, minimoDeContras: 2, minimoDeCaracteres: 0);
            var relatorio = Rascunho(positivos: [PositivoLongo], negativos: [NegativoA, NegativoB]);

            // Act
            var atende = especificacao.IsSatisfiedBy(relatorio);

            // Assert
            Assert.False(atende);
        }

        [Fact]
        public void Um_ponto_negativo_a_menos_nao_atende()
        {
            // Arrange
            var especificacao = new RelatorioComConteudoMinimoSpecification(
                minimoDePros: 2, minimoDeContras: 2, minimoDeCaracteres: 0);
            var relatorio = Rascunho(positivos: [PositivoLongo, PositivoCurto], negativos: [NegativoA]);

            // Act
            var atende = especificacao.IsSatisfiedBy(relatorio);

            // Assert
            Assert.False(atende);
        }

        [Fact]
        public void Um_caractere_a_menos_nao_atende()
        {
            // Arrange
            var especificacao = new RelatorioComConteudoMinimoSpecification(
                minimoDePros: 2, minimoDeContras: 2, minimoDeCaracteres: TotalDeCaracteresDosPontos + 1);

            // Act
            var atende = especificacao.IsSatisfiedBy(RascunhoNoLimiar());

            // Assert
            Assert.False(atende);
        }

        [Fact]
        public void O_minimo_de_caracteres_soma_os_pontos_positivos_e_negativos()
        {
            // Arrange
            var especificacao = new RelatorioComConteudoMinimoSpecification(
                minimoDePros: 1, minimoDeContras: 1, minimoDeCaracteres: 20);
            var soPositivos = Rascunho(positivos: [PositivoLongo], negativos: [NegativoA]);   // 15 + 5 = 20

            // Act
            var atende = especificacao.IsSatisfiedBy(soPositivos);

            // Assert
            Assert.True(atende);
        }

        [Fact]
        public void O_texto_do_relatorio_nao_conta_para_o_minimo_de_caracteres()
        {
            // A politica mede os pontos observados, nao a redacao livre: um texto longo
            // com um unico ponto de uma palavra nao atende.

            // Arrange
            var especificacao = new RelatorioComConteudoMinimoSpecification(
                minimoDePros: 1, minimoDeContras: 1, minimoDeCaracteres: 20);
            var relatorio = Rascunho(
                positivos: ["Bom"],
                negativos: ["Mau"],
                texto: new string('a', 500));

            // Act
            var atende = especificacao.IsSatisfiedBy(relatorio);

            // Assert
            Assert.False(atende);
        }

        [Fact]
        public void Politica_zerada_aceita_qualquer_rascunho()
        {
            // Arrange
            var especificacao = new RelatorioComConteudoMinimoSpecification(0, 0, 0);
            var vazio = Rascunho(positivos: [], negativos: []);

            // Act
            var atende = especificacao.IsSatisfiedBy(vazio);

            // Assert
            Assert.True(atende);
        }
    }
}
