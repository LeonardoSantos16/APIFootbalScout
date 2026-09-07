using APIFootballScout.Domain.Analise.ValueObject;

namespace APIFootballScout.Domain.CatalogoDeJogador
{
    public interface ICatalogoDeJogador
    {
        Task<PerfilDoJogador?> ObterPerfilDoJogador(int jogadorId, Recorte recorte, CancellationToken cancellationToken = default);

        Task<ConjuntoDeEstatisticas?> ObterEstatisticas(int jogadorId, Recorte recorte, CancellationToken cancellationToken = default);
    }
}
