using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.Base;

namespace APIFootballScout.Domain.Analise.Specifications
{
    public sealed class AmostraSuficienteSpecification(AmostraMinima minima) : Specification<Minutagem>
    {
        public override bool IsSatisfiedBy(Minutagem minutagem)
           => minutagem.Minutos >= minima.Minutos;
    }
}
