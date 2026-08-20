using APIFootballScout.Infrastructure.SofascoreExternalAdapter.player;

namespace APIFootballScout.Domain.CatalagoDeJogador
{
    public interface ICatalogoDeJogador
    {
        Task<PerfilDoJogador> ObterPerfilDoJogador(int jogadorId, int comperticaoId, int temporadaId, CancellationToken cancellationToken = default);
    }
}
