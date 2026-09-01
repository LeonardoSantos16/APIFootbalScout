using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.Specifications;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Tests.Relatorios
{
    // R5.7 - o relatorio conclui em um parecer: contratar, monitorar, reavaliar ou
    // descartar. O conjunto e fechado e mora no tipo. O relatorio existe para sustentar
    // uma decisao, entao terminar a leitura sem saber o que o olheiro recomenda o
    // esvazia de proposito.
    public class ParecerDoRelatorioTests
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

        public static TheoryData<Parecer> PareceresDoConjunto => new()
        {
            Parecer.Contratar, Parecer.Monitorar, Parecer.Reavaliar, Parecer.Descartar
        };

        [Theory]
        [MemberData(nameof(PareceresDoConjunto))]
        public void O_rascunho_aceita_qualquer_parecer_do_conjunto(Parecer parecer)
        {
            // Arrange
            var relatorio = NovoRascunho();

            // Act
            relatorio.DefinirParecer(parecer);

            // Assert
            Assert.Equal(parecer, relatorio.Parecer);
        }

        [Fact]
        public void O_conjunto_de_pareceres_tem_exatamente_os_quatro_valores_da_regra()
        {
            // A regra nomeia quatro conclusoes e so quatro. Este teste falha se alguem
            // acrescentar ou remover uma sem passar pela regra de negocio.

            // Act
            var conjunto = Enum.GetValues<Parecer>();

            // Assert
            Assert.Equal(
                [Parecer.Contratar, Parecer.Monitorar, Parecer.Reavaliar, Parecer.Descartar],
                conjunto);
        }

        [Fact]
        public void Um_parecer_fora_do_conjunto_e_recusado()
        {
            // O enum nao fecha o conjunto sozinho: um cast atravessa, e a
            // desserializacao de um documento antigo tambem. A guarda esta no agregado
            // para que nenhum caminho de escrita entre com uma conclusao inexistente.

            // Arrange
            var relatorio = NovoRascunho();

            // Act
            var erro = Assert.Throws<ValorInvalidoException>(
                () => relatorio.DefinirParecer((Parecer)99));

            // Assert
            Assert.Equal("relatorio.parecer_invalido", erro.Codigo);
        }

        [Fact]
        public void A_recusa_de_parecer_invalido_nao_deixa_o_rascunho_com_conclusao()
        {
            // Arrange
            var relatorio = NovoRascunho();

            // Act
            Assert.Throws<ValorInvalidoException>(() => relatorio.DefinirParecer((Parecer)0));

            // Assert
            Assert.Null(relatorio.Parecer);
        }

        [Fact]
        public void O_rascunho_nasce_sem_parecer()
        {
            // A conclusao e opcional enquanto se escreve; so a finalizacao a exige.

            // Arrange & Act
            var relatorio = NovoRascunho();

            // Assert
            Assert.Null(relatorio.Parecer);
        }

        [Fact]
        public void O_parecer_pode_mudar_enquanto_o_relatorio_e_rascunho()
        {
            // O olheiro muda de ideia ate fechar: e o que a R5.1 garante ao rascunho.

            // Arrange
            var relatorio = NovoRascunho();
            relatorio.DefinirParecer(Parecer.Contratar);

            // Act
            relatorio.DefinirParecer(Parecer.Descartar);

            // Assert
            Assert.Equal(Parecer.Descartar, relatorio.Parecer);
        }

        [Fact]
        public void O_parecer_sobrevive_a_finalizacao()
        {
            // A conclusao e o que resta do relatorio para quem decide. Ela precisa
            // continuar legivel depois que o relatorio vira definitivo.

            // Arrange
            var relatorio = NovoRascunho();
            relatorio.AtribuirNota(new Nota(4m));
            relatorio.DefinirParecer(Parecer.Reavaliar);

            // Act
            relatorio.Finalizar(SemExigenciaDeConteudo, EscritoEm);

            // Assert
            Assert.Equal(Parecer.Reavaliar, relatorio.Parecer);
        }
    }
}
