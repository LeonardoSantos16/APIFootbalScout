using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.Specifications;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Tests.Relatorios
{
    // R5.3 - a correcao de relatorio finalizado se da por novo relatorio que
    // referencia o anterior. Nao ha edicao do finalizado (R5.1): a correcao e
    // um relatorio proprio, em rascunho, com elo para o que ele corrige.
    public class CorrecaoDeRelatorioTests
    {
        private static readonly DateTimeOffset ObservadoEm = new(2026, 8, 20, 15, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset AbertoEm = new(2026, 8, 25, 12, 0, 0, TimeSpan.Zero);
        private static readonly DateTime EscritoEm = new(2026, 8, 25, 14, 0, 0, DateTimeKind.Utc);
        private static readonly DateTimeOffset CorrigidoEm = new(2026, 8, 27, 9, 0, 0, TimeSpan.Zero);
        private static readonly RelatorioComConteudoMinimoSpecification SemExigenciaDeConteudo = new(0, 0, 0);

        private const int JogadorObservado = 42;
        private static readonly Guid Olheiro = Guid.Parse("9a1d7c34-1c2b-4f0e-9f31-4c6a2b8d5e70");

        private static Relatorio RelatorioFinalizado()
        {
            var relatorio = Relatorio.AbrirRascunho(
                jogadorId: JogadorObservado,
                olheiroId: Olheiro,
                texto: "Bom posicionamento sem bola.",
                observadoEm: ObservadoEm,
                agora: AbertoEm);

            relatorio.AtribuirNota(new Nota(8.5m));
            relatorio.AdicionarPontoPositivo("Leitura de jogo");
            relatorio.DefinirParecer("Contratar");
            relatorio.Finalizar(SemExigenciaDeConteudo, EscritoEm);

            return relatorio;
        }

        [Fact]
        public void A_correcao_referencia_o_relatorio_que_corrige()
        {
            // Arrange
            var original = RelatorioFinalizado();

            // Act
            var correcao = Relatorio.AbrirCorrecao(original, "Revisto: erra a saida de bola.", CorrigidoEm);

            // Assert
            Assert.Equal(original.Id, correcao.CorrigeRelatorioId);
        }

        [Fact]
        public void A_correcao_e_um_relatorio_proprio_em_rascunho()
        {
            // A correcao nasce editavel: e um relatorio novo, nao uma copia finalizada.

            // Arrange
            var original = RelatorioFinalizado();

            // Act
            var correcao = Relatorio.AbrirCorrecao(original, "Revisto: erra a saida de bola.", CorrigidoEm);

            // Assert
            Assert.NotEqual(original.Id, correcao.Id);
            Assert.Equal(StatusRelatorio.Rascunho, correcao.Status);
            Assert.Equal("Revisto: erra a saida de bola.", correcao.Texto);
        }

        [Fact]
        public void A_correcao_herda_o_jogador_o_olheiro_e_a_observacao_do_original()
        {
            // A correcao fala da mesma observacao, feita pelo mesmo olheiro sobre o
            // mesmo jogador. So a redacao e nova.

            // Arrange
            var original = RelatorioFinalizado();

            // Act
            var correcao = Relatorio.AbrirCorrecao(original, "Revisto: erra a saida de bola.", CorrigidoEm);

            // Assert
            Assert.Equal(JogadorObservado, correcao.JogadorId);
            Assert.Equal(Olheiro, correcao.OlheiroId);
            Assert.Equal(ObservadoEm, correcao.ObservadoEm);
        }

        [Fact]
        public void A_correcao_nao_nasce_com_a_conclusao_do_original()
        {
            // Corrigir e reescrever: nota e parecer sao decisao da nova redacao,
            // e a correcao nao chega finalizada.

            // Arrange
            var original = RelatorioFinalizado();

            // Act
            var correcao = Relatorio.AbrirCorrecao(original, "Revisto: erra a saida de bola.", CorrigidoEm);

            // Assert
            Assert.Null(correcao.Nota);
            Assert.Null(correcao.Parecer);
            Assert.Null(correcao.FinalizadoEm);
        }

        [Fact]
        public void Abrir_a_correcao_nao_altera_o_relatorio_original()
        {
            // O finalizado permanece intacto e finalizado: e o registro historico
            // que a correcao referencia, nao um rascunho reaberto.

            // Arrange
            var original = RelatorioFinalizado();

            // Act
            Relatorio.AbrirCorrecao(original, "Revisto: erra a saida de bola.", CorrigidoEm);

            // Assert
            Assert.Equal(StatusRelatorio.Finalizado, original.Status);
            Assert.Equal("Bom posicionamento sem bola.", original.Texto);
            Assert.Equal(new Nota(8.5m), original.Nota);
            Assert.Equal("Contratar", original.Parecer);
            Assert.Null(original.CorrigeRelatorioId);
        }

        [Fact]
        public void O_relatorio_aberto_como_rascunho_nao_corrige_ninguem()
        {
            // Arrange & Act
            var relatorio = Relatorio.AbrirRascunho(
                JogadorObservado, Olheiro, "Observado no classico.", ObservadoEm, AbertoEm);

            // Assert
            Assert.Null(relatorio.CorrigeRelatorioId);
        }
    }
}
