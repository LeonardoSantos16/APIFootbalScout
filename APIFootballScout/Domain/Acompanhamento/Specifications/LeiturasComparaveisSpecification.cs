using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base;

namespace APIFootballScout.Domain.Acompanhamento.Specifications
{
    public sealed class LeiturasComparaveisSpecification : Specification<ComMudanca>
    {
        public override bool IsSatisfiedBy(ComMudanca mudanca) => mudanca switch
        {
            MudancaDeMinutagem m => m.Anterior.Recorte == m.Atual.Recorte,
            MudancaDeValorDeMercado => true,
            MudancaCategorica => true,
            _ => false
        };
    }
}
