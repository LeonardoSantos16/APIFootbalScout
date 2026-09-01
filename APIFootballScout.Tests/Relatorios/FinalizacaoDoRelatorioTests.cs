using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.Specifications;

namespace APIFootballScout.Tests.Relatorios
{
    public class FinalizacaoDoRelatorioTests
    {
        private static readonly DateTimeOffset ObservadoEm = new(2026, 8, 20, 15, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset AbertoEm = new(2026, 8, 25, 12, 0, 0, TimeSpan.Zero);
        private static readonly DateTime EscritoEm = new(2026, 8, 25, 14, 0, 0, DateTimeKind.Utc);

        private static readonly RelatorioComConteudoMinimoSpecification ConteudoMinimo = new(
            minimoDePros: 2, minimoDeContras: 2, minimoDeCaracteres: 20);

        private static Relatorio RascunhoIncompleto()
        {
            var relatorio = Relatorio.AbrirRascunho(
                jogadorId: 42,
                olheiroId: Guid.NewGuid(),
                texto: "Observado no classico.",
                observadoEm: ObservadoEm,
                agora: AbertoEm);

            relatorio.AtribuirNota(8.5m);
            relatorio.DefinirParecer("Contratar");

            return relatorio;
        }

        private static Relatorio RascunhoCompleto()
        {
            var relatorio = RascunhoIncompleto();

            relatorio.AdicionarPontoPositivo("Leitura de jogo");
            relatorio.AdicionarPontoPositivo("Chute");
            relatorio.AdicionarPontoNegativo("Ritmo");
            relatorio.AdicionarPontoNegativo("Duelo");

            return relatorio;
        }

        [Fact]
        public void Rascunho_sem_o_conteudo_minimo_nao_finaliza()
        {
            // Arrange
            var relatorio = RascunhoIncompleto();

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => relatorio.Finalizar(ConteudoMinimo, EscritoEm));

            // Assert
            Assert.Equal("relatorio.conteudo_minimo_nao_atendido", erro.Codigo);
        }

        [Fact]
        public void Rascunho_com_o_conteudo_minimo_finaliza()
        {
            // Arrange
            var relatorio = RascunhoCompleto();

            // Act
            relatorio.Finalizar(ConteudoMinimo, EscritoEm);

            // Assert
            Assert.Equal(StatusRelatorio.Finalizado, relatorio.Status);
            Assert.Equal(new DateTimeOffset(EscritoEm), relatorio.EscritoEm);
        }

        [Fact]
        public void A_recusa_por_conteudo_minimo_mantem_o_rascunho_editavel()
        {
            // A politica barra a transicao, nao o rascunho: o olheiro precisa poder
            // completar o que faltou e tentar de novo.

            // Arrange
            var relatorio = RascunhoIncompleto();
            Assert.Throws<RegraDeNegocioException>(() => relatorio.Finalizar(ConteudoMinimo, EscritoEm));

            // Act
            relatorio.AdicionarPontoPositivo("Leitura de jogo");
            relatorio.AdicionarPontoPositivo("Chute");
            relatorio.AdicionarPontoNegativo("Ritmo");
            relatorio.AdicionarPontoNegativo("Duelo");
            relatorio.Finalizar(ConteudoMinimo, EscritoEm);

            // Assert
            Assert.Equal(StatusRelatorio.Finalizado, relatorio.Status);
        }

        [Fact]
        public void Rascunho_sem_nota_nao_finaliza()
        {
            // Arrange
            var relatorio = Relatorio.AbrirRascunho(42, Guid.NewGuid(), "Observado no classico.", ObservadoEm, AbertoEm);
            relatorio.DefinirParecer("Contratar");

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => relatorio.Finalizar(ConteudoMinimo, EscritoEm));

            // Assert
            Assert.Equal("relatorio.conclusao_ausente", erro.Codigo);
        }

        [Fact]
        public void Rascunho_sem_parecer_nao_finaliza()
        {
            // Arrange
            var relatorio = Relatorio.AbrirRascunho(42, Guid.NewGuid(), "Observado no classico.", ObservadoEm, AbertoEm);
            relatorio.AtribuirNota(8.5m);

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => relatorio.Finalizar(ConteudoMinimo, EscritoEm));

            // Assert
            Assert.Equal("relatorio.conclusao_ausente", erro.Codigo);
        }

        [Fact]
        public void A_conclusao_e_cobrada_antes_da_politica_de_conteudo()
        {
            // Um rascunho que falha nas duas coisas responde pela conclusao ausente.
            // A ordem das guardas decide qual erro chega ao olheiro, entao ela e
            // comportamento observavel, nao detalhe interno.

            // Arrange
            var relatorio = Relatorio.AbrirRascunho(42, Guid.NewGuid(), "Observado no classico.", ObservadoEm, AbertoEm);

            // Act
            var erro = Assert.Throws<RegraDeNegocioException>(
                () => relatorio.Finalizar(ConteudoMinimo, EscritoEm));

            // Assert
            Assert.Equal("relatorio.conclusao_ausente", erro.Codigo);
        }

        [Fact]
        public void A_politica_e_avaliada_apenas_na_transicao()
        {
            // O agregado nao guarda a especificacao: uma politica mais dura depois da
            // finalizacao nao desfaz o que ja foi finalizado. E o que sustenta passar a
            // spec como parametro de Finalizar em vez de injeta-la no agregado.

            // Arrange
            var relatorio = RascunhoCompleto();
            relatorio.Finalizar(ConteudoMinimo, EscritoEm);

            var politicaMaisDura = new RelatorioComConteudoMinimoSpecification(
                minimoDePros: 10, minimoDeContras: 10, minimoDeCaracteres: 500);

            // Act
            var erro = Assert.Throws<ConflitoDeDominioException>(
                () => relatorio.Finalizar(politicaMaisDura, EscritoEm.AddDays(1)));

            // Assert
            Assert.Equal("relatorio.ja_finalizado", erro.Codigo);
            Assert.Equal(StatusRelatorio.Finalizado, relatorio.Status);
        }
    }
}
