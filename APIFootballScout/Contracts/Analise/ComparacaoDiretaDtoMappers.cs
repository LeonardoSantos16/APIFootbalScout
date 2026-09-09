using APIFootballScout.Application.Analise;
using APIFootballScout.Contracts.Acompanhamento;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Contracts.Analise
{
    public static class ComparacaoDiretaRequestDtoMapper
    {
        public static ComparacaoDiretaRequest ParaRequest(this ComparacaoDiretaRequestDto dto)
        {
            return new ComparacaoDiretaRequest(
                JogadorA: dto.JogadorA,
                JogadorB: dto.JogadorB,
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

    public static class ComparacaoDiretaResponseDtoMapper
    {
        public static ComparacaoDiretaResponseDto ParaResponse(this ComparacaoDiretaResult result)
            => throw new NotImplementedException();
    }
}
