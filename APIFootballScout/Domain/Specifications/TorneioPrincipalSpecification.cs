using APIFootballScout.Domain.Specifications.Base;

namespace APIFootballScout.Domain.Specifications
{
    public sealed class TorneioPrincipalSpecification : Specification<int>
    {
        private readonly HashSet<int> _torneiosPrincipais;

        public TorneioPrincipalSpecification(IEnumerable<int> torneiosPrincipais)
            => _torneiosPrincipais = [.. torneiosPrincipais];

        public override bool IsSatisfiedBy(int torneioId)
            => _torneiosPrincipais.Contains(torneioId);
    }
}
