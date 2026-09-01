using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Application.RelatorioScouting
{
    internal static class RelatorioResultMapper
    {
        public static RelatorioResult ParaResult(this Relatorio relatorio) => new(
            RelatorioId: relatorio.Id,
            JogadorId: relatorio.JogadorId,
            OlheiroId: relatorio.OlheiroId,
            Status: relatorio.Status,
            Texto: relatorio.Texto,
            Nota: relatorio.Nota?.Valor,
            PontosPositivos: [.. relatorio.PontosPositivos],
            PontosNegativos: [.. relatorio.PontosNegativos],
            Parecer: relatorio.Parecer,
            ObservadoEm: relatorio.ObservadoEm,
            EscritoEm: relatorio.EscritoEm,
            FinalizadoEm: relatorio.FinalizadoEm,
            CorrigeRelatorioId: relatorio.CorrigeRelatorioId);
    }
}
