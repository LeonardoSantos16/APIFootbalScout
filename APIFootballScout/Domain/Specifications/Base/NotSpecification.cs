namespace APIFootballScout.Domain.Specifications.Base
{
    internal sealed class NotSpecification<T>(ISpecification<T> inner) : Specification<T>
    {
        private readonly ISpecification<T> _inner = inner ?? throw new ArgumentNullException(nameof(inner));

        public override bool IsSatisfiedBy(T candidate) => !_inner.IsSatisfiedBy(candidate);
    }
}
