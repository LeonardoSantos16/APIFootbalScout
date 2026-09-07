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
                Metricas: [.. result.Metricas.Select(ParaMetrica)],
                Derivados: [.. result.Derivados.Select(ParaValorDerivado)]);
        }

        private static RecorteDto ParaRecorte(Recorte recorte)
            => new(recorte.CompeticaoId, recorte.TemporadaId, ParaContexto(recorte.Contexto));

        private static MetricaPor90Dto ParaMetrica(MetricaDoJogador metrica) => metrica.Metrica switch
        {
            MetricaCalculada calculada => new(
                ParaTipo(metrica.Tipo),
                ResultadoDoCalculoDto.Calculada,
                Valor: calculada.Valor,
                AmostraEmMinutos: calculada.Amostra.Minutos),
            CalculoRecusado recusado => new(
                ParaTipo(metrica.Tipo),
                ResultadoDoCalculoDto.Recusada,
                Motivo: ParaMotivo(recusado.Motivo)),
            _ => throw new ValorInvalidoException(
                "metrica_por_90.tipo_invalido",
                $"Unexpected metric result: {metrica.Metrica.GetType().Name}.")
        };

        private static ValorDerivadoDto ParaValorDerivado(ValorDerivado derivado)
            => new(ParaTipoDerivado(derivado.Tipo), derivado.Valor);

        private static ContextoDeRecorteDto ParaContexto(ContextoDeRecorte contexto) => contexto switch
        {
            ContextoDeRecorte.Clube => ContextoDeRecorteDto.Clube,
            ContextoDeRecorte.Selecao => ContextoDeRecorteDto.Selecao,
            _ => throw new ValorInvalidoException(
                "recorte.contexto_invalido",
                $"Invalid scope context: {contexto}.")
        };

        private static TipoDeEstatisticaDto ParaTipo(TipoDeEstatistica tipo) => tipo switch
        {
            TipoDeEstatistica.Gols => TipoDeEstatisticaDto.Gols,
            TipoDeEstatistica.Assistencias => TipoDeEstatisticaDto.Assistencias,
            TipoDeEstatistica.PassesDecisivos => TipoDeEstatisticaDto.PassesDecisivos,
            TipoDeEstatistica.Desarmes => TipoDeEstatisticaDto.Desarmes,
            TipoDeEstatistica.Interceptacoes => TipoDeEstatisticaDto.Interceptacoes,
            _ => throw new ValorInvalidoException(
                "estatistica.tipo_invalido",
                $"Unknown statistic type: {tipo}.")
        };

        private static TipoDeValorDerivadoDto ParaTipoDerivado(TipoDeValorDerivado tipo) => tipo switch
        {
            TipoDeValorDerivado.Rating => TipoDeValorDerivadoDto.Rating,
            TipoDeValorDerivado.PrecisaoDePasse => TipoDeValorDerivadoDto.PrecisaoDePasse,
            _ => throw new ValorInvalidoException(
                "valor_derivado.tipo_invalido",
                $"Unknown derived value type: {tipo}.")
        };

        private static MotivoDaRecusaDto ParaMotivo(MotivoDaRecusa motivo) => motivo switch
        {
            MotivoDaRecusa.AmostraInsuficiente => MotivoDaRecusaDto.AmostraInsuficiente,
            _ => throw new ValorInvalidoException(
                "metrica_por_90.motivo_invalido",
                $"Unknown refusal reason: {motivo}.")
        };
    }
}
