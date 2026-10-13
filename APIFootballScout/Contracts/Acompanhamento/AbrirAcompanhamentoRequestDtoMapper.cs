using APIFootballScout.Application.Acompanhamento;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Contracts.Acompanhamento
{
    public static class AbrirAcompanhamentoRequestDtoMapper
    {
        public static AbrirAcompanhamentoRequest ParaRequest(this AbrirAcompanhamentoRequestDto dto, Guid olheiroId)
        {
            return new AbrirAcompanhamentoRequest(
                OlheiroId: olheiroId,
                JogadorId: dto.JogadorId,
                CompeticaoId: dto.CompeticaoId,
                TemporadaId: dto.TemporadaId,
                Contexto: ParaContexto(dto.Contexto));
        }

        private static ContextoDeRecorte ParaContexto(ContextoDeRecorteDto contexto) => contexto switch
        {
            ContextoDeRecorteDto.Clube => ContextoDeRecorte.Clube,
            ContextoDeRecorteDto.Selecao => ContextoDeRecorte.Selecao,
            _ => throw new ValorInvalidoException(
                "recorte.contexto_invalido",
                $"Invalid scope context: {contexto}.")
        };
    }
}