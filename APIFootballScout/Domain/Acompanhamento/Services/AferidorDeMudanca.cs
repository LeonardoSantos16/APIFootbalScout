using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Domain.Acompanhamento.Services
{
    public sealed class AferidorDeMudanca(
        MudancaRelevanteSpecification relevante,
        LeiturasComparaveisSpecification comparaveis)
    {
        public AfericaoDeMudanca Aferir(ComMudanca mudanca) =>
            throw new NotImplementedException();

        public AfericaoDeMudanca AferirValorDeMercado(Dinheiro anterior, Dinheiro? atual) =>
            throw new NotImplementedException();
    }
}
