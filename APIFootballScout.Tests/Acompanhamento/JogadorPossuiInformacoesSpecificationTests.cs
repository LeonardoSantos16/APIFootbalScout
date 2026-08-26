using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Tests.Acompanhamento
{
    public class JogadorPossuiInformacoesSpecificationTests
    {
        private static PerfilDoJogador Perfil(string nome, string? posicao, string? clube) => new(
            JogadorId: 42,
            Nome: nome,
            Posicao: posicao,
            Clube: clube,
            ValorDeMercado: new Dinheiro(50_000_000_00, "EUR"),
            MinutosJogados: 900,
            Recorte: new Recorte(325, 63814, ContextoDeRecorte.Clube),
            LidoEm: new DateTime(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc));

        [Fact]
        public void Perfil_completo_e_acompanhavel()
        {
            var acompanhavel = new JogadorPossuiInformacoesSpecification()
                .IsSatisfiedBy(Perfil("Neymar", "F", "Santos"));

            Assert.True(acompanhavel);
        }

        [Theory]
        [InlineData("", "F", "Santos")]
        [InlineData("   ", "F", "Santos")]
        [InlineData("Neymar", null, "Santos")]
        [InlineData("Neymar", "", "Santos")]
        [InlineData("Neymar", "   ", "Santos")]
        [InlineData("Neymar", "F", null)]
        [InlineData("Neymar", "F", "")]
        [InlineData("Neymar", "F", "   ")]
        public void Perfil_sem_base_comparavel_nao_e_acompanhavel(string nome, string? posicao, string? clube)
        {
            var acompanhavel = new JogadorPossuiInformacoesSpecification()
                .IsSatisfiedBy(Perfil(nome, posicao, clube));

            Assert.False(acompanhavel);
        }
    }
}
