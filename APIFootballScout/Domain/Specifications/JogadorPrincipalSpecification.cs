namespace APIFootballScout.Domain.Specifications
{
    public sealed class JogadorPrincipalSpecification : ISpecification<int> //implementar interface de specification
    {
        private readonly HashSet<int> _jogadoresPrincipais;

        public JogadorPrincipalSpecification(IEnumerable<int> jogadoresPrincipais)
            => _jogadoresPrincipais = [.. jogadoresPrincipais];

        public bool IsSatisfiedBy(int jogadorId)
            => _jogadoresPrincipais.Contains(jogadorId);
    }
}
