namespace APIFootballScout.Domain.Acompanhamento.ValueObject
{
    public sealed record Indisponivel(MotivoDeIndisponibilidade Motivo) : AfericaoDeMudanca;
}
