using APIFootballScout.Application.Acompanhamento;

namespace APIFootballScout.Contracts.Acompanhamento
{
    public static class ConsultarAcompanhamentoRequestDtoMapper
    {
        public static ConsultarMudancaAcompanhamentoRequest ParaRequest(this ConsultarMudancaAcompanhamentoRequestDto dto, Guid olheiroId)
        {
            return new ConsultarMudancaAcompanhamentoRequest(
                OlheiroId: olheiroId,
                JogadorId: dto.JogadorId);
        }
    }
}
