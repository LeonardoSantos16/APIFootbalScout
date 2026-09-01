using System.Text.Json;
using APIFootballScout.Application.RelatorioScouting;
using APIFootballScout.Contracts.RelatorioScouting;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Tests.Contracts
{
    public class RelatorioDtoMapperTests
    {
        private static readonly Guid RelatorioId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid OlheiroId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static readonly DateTimeOffset ObservadoEm = new(2026, 8, 20, 15, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset EscritoEm = new(2026, 8, 25, 12, 0, 0, TimeSpan.Zero);

        private static readonly JsonSerializerOptions OpcoesDaApi = new(JsonSerializerDefaults.Web);

        private static RelatorioResult Rascunho(
            decimal? nota = null,
            Parecer? parecer = null,
            DateTimeOffset? finalizadoEm = null,
            StatusRelatorio status = StatusRelatorio.Rascunho)
            => new(
                RelatorioId: RelatorioId,
                JogadorId: 42,
                OlheiroId: OlheiroId,
                Status: status,
                Texto: "Bom posicionamento sem bola.",
                Nota: nota,
                PontosPositivos: ["Leitura de jogo"],
                PontosNegativos: [],
                Parecer: parecer,
                ObservadoEm: ObservadoEm,
                EscritoEm: EscritoEm,
                FinalizadoEm: finalizadoEm,
                CorrigeRelatorioId: null);

        [Fact]
        public void Dto_de_abertura_vira_request_com_o_olheiro_do_token()
        {
            // Arrange
            var dto = new AbrirRascunhoRelatorioRequestDto
            {
                JogadorId = 42,
                Texto = "Bom posicionamento sem bola.",
                ObservadoEm = ObservadoEm
            };

            // Act
            var request = dto.ParaRequest(OlheiroId);

            // Assert
            Assert.Equal(OlheiroId, request.OlheiroId);
            Assert.Equal(42, request.JogadorId);
            Assert.Equal("Bom posicionamento sem bola.", request.Texto);
            Assert.Equal(ObservadoEm, request.ObservadoEm);
        }

        [Fact]
        public void Edicao_parcial_so_altera_os_campos_informados()
        {
            // Arrange — o cliente manda apenas a nota.
            var dto = new EditarRascunhoRelatorioRequestDto { Nota = 7.5m };

            // Act
            var request = dto.ParaRequest(OlheiroId, RelatorioId);

            // Assert
            Assert.Equal(OlheiroId, request.OlheiroId);
            Assert.Equal(RelatorioId, request.RelatorioId);
            Assert.Equal(7.5m, request.Nota);
            Assert.Null(request.Texto);
            Assert.Null(request.PontosPositivos);
            Assert.Null(request.PontosNegativos);
            Assert.Null(request.Parecer);
        }

        [Theory]
        [InlineData(ParecerDto.Contratar, Parecer.Contratar)]
        [InlineData(ParecerDto.Monitorar, Parecer.Monitorar)]
        [InlineData(ParecerDto.Reavaliar, Parecer.Reavaliar)]
        [InlineData(ParecerDto.Descartar, Parecer.Descartar)]
        public void Cada_parecer_do_contrato_vira_o_parecer_do_dominio(ParecerDto dto, Parecer esperado)
        {
            // Arrange & Act
            var request = new EditarRascunhoRelatorioRequestDto { Parecer = dto }
                .ParaRequest(OlheiroId, RelatorioId);

            // Assert
            Assert.Equal(esperado, request.Parecer);
        }

        public static TheoryData<Parecer> PareceresDoDominio => [.. Enum.GetValues<Parecer>()];

        public static TheoryData<StatusRelatorio> StatusDoDominio => [.. Enum.GetValues<StatusRelatorio>()];

        [Theory]
        [MemberData(nameof(PareceresDoDominio))]
        public void Todo_parecer_do_dominio_tem_forma_no_contrato(Parecer parecer)
        {
            // O enum do contrato existe para poder divergir do dominio. Enquanto
            // nao divergir, a resposta precisa nomear o parecer, nunca numera-lo.

            // Act
            var response = Rascunho(nota: 8.5m, parecer: parecer).ParaResponse();

            // Assert
            Assert.True(Enum.IsDefined(response.Parecer!.Value), $"{parecer} nao tem forma no contrato");
            Assert.Equal(parecer.ToString(), response.Parecer.Value.ToString());
        }

        [Theory]
        [MemberData(nameof(StatusDoDominio))]
        public void Todo_status_do_dominio_tem_forma_no_contrato(StatusRelatorio status)
        {
            // Act
            var response = Rascunho(status: status).ParaResponse();

            // Assert
            Assert.True(Enum.IsDefined(response.Status), $"{status} nao tem forma no contrato");
            Assert.Equal(status.ToString(), response.Status.ToString());
        }

        [Fact]
        public void Parecer_do_dominio_desconhecido_e_recusado_na_resposta()
        {
            // Arrange
            var resultado = Rascunho(nota: 8.5m, parecer: (Parecer)99);

            // Act
            var erro = Assert.Throws<ValorInvalidoException>(() => resultado.ParaResponse());

            // Assert
            Assert.Equal("relatorio.parecer_invalido", erro.Codigo);
        }

        [Fact]
        public void Parecer_invalido_no_dto_e_recusado()
        {
            // Arrange
            var dto = new EditarRascunhoRelatorioRequestDto { Parecer = (ParecerDto)99 };

            // Act
            var erro = Assert.Throws<ValorInvalidoException>(() => dto.ParaRequest(OlheiroId, RelatorioId));

            // Assert
            Assert.Equal("relatorio.parecer_invalido", erro.Codigo);
        }

        [Fact]
        public void A_resposta_do_rascunho_expoe_o_status_e_as_datas()
        {
            // Arrange
            var resultado = Rascunho(nota: 7.5m, parecer: Parecer.Monitorar);

            // Act
            var response = resultado.ParaResponse();

            // Assert
            Assert.Equal(RelatorioId, response.RelatorioId);
            Assert.Equal(42, response.JogadorId);
            Assert.Equal(StatusRelatorioDto.Rascunho, response.Status);
            Assert.Equal(ObservadoEm, response.ObservadoEm);
            Assert.Equal(EscritoEm, response.EscritoEm);
            Assert.Null(response.FinalizadoEm);
            Assert.Equal(7.5m, response.Nota);
            Assert.Equal(ParecerDto.Monitorar, response.Parecer);
            Assert.Equal(["Leitura de jogo"], response.PontosPositivos);
            Assert.Empty(response.PontosNegativos);
        }

        [Fact]
        public void A_resposta_serializada_do_rascunho_omite_o_que_ainda_nao_existe()
        {
            // Arrange
            var resultado = Rascunho();

            // Act
            var json = JsonSerializer.Serialize(resultado.ParaResponse(), OpcoesDaApi);

            // Assert
            Assert.Contains("\"status\":\"Rascunho\"", json);
            Assert.DoesNotContain("finalizadoEm", json);
            Assert.DoesNotContain("nota", json);
            Assert.DoesNotContain("parecer", json);
            Assert.DoesNotContain("null", json);
        }

        [Fact]
        public void A_resposta_do_finalizado_declara_a_data_de_finalizacao()
        {
            // Arrange
            var finalizadoEm = EscritoEm.AddHours(2);
            var resultado = Rascunho(
                nota: 8.5m,
                parecer: Parecer.Contratar,
                finalizadoEm: finalizadoEm,
                status: StatusRelatorio.Finalizado);

            // Act
            var json = JsonSerializer.Serialize(resultado.ParaResponse(), OpcoesDaApi);

            // Assert
            Assert.Equal(StatusRelatorioDto.Finalizado, resultado.ParaResponse().Status);
            Assert.Equal(finalizadoEm, resultado.ParaResponse().FinalizadoEm);
            Assert.Contains("\"status\":\"Finalizado\"", json);
            Assert.Contains("\"parecer\":\"Contratar\"", json);
            Assert.Contains("finalizadoEm", json);
        }
    }
}
