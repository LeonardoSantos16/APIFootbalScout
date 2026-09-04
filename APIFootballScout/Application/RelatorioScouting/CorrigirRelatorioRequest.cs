namespace APIFootballScout.Application.RelatorioScouting
{
    public sealed record CorrigirRelatorioRequest(Guid OlheiroId, Guid RelatorioId, string Texto);
}
