using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Domain.CatalogoDeJogador
{
    public sealed record PerfilDoJogador(
        int JogadorId,
        string Nome,
        string? Posicao,
        string? Clube,
        Dinheiro ValorDeMercado,
        int MinutosJogados,
        Recorte Recorte,
        DateTime LidoEm
    );
}
