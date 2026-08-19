namespace APIFootballScout.Domain.Specifications.Base
{
    internal sealed class OrSpecification<T>(ISpecification<T> left, ISpecification<T> right) : Specification<T>
    {
        private readonly ISpecification<T> _left = left ?? throw new ArgumentNullException(nameof(left));
        private readonly ISpecification<T> _right = right ?? throw new ArgumentNullException(nameof(right));

        public override bool IsSatisfiedBy(T candidate)
            => _left.IsSatisfiedBy(candidate) || _right.IsSatisfiedBy(candidate);
    }
}
