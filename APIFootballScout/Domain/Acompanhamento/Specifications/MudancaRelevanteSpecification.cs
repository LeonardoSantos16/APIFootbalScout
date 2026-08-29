using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base;

namespace APIFootballScout.Domain.Acompanhamento.Specifications
{
    public sealed class MudancaRelevanteSpecification(
        LimiarPercentual valorDeMercado,
        LimiarAbsoluto minutagem) : Specification<MudancaQuantitativa>
    {
        public override bool IsSatisfiedBy(MudancaQuantitativa mudanca) => mudanca switch
        {
            MudancaDeValorDeMercado m => Math.Abs(m.VariacaoPercentual) > valorDeMercado.Percentual,
            MudancaDeMinutagem m => m.VariacaoAbsoluta() >= minutagem.Minutos,
            _ => false
        };
    }
}
