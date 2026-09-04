using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Application.RelatorioScouting
{
    public sealed record RelatorioResult(
        Guid RelatorioId,
        int JogadorId,
        Guid OlheiroId,
        StatusRelatorio Status,
        string Texto,
        decimal? Nota,
        IReadOnlyList<string> PontosPositivos,
        IReadOnlyList<string> PontosNegativos,
        Parecer? Parecer,
        DateTimeOffset ObservadoEm,
        DateTimeOffset EscritoEm,
        DateTimeOffset? FinalizadoEm,
        Guid? CorrigeRelatorioId);
}
