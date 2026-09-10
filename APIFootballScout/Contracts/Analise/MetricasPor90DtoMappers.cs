using APIFootballScout.Application.Analise;
using APIFootballScout.Contracts.Acompanhamento;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Contracts.Analise
{
    public static class MetricasPor90RequestDtoMapper
    {
        public static ConsultarMetricasPor90Request ParaRequest(this ConsultarMetricasPor90RequestDto dto, int jogadorId)
        {
            return new ConsultarMetricasPor90Request(
                JogadorId: jogadorId,
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

    public static class MetricasPor90ResponseDtoMapper
    {
        public static MetricasPor90ResponseDto ParaResponse(this ConsultarMetricasPor90Result result)
        {
            return new MetricasPor90ResponseDto(
                JogadorId: result.JogadorId,
                Recorte: ParaRecorte(result.Recorte),
                Atributos: [.. result.Atributos.Select(ParaAtributo)]);
        }

        internal static RecorteDto ParaRecorte(Recorte recorte)
            => new(recorte.CompeticaoId, recorte.TemporadaId, ParaContexto(recorte.Contexto));

        private static AtributoDto ParaAtributo(AtributoDoJogador atributo) => atributo.Resultado switch
        {
            AtributoCalculado calculado => new(
                ParaTipo(atributo.Tipo),
                ResultadoDoCalculoDto.Calculada,
                Valor: calculado.Valor,
                AmostraEmMinutos: calculado.Amostra.Minutos),
            AtributoRecusado recusado => new(
                ParaTipo(atributo.Tipo),
                ResultadoDoCalculoDto.Recusada,
                Motivo: ParaMotivo(recusado.Motivo)),
            _ => throw new ValorInvalidoException(
                "atributo.resultado_invalido",
                $"Unexpected attribute result: {atributo.Resultado.GetType().Name}.")
        };

        internal static ContextoDeRecorteDto ParaContexto(ContextoDeRecorte contexto) => contexto switch
        {
            ContextoDeRecorte.Clube => ContextoDeRecorteDto.Clube,
            ContextoDeRecorte.Selecao => ContextoDeRecorteDto.Selecao,
            _ => throw new ValorInvalidoException(
                "recorte.contexto_invalido",
                $"Invalid scope context: {contexto}.")
        };

        internal static TipoDeAtributoDto ParaTipo(TipoDeAtributo tipo) => tipo switch
        {
            TipoDeAtributo.Gols => TipoDeAtributoDto.Gols,
            TipoDeAtributo.Assistencias => TipoDeAtributoDto.Assistencias,
            TipoDeAtributo.PassesDecisivos => TipoDeAtributoDto.PassesDecisivos,
            TipoDeAtributo.Desarmes => TipoDeAtributoDto.Desarmes,
            TipoDeAtributo.Interceptacoes => TipoDeAtributoDto.Interceptacoes,
            TipoDeAtributo.Rating => TipoDeAtributoDto.Rating,
            TipoDeAtributo.PrecisaoDePasse => TipoDeAtributoDto.PrecisaoDePasse,
            _ => throw new ValorInvalidoException(
                "atributo.tipo_invalido",
                $"Unknown attribute type: {tipo}.")
        };

        internal static MotivoDaRecusaDto ParaMotivo(MotivoDaRecusa motivo) => motivo switch
        {
            MotivoDaRecusa.AmostraInsuficiente => MotivoDaRecusaDto.AmostraInsuficiente,
            MotivoDaRecusa.FonteNaoAtribuiu => MotivoDaRecusaDto.FonteNaoAtribuiu,
            _ => throw new ValorInvalidoException(
                "atributo.motivo_invalido",
                $"Unknown refusal reason: {motivo}.")
        };
    }
}
