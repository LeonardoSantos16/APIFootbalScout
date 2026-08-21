using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Application.Acompanhamento
{
    public sealed record AbrirAcompanhamentoRequest(
        Guid OlheiroId,
        int JogadorId,
        int CompeticaoId,
        int TemporadaId,
        ContextoDeRecorte Contexto);
}
