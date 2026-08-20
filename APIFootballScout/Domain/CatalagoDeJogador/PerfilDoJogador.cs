using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Domain.CatalagoDeJogador
{
    public sealed record PerfilDoJogador(
        int JogadorId,
        string Nome,
        string Posicao,
        string Clube,
        Dinheiro ReceitaEstimada,
        int MinutosJogados,
        Recorte Recorte
    );
}
