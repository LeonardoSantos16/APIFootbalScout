namespace APIFootballScout.Domain.Base
{
    public interface ISpecification<in T>
    {
        bool IsSatisfiedBy(T candidate);
    }
}
