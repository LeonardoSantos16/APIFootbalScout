using APIFootballScout.Domain.Specifications.Base;

namespace APIFootballScout.Domain.Specifications
{
    public sealed class JogadorPrincipalSpecification : Specification<int>
    {
        private readonly HashSet<int> _jogadoresPrincipais;

        public JogadorPrincipalSpecification(IEnumerable<int> jogadoresPrincipais)
            => _jogadoresPrincipais = [.. jogadoresPrincipais];

        public override bool IsSatisfiedBy(int jogadorId)
            => _jogadoresPrincipais.Contains(jogadorId);
    }
}
