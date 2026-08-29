using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Domain.Acompanhamento.Services
{
    public sealed class AferidorDeMudanca(
        MudancaRelevanteSpecification relevante,
        LeiturasComparaveisSpecification comparaveis)
    {
        public AfericaoDeMudanca Aferir(ComMudanca mudanca)
        {
            if (!comparaveis.IsSatisfiedBy(mudanca))
                return new Indisponivel(MotivoDeIndisponibilidade.TemporadaVirada);

            if (!relevante.IsSatisfiedBy(mudanca))
                return new SemMudancaRelevante();

            return mudanca;
        }

        public AfericaoDeMudanca AferirValorDeMercado(Dinheiro anterior, Dinheiro? atual)
        {
            if (atual is null)
                return new Indisponivel(MotivoDeIndisponibilidade.MoedaInesperada);

            return Aferir(new MudancaDeValorDeMercado(anterior, atual));
        }
    }
}
