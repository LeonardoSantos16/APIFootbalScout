using System.Text.Json;
using APIFootballScout.Application.ShortlistPersonalizada;
using APIFootballScout.Contracts.Acompanhamento;
using APIFootballScout.Contracts.ShortlistPersonalizada;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Tests.Contracts
{
    public class ShortlistDtoMapperTests
    {
        private static readonly Guid ShortlistId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        private static readonly Guid OlheiroId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        private static readonly JsonSerializerOptions OpcoesDaApi = new(JsonSerializerDefaults.Web);

        private static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        private static ShortlistResult Shortlist(params AlvoResult[] alvos)
            => new(
                ShortlistId: ShortlistId,
                OlheiroId: OlheiroId,
                Nome: "Laterais esquerdos 2026",
                LimiteDeAlvos: 25,
                Alvos: alvos,
                CustoTotal: alvos.Length == 0
                    ? null
                    : alvos.Select(a => a.CustoEstimado).Aggregate((a, b) => a.Somar(b)));

        private static AlvoResult Alvo(int jogadorId, int prioridade, long milhoes)
            => new(JogadorId: jogadorId, Prioridade: prioridade, CustoEstimado: Euros(milhoes));

        [Fact]
        public void Dto_de_criacao_vira_request_com_o_olheiro_do_token()
        {
            // Arrange
            var dto = new CriarShortlistRequestDto { Nome = "Laterais esquerdos 2026" };

            // Act
            var request = dto.ParaRequest(OlheiroId);

            // Assert
            Assert.Equal(OlheiroId, request.OlheiroId);
            Assert.Equal("Laterais esquerdos 2026", request.Nome);
        }

        [Fact]
        public void Dto_de_adicao_vira_request_com_a_shortlist_da_rota()
        {
            // Arrange
            var dto = new AdicionarAlvoRequestDto
            {
                JogadorId = 1001,
                Prioridade = 2,
                CustoEstimado = new DinheiroDto(QuantiaEmCentavos: 500_000_000, Moeda: "EUR")
            };

            // Act
            var request = dto.ParaRequest(OlheiroId, ShortlistId);

            // Assert
            Assert.Equal(OlheiroId, request.OlheiroId);
            Assert.Equal(ShortlistId, request.ShortlistId);
            Assert.Equal(1001, request.JogadorId);
            Assert.Equal(2, request.Prioridade);
            Assert.Equal(Euros(5), request.CustoEstimado);
        }

        [Fact]
        public void Dto_de_repriorizacao_vira_request_com_a_shortlist_e_o_jogador_da_rota()
        {
            // Arrange
            var dto = new RepriorizarAlvoRequestDto { Prioridade = 3 };

            // Act
            var request = dto.ParaRequest(OlheiroId, ShortlistId, jogadorId: 1001);

            // Assert
            Assert.Equal(OlheiroId, request.OlheiroId);
            Assert.Equal(ShortlistId, request.ShortlistId);
            Assert.Equal(1001, request.JogadorId);
            Assert.Equal(3, request.Prioridade);
        }

        [Fact]
        public void A_resposta_traz_os_alvos_na_ordem_da_lista()
        {
            // Arrange
            var result = Shortlist(Alvo(1001, 1, 4), Alvo(1002, 2, 9));

            // Act
            var response = result.ParaResponse();

            // Assert
            Assert.Equal(ShortlistId, response.ShortlistId);
            Assert.Equal("Laterais esquerdos 2026", response.Nome);
            Assert.Equal(25, response.LimiteDeAlvos);
            Assert.Equal([(1001, 1), (1002, 2)],
                response.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade)));
        }

        [Fact]
        public void O_custo_vai_em_centavos_e_moeda()
        {
            // Arrange
            var result = Shortlist(Alvo(1001, 1, 4), Alvo(1002, 2, 9));

            // Act
            var response = result.ParaResponse();

            // Assert
            Assert.Equal(new DinheiroDto(400_000_000, "EUR"),
                response.Alvos.Single(alvo => alvo.JogadorId == 1001).CustoEstimado);
            Assert.Equal(new DinheiroDto(1_300_000_000, "EUR"), response.CustoTotal);
        }

        [Fact]
        public void A_resposta_da_lista_vazia_omite_o_custo_total()
        {
            // Arrange
            var result = Shortlist();

            // Act
            var response = result.ParaResponse();
            var json = JsonSerializer.Serialize(response, OpcoesDaApi);

            // Assert
            Assert.Empty(response.Alvos);
            Assert.Null(response.CustoTotal);
            Assert.DoesNotContain("custoTotal", json);
        }

        [Fact]
        public void A_resposta_nao_expoe_o_olheiro()
        {
            // Arrange
            var result = Shortlist(Alvo(1001, 1, 4));

            // Act
            var json = JsonSerializer.Serialize(result.ParaResponse(), OpcoesDaApi);

            // Assert
            Assert.DoesNotContain("olheiro", json, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void A_listagem_vira_resposta_preservando_a_ordem()
        {
            // Arrange
            var primeira = Shortlist(Alvo(1001, 1, 4));
            var segunda = Shortlist(Alvo(2001, 1, 7)) with
            {
                ShortlistId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Nome = "Zagueiros 2026"
            };

            // Act
            var response = new[] { primeira, segunda }.ParaResponse();

            // Assert
            Assert.Equal([ShortlistId, segunda.ShortlistId],
                response.Select(shortlist => shortlist.ShortlistId));
            Assert.Equal(["Laterais esquerdos 2026", "Zagueiros 2026"],
                response.Select(shortlist => shortlist.Nome));
        }
    }
}
