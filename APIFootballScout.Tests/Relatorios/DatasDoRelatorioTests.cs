using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.Specifications;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Tests.Relatorios
{
    // R5.5 - o relatorio registra a data da observacao, distinta da data de escrita.
    // Sao dois campos independentes porque os dois momentos sao independentes: guardar
    // so a redacao impede aferir a atualidade do relatorio e ordena mal a cronologia.
    public class DatasDoRelatorioTests
    {
        private static readonly DateTimeOffset ObservadoEm = new(2026, 8, 20, 15, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset AbertoEm = new(2026, 8, 25, 12, 0, 0, TimeSpan.Zero);
        private static readonly DateTime FinalizadoEm = new(2026, 8, 26, 9, 30, 0, DateTimeKind.Utc);
        private static readonly DateTimeOffset CorrigidoEm = new(2026, 9, 3, 11, 0, 0, TimeSpan.Zero);
        private static readonly RelatorioComConteudoMinimoSpecification SemExigenciaDeConteudo = new(0, 0, 0);

        private const int JogadorObservado = 42;
        private static readonly Guid Olheiro = Guid.Parse("9a1d7c34-1c2b-4f0e-9f31-4c6a2b8d5e70");

        private static Relatorio NovoRascunho(
            DateTimeOffset? observadoEm = null, DateTimeOffset? agora = null) =>
            Relatorio.AbrirRascunho(
                jogadorId: JogadorObservado,
                olheiroId: Olheiro,
                texto: "Bom posicionamento sem bola.",
                observadoEm: observadoEm ?? ObservadoEm,
                agora: agora ?? AbertoEm);

        private static Relatorio Finalizado(DateTime? em = null)
        {
            var relatorio = NovoRascunho();
            relatorio.AtribuirNota(new Nota(8.5m));
            relatorio.DefinirParecer("Contratar");
            relatorio.Finalizar(SemExigenciaDeConteudo, em ?? FinalizadoEm);

            return relatorio;
        }

        [Fact]
        public void O_rascunho_registra_a_observacao_e_a_redacao_como_datas_distintas()
        {
            // Arrange & Act
            var relatorio = NovoRascunho();

            // Assert
            Assert.Equal(ObservadoEm, relatorio.ObservadoEm);
            Assert.Equal(AbertoEm, relatorio.EscritoEm);
            Assert.NotEqual(relatorio.ObservadoEm, relatorio.EscritoEm);
        }

        [Fact]
        public void A_observacao_pode_anteceder_a_redacao_por_qualquer_intervalo()
        {
            // Nao ha prazo para redigir: o olheiro pode escrever meses depois do jogo.
            // O que a regra exige e que o intervalo seja aferivel, nao que seja curto.

            // Arrange
            var observadoHaMeses = AbertoEm.AddMonths(-8);

            // Act
            var relatorio = NovoRascunho(observadoEm: observadoHaMeses);

            // Assert
            Assert.Equal(observadoHaMeses, relatorio.ObservadoEm);
            Assert.Equal(AbertoEm, relatorio.EscritoEm);
        }

        [Fact]
        public void A_redacao_no_mesmo_instante_da_observacao_e_aceita()
        {
            // Borda: o olheiro que escreve na arquibancada, ainda no jogo. As datas
            // coincidem sem que uma deixe de existir.

            // Act
            var relatorio = NovoRascunho(observadoEm: AbertoEm, agora: AbertoEm);

            // Assert
            Assert.Equal(AbertoEm, relatorio.ObservadoEm);
            Assert.Equal(AbertoEm, relatorio.EscritoEm);
        }

        [Fact]
        public void A_observacao_no_futuro_e_recusada()
        {
            // Nao se relata o que ainda nao foi visto.

            // Act
            var erro = Assert.Throws<ConflitoDeDominioException>(
                () => NovoRascunho(observadoEm: AbertoEm.AddDays(1)));

            // Assert
            Assert.Equal("relatorio.observacao_futura", erro.Codigo);
        }

        [Fact]
        public void O_rascunho_ainda_nao_tem_data_de_finalizacao()
        {
            // Arrange & Act
            var relatorio = NovoRascunho();

            // Assert
            Assert.Null(relatorio.FinalizadoEm);
        }

        [Fact]
        public void A_finalizacao_grava_a_data_em_que_o_relatorio_virou_definitivo()
        {
            // Arrange & Act
            var relatorio = Finalizado();

            // Assert
            Assert.Equal(new DateTimeOffset(FinalizadoEm), relatorio.FinalizadoEm);
        }

        [Fact]
        public void A_finalizacao_move_a_redacao_para_o_instante_do_fecho()
        {
            // EscritoEm e quando a redacao terminou, nao quando comecou: a finalizacao
            // o traz da abertura do rascunho para o momento em que o texto ficou pronto.

            // Arrange & Act
            var relatorio = Finalizado();

            // Assert
            Assert.Equal(new DateTimeOffset(FinalizadoEm), relatorio.EscritoEm);
            Assert.NotEqual(AbertoEm, relatorio.EscritoEm);
        }

        [Fact]
        public void A_finalizacao_nao_move_a_data_da_observacao()
        {
            // O que foi observado num dia nao passa a ter sido observado noutro.

            // Arrange & Act
            var relatorio = Finalizado();

            // Assert
            Assert.Equal(ObservadoEm, relatorio.ObservadoEm);
        }

        [Fact]
        public void A_correcao_tem_redacao_propria_e_conserva_a_observacao_do_original()
        {
            // O elo entre R5.3 e R5.5: corrigir e reescrever, nao reobservar. A correcao
            // fala da mesma partida, com data de redacao sua. Sem isso, uma correcao
            // escrita hoje pareceria uma observacao de hoje.

            // Arrange
            var original = Finalizado();

            // Act
            var correcao = Relatorio.AbrirCorrecao(original, "Revisto: erra a saida de bola.", CorrigidoEm);

            // Assert
            Assert.Equal(ObservadoEm, correcao.ObservadoEm);
            Assert.Equal(CorrigidoEm, correcao.EscritoEm);
            Assert.NotEqual(original.EscritoEm, correcao.EscritoEm);
            Assert.Null(correcao.FinalizadoEm);
        }
    }
}
