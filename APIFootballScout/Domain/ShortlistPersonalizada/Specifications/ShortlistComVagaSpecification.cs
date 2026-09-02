using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;

namespace APIFootballScout.Domain.ShortlistPersonalizada.Specifications
{
    public sealed class ShortlistComVagaSpecification(int limiteDeAlvos) : Specification<Shortlist>
    {
        public override bool IsSatisfiedBy(Shortlist candidate)
            => throw new NotImplementedException();
    }
}
