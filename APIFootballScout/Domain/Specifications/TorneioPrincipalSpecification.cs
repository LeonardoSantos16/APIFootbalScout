namespace APIFootballScout.Domain.Specifications
{
    public sealed class TorneioPrincipalSpecification : ISpecification<int> //implementar interface de specification
    {
        private readonly HashSet<int> _torneiosPrincipais;

        public TorneioPrincipalSpecification(IEnumerable<int> torneiosPrincipais)
            => _torneiosPrincipais = [.. torneiosPrincipais];

        public bool IsSatisfiedBy(int torneioId)
            => _torneiosPrincipais.Contains(torneioId);
    }
}
