using APIFootballScout.Application.Acompanhamento;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Contracts.Acompanhamento
{
    public static class ConsultarMudancaAcompanhamentoResponseDtoMapper
    {
        public static ConsultarMudancaAcompanhamentoResponseDto ParaResponse(
            this ConsultarMudancaAcompanhamentoResult result)
        {
            return new ConsultarMudancaAcompanhamentoResponseDto(
                DossieId: result.DossieId,
                JogadorId: result.JogadorId,
                Janela: ParaJanela(result.Janela),
                Clube: ParaClube(result.Clube),
                ValorDeMercado: ParaValorDeMercado(result.ValorDeMercado),
                Minutagem: ParaMinutagem(result.Minutagem));
        }

        private static JanelaDaComparacaoDto ParaJanela(JanelaDaComparacao janela)
            => new(janela.De, janela.Ate, janela.Duracao().TotalDays);

        private static AfericaoDeClubeDto ParaClube(AfericaoDeMudanca afericao) => afericao switch
        {
            MudancaDeClube m => new(ResultadoDaAfericaoDto.ComMudanca, Anterior: m.Anterior, Atual: m.Atual),
            SemMudancaRelevante => new(ResultadoDaAfericaoDto.SemMudancaRelevante),
            Indisponivel i => new(ResultadoDaAfericaoDto.Indisponivel, Motivo: ParaMotivo(i.Motivo)),
            _ => throw AfericaoInesperada(afericao)
        };

        private static AfericaoDeValorDeMercadoDto ParaValorDeMercado(AfericaoDeMudanca afericao) => afericao switch
        {
            MudancaDeValorDeMercado m => new(
                ResultadoDaAfericaoDto.ComMudanca,
                Anterior: ParaDinheiro(m.Anterior),
                Atual: ParaDinheiro(m.Atual),
                VariacaoPercentualAbsoluta: m.VariacaoPercentualAbsoluta),
            SemMudancaRelevante => new(ResultadoDaAfericaoDto.SemMudancaRelevante),
            Indisponivel i => new(ResultadoDaAfericaoDto.Indisponivel, Motivo: ParaMotivo(i.Motivo)),
            _ => throw AfericaoInesperada(afericao)
        };

        private static AfericaoDeMinutagemDto ParaMinutagem(AfericaoDeMudanca afericao) => afericao switch
        {
            MudancaDeMinutagem m => new(
                ResultadoDaAfericaoDto.ComMudanca,
                Anterior: m.Anterior.Minutos,
                Atual: m.Atual.Minutos,
                VariacaoAbsoluta: m.VariacaoAbsoluta()),
            SemMudancaRelevante => new(ResultadoDaAfericaoDto.SemMudancaRelevante),
            Indisponivel i => new(ResultadoDaAfericaoDto.Indisponivel, Motivo: ParaMotivo(i.Motivo)),
            _ => throw AfericaoInesperada(afericao)
        };

        private static DinheiroDto ParaDinheiro(Dinheiro dinheiro)
            => new(dinheiro.QuantiaEmCentavos, dinheiro.Moeda);

        private static MotivoDeIndisponibilidadeDto ParaMotivo(MotivoDeIndisponibilidade motivo) => motivo switch
        {
            MotivoDeIndisponibilidade.MoedaInesperada => MotivoDeIndisponibilidadeDto.MoedaInesperada,
            MotivoDeIndisponibilidade.TemporadaVirada => MotivoDeIndisponibilidadeDto.TemporadaVirada,
            _ => throw new ValorInvalidoException(
                "afericao.motivo_invalido",
                $"Unknown unavailability reason: {motivo}.")
        };

        private static Exception AfericaoInesperada(AfericaoDeMudanca afericao)
            => new ValorInvalidoException(
                "afericao.tipo_invalido",
                $"Unexpected assessment for this change type: {afericao.GetType().Name}.");
    }
}
