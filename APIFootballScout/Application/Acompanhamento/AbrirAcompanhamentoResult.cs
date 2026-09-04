namespace APIFootballScout.Application.Acompanhamento
{
    public sealed record AbrirAcompanhamentoResult(Guid DossieId, DateTime AbertoEm, DateTime MedidaEm);
}
