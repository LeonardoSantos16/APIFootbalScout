using APIFootballScout.Application.Analise;
using APIFootballScout.Contracts.Acompanhamento;
using APIFootballScout.Domain.Analise.ValueObject;
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
        {
            var jogadores = result.Jogadores.ToDictionary(
                jogador => jogador.JogadorId,
                jogador => new JogadorNaComparacaoDto(jogador.Nome, ParaPosicao(jogador.Posicao)));

            return result.Resultado switch
            {
                ComparacaoRealizada realizada => new(
                    jogadores,
                    ResultadoDaComparacaoDto.Realizada,
                    Recorte: MetricasPor90ResponseDtoMapper.ParaRecorte(realizada.Recorte),
                    Atributos: [.. realizada.Atributos.Select(ParaAtributo)]),
                ComparacaoRecusada recusada => new(
                    jogadores,
                    ResultadoDaComparacaoDto.Recusada,
                    Motivo: ParaMotivo(recusada.Motivo)),
                _ => throw new ValorInvalidoException(
                    "comparacao.resultado_invalido",
                    $"Unexpected comparison result: {result.Resultado.GetType().Name}.")
            };
        }

        private static AtributoComparadoDto ParaAtributo(AtributoComparado atributo)
            => new(
                MetricasPor90ResponseDtoMapper.ParaTipo(atributo.Tipo),
                atributo.Valores.ToDictionary(valor => valor.Key, valor => ParaValor(valor.Value)));

        private static ValorComparadoDto ParaValor(ResultadoDeAtributo resultado) => resultado switch
        {
            AtributoCalculado calculado => new(
                ResultadoDoCalculoDto.Calculada,
                Valor: calculado.Valor,
                AmostraEmMinutos: calculado.Amostra.Minutos),
            AtributoRecusado recusado => new(
                ResultadoDoCalculoDto.Recusada,
                Motivo: MetricasPor90ResponseDtoMapper.ParaMotivo(recusado.Motivo)),
            _ => throw new ValorInvalidoException(
                "atributo.resultado_invalido",
                $"Unexpected attribute result: {resultado.GetType().Name}.")
        };

        private static PosicaoDto? ParaPosicao(Posicao? posicao) => posicao switch
        {
            null => null,
            Posicao.Goleiro => PosicaoDto.Goleiro,
            Posicao.Defesa => PosicaoDto.Defesa,
            Posicao.MeioCampo => PosicaoDto.MeioCampo,
            Posicao.Ataque => PosicaoDto.Ataque,
            _ => throw new ValorInvalidoException(
                "posicao.invalida",
                $"Unknown position: {posicao}.")
        };

        private static MotivoDaRecusaDaComparacaoDto ParaMotivo(MotivoDaRecusaDaComparacao motivo) => motivo switch
        {
            MotivoDaRecusaDaComparacao.PosicoesIncompativeis => MotivoDaRecusaDaComparacaoDto.PosicoesIncompativeis,
            MotivoDaRecusaDaComparacao.PosicaoDesconhecida => MotivoDaRecusaDaComparacaoDto.PosicaoDesconhecida,
            MotivoDaRecusaDaComparacao.RecorteDivergente => MotivoDaRecusaDaComparacaoDto.RecorteDivergente,
            MotivoDaRecusaDaComparacao.AmostraInsuficiente => MotivoDaRecusaDaComparacaoDto.AmostraInsuficiente,
            _ => throw new ValorInvalidoException(
                "comparacao.motivo_invalido",
                $"Unknown refusal reason: {motivo}.")
        };
    }
}
