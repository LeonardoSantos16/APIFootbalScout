using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Application.Analise
{
    public sealed record ComparacaoDiretaRequest(
        int JogadorA,
        int JogadorB,
        int CompeticaoId,
        int TemporadaId,
        ContextoDeRecorte Contexto);
}
