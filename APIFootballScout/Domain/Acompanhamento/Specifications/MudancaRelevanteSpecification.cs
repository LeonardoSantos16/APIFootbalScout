using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base;

namespace APIFootballScout.Domain.Acompanhamento.Specifications
{
    public sealed class MudancaRelevanteSpecification(
        LimiarPercentual valorDeMercado,
        LimiarAbsoluto minutagem) : Specification<ComMudanca>
    {
        public override bool IsSatisfiedBy(ComMudanca mudanca) => mudanca switch
        {
            MudancaDeValorDeMercado m => m.VariacaoPercentualAbsoluta > valorDeMercado.Percentual,
            MudancaDeMinutagem m => (m.VariacaoAbsoluta()) > minutagem.Minutos,
            _ => false
        };
    }
}
