namespace APIFootballScout.Domain.Specifications.Base
{
    public interface ISpecification<in T>
    {
        bool IsSatisfiedBy(T candidate);
    }
}
