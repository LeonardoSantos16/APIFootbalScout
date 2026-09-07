using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Application.Analise
{
    public sealed record ConsultarMetricasPor90Request(
        int JogadorId,
        int CompeticaoId,
        int TemporadaId,
        ContextoDeRecorte Contexto);
}
