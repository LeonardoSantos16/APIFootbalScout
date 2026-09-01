namespace APIFootballScout.Application.RelatorioScouting
{
    public sealed record AbrirRascunhoRelatorioRequest(
        Guid OlheiroId,
        int JogadorId,
        string Texto,
        DateTimeOffset ObservadoEm);
}
