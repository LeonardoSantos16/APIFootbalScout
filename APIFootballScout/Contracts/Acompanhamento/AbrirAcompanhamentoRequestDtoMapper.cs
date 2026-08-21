using APIFootballScout.Application.Acompanhamento;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Contracts.Acompanhamento
{
    public static class AbrirAcompanhamentoRequestDtoMapper
    {
        public static AbrirAcompanhamentoRequest ParaRequest(this AbrirAcompanhamentoRequestDto dto)
        {
            return new AbrirAcompanhamentoRequest(
                OlheiroId: dto.OlheiroId,
                JogadorId: dto.JogadorId,
                CompeticaoId: dto.CompeticaoId,
                TemporadaId: dto.TemporadaId,
                Contexto: ParaContexto(dto.Contexto));
        }

        private static ContextoDeRecorte ParaContexto(ContextoDeRecorteDto contexto) => contexto switch
        {
            ContextoDeRecorteDto.Clube => ContextoDeRecorte.Clube,
            ContextoDeRecorteDto.Selecao => ContextoDeRecorte.Selecao,
            _ => throw new ArgumentOutOfRangeException(nameof(contexto), contexto, "Contexto de recorte invalido.")
        };
    }
}
