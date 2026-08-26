using APIFootballScout.Domain.Acompanhamento.Aggregate;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Tests.Acompanhamento
{
    public class DossieTests
    {
        private static readonly DateTime AbertoEm = new(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime EncerradoEm = AbertoEm.AddDays(30);

        private static Dossie NovoDossie() => new(
            jogadorId: 42,
            olheiroId: Guid.NewGuid(),
            abertoEm: AbertoEm,
            new LinhaDeBase(
                MedidaEm: AbertoEm.AddMinutes(-3),
                Clube: "Santos",
                ValorDeMercado: new Dinheiro(50_000_000_00, "EUR"),
                Minutagem: new Minutagem(900, new Recorte(325, 63814, ContextoDeRecorte.Clube))));

        [Fact]
        public void Encerrar_transiciona_o_status_e_grava_a_data()
        {
            var dossie = NovoDossie();

            dossie.Encerrar(EncerradoEm);

            // Assert
            Assert.Equal(StatusDossie.Encerrado, dossie.Status);
            Assert.Equal(EncerradoEm, dossie.EncerradoEm);
        }

        [Fact]
        public void Encerrar_dossie_ja_encerrado_gera_erro()
        {
            var dossie = NovoDossie();
            dossie.Encerrar(EncerradoEm);

            // Assert
            var excecao = Assert.Throws<ConflitoDeDominioException>(() => dossie.Encerrar(EncerradoEm.AddDays(1)));
            Assert.Equal("dossie.ja_encerrado", excecao.Codigo);

            Assert.Equal(EncerradoEm, dossie.EncerradoEm);
        }

        [Fact]
        public void Encerrar_com_data_igual_a_AbertoEm_Lanca_erro()
        {
            var dossie = NovoDossie();

            var excecao = Assert.Throws<ValorInvalidoException>(
                () => dossie.Encerrar(AbertoEm));

            Assert.Equal("dossie.data_de_encerramento_invalida", excecao.Codigo);

            Assert.Equal(StatusDossie.Ativo, dossie.Status);
            Assert.Null(dossie.EncerradoEm);
        }

        [Fact]
        public void Encerrar_preserva_a_LinhaDeBase()
        {
            var dossie = NovoDossie();
            var linhaDeBase = dossie.LinhaDeBase;

            dossie.Encerrar(EncerradoEm);

            Assert.Same(linhaDeBase, dossie.LinhaDeBase);

            Assert.Equal(AbertoEm.AddMinutes(-3), dossie.LinhaDeBase.MedidaEm);
            Assert.Equal("Santos", dossie.LinhaDeBase.Clube);
            Assert.Equal(new Dinheiro(50_000_000_00, "EUR"), dossie.LinhaDeBase.ValorDeMercado);
            Assert.Equal(
                new Minutagem(900, new Recorte(325, 63814, ContextoDeRecorte.Clube)),
                dossie.LinhaDeBase.Minutagem);

            Assert.Equal(AbertoEm, dossie.AbertoEm);
            Assert.Equal(42, dossie.JogadorId);
        }
    }
}
