namespace APIFootballScout.Domain.CatalogoDeJogador
{
    public interface ICatalogoDeJogador
    {
        Task<PerfilDoJogador> ObterPerfilDoJogador(int jogadorId, Recorte recorte, CancellationToken cancellationToken = default);
    }
}
