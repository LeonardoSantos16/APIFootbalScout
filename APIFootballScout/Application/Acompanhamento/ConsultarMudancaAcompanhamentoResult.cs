using APIFootballScout.Domain.Acompanhamento.ValueObject;

namespace APIFootballScout.Application.Acompanhamento
{
    public sealed record ConsultarMudancaAcompanhamentoResult(
        Guid DossieId,
        int JogadorId,
        JanelaDaComparacao Janela,
        AfericaoDeMudanca Clube,
        AfericaoDeMudanca ValorDeMercado,
        AfericaoDeMudanca Minutagem);
}
