using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.Specifications;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Tests.Relatorios
{
    public class RelatorioTests
    {
        private static readonly DateTimeOffset ObservadoEm = new(2026, 8, 20, 15, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset AbertoEm = new(2026, 8, 25, 12, 0, 0, TimeSpan.Zero);
        private static readonly DateTime EscritoEm = new(2026, 8, 25, 14, 0, 0, DateTimeKind.Utc);
        private static readonly RelatorioComConteudoMinimoSpecification SemExigenciaDeConteudo = new(0, 0, 0);

        private static Relatorio NovoRascunho() => Relatorio.AbrirRascunho(
            jogadorId: 42,
            olheiroId: Guid.NewGuid(),
            texto: "Bom posicionamento sem bola.",
            observadoEm: ObservadoEm,
            agora: AbertoEm);

        private static Relatorio RelatorioFinalizado()
        {
            var relatorio = NovoRascunho();
            relatorio.AtribuirNota(new Nota(8.5m));
            relatorio.AdicionarPontoPositivo("Leitura de jogo");
            relatorio.DefinirParecer("Contratar");
            relatorio.Finalizar(SemExigenciaDeConteudo, EscritoEm);

            return relatorio;
        }

        public static TheoryData<string, Action<Relatorio>> Mutadores => new()
        {
            { "AlterarTexto", r => r.AlterarTexto("Reavaliado apos o classico.") },
            { "AtribuirNota", r => r.AtribuirNota(new Nota(9m)) },
            { "AdicionarPontoPositivo", r => r.AdicionarPontoPositivo("Finalizacao") },
            { "AdicionarPontoNegativo", r => r.AdicionarPontoNegativo("Fragilidade defensiva") },
            { "DefinirParecer", r => r.DefinirParecer("Monitorar") },
            { "Finalizar", r => r.Finalizar(SemExigenciaDeConteudo, EscritoEm.AddDays(1)) }
        };

        [Theory]
        [MemberData(nameof(Mutadores))]
        public void Relatorio_finalizado_recusa_qualquer_alteracao(string operacao, Action<Relatorio> alterar)
        {
            // Arrange
            var relatorio = RelatorioFinalizado();

            // Act
            var erro = Assert.Throws<ConflitoDeDominioException>(() => alterar(relatorio));

            // Assert
            Assert.True(
                erro.Codigo == "relatorio.ja_finalizado",
                $"{operacao} deveria ser recusada com relatorio.ja_finalizado, veio {erro.Codigo}");
        }

        [Fact]
        public void A_recusa_nao_altera_o_relatorio_finalizado()
        {
            // Arrange
            var relatorio = RelatorioFinalizado();

            // Act
            Assert.Throws<ConflitoDeDominioException>(() => relatorio.AlterarTexto("outro texto"));

            // Assert
            Assert.Equal("Bom posicionamento sem bola.", relatorio.Texto);
            Assert.Equal(new Nota(8.5m), relatorio.Nota);
            Assert.Equal("Contratar", relatorio.Parecer);
            Assert.Equal(StatusRelatorio.Finalizado, relatorio.Status);
            Assert.Equal(new DateTimeOffset(EscritoEm), relatorio.EscritoEm);
        }

        [Fact]
        public void Rascunho_aceita_alteracao_do_texto()
        {
            // Arrange
            var relatorio = NovoRascunho();

            // Act
            relatorio.AlterarTexto("Reavaliado apos o classico.");

            // Assert
            Assert.Equal("Reavaliado apos o classico.", relatorio.Texto);
        }

        [Fact]
        public void Rascunho_nasce_sem_nota()
        {
            // R5.4 - a nota e opcional enquanto o relatorio e rascunho. So a
            // finalizacao a exige (R5.7, relatorio.conclusao_ausente).

            // Arrange & Act
            var relatorio = NovoRascunho();

            // Assert
            Assert.Null(relatorio.Nota);
        }

        [Fact]
        public void Rascunho_aceita_nota()
        {
            // Arrange
            var relatorio = NovoRascunho();

            // Act
            relatorio.AtribuirNota(new Nota(7.5m));

            // Assert
            Assert.Equal(new Nota(7.5m), relatorio.Nota);
        }

        [Fact]
        public void Rascunho_aceita_os_pontos_observados()
        {
            // Arrange
            var relatorio = NovoRascunho();

            // Act
            relatorio.AdicionarPontoPositivo("Leitura de jogo");
            relatorio.AdicionarPontoNegativo("Fragilidade defensiva");

            // Assert
            Assert.Equal(["Leitura de jogo"], relatorio.PontosPositivos);
            Assert.Equal(["Fragilidade defensiva"], relatorio.PontosNegativos);
        }

        [Fact]
        public void Rascunho_aceita_parecer()
        {
            // Arrange
            var relatorio = NovoRascunho();

            // Act
            relatorio.DefinirParecer("Contratar");

            // Assert
            Assert.Equal("Contratar", relatorio.Parecer);
        }

        [Fact]
        public void Finalizar_transiciona_o_status_e_grava_a_data()
        {
            // Arrange
            var relatorio = NovoRascunho();
            relatorio.AtribuirNota(new Nota(8.5m));
            relatorio.AdicionarPontoPositivo("Leitura de jogo");
            relatorio.DefinirParecer("Contratar");

            // Act
            relatorio.Finalizar(SemExigenciaDeConteudo, EscritoEm);

            // Assert
            Assert.Equal(StatusRelatorio.Finalizado, relatorio.Status);
            Assert.Equal(new DateTimeOffset(EscritoEm), relatorio.EscritoEm);
        }
    }
}
