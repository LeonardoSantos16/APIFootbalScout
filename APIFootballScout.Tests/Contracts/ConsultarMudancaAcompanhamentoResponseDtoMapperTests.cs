using System.Text.Json;
using APIFootballScout.Application.Acompanhamento;
using APIFootballScout.Contracts.Acompanhamento;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Tests.Contracts
{
    public class ConsultarMudancaAcompanhamentoResponseDtoMapperTests
    {
        private static readonly DateTime MedidaEm = new(2024, 1, 10, 9, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime LidoEm = new(2024, 2, 9, 9, 0, 0, DateTimeKind.Utc);
        private static readonly Recorte Recorte = new(325, 61627, ContextoDeRecorte.Clube);

        private static readonly JsonSerializerOptions OpcoesDaApi = new(JsonSerializerDefaults.Web);

        private static ConsultarMudancaAcompanhamentoResult Resultado(
            AfericaoDeMudanca clube,
            AfericaoDeMudanca valorDeMercado,
            AfericaoDeMudanca minutagem)
            => new(
                DossieId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                JogadorId: 12994,
                Janela: new JanelaDaComparacao(MedidaEm, LidoEm),
                Clube: clube,
                ValorDeMercado: valorDeMercado,
                Minutagem: minutagem);

        private static MudancaDeMinutagem MudancaDeMinutagem(int anterior, int atual)
            => new(new Minutagem(anterior, Recorte), new Minutagem(atual, Recorte));

        [Fact]
        public void Cada_tipo_de_mudanca_vira_a_sua_forma_no_contrato()
        {
            // Arrange
            var resultado = Resultado(
                clube: new MudancaDeClube("Santos", "Al-Hilal"),
                valorDeMercado: new MudancaDeValorDeMercado(
                    new Dinheiro(50_000_000_00, "EUR"),
                    new Dinheiro(80_000_000_00, "EUR")),
                minutagem: MudancaDeMinutagem(900, 1500));

            // Act
            var response = resultado.ParaResponse();

            // Assert
            Assert.Equal(resultado.DossieId, response.DossieId);
            Assert.Equal(12994, response.JogadorId);

            Assert.Equal(ResultadoDaAfericaoDto.ComMudanca, response.Clube.Resultado);
            Assert.Equal("Santos", response.Clube.Anterior);
            Assert.Equal("Al-Hilal", response.Clube.Atual);
            Assert.Null(response.Clube.Motivo);

            Assert.Equal(ResultadoDaAfericaoDto.ComMudanca, response.ValorDeMercado.Resultado);
            Assert.Equal(new DinheiroDto(50_000_000_00, "EUR"), response.ValorDeMercado.Anterior);
            Assert.Equal(new DinheiroDto(80_000_000_00, "EUR"), response.ValorDeMercado.Atual);
            Assert.Equal(60m, response.ValorDeMercado.VariacaoPercentualAbsoluta);

            Assert.Equal(ResultadoDaAfericaoDto.ComMudanca, response.Minutagem.Resultado);
            Assert.Equal(900, response.Minutagem.Anterior);
            Assert.Equal(1500, response.Minutagem.Atual);
            Assert.Equal(600, response.Minutagem.VariacaoAbsoluta);
        }

        [Fact]
        public void A_janela_reporta_a_duracao_em_dias()
        {
            // Arrange
            var resultado = Resultado(
                new SemMudancaRelevante(), new SemMudancaRelevante(), new SemMudancaRelevante());

            // Act
            var janela = resultado.ParaResponse().Janela;

            // Assert
            Assert.Equal(MedidaEm, janela.De);
            Assert.Equal(LidoEm, janela.Ate);
            Assert.Equal(30d, janela.DuracaoEmDias);
        }

        [Fact]
        public void Sem_mudanca_relevante_omite_os_valores_da_comparacao()
        {
            // Arrange
            var resultado = Resultado(
                new SemMudancaRelevante(), new SemMudancaRelevante(), new SemMudancaRelevante());

            // Act
            var response = resultado.ParaResponse();

            // Assert
            Assert.Equal(ResultadoDaAfericaoDto.SemMudancaRelevante, response.Clube.Resultado);
            Assert.Null(response.Clube.Anterior);
            Assert.Null(response.Clube.Atual);
            Assert.Null(response.Clube.Motivo);

            Assert.Equal(ResultadoDaAfericaoDto.SemMudancaRelevante, response.ValorDeMercado.Resultado);
            Assert.Null(response.ValorDeMercado.Anterior);
            Assert.Null(response.ValorDeMercado.VariacaoPercentualAbsoluta);

            Assert.Equal(ResultadoDaAfericaoDto.SemMudancaRelevante, response.Minutagem.Resultado);
            Assert.Null(response.Minutagem.Atual);
            Assert.Null(response.Minutagem.VariacaoAbsoluta);
        }

        [Theory]
        [InlineData(MotivoDeIndisponibilidade.MoedaInesperada, MotivoDeIndisponibilidadeDto.MoedaInesperada)]
        [InlineData(MotivoDeIndisponibilidade.TemporadaVirada, MotivoDeIndisponibilidadeDto.TemporadaVirada)]
        public void Indisponivel_expoe_o_motivo_e_omite_os_valores(
            MotivoDeIndisponibilidade motivo,
            MotivoDeIndisponibilidadeDto esperado)
        {
            // Arrange
            var indisponivel = new Indisponivel(motivo);
            var resultado = Resultado(indisponivel, indisponivel, indisponivel);

            // Act
            var response = resultado.ParaResponse();

            // Assert
            Assert.Equal(ResultadoDaAfericaoDto.Indisponivel, response.Clube.Resultado);
            Assert.Equal(esperado, response.Clube.Motivo);
            Assert.Null(response.Clube.Anterior);

            Assert.Equal(esperado, response.ValorDeMercado.Motivo);
            Assert.Null(response.ValorDeMercado.Atual);

            Assert.Equal(esperado, response.Minutagem.Motivo);
            Assert.Null(response.Minutagem.VariacaoAbsoluta);
        }

        [Fact]
        public void Afericao_de_outro_tipo_de_mudanca_e_recusada()
        {
            var resultado = Resultado(
                clube: MudancaDeMinutagem(900, 1500),
                valorDeMercado: new SemMudancaRelevante(),
                minutagem: new SemMudancaRelevante());

            // Act
            var erro = Assert.Throws<ValorInvalidoException>(() => resultado.ParaResponse());

            // Assert
            Assert.Equal("afericao.tipo_invalido", erro.Codigo);
        }

        [Fact]
        public void A_resposta_serializada_expoe_os_valores_de_cada_afericao()
        {        

            // Arrange
            var resultado = Resultado(
                clube: new MudancaDeClube("Santos", "Al-Hilal"),
                valorDeMercado: new MudancaDeValorDeMercado(
                    new Dinheiro(50_000_000_00, "EUR"),
                    new Dinheiro(80_000_000_00, "EUR")),
                minutagem: MudancaDeMinutagem(900, 1500));

            // Act
            var json = JsonSerializer.Serialize(resultado.ParaResponse(), OpcoesDaApi);

            // Assert
            Assert.DoesNotContain("{}", json);
            Assert.Contains("\"clube\":{\"resultado\":\"ComMudanca\",\"anterior\":\"Santos\",\"atual\":\"Al-Hilal\"}", json);
            Assert.Contains("\"quantiaEmCentavos\":5000000000,\"moeda\":\"EUR\"", json);
            Assert.Contains("\"variacaoPercentualAbsoluta\":60", json);
            Assert.Contains("\"variacaoAbsoluta\":600", json);
        }

        [Fact]
        public void A_resposta_serializada_omite_os_campos_ausentes_e_nomeia_o_motivo()
        {
            // Arrange
            var resultado = Resultado(
                clube: new SemMudancaRelevante(),
                valorDeMercado: new Indisponivel(MotivoDeIndisponibilidade.MoedaInesperada),
                minutagem: new Indisponivel(MotivoDeIndisponibilidade.TemporadaVirada));

            // Act
            var json = JsonSerializer.Serialize(resultado.ParaResponse(), OpcoesDaApi);

            // Assert
            Assert.Contains("\"clube\":{\"resultado\":\"SemMudancaRelevante\"}", json);
            Assert.Contains("\"valorDeMercado\":{\"resultado\":\"Indisponivel\",\"motivo\":\"MoedaInesperada\"}", json);
            Assert.Contains("\"minutagem\":{\"resultado\":\"Indisponivel\",\"motivo\":\"TemporadaVirada\"}", json);
            Assert.DoesNotContain("anterior", json);
            Assert.DoesNotContain("null", json);
        }
    }
}
