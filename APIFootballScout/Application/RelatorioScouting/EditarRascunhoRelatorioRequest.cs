using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Application.RelatorioScouting
{
    public sealed record EditarRascunhoRelatorioRequest(
        Guid OlheiroId,
        Guid RelatorioId,
        string? Texto = null,
        decimal? Nota = null,
        IReadOnlyList<string>? PontosPositivos = null,
        IReadOnlyList<string>? PontosNegativos = null,
        Parecer? Parecer = null);
}
